namespace FingerprintUI
{
    partial class AttendancePortal
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlHeader;
        private Panel pnlMain;
        private Label lblTitle;
        private Label lblCurrentEvent;
        private Button btnAdminDashboard;
        private Button btnCheckAttendance;
        private Button btnClose;
        private Label lblInstruction;
        private PictureBox picFingerprint;
        private Label lblStudentInfo;
        private Label lblTimeInOut;
        private Label lblStatus;
        private Button btnTimeIn;
        private Button btnTimeOut;
        private Panel statusStrip;
        private Label statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new Panel();
            this.pnlMain = new Panel();
            this.statusStrip = new Panel();
            this.lblTitle = new Label();
            this.lblCurrentEvent = new Label();
            this.btnAdminDashboard = new Button();
            this.btnCheckAttendance = new Button();
            this.btnClose = new Button();
            this.lblInstruction = new Label();
            this.picFingerprint = new PictureBox();
            this.lblStudentInfo = new Label();
            this.lblTimeInOut = new Label();
            this.lblStatus = new Label();
            this.btnTimeIn = new Button();
            this.btnTimeOut = new Button();
            this.statusLabel = new Label();
            
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFingerprint)).BeginInit();
            this.SuspendLayout();
            
            // 
            // AttendancePortal
            // 
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ClientSize = new Size(1920, 1080);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "AttendancePortal";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Attendance Portal";
            this.WindowState = FormWindowState.Maximized;
            
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = Color.FromArgb(30, 41, 59);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblCurrentEvent);
            this.pnlHeader.Controls.Add(this.btnAdminDashboard);
            this.pnlHeader.Controls.Add(this.btnCheckAttendance);
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Location = new Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new Padding(30, 10, 30, 10);
            this.pnlHeader.Size = new Size(1920, 80);
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(30, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "ATTENDANCE PORTAL";
            
            // 
            // lblCurrentEvent
            // 
            this.lblCurrentEvent.AutoSize = true;
            this.lblCurrentEvent.Font = new Font("Segoe UI", 10F);
            this.lblCurrentEvent.ForeColor = Color.FromArgb(148, 163, 184);
            this.lblCurrentEvent.Location = new Point(30, 48);
            this.lblCurrentEvent.Name = "lblCurrentEvent";
            this.lblCurrentEvent.Text = "Event: Loading...";
            
            // 
            // btnAdminDashboard
            // 
            this.btnAdminDashboard.BackColor = Color.FromArgb(79, 70, 229);
            this.btnAdminDashboard.Cursor = Cursors.Hand;
            this.btnAdminDashboard.FlatAppearance.BorderSize = 0;
            this.btnAdminDashboard.FlatStyle = FlatStyle.Flat;
            this.btnAdminDashboard.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnAdminDashboard.ForeColor = Color.White;
            this.btnAdminDashboard.Location = new Point(1620, 18);
            this.btnAdminDashboard.Name = "btnAdminDashboard";
            this.btnAdminDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnAdminDashboard.Size = new Size(120, 44);
            this.btnAdminDashboard.Text = "⚙ ADMIN";
            this.btnAdminDashboard.Click += new EventHandler(this.BtnAdminDashboard_Click!);
            
            // 
            // btnCheckAttendance
            // 
            this.btnCheckAttendance.BackColor = Color.FromArgb(16, 185, 129);
            this.btnCheckAttendance.Cursor = Cursors.Hand;
            this.btnCheckAttendance.FlatAppearance.BorderSize = 0;
            this.btnCheckAttendance.FlatStyle = FlatStyle.Flat;
            this.btnCheckAttendance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnCheckAttendance.ForeColor = Color.White;
            this.btnCheckAttendance.Location = new Point(1750, 18);
            this.btnCheckAttendance.Name = "btnCheckAttendance";
            this.btnCheckAttendance.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnCheckAttendance.Size = new Size(120, 44);
            this.btnCheckAttendance.Text = "📊 RECORDS";
            this.btnCheckAttendance.Click += new EventHandler(this.BtnCheckAttendance_Click!);
            
            // 
            // btnClose
            // 
            this.btnClose.BackColor = Color.FromArgb(220, 38, 38);
            this.btnClose.Cursor = Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.btnClose.ForeColor = Color.White;
            this.btnClose.Location = new Point(1878, 18);
            this.btnClose.Name = "btnClose";
            this.btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.btnClose.Size = new Size(44, 44);
            this.btnClose.Text = "✕";
            this.btnClose.Click += new EventHandler((s, e) => Application.Exit());
            
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = Color.FromArgb(15, 23, 42);
            this.pnlMain.Controls.Add(this.lblInstruction);
            this.pnlMain.Controls.Add(this.lblStatus);
            this.pnlMain.Controls.Add(this.picFingerprint);
            this.pnlMain.Controls.Add(this.lblStudentInfo);
            this.pnlMain.Controls.Add(this.lblTimeInOut);
            this.pnlMain.Controls.Add(this.btnTimeIn);
            this.pnlMain.Controls.Add(this.btnTimeOut);
            this.pnlMain.Dock = DockStyle.Fill;
            this.pnlMain.Location = new Point(0, 80);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new Padding(50, 40, 50, 40);
            this.pnlMain.Size = new Size(1920, 965);
            
            // 
            // lblInstruction
            // 
            this.lblInstruction.Dock = DockStyle.Top;
            this.lblInstruction.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblInstruction.ForeColor = Color.FromArgb(148, 163, 184);
            this.lblInstruction.Location = new Point(50, 40);
            this.lblInstruction.Name = "lblInstruction";
            this.lblInstruction.Size = new Size(1820, 60);
            this.lblInstruction.Text = "👆 PLACE YOUR FINGER ON THE SCANNER";
            this.lblInstruction.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblStatus
            // 
            this.lblStatus.Font = new Font("Segoe UI", 12F);
            this.lblStatus.ForeColor = Color.FromArgb(148, 163, 184);
            this.lblStatus.Location = new Point(50, 110);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(1820, 30);
            this.lblStatus.Text = "Ready to scan fingerprint...";
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // picFingerprint
            // 
            this.picFingerprint.BackColor = Color.FromArgb(51, 65, 85);
            this.picFingerprint.BorderStyle = BorderStyle.None;
            this.picFingerprint.Location = new Point(735, 160);
            this.picFingerprint.Name = "picFingerprint";
            this.picFingerprint.Size = new Size(450, 450);
            this.picFingerprint.SizeMode = PictureBoxSizeMode.Zoom;
            
            // 
            // lblStudentInfo
            // 
            this.lblStudentInfo.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            this.lblStudentInfo.ForeColor = Color.White;
            this.lblStudentInfo.Location = new Point(50, 630);
            this.lblStudentInfo.Name = "lblStudentInfo";
            this.lblStudentInfo.Size = new Size(1820, 40);
            this.lblStudentInfo.Text = "";
            this.lblStudentInfo.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // lblTimeInOut
            // 
            this.lblTimeInOut.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTimeInOut.ForeColor = Color.FromArgb(148, 163, 184);
            this.lblTimeInOut.Location = new Point(50, 680);
            this.lblTimeInOut.Name = "lblTimeInOut";
            this.lblTimeInOut.Size = new Size(1820, 35);
            this.lblTimeInOut.Text = "";
            this.lblTimeInOut.TextAlign = ContentAlignment.MiddleCenter;
            
            // 
            // btnTimeIn
            // 
            this.btnTimeIn.BackColor = Color.FromArgb(34, 197, 94);
            this.btnTimeIn.Cursor = Cursors.Hand;
            this.btnTimeIn.FlatAppearance.BorderSize = 0;
            this.btnTimeIn.FlatStyle = FlatStyle.Flat;
            this.btnTimeIn.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.btnTimeIn.ForeColor = Color.White;
            this.btnTimeIn.Location = new Point(630, 750);
            this.btnTimeIn.Name = "btnTimeIn";
            this.btnTimeIn.Size = new Size(300, 90);
            this.btnTimeIn.Text = "⬇ TIME IN";
            this.btnTimeIn.Click += new EventHandler(this.BtnTimeIn_Click!);
            
            // 
            // btnTimeOut
            // 
            this.btnTimeOut.BackColor = Color.FromArgb(239, 68, 68);
            this.btnTimeOut.Cursor = Cursors.Hand;
            this.btnTimeOut.FlatAppearance.BorderSize = 0;
            this.btnTimeOut.FlatStyle = FlatStyle.Flat;
            this.btnTimeOut.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.btnTimeOut.ForeColor = Color.White;
            this.btnTimeOut.Location = new Point(990, 750);
            this.btnTimeOut.Name = "btnTimeOut";
            this.btnTimeOut.Size = new Size(300, 90);
            this.btnTimeOut.Text = "⬆ TIME OUT";
            this.btnTimeOut.Click += new EventHandler(this.BtnTimeOut_Click!);
            
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = Color.FromArgb(30, 41, 59);
            this.statusStrip.Controls.Add(this.statusLabel);
            this.statusStrip.Dock = DockStyle.Bottom;
            this.statusStrip.Location = new Point(0, 1045);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new Padding(20, 0, 20, 0);
            this.statusStrip.Size = new Size(1920, 35);
            
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = false;
            this.statusLabel.Font = new Font("Segoe UI", 9F);
            this.statusLabel.ForeColor = Color.FromArgb(34, 197, 94);
            this.statusLabel.Location = new Point(20, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(400, 35);
            this.statusLabel.Text = "✓ Scanner connected and ready";
            this.statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            
            // 
            // Add controls to form
            // 
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picFingerprint)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
