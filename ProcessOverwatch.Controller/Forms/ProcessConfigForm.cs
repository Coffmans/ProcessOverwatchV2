using Akka.Actor;
using ProcessOverwatch.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessOverwatch.Controller
{
    public partial class ProcessConfigForm : Form
    {
        public MonitoredProcess Process { get; private set; }

        private const string TestResult = "Test Result";
        private readonly ActorSystem? _actorSystem;

        public ProcessConfigForm(MonitoredProcess? process = null, ActorSystem? actorSystem = null)
        {
            InitializeComponent();
            _actorSystem = actorSystem;

            if (process is null)
            {
                Process = new MonitoredProcess();
            }
            else
            {
                Process = process;
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtExePath.Text = dlg.FileName;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFriendlyName.Text))
            {
                MessageBox.Show("Friendly name is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtExePath.Text))
            {
                MessageBox.Show("Executable path is required.");
                return;
            }
            Process.FriendlyName = txtFriendlyName.Text.Trim();
            Process.ExecutablePath = txtExePath.Text.Trim();
            Process.Arguments = txtArguments.Text.Trim();
            Process.IsEnabled = chkEnabled.Checked;
            Process.RestartIfNotRunning = chkRestart.Checked;
            Process.IPAddress = txtRemoteIP.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e) => Close();

        private void ProcessConfigForm_Load(object sender, EventArgs e)
        {
            txtFriendlyName.Text = Process.FriendlyName;
            txtExePath.Text = Process.ExecutablePath;
            txtArguments.Text = Process.Arguments;
            chkEnabled.Checked = Process.IsEnabled;
            chkRestart.Checked = Process.RestartIfNotRunning;
            txtRemoteIP.Text = Process.IPAddress;
        }

        private async void BtnTestForProcess_Click(object sender, EventArgs e)
        {
            var exePath = txtExePath.Text.Trim();

            if (string.IsNullOrWhiteSpace(exePath))
            {
                MessageBox.Show("Executable path is required to test.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ipAddress = txtRemoteIP.Text.Trim();
            var processName = Path.GetFileNameWithoutExtension(exePath);

            if (string.IsNullOrEmpty(ipAddress))
            {
                TestLocalProcess(processName, exePath);
            }
            else
            {
                await TestRemoteProcessAsync(exePath, ipAddress);
            }
        }

        private void TestLocalProcess(string processName, string exePath)
        {
            bool isRunning = System.Diagnostics.Process.GetProcessesByName(processName).Length != 0;

            if (isRunning)
            {
                MessageBox.Show($"✅ '{processName}' is currently running on this machine.",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(exePath))
            {
                MessageBox.Show(
                    $"❌ '{processName}' is NOT running and the executable was not found at:\n\n{exePath}\n\n" +
                    "The process cannot be restarted with this path.",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (chkRestart.Checked)
            {
                var result = MessageBox.Show(
                    $"❌ '{processName}' is NOT running, but the executable was found.\n\n" +
                    "Restart is enabled. Would you like to attempt to start it now?",
                    TestResult, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = exePath,
                            Arguments = txtArguments.Text.Trim(),
                            UseShellExecute = true
                        };

                        System.Diagnostics.Process.Start(startInfo);
                        Thread.Sleep(1500);

                        bool nowRunning = System.Diagnostics.Process.GetProcessesByName(processName).Length != 0;

                        if (nowRunning)
                        {
                            MessageBox.Show($"🔁 Successfully started '{processName}'.",
                                TestResult, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"⚠️ Start command was issued but '{processName}' was not detected running.",
                                TestResult, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"⚠️ Failed to start '{processName}'.\n\n{ex.Message}",
                            TestResult, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(
                    $"❌ '{processName}' is NOT running.\n\n" +
                    $"The executable was found at:\n{exePath}\n\n" +
                    "Restart is not enabled for this process.",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task TestRemoteProcessAsync(string exePath, string ipAddress)
        {
            // First verify the remote machine is reachable
            try
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = ping.Send(ipAddress, 3000);

                if (reply.Status != System.Net.NetworkInformation.IPStatus.Success)
                {
                    MessageBox.Show(
                        $"❌ Remote machine '{ipAddress}' is NOT reachable (status: {reply.Status}).\n\n" +
                        "Ensure the machine is online and the agent is installed.",
                        TestResult, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"⚠️ Failed to reach remote machine '{ipAddress}'.\n\n{ex.Message}",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Now attempt to contact the remote agent and check the process
            if (_actorSystem is null)
            {
                MessageBox.Show(
                    $"✅ Remote machine '{ipAddress}' is reachable, but the actor system is not available.\n\n" +
                    "Start monitoring first to enable full remote process testing.",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var agentAddress = $"akka.tcp://ProcessOverwatchAgent@{ipAddress}:8935/user/agent";
                var remoteAgent = await _actorSystem.ActorSelection(agentAddress).ResolveOne(TimeSpan.FromSeconds(10));

                // Build a temporary process to check
                var testProcess = new MonitoredProcess
                {
                    FriendlyName = txtFriendlyName.Text.Trim(),
                    ExecutablePath = exePath,
                    Arguments = txtArguments.Text.Trim(),
                    IsEnabled = true,
                    RestartIfNotRunning = chkRestart.Checked,
                    IPAddress = ipAddress
                };

                // Ask the remote agent to check the process
                var response = await remoteAgent.Ask<ProcessStatusResponse>(
                    new CheckProcess([testProcess]),
                    TimeSpan.FromSeconds(15));

                string restartInfo = chkRestart.Checked && !response.IsRunning
                    ? "\n\nRestart is enabled — the agent attempted to start the process."
                    : "";

                MessageBox.Show(
                    $"Remote agent on '{ipAddress}' ({response.MachineName}) responded:\n\n" +
                    $"{response.Status}{restartInfo}",
                    TestResult, MessageBoxButtons.OK,
                    response.IsRunning ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (ActorNotFoundException)
            {
                MessageBox.Show(
                    $"✅ Remote machine '{ipAddress}' is reachable, but the ProcessOverwatch Agent is not running.\n\n" +
                    "Ensure the agent service is installed and started on the remote machine.",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"⚠️ Failed to communicate with the remote agent on '{ipAddress}'.\n\n{ex.Message}",
                    TestResult, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
