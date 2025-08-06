using Akka.Actor;
using Akka.Configuration;
using ProcessOverwatch.Controller.Actors;
using ProcessOverwatch.Shared;
using Serilog;
using System.ComponentModel;
using System.Net.Mail;
using System.Reflection;
using System.Timers;
using Serilog;

namespace ProcessOverwatch.Controller
{
    public partial class MainForm : Form
    {
        private BindingList<MonitoredProcess> _processesEnabled = new();
        private BindingList<MonitoredProcess> _processesDisabled = new();

        private ActorSystem _actorSystem = null!;
        private IActorRef _localMonitorActor = null!;
        private List<IActorRef> _remoteAgents = new List<IActorRef>();
        private IActorRef _notifierActor = null!;

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
            ProcessConfig.LoadConfig();

            _processesEnabled = new BindingList<MonitoredProcess>(ProcessConfig.Processes.Where(p => p.IsEnabled).ToList());
            _processesDisabled = new BindingList<MonitoredProcess>(ProcessConfig.Processes.Where(p => !p.IsEnabled).ToList());
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
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
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
                Visible = false
            });

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Remote IP/Port",
                HeaderText = "Remote",
                Name = "RemoteIPPort",
                Width = 80,
                Resizable = DataGridViewTriState.True
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Status",
                HeaderText = "Status",
                Name = "Status",
                Width = 80,
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
                        hostname = 192.168.1.100
                    }
                }");

            _actorSystem = ActorSystem.Create("ProcessMonitorCoordinator");
            _localMonitorActor = _actorSystem.ActorOf(Props.Create(() => new LocalMonitorActor()), "localMonitor");

            var remoteAddresses = new List<string>
            {
                "akka.tcp://AgentSystem@127.0.0.1:8091/user/agent"
                // Add more addresses here
            };
            foreach (var address in remoteAddresses)
            {
                var remoteActor = _actorSystem.ActorSelection(address);
                _remoteAgents.Add(remoteActor.ResolveOne(TimeSpan.FromSeconds(5)).Result);
            }
            _notifierActor = _actorSystem.ActorOf(Props.Create(() => new EmailNotifierActor(AppState.Config)));
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
                if (AppState.Processes.Any(p =>
                    !ReferenceEquals(p, selected) &&
                    string.Equals(p.ExecutablePath, form.Process.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Another process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LogTextBox($"Editing process: {form.Process.FriendlyName}");
                Log.Information($"Editing process: {form.Process.FriendlyName}");
                SaveAndReload();
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
            base.OnFormClosing(e);
            _actorSystem.Terminate().Wait();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadState();

            SetupDataBindings();

            SetupActors();

            SetupTimer();

            if(AppState.Config.AutosStartMonitoring)
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
                this.WindowState = FormWindowState.Minimized;
            }

        }
        private void StartOrStopMonitoring()
        {
            try
            {
                _localMonitorActor.Tell(new CheckProcess());

                // Tell all remote agents to perform check
                foreach (var agent in _remoteAgents)
                {
                    agent.Tell(new CheckProcess());
                }

                _nextCheck = DateTime.Now.AddMinutes(AppState.Config.MonitorIntervalMinutes);

                // Update the label safely
                if (lblNextCheck.InvokeRequired)
                {
                    lblNextCheck.Invoke(() =>
                    {
                        lblNextCheck.Text = $"Next Check At: {_nextCheck:HH:mm}";
                    });
                }
                else
                {
                    lblNextCheck.Text = $"Next Check At: {_nextCheck:HH:mm}";
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failure in Monitoring");
                throw;
            }
        }
        private void EnableDisableControls(bool enable)
        {
            btnAddProcess.Enabled = enable;
            btnEditProcess.Enabled = enable;
            btnDeleteProcess.Enabled = enable;
            btnConfig.Enabled = enable;
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
                // Check for uniqueness excluding the currently edited one
                if (AppState.Processes.Any(p =>
                    !ReferenceEquals(p, selected) &&
                    string.Equals(p.ExecutablePath, form.Process.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Another process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                LogTextBox($"Editing process: {form.Process.FriendlyName}");
                Log.Information($"Editing process: {form.Process.FriendlyName}");
                SaveAndReload();
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
                // Check for uniqueness excluding the currently edited one
                if (AppState.Processes.Any(p =>
                    !ReferenceEquals(p, selected) &&
                    string.Equals(p.ExecutablePath, form.Process.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Another process with the same executable path already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LogTextBox($"Editing process: {form.Process.FriendlyName}");
                Log.Information($"Editing process: {form.Process.FriendlyName}");
                SaveAndReload();
            }

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
    }
}
