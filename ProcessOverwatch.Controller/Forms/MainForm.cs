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
        private readonly List<IActorRef> _remoteAgents = [];
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

        public MainForm() => InitializeComponent();

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

        private async Task SetupActorsAsync()
        {
            var config = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = remote
                }
                remote {
                    dot-netty.tcp {
                        hostname = ""0.0.0.0""
                        public-hostname = ""192.168.1.139""
                        port = 8935
                    }
                }
            }");

            _actorSystem = ActorSystem.Create("ProcessMonitor", config);
            _statusUpdateActor = _actorSystem.ActorOf(Props.Create(() => new StatusUpdateActor(this)), "statusUpdate");
            _localMonitorActor = _actorSystem.ActorOf(Props.Create(() => new LocalMonitorActor(_statusUpdateActor)), "localMonitor");
            _localCoordinatorActor = _actorSystem.ActorOf(Props.Create(() => new CoordinatorActor(_statusUpdateActor, _localMonitorActor)), "localCoordinator");
            SetNotifierActor(_actorSystem.ActorOf(Props.Create(() => new EmailNotifierActor(AppState.GetConfig()))));

            
            var remoteAddresses = new List<string>();
            var remoteProcesses = _processesEnabled.Where(p => !string.IsNullOrEmpty(p.IPAddress)).ToList();
            remoteAddresses.AddRange(from process in remoteProcesses
                                     where !remoteAddresses.Contains(process.IPAddress)
                                     select $@"akka.tcp://ProcessOverwatchAgent@{process.IPAddress}:8935/user/agent");
            foreach (var address in remoteAddresses)
            {
                try
                {
                    var remoteActor = await _actorSystem.ActorSelection(address).ResolveOne(TimeSpan.FromSeconds(10));
                    _remoteAgents.Add(remoteActor);
                }
                catch (ActorNotFoundException ex)
                {
                    // Log and continue - remote agent not available
                    Log.Warning(ex, "Remote agent not available at {Address}", address);
                }
            }
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

            _timer.Elapsed += new ElapsedEventHandler(MonitorTimer!);
            _timer.Enabled = true;
        }

        private void MonitorTimer(object source, ElapsedEventArgs e)
        {
            StartOrStopMonitoring();
        }

        private void LogTextBox(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(() => (DateTime.Now + " - " + message + Environment.NewLine) + txtLog.Text);
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
                await SaveAndReload();
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
                await SaveAndReload();
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
                await SaveAndReload();
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
            await SaveAndReload();

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

        private async Task SaveAndReload()
        {
            AppState.SaveState();
            LoadState();
            SetupDataBindings();
            await RefreshRemoteAgentsAsync();
            SetupTimer();
            LogTextBox("Configuration saved and reloaded.");
        }

        private async Task RefreshRemoteAgentsAsync()
        {
            // Build the set of addresses that SHOULD be connected
            var desiredAddresses = _processesEnabled
                .Where(p => !string.IsNullOrEmpty(p.IPAddress))
                .Select(p => p.IPAddress)
                .Distinct()
                .Select(ip => $@"akka.tcp://ProcessOverwatchAgent@{ip}:8935/user/agent")
                .ToHashSet();

            // Remove agents whose address is no longer in the desired set
            _remoteAgents.RemoveAll(a => !desiredAddresses.Any(addr => a.Path.ToString().Contains(addr)));

            // Determine which addresses are already connected
            var connectedAddresses = _remoteAgents
                .Select(a => a.Path.ToString())
                .ToHashSet();

            // Connect to new addresses that aren't already connected
            foreach (var address in desiredAddresses.Where(a => !connectedAddresses.Any(c => c.Contains(a))))
            {
                try
                {
                    IActorRef remoteActor = await _actorSystem!.ActorSelection(address).ResolveOne(TimeSpan.FromSeconds(10));
                    _remoteAgents.Add(remoteActor);
                    LogTextBox($"Connected to remote agent at {address}");
                }
                catch (ActorNotFoundException ex)
                {
                    Log.Warning(ex, "Remote agent not available at {Address}", address);
                    LogTextBox(Properties.Resources.Remote_Agent_Not_Available + address);  
                }
            }
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

            await SetupActorsAsync();

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
        private void StartOrStopMonitoring()
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

                // Send remote processes to appropriate Agents
                foreach (var group in remoteProcesses.GroupBy(p => p.IPAddress))
                {
                    var agent = _remoteAgents.FirstOrDefault(a => a.Path.ToString().Contains(group.Key));
                    if (agent != null)
                    {
                        agent.Tell(new CheckProcess([.. group]), _localCoordinatorActor);
                    }
                    else
                    {
                        LogTextBox($"Process Watchdog agent not found for {group.Key}");
                    }
                }

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
