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

        public ProcessConfigForm(MonitoredProcess? process = null)
        {
            InitializeComponent();

            if (process is null)
            {
                Process = new MonitoredProcess();
            }
            else
            {
                Process = process;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtExePath.Text = dlg.FileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
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
            Process.RemoteIPAndPort = txtRemoteIP.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => Close();

        private void ProcessConfigForm_Load(object sender, EventArgs e)
        {
            txtFriendlyName.Text = Process.FriendlyName;
            txtExePath.Text = Process.ExecutablePath;
            txtArguments.Text = Process.Arguments;
            chkEnabled.Checked = Process.IsEnabled;
            chkRestart.Checked = Process.RestartIfNotRunning;
            txtRemoteIP.Text = Process.RemoteIPAndPort;
        }
    }
}
