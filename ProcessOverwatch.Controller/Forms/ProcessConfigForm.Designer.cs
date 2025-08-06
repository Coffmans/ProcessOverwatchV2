namespace ProcessOverwatch.Controller
{
    partial class ProcessConfigForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblFriendlyName;
        private System.Windows.Forms.TextBox txtFriendlyName;
        private System.Windows.Forms.Label lblExePath;
        private System.Windows.Forms.TextBox txtExePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblArguments;
        private System.Windows.Forms.TextBox txtArguments;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkRestart;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblFriendlyName = new Label();
            txtFriendlyName = new TextBox();
            lblExePath = new Label();
            txtExePath = new TextBox();
            btnBrowse = new Button();
            lblArguments = new Label();
            txtArguments = new TextBox();
            chkEnabled = new CheckBox();
            chkRestart = new CheckBox();
            btnOK = new Button();
            btnCancel = new Button();
            label1 = new Label();
            txtRemoteIP = new TextBox();
            SuspendLayout();
            // 
            // lblFriendlyName
            // 
            lblFriendlyName.AutoSize = true;
            lblFriendlyName.Location = new Point(6, 15);
            lblFriendlyName.Name = "lblFriendlyName";
            lblFriendlyName.Size = new Size(84, 15);
            lblFriendlyName.TabIndex = 0;
            lblFriendlyName.Text = "Friendly Name";
            // 
            // txtFriendlyName
            // 
            txtFriendlyName.Location = new Point(110, 12);
            txtFriendlyName.Name = "txtFriendlyName";
            txtFriendlyName.Size = new Size(571, 23);
            txtFriendlyName.TabIndex = 1;
            // 
            // lblExePath
            // 
            lblExePath.AutoSize = true;
            lblExePath.Location = new Point(6, 50);
            lblExePath.Name = "lblExePath";
            lblExePath.Size = new Size(90, 15);
            lblExePath.TabIndex = 2;
            lblExePath.Text = "Executable Path";
            // 
            // txtExePath
            // 
            txtExePath.Location = new Point(110, 47);
            txtExePath.Name = "txtExePath";
            txtExePath.Size = new Size(530, 23);
            txtExePath.TabIndex = 3;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(646, 45);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(35, 25);
            btnBrowse.TabIndex = 4;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // lblArguments
            // 
            lblArguments.AutoSize = true;
            lblArguments.Location = new Point(6, 85);
            lblArguments.Name = "lblArguments";
            lblArguments.Size = new Size(66, 15);
            lblArguments.TabIndex = 5;
            lblArguments.Text = "Arguments";
            // 
            // txtArguments
            // 
            txtArguments.Location = new Point(110, 82);
            txtArguments.Name = "txtArguments";
            txtArguments.Size = new Size(571, 23);
            txtArguments.TabIndex = 6;
            // 
            // chkEnabled
            // 
            chkEnabled.AutoSize = true;
            chkEnabled.Location = new Point(447, 120);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(68, 19);
            chkEnabled.TabIndex = 7;
            chkEnabled.Text = "Enabled";
            chkEnabled.UseVisualStyleBackColor = true;
            // 
            // chkRestart
            // 
            chkRestart.AutoSize = true;
            chkRestart.Location = new Point(538, 118);
            chkRestart.Name = "chkRestart";
            chkRestart.Size = new Size(143, 19);
            chkRestart.TabIndex = 8;
            chkRestart.Text = "Restart if Not Running";
            chkRestart.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(257, 158);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 9;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(367, 158);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.TabIndex = 10;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 120);
            label1.Name = "label1";
            label1.Size = new Size(98, 15);
            label1.TabIndex = 11;
            label1.Text = "Remote IP/Name";
            // 
            // txtRemoteIP
            // 
            txtRemoteIP.Location = new Point(110, 116);
            txtRemoteIP.Name = "txtRemoteIP";
            txtRemoteIP.Size = new Size(331, 23);
            txtRemoteIP.TabIndex = 12;
            // 
            // ProcessConfigForm
            // 
            ClientSize = new Size(715, 200);
            Controls.Add(txtRemoteIP);
            Controls.Add(label1);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkRestart);
            Controls.Add(chkEnabled);
            Controls.Add(txtArguments);
            Controls.Add(lblArguments);
            Controls.Add(btnBrowse);
            Controls.Add(txtExePath);
            Controls.Add(lblExePath);
            Controls.Add(txtFriendlyName);
            Controls.Add(lblFriendlyName);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProcessConfigForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Process Configuration";
            Load += ProcessConfigForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }
        private Label label1;
        private TextBox txtRemoteIP;
    }
}