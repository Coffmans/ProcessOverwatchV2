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
    public partial class AppConfigForm : Form
    {
        public AppConfig Config { get; private set; }

        public AppConfigForm(AppConfig config)
        {
            InitializeComponent();

            Config = config;

            txtSmtpServer.Text = Config.SmtpServer;
            numSmtpPort.Value = Config.SmtpPort > 0 ? Config.SmtpPort : 587;
            txtSmtpUser.Text = Config.SmtpUser;
            txtSmtpPass.Text = Config.SmtpPassword;
            txtFromEmail.Text = Config.EmailFrom;
            txtToEmail.Text = Config.EmailTo;
            numCheckInterval.Value = Config.MonitorIntervalMinutes > 0 ? Config.MonitorIntervalMinutes : 60;
            chkAutostartMonitoring.Checked = Config.AutoStartMonitoring;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool _bNoEmailSettingsToBeSaved = false;
            if (string.IsNullOrWhiteSpace(txtSmtpServer.Text))
            {
                if (DialogResult.No == MessageBox.Show("SMTP Server is missing. Without email settings, you will not be notified if a process is not running or restarted!\n\nDo you want to continue saving settings?",
                        "Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning))
                    return;
                _bNoEmailSettingsToBeSaved = true;
            }
            if (string.IsNullOrWhiteSpace(txtFromEmail.Text) && !_bNoEmailSettingsToBeSaved)
            {
                MessageBox.Show("From Email is required.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtToEmail.Text) && !_bNoEmailSettingsToBeSaved)
            {
                MessageBox.Show("To Email is required.");
                return;
            }

            Config.SmtpServer = txtSmtpServer.Text.Trim();
            Config.SmtpPort = (int)numSmtpPort.Value;
            Config.SmtpUser = txtSmtpUser.Text.Trim();
            Config.SmtpPassword = txtSmtpPass.Text;
            Config.EmailFrom = txtFromEmail.Text.Trim();
            Config.EmailTo = txtToEmail.Text.Trim();
            Config.MonitorIntervalMinutes = (int)numCheckInterval.Value;
            Config.AutoStartMonitoring = chkAutostartMonitoring.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) => Close();

        private void button1_Click(object sender, EventArgs e)
        {
            txtSmtpPass.UseSystemPasswordChar = !txtSmtpPass.UseSystemPasswordChar;
        }
    }
}
