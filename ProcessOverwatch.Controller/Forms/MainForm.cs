using Akka.Actor;
using Akka.Configuration;
using ProcessOverwatch.Controller.Actors;
using ProcessOverwatch.Shared;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using System.Reflection;
using System.Timers;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProcessOverwatch.Controller
{
    public partial class MainForm : Form
    {
        private BindingList<MonitoredProcess> _processesEnabled = [];
        private BindingList<MonitoredProcess> _processesDisabled = [];

        private ActorSystem? _actorSystem;
        private IActorRef _localMonitorActor = null!;
        private IActorRef _statusUpdateActor = null!;
        private IActorRef _localCoordinatorActor = null!;
        private IActorRef notifierActor = null!;

        private readonly System.Timers.Timer _timer = new();
        private DateTime _nextCheck;

        private bool _isMonitoring = false;
        private bool _isExecutingMonitorCheck;

        public IActorRef GetNotifierActor()
        {
            return notifierActor;
        }

        public void SetNotifierActor(IActorRef value)
        {
            notifierActor = value;
        }

        private delegate void InvokeNextCheckLabelDelegate(string sText);

        public MainForm()
        {
            InitializeComponent();
            _timer.Elapsed += MonitorTimer;
        }

        private void LoadState()
        {
            AppState.LoadState();

            _processesEnabled = new BindingList<MonitoredProcess>([.. AppState.GetProcesses().Where(p =>p.IsEnabled)]);
            _processesDisabled = new BindingList<MonitoredProcess>([.. AppState.GetProcesses().Where(p => !p.IsEnabled)]);
        }

        private void SetupDataBindings()
        {
            dgvEnabled.AutoGenerateColumns = false;
            dgvDisabled.AutoGenerateColumns = false;

            dgvEnabled.AllowUserToResizeColumns = true;
            dgvDisabled.AllowUserToResizeColumns = true;

            dgvEnabled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvDisabled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;

            dgvEnabled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvDisabled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // Set up defined columns
            SetupColumns(dgvEnabled);
            SetupColumns(dgvDisabled);

            dgvEnabled.DataSource = _processesEnabled;
            dgvDisabled.DataSource = _processesDisabled;

        }

        private static void SetupColumns(DataGridView dgv)
        {
            dgv.Columns.Clear();

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "FriendlyName",
                HeaderText = "Friendly Name",
                Name = "FriendlyName",
                Width = 150,
                Resizable = DataGridViewTriState.True
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ExecutablePath",
                HeaderText = "Executable Path",
                Name = "ExecutablePath",
                Width = 500,
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Resizable = DataGridViewTriState.True
            });

            dgv.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "RestartIfNotRunning",
                HeaderText = "Restart",
                Name = "RestartIfNotRunning",
                Width = 80,
                Resizable = DataGridViewTriState.True
            });

            // Optional: hide arguments column if needed
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Arguments",
                Name = "Arguments",
                Visible = false,
                Resizable = DataGridViewTriState.True
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "IPAddress",
                HeaderText = "IP Address",
                Name = "IPAddress",
                Width = 140,
                Resizable = DataGridViewTriState.True
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                Name = "Status",
                Width = 270,
                Resizable = DataGridViewTriState.True
            });
        }

        private void SetupActors()
        {
            // Port 0 lets the OS pick a free port so the controller can run alongside a local agent (which owns 8935)
            var config = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.public-hostname = \"{NetworkHelper.GetPublicHostname()}\"")
                .WithFallback(ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = remote
                }
                remote {
                    dot-netty.tcp {
                        hostname = ""0.0.0.0""
                        port = 0
                    }
                }
            }"));

            _actorSystem = ActorSystem.Create("ProcessMonitor", config);
            _statusUpdateActor = _actorSystem.ActorOf(Props.Create(() => new StatusUpdateActor(this)), "statusUpdate");
            _localMonitorActor = _actorSystem.ActorOf(Props.Create(() => new LocalMonitorActor(_statusUpdateActor)), "localMonitor");
            _localCoordinatorActor = _actorSystem.ActorOf(Props.Create(() => new CoordinatorActor(_statusUpdateActor, _localMonitorActor)), "localCoordinator");
            SetNotifierActor(_actorSystem.ActorOf(Props.Create(() => new EmailNotifierActor()), "emailNotifier"));
        }

        private void InvokeToNextCheckLabel(string sText)
        {
            if (lblNextCheck.InvokeRequired)
            {
                this.Invoke(new InvokeNextCheckLabelDelegate(InvokeToNextCheckLabel), sText);
                return;
            }

            lblNextCheck.Text = sText;
        }

        private void SetupTimer()
        {
            _timer.Interval = (AppState.GetConfig().MonitorIntervalMinutes * 60000);
            _nextCheck = DateTime.Now.AddMinutes(AppState.GetConfig().MonitorIntervalMinutes);
            lblNextCheck.Text = $"Next Check At: {_nextCheck:HH:mm}";

            // Only tick while monitoring; the Elapsed handler is attached once in the constructor
            _timer.Enabled = _isMonitoring;
        }

        private async void MonitorTimer(object? source, ElapsedEventArgs e)
        {
            await RunMonitorCheckAsync();
        }

        private void LogTextBox(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(() => txtLog.Text = (DateTime.Now + " - " + message + Environment.NewLine) + txtLog.Text);
            }
            else
            {
                txtLog.Text = (DateTime.Now + " - " + message + Environment.NewLine) + txtLog.Text;
            }
        }

        private async void BtnAddProcess_Click(object sender, EventArgs e)
        {
            var form = new ProcessConfigForm();
            if (await form.ShowDialogAsync() == DialogResult.OK)
            {
                if (AppState.GetProcesses().Any(p => string.Equals(p.ExecutablePath, form.Process.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Log.Information("Adding process: {FriendlyName}", form.Process.FriendlyName);
                LogTextBox($"Adding process: {form.Process.FriendlyName}");
                AppState.GetProcesses().Add(form.Process);
                SaveAndReload();
            }
        }

        private async void BtnEditProcess_Click(object sender, EventArgs e)
        {
            MonitoredProcess? selected;
            if (tabControl.SelectedTab == tabEnabled)
                selected = dgvEnabled.CurrentRow?.DataBoundItem as MonitoredProcess;
            else
                selected = dgvDisabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to edit.");
                return;
            }

            var form = new ProcessConfigForm(selected, _actorSystem);
            if (await form.ShowDialogAsync() == DialogResult.OK)
            {
                await ModifyProcess(selected, form.Process);
            }
        }

        private async void BtnDeleteProcess_Click(object sender, EventArgs e)
        {
            MonitoredProcess? selected;
            if (tabControl.SelectedTab == tabEnabled)
                selected = dgvEnabled.CurrentRow?.DataBoundItem as MonitoredProcess;
            else
                selected = dgvDisabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to delete.");
                return;
            }

            var res = MessageBox.Show($"Delete process '{selected.FriendlyName}'?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                Log.Information("Deleting process: {FriendlyName}", selected.FriendlyName);
                AppState.GetProcesses().Remove(selected);
                SaveAndReload();
            }
        }

        private async void BtnConfig_Click(object sender, EventArgs e)
        {
            var form = new AppConfigForm(AppState.GetConfig());
            if (await form.ShowDialogAsync() == DialogResult.OK)
            {
                LogTextBox("Updating application configuration.");
                Log.Information("Updating application configuration.");
                AppState.SetConfig(form.Config);
                SaveAndReload();
            }
        }
        private async void DgvEnabled_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MonitoredProcess? selected;
            selected = dgvEnabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to edit.");
                return;
            }

            var form = new ProcessConfigForm(selected, _actorSystem);
            if (await form.ShowDialogAsync() == DialogResult.OK)
            {
                await ModifyProcess(selected, form.Process);
            }

        }

        private async void DgvDisabled_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MonitoredProcess? selected;
            selected = dgvDisabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to edit.");
                return;
            }

            var form = new ProcessConfigForm(selected, _actorSystem);
            if (await form.ShowDialogAsync() == DialogResult.OK)
            {
                await ModifyProcess(selected, form.Process);
            }

        }

        private async Task ModifyProcess( MonitoredProcess monitoredProcess, MonitoredProcess updatedMonitoredProcess)
        {
            if (AppState.GetProcesses().Any(p => string.Equals(p.ExecutablePath, updatedMonitoredProcess.ExecutablePath, StringComparison.OrdinalIgnoreCase)) && monitoredProcess.ExecutablePath != updatedMonitoredProcess.ExecutablePath)
            {
                MessageBox.Show("A process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int index = AppState.GetProcesses().FindIndex(x => x == monitoredProcess);
            if (index != -1)
            {
                AppState.GetProcesses()[index] = updatedMonitoredProcess;
            }
            LogTextBox($"Editing process: {updatedMonitoredProcess.FriendlyName}");
            Log.Information("Editing process: {FriendlyName}", updatedMonitoredProcess.FriendlyName);
            SaveAndReload();

        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;

                notifySystemTrayIcon.Visible = true;
            }
            else
            {
                notifySystemTrayIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void NotifySystemTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void SaveAndReload()
        {
            AppState.SaveState();
            LoadState();
            SetupDataBindings();
            SetupTimer();
            LogTextBox("Configuration saved and reloaded.");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _actorSystem?.Terminate().Wait();
            base.OnFormClosing(e);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await MainForm_LoadAsync();
        }

        private async Task MainForm_LoadAsync()
        {
            LoadState();

            SetupDataBindings();

            SetupActors();

            SetupTimer();

            if(AppState.GetConfig().AutoStartMonitoring)
            {
                InvokeMonitoringProcess();
            }

        }

        private void BtnStartMonitoring_Click(object sender, EventArgs e)
        {
            InvokeMonitoringProcess();
        }

        private void InvokeMonitoringProcess()
        {
            if (_isMonitoring)
            {
                _timer.Stop();
                _isMonitoring = false;
                EnableDisableControls(true);
                LogTextBox("Monitoring stopped.");
                btnStartMonitoring.Text = "Start";
            }
            else
            {
                _isMonitoring = true;
                EnableDisableControls(false);
                btnStartMonitoring.Text = "Stop";
                LogTextBox("Monitoring started.");
                _timer.Start();
            }

        }
        private async Task RunMonitorCheckAsync()
        {
            if (_isExecutingMonitorCheck)
                return;

            _isExecutingMonitorCheck = true;
            try
            {
                // Filter local and remote processes
                var localProcesses = _processesEnabled.Where(p => string.IsNullOrEmpty(p.IPAddress)).ToList();
                var remoteProcesses = _processesEnabled.Where(p => !string.IsNullOrEmpty(p.IPAddress)).ToList();

                // Send local processes to LocalMonitorActor
                if (localProcesses.Count != 0)
                {
                    _localCoordinatorActor.Tell(new CheckProcess(localProcesses));
                }

                // Send remote processes to their agents, checking all machines in parallel
                await Task.WhenAll(remoteProcesses
                    .GroupBy(p => p.IPAddress)
                    .Select(group => CheckRemoteMachineAsync(group.Key, [.. group])));

                _nextCheck = DateTime.Now.AddMinutes(AppState.GetConfig().MonitorIntervalMinutes);
                InvokeToNextCheckLabel($"Next Check At: {_nextCheck:HH:mm}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure in Monitoring");
                LogTextBox($"Monitoring error: {ex.Message}");
            }
            finally
            {
                _isExecutingMonitorCheck = false;
            }
        }

        private async Task CheckRemoteMachineAsync(string ipAddress, List<MonitoredProcess> processes)
        {
            // Resolve the agent on every check: a cached reference silently stops working if the agent restarts
            var address = $@"akka.tcp://ProcessOverwatchAgent@{ipAddress}:8935/user/agent";
            try
            {
                var agent = await _actorSystem!.ActorSelection(address).ResolveOne(TimeSpan.FromSeconds(10));
                agent.Tell(new CheckProcess(processes), _localCoordinatorActor);
                GetNotifierActor().Tell(new RemoteMachineReachable(ipAddress));
            }
            catch (ActorNotFoundException ex)
            {
                bool machineResponds = await PingAsync(ipAddress);
                string status = machineResponds
                    ? $"⚠️ Agent not responding on {ipAddress}"
                    : $"⚠️ Machine {ipAddress} is not reachable";

                Log.Warning(ex, "Remote agent not available at {Address}", address);
                LogTextBox(status);
                SetProcessStatus(processes, status);

                GetNotifierActor().Tell(new RemoteMachineUnreachable(ipAddress, machineResponds, [.. processes.Select(p => p.FriendlyName)]));
            }
        }

        private static async Task<bool> PingAsync(string ipAddress)
        {
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = await ping.SendPingAsync(ipAddress, 3000);
                return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
            }
            catch (System.Net.NetworkInformation.PingException)
            {
                return false;
            }
        }

        private void SetProcessStatus(List<MonitoredProcess> processes, string status)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetProcessStatus(processes, status));
                return;
            }

            foreach (var process in processes)
                process.Status = status;
            _processesEnabled.ResetBindings();
        }

        public void UpdateProcessStatus(ProcessStatusResponse response)
        {
            var process = _processesEnabled.FirstOrDefault(p =>
                p.FriendlyName == response.FriendlyName &&
                p.ExecutablePath == response.ExecutablePath &&
                p.IPAddress == response.RemoteIPPort);
            if (process != null)
            {
                process.Status = response.Status;
                LogTextBox($"{process.FriendlyName} on {process.IPAddress}: {process.Status}");
                _processesEnabled.ResetBindings(); 
            }
            else
            {
                LogTextBox($"Process not found: {response.FriendlyName} ({response.ExecutablePath}) on {response.MachineName}");
            }
        }

        private void EnableDisableControls(bool enable)
        {
            btnAddProcess.Enabled = enable;
            btnEditProcess.Enabled = enable;
            btnDeleteProcess.Enabled = enable;
            btnConfig.Enabled = enable;

            dgvEnabled.Enabled = enable;
            dgvDisabled.Enabled = enable;
        }


    }
}
