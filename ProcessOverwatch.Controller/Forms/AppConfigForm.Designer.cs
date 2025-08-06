namespace ProcessOverwatch.Controller
{
    partial class AppConfigForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblSmtpServer;
        private System.Windows.Forms.TextBox txtSmtpServer;
        private System.Windows.Forms.Label lblSmtpPort;
        private System.Windows.Forms.NumericUpDown numSmtpPort;
        private System.Windows.Forms.Label lblSmtpUser;
        private System.Windows.Forms.TextBox txtSmtpUser;
        private System.Windows.Forms.Label lblSmtpPass;
        private System.Windows.Forms.TextBox txtSmtpPass;
        private System.Windows.Forms.Label lblFromEmail;
        private System.Windows.Forms.TextBox txtFromEmail;
        private System.Windows.Forms.Label lblToEmail;
        private System.Windows.Forms.TextBox txtToEmail;
        private System.Windows.Forms.Label lblCheckInterval;
        private System.Windows.Forms.NumericUpDown numCheckInterval;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppConfigForm));
            lblSmtpServer = new Label();
            txtSmtpServer = new TextBox();
            lblSmtpPort = new Label();
            numSmtpPort = new NumericUpDown();
            lblSmtpUser = new Label();
            txtSmtpUser = new TextBox();
            lblSmtpPass = new Label();
            txtSmtpPass = new TextBox();
            lblFromEmail = new Label();
            txtFromEmail = new TextBox();
            lblToEmail = new Label();
            txtToEmail = new TextBox();
            lblCheckInterval = new Label();
            numCheckInterval = new NumericUpDown();
            btnOK = new Button();
            btnCancel = new Button();
            btnShowPassword = new Button();
            chkAutostartMonitoring = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)numSmtpPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCheckInterval).BeginInit();
            SuspendLayout();
            // 
            // lblSmtpServer
            // 
            lblSmtpServer.AutoSize = true;
            lblSmtpServer.Location = new Point(20, 15);
            lblSmtpServer.Name = "lblSmtpServer";
            lblSmtpServer.Size = new Size(73, 15);
            lblSmtpServer.TabIndex = 0;
            lblSmtpServer.Text = "SMTP Server";
            // 
            // txtSmtpServer
            // 
            txtSmtpServer.Location = new Point(140, 12);
            txtSmtpServer.Name = "txtSmtpServer";
            txtSmtpServer.Size = new Size(249, 23);
            txtSmtpServer.TabIndex = 1;
            // 
            // lblSmtpPort
            // 
            lblSmtpPort.AutoSize = true;
            lblSmtpPort.Location = new Point(20, 50);
            lblSmtpPort.Name = "lblSmtpPort";
            lblSmtpPort.Size = new Size(63, 15);
            lblSmtpPort.TabIndex = 2;
            lblSmtpPort.Text = "SMTP Port";
            // 
            // numSmtpPort
            // 
            numSmtpPort.Location = new Point(140, 48);
            numSmtpPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numSmtpPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSmtpPort.Name = "numSmtpPort";
            numSmtpPort.Size = new Size(120, 23);
            numSmtpPort.TabIndex = 3;
            numSmtpPort.Value = new decimal(new int[] { 587, 0, 0, 0 });
            // 
            // lblSmtpUser
            // 
            lblSmtpUser.AutoSize = true;
            lblSmtpUser.Location = new Point(20, 85);
            lblSmtpUser.Name = "lblSmtpUser";
            lblSmtpUser.Size = new Size(64, 15);
            lblSmtpUser.TabIndex = 4;
            lblSmtpUser.Text = "SMTP User";
            // 
            // txtSmtpUser
            // 
            txtSmtpUser.Location = new Point(140, 82);
            txtSmtpUser.Name = "txtSmtpUser";
            txtSmtpUser.Size = new Size(247, 23);
            txtSmtpUser.TabIndex = 5;
            // 
            // lblSmtpPass
            // 
            lblSmtpPass.AutoSize = true;
            lblSmtpPass.Location = new Point(20, 120);
            lblSmtpPass.Name = "lblSmtpPass";
            lblSmtpPass.Size = new Size(91, 15);
            lblSmtpPass.TabIndex = 6;
            lblSmtpPass.Text = "SMTP Password";
            // 
            // txtSmtpPass
            // 
            txtSmtpPass.Location = new Point(140, 117);
            txtSmtpPass.Name = "txtSmtpPass";
            txtSmtpPass.Size = new Size(220, 23);
            txtSmtpPass.TabIndex = 7;
            txtSmtpPass.UseSystemPasswordChar = true;
            // 
            // lblFromEmail
            // 
            lblFromEmail.AutoSize = true;
            lblFromEmail.Location = new Point(20, 155);
            lblFromEmail.Name = "lblFromEmail";
            lblFromEmail.Size = new Size(67, 15);
            lblFromEmail.TabIndex = 8;
            lblFromEmail.Text = "From Email";
            // 
            // txtFromEmail
            // 
            txtFromEmail.Location = new Point(140, 152);
            txtFromEmail.Name = "txtFromEmail";
            txtFromEmail.Size = new Size(247, 23);
            txtFromEmail.TabIndex = 9;
            // 
            // lblToEmail
            // 
            lblToEmail.AutoSize = true;
            lblToEmail.Location = new Point(20, 190);
            lblToEmail.Name = "lblToEmail";
            lblToEmail.Size = new Size(52, 15);
            lblToEmail.TabIndex = 10;
            lblToEmail.Text = "To Email";
            // 
            // txtToEmail
            // 
            txtToEmail.Location = new Point(140, 187);
            txtToEmail.Name = "txtToEmail";
            txtToEmail.Size = new Size(247, 23);
            txtToEmail.TabIndex = 11;
            // 
            // lblCheckInterval
            // 
            lblCheckInterval.AutoSize = true;
            lblCheckInterval.Location = new Point(20, 225);
            lblCheckInterval.Name = "lblCheckInterval";
            lblCheckInterval.Size = new Size(104, 15);
            lblCheckInterval.TabIndex = 12;
            lblCheckInterval.Text = "Check Interval (m)";
            // 
            // numCheckInterval
            // 
            numCheckInterval.Location = new Point(140, 222);
            numCheckInterval.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numCheckInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCheckInterval.Name = "numCheckInterval";
            numCheckInterval.Size = new Size(120, 23);
            numCheckInterval.TabIndex = 13;
            numCheckInterval.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // btnOK
            // 
            btnOK.Location = new Point(106, 268);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 14;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(226, 268);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnShowPassword
            // 
            btnShowPassword.Image = (Image)resources.GetObject("btnShowPassword.Image");
            btnShowPassword.Location = new Point(359, 117);
            btnShowPassword.Name = "btnShowPassword";
            btnShowPassword.Size = new Size(30, 23);
            btnShowPassword.TabIndex = 16;
            btnShowPassword.UseVisualStyleBackColor = true;
            btnShowPassword.Click += button1_Click;
            // 
            // chkAutostartMonitoring
            // 
            chkAutostartMonitoring.AutoSize = true;
            chkAutostartMonitoring.Location = new Point(312, 226);
            chkAutostartMonitoring.Name = "chkAutostartMonitoring";
            chkAutostartMonitoring.Size = new Size(75, 19);
            chkAutostartMonitoring.TabIndex = 17;
            chkAutostartMonitoring.Text = "Autostart";
            chkAutostartMonitoring.UseVisualStyleBackColor = true;
            // 
            // AppConfigForm
            // 
            ClientSize = new Size(399, 315);
            Controls.Add(chkAutostartMonitoring);
            Controls.Add(btnShowPassword);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(numCheckInterval);
            Controls.Add(lblCheckInterval);
            Controls.Add(txtToEmail);
            Controls.Add(lblToEmail);
            Controls.Add(txtFromEmail);
            Controls.Add(lblFromEmail);
            Controls.Add(txtSmtpPass);
            Controls.Add(lblSmtpPass);
            Controls.Add(txtSmtpUser);
            Controls.Add(lblSmtpUser);
            Controls.Add(numSmtpPort);
            Controls.Add(lblSmtpPort);
            Controls.Add(txtSmtpServer);
            Controls.Add(lblSmtpServer);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AppConfigForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Application Configuration";
            ((System.ComponentModel.ISupportInitialize)numSmtpPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCheckInterval).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private Button btnShowPassword;
        private CheckBox chkAutostartMonitoring;
    }

}