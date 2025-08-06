namespace ProcessOverwatch.Controller
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabEnabled;
        private System.Windows.Forms.TabPage tabDisabled;
        private System.Windows.Forms.DataGridView dgvEnabled;
        private System.Windows.Forms.DataGridView dgvDisabled;
        private System.Windows.Forms.Button btnAddProcess;
        private System.Windows.Forms.Button btnEditProcess;
        private System.Windows.Forms.Button btnDeleteProcess;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.Label lblNextCheck;
        private System.Windows.Forms.TextBox txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tabControl = new TabControl();
            tabEnabled = new TabPage();
            dgvEnabled = new DataGridView();
            tabDisabled = new TabPage();
            dgvDisabled = new DataGridView();
            btnAddProcess = new Button();
            btnEditProcess = new Button();
            btnDeleteProcess = new Button();
            btnConfig = new Button();
            lblNextCheck = new Label();
            txtLog = new TextBox();
            btnStartMonitoring = new Button();
            label1 = new Label();
            notifySystemTrayIcon = new NotifyIcon(components);
            tabControl.SuspendLayout();
            tabEnabled.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvEnabled).BeginInit();
            tabDisabled.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDisabled).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabEnabled);
            tabControl.Controls.Add(tabDisabled);
            tabControl.Location = new Point(12, 40);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1158, 332);
            tabControl.TabIndex = 0;
            // 
            // tabEnabled
            // 
            tabEnabled.Controls.Add(dgvEnabled);
            tabEnabled.Location = new Point(4, 24);
            tabEnabled.Name = "tabEnabled";
            tabEnabled.Padding = new Padding(3);
            tabEnabled.Size = new Size(1150, 304);
            tabEnabled.TabIndex = 0;
            tabEnabled.Text = "Enabled";
            tabEnabled.UseVisualStyleBackColor = true;
            // 
            // dgvEnabled
            // 
            dgvEnabled.AllowUserToAddRows = false;
            dgvEnabled.AllowUserToDeleteRows = false;
            dgvEnabled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEnabled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEnabled.Dock = DockStyle.Fill;
            dgvEnabled.Location = new Point(3, 3);
            dgvEnabled.MultiSelect = false;
            dgvEnabled.Name = "dgvEnabled";
            dgvEnabled.ReadOnly = true;
            dgvEnabled.RowHeadersVisible = false;
            dgvEnabled.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvEnabled.Size = new Size(1144, 298);
            dgvEnabled.TabIndex = 0;
            dgvEnabled.CellDoubleClick += dgvEnabled_CellDoubleClick;
            // 
            // tabDisabled
            // 
            tabDisabled.Controls.Add(dgvDisabled);
            tabDisabled.Location = new Point(4, 24);
            tabDisabled.Name = "tabDisabled";
            tabDisabled.Padding = new Padding(3);
            tabDisabled.Size = new Size(823, 304);
            tabDisabled.TabIndex = 1;
            tabDisabled.Text = "Disabled";
            tabDisabled.UseVisualStyleBackColor = true;
            // 
            // dgvDisabled
            // 
            dgvDisabled.AllowUserToAddRows = false;
            dgvDisabled.AllowUserToDeleteRows = false;
            dgvDisabled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDisabled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDisabled.Dock = DockStyle.Fill;
            dgvDisabled.Location = new Point(3, 3);
            dgvDisabled.MultiSelect = false;
            dgvDisabled.Name = "dgvDisabled";
            dgvDisabled.ReadOnly = true;
            dgvDisabled.RowHeadersVisible = false;
            dgvDisabled.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDisabled.Size = new Size(817, 298);
            dgvDisabled.TabIndex = 0;
            dgvDisabled.CellDoubleClick += dgvDisabled_CellDoubleClick;
            // 
            // btnAddProcess
            // 
            btnAddProcess.Location = new Point(1176, 64);
            btnAddProcess.Name = "btnAddProcess";
            btnAddProcess.Size = new Size(100, 30);
            btnAddProcess.TabIndex = 1;
            btnAddProcess.Text = "Add";
            btnAddProcess.UseVisualStyleBackColor = true;
            btnAddProcess.Click += btnAddProcess_Click;
            // 
            // btnEditProcess
            // 
            btnEditProcess.Location = new Point(1176, 104);
            btnEditProcess.Name = "btnEditProcess";
            btnEditProcess.Size = new Size(100, 30);
            btnEditProcess.TabIndex = 2;
            btnEditProcess.Text = "Edit";
            btnEditProcess.UseVisualStyleBackColor = true;
            btnEditProcess.Click += btnEditProcess_Click;
            // 
            // btnDeleteProcess
            // 
            btnDeleteProcess.Location = new Point(1176, 144);
            btnDeleteProcess.Name = "btnDeleteProcess";
            btnDeleteProcess.Size = new Size(100, 30);
            btnDeleteProcess.TabIndex = 3;
            btnDeleteProcess.Text = "Delete";
            btnDeleteProcess.UseVisualStyleBackColor = true;
            btnDeleteProcess.Click += btnDeleteProcess_Click;
            // 
            // btnConfig
            // 
            btnConfig.Location = new Point(1176, 234);
            btnConfig.Name = "btnConfig";
            btnConfig.Size = new Size(100, 30);
            btnConfig.TabIndex = 4;
            btnConfig.Text = "Config";
            btnConfig.UseVisualStyleBackColor = true;
            btnConfig.Click += btnConfig_Click;
            // 
            // lblNextCheck
            // 
            lblNextCheck.AutoSize = true;
            lblNextCheck.Location = new Point(12, 15);
            lblNextCheck.Name = "lblNextCheck";
            lblNextCheck.Size = new Size(84, 15);
            lblNextCheck.TabIndex = 5;
            lblNextCheck.Text = "Next check at: ";
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 409);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(1264, 253);
            txtLog.TabIndex = 6;
            // 
            // btnStartMonitoring
            // 
            btnStartMonitoring.Location = new Point(1176, 335);
            btnStartMonitoring.Name = "btnStartMonitoring";
            btnStartMonitoring.Size = new Size(100, 30);
            btnStartMonitoring.TabIndex = 7;
            btnStartMonitoring.Text = "Start";
            btnStartMonitoring.UseVisualStyleBackColor = true;
            btnStartMonitoring.Click += btnStartMonitoring_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 392);
            label1.Name = "label1";
            label1.Size = new Size(51, 15);
            label1.TabIndex = 8;
            label1.Text = "Logging";
            // 
            // notifySystemTrayIcon
            // 
            notifySystemTrayIcon.BalloonTipText = "Process Overwatch";
            notifySystemTrayIcon.Icon = (Icon)resources.GetObject("notifySystemTrayIcon.Icon");
            notifySystemTrayIcon.Text = "Process Overwatch";
            notifySystemTrayIcon.Visible = true;
            notifySystemTrayIcon.MouseDoubleClick += notifySystemTrayIcon_MouseDoubleClick;
            // 
            // MainForm
            // 
            ClientSize = new Size(1304, 674);
            Controls.Add(label1);
            Controls.Add(btnStartMonitoring);
            Controls.Add(txtLog);
            Controls.Add(lblNextCheck);
            Controls.Add(btnConfig);
            Controls.Add(btnDeleteProcess);
            Controls.Add(btnEditProcess);
            Controls.Add(btnAddProcess);
            Controls.Add(tabControl);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Process Monitor";
            Load += MainForm_Load;
            Resize += MainForm_Resize;
            tabControl.ResumeLayout(false);
            tabEnabled.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvEnabled).EndInit();
            tabDisabled.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDisabled).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private Button btnStartMonitoring;
        private Label label1;
        private NotifyIcon notifySystemTrayIcon;
    }

}
