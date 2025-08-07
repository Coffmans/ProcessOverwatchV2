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


namespace ProcessOverwatch.Controller
{
    public partial class MainForm : Form
    {
        private BindingList<MonitoredProcess> _processesEnabled = new();
        private BindingList<MonitoredProcess> _processesDisabled = new();

        private ActorSystem _actorSystem = null!;
        private IActorRef _localMonitorActor = null!;
        private IActorRef _statusUpdateActor = null!;
        private IActorRef _localCoordinatorActor = null!;
        private List<IActorRef> _remoteAgents = new List<IActorRef>();
        public IActorRef _notifierActor = null!;

        private System.Timers.Timer _timer = new System.Timers.Timer();
        private DateTime _nextCheck;

        private bool _isMonitoring = false;

        private delegate void InvokeNextCheckLabelDelegate(string sText);

        public MainForm()
        {
            InitializeComponent();

        }

        private void LoadState()
        {
            AppState.LoadState();

            _processesEnabled = new BindingList<MonitoredProcess>(AppState.Processes.Where(p => p.IsEnabled).ToList());
            _processesDisabled = new BindingList<MonitoredProcess>(AppState.Processes.Where(p => !p.IsEnabled).ToList());
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

        private void SetupColumns(DataGridView dgv)
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
            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor.provider = remote
                    remote.dot-netty.tcp {
                        port = 8090
                        hostname = 127.0.0.1
                    }
                }");

            _actorSystem = ActorSystem.Create("ProcessMonitorCoordinator");
            _statusUpdateActor = _actorSystem.ActorOf(Props.Create(() => new StatusUpdateActor(this)), "statusUpdate");
            _localMonitorActor = _actorSystem.ActorOf(Props.Create(() => new LocalMonitorActor(_statusUpdateActor)), "localMonitor");
            _localCoordinatorActor = _actorSystem.ActorOf(Props.Create(() => new CoordinatorActor(_statusUpdateActor, _localMonitorActor)), "localCoordinator");
            _notifierActor = _actorSystem.ActorOf(Props.Create(() => new EmailNotifierActor(AppState.Config)));

            
            var remoteAddresses = new List<string>();
            var remoteProcesses = _processesEnabled.Where(p => !string.IsNullOrEmpty(p.IPAddress)).ToList();

            foreach (var process in remoteProcesses)
            {
                if (!remoteAddresses.Contains(process.IPAddress))
                {
                    remoteAddresses.Add($@"akka.tcp://ProcessMonitor@{process.IPAddress}:8935/user/agent");
                }
            }
            foreach (var address in remoteAddresses)
            {
                var remoteActor = _actorSystem.ActorSelection(address).ResolveOne(TimeSpan.FromSeconds(5)).Result;
                _remoteAgents.Add(remoteActor);
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
            _timer.Interval = (AppState.Config.MonitorIntervalMinutes * 60000);
            _nextCheck = DateTime.Now.AddMinutes(AppState.Config.MonitorIntervalMinutes);
            lblNextCheck.Text = $"Next Check At: {_nextCheck:HH:mm}";

            _timer.Elapsed += new ElapsedEventHandler(MonitorTimer!);
            _timer.AutoReset = true;
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

        private void btnAddProcess_Click(object sender, EventArgs e)
        {
            var form = new ProcessConfigForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (AppState.Processes.Any(p => string.Equals(p.ExecutablePath, form.Process.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Log.Information($"Adding process: {form.Process.FriendlyName}");
                LogTextBox($"Adding process: {form.Process.FriendlyName}");
                AppState.Processes.Add(form.Process);
                SaveAndReload();
            }
        }

        private void btnEditProcess_Click(object sender, EventArgs e)
        {
            MonitoredProcess? selected = null;
            if (tabControl.SelectedTab == tabEnabled)
                selected = dgvEnabled.CurrentRow?.DataBoundItem as MonitoredProcess;
            else
                selected = dgvDisabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to edit.");
                return;
            }

            var form = new ProcessConfigForm(selected);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ModifyProcess(selected, form.Process);
            }
        }

        private void btnDeleteProcess_Click(object sender, EventArgs e)
        {
            MonitoredProcess? selected = null;
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
                Log.Information($"Deleting process: {selected.FriendlyName}");
                AppState.Processes.Remove(selected);
                SaveAndReload();
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            var form = new AppConfigForm(AppState.Config);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LogTextBox("Updating application configuration.");
                Log.Information("Updating application configuration.");
                AppState.Config = form.Config;
                SaveAndReload();
            }
        }
        private void dgvEnabled_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MonitoredProcess? selected = null;
            selected = dgvEnabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to edit.");
                return;
            }

            var form = new ProcessConfigForm(selected);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ModifyProcess(selected, form.Process);
            }

        }

        private void dgvDisabled_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MonitoredProcess? selected = null;
            selected = dgvDisabled.CurrentRow?.DataBoundItem as MonitoredProcess;

            if (selected == null)
            {
                MessageBox.Show("Please select a process to edit.");
                return;
            }

            var form = new ProcessConfigForm(selected);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ModifyProcess(selected, form.Process);
            }

        }

        private void ModifyProcess( MonitoredProcess monitoredProcess, MonitoredProcess updatedMonitoredProcess)
        {
            if (AppState.Processes.Any(p => string.Equals(p.ExecutablePath, updatedMonitoredProcess.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
            {
                // Check if the selected process is the same as the one being edited
                if (monitoredProcess.ExecutablePath != updatedMonitoredProcess.ExecutablePath)
                {
                    MessageBox.Show("A process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            int index = AppState.Processes.FindIndex(x => x == monitoredProcess);
            if (index != -1)
            {
                AppState.Processes[index] = updatedMonitoredProcess;
            }
            LogTextBox($"Editing process: {updatedMonitoredProcess.FriendlyName}");
            Log.Information($"Editing process: {updatedMonitoredProcess.FriendlyName}");
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

        private void notifySystemTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
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
            _actorSystem.Terminate().Wait();
            base.OnFormClosing(e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadState();

            SetupDataBindings();

            SetupActors();

            SetupTimer();

            if(AppState.Config.AutoStartMonitoring)
            {
                InvokeMonitoringProcess();
            }

        }

        private void btnStartMonitoring_Click(object sender, EventArgs e)
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
            try
            {
                // Filter local and remote processes
                var localProcesses = _processesEnabled.Where(p => string.IsNullOrEmpty(p.IPAddress)).ToList();
                var remoteProcesses = _processesEnabled.Where(p => !string.IsNullOrEmpty(p.IPAddress)).ToList();

                // Send local processes to LocalMonitorActor
                if (localProcesses.Any())
                {
                    _localCoordinatorActor.Tell(new CheckProcess(localProcesses));
                }

                // Send remote processes to appropriate Agents
                foreach (var group in remoteProcesses.GroupBy(p => p.IPAddress))
                {
                    var agent = _remoteAgents.FirstOrDefault(a => a.Path.ToString().Contains(group.Key));
                    if (agent != null)
                    {
                        agent.Tell(new CheckProcess(group.ToList()), _localCoordinatorActor);
                    }
                    else
                    {
                        LogTextBox($"Process Watchdog agent not found for {group.Key}");
                    }
                }

                _nextCheck = DateTime.Now.AddMinutes(AppState.Config.MonitorIntervalMinutes);
                InvokeToNextCheckLabel($"Next Check At: {_nextCheck:HH:mm}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure in Monitoring");
                LogTextBox($"Monitoring error: {ex.Message}");
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
