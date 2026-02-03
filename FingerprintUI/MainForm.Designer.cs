namespace FingerprintUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TabControl tabControl;
        private TabPage tabEnroll;
        private TabPage tabVerify;
        private TabPage tabIdentify;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        // Enroll tab controls
        private Label lblEnrollUserId;
        private TextBox txtEnrollUserId;
        private Label lblEnrollName;
        private TextBox txtEnrollName;
        private Button btnEnroll;
        private PictureBox picEnrollFingerprint;
        private Label lblEnrollStatus;

        // Verify tab controls
        private Label lblVerifyUserId;
        private ComboBox cmbVerifyUserId;
        private Button btnVerify;
        private PictureBox picVerifyFingerprint;
        private Label lblVerifyStatus;

        // Identify tab controls
        private Button btnIdentify;
        private PictureBox picIdentifyFingerprint;
        private Label lblIdentifyStatus;
        private ListBox lstEnrolledUsers;
        private Label lblEnrolledCount;

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
            
            // Main form
            this.Text = "Fingerprint Biometric System";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // TabControl
            this.tabControl = new TabControl();
            this.tabControl.Dock = DockStyle.Fill;
            
            // Status strip
            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel("Checking connection...");
            this.statusStrip.Items.Add(this.statusLabel);

            // Tab pages
            this.tabEnroll = new TabPage("Enroll");
            this.tabVerify = new TabPage("Verify");
            this.tabIdentify = new TabPage("Identify");

            this.tabControl.TabPages.Add(this.tabEnroll);
            this.tabControl.TabPages.Add(this.tabVerify);
            this.tabControl.TabPages.Add(this.tabIdentify);

            InitializeEnrollTab();
            InitializeVerifyTab();
            InitializeIdentifyTab();

            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip);
        }

        private void InitializeEnrollTab()
        {
            // User ID
            this.lblEnrollUserId = new Label
            {
                Text = "User ID:",
                Location = new Point(20, 20),
                Size = new Size(100, 23)
            };

            this.txtEnrollUserId = new TextBox
            {
                Location = new Point(130, 20),
                Size = new Size(200, 23)
            };

            // Name
            this.lblEnrollName = new Label
            {
                Text = "Full Name:",
                Location = new Point(20, 60),
                Size = new Size(100, 23)
            };

            this.txtEnrollName = new TextBox
            {
                Location = new Point(130, 60),
                Size = new Size(200, 23)
            };

            // Enroll button
            this.btnEnroll = new Button
            {
                Text = "Capture && Enroll Fingerprint",
                Location = new Point(130, 100),
                Size = new Size(200, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.btnEnroll.Click += BtnEnroll_Click;

            // Fingerprint picture
            this.picEnrollFingerprint = new PictureBox
            {
                Location = new Point(400, 20),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            // Status label
            this.lblEnrollStatus = new Label
            {
                Location = new Point(20, 160),
                Size = new Size(350, 250),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Blue
            };

            this.tabEnroll.Controls.Add(this.lblEnrollUserId);
            this.tabEnroll.Controls.Add(this.txtEnrollUserId);
            this.tabEnroll.Controls.Add(this.lblEnrollName);
            this.tabEnroll.Controls.Add(this.txtEnrollName);
            this.tabEnroll.Controls.Add(this.btnEnroll);
            this.tabEnroll.Controls.Add(this.picEnrollFingerprint);
            this.tabEnroll.Controls.Add(this.lblEnrollStatus);
        }

        private void InitializeVerifyTab()
        {
            // User ID combo
            this.lblVerifyUserId = new Label
            {
                Text = "Select User:",
                Location = new Point(20, 20),
                Size = new Size(100, 23)
            };

            this.cmbVerifyUserId = new ComboBox
            {
                Location = new Point(130, 20),
                Size = new Size(250, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Verify button
            this.btnVerify = new Button
            {
                Text = "Scan && Verify Fingerprint",
                Location = new Point(130, 60),
                Size = new Size(250, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.btnVerify.Click += BtnVerify_Click;

            // Fingerprint picture
            this.picVerifyFingerprint = new PictureBox
            {
                Location = new Point(400, 20),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            // Status label
            this.lblVerifyStatus = new Label
            {
                Location = new Point(20, 120),
                Size = new Size(350, 280),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Blue
            };

            this.tabVerify.Controls.Add(this.lblVerifyUserId);
            this.tabVerify.Controls.Add(this.cmbVerifyUserId);
            this.tabVerify.Controls.Add(this.btnVerify);
            this.tabVerify.Controls.Add(this.picVerifyFingerprint);
            this.tabVerify.Controls.Add(this.lblVerifyStatus);
        }

        private void InitializeIdentifyTab()
        {
            // Identify button
            this.btnIdentify = new Button
            {
                Text = "Scan && Identify Fingerprint",
                Location = new Point(20, 20),
                Size = new Size(250, 50),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            this.btnIdentify.Click += BtnIdentify_Click;

            // Enrolled users list
            this.lblEnrolledCount = new Label
            {
                Text = "Enrolled Users: 0",
                Location = new Point(20, 90),
                Size = new Size(250, 23),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            this.lstEnrolledUsers = new ListBox
            {
                Location = new Point(20, 120),
                Size = new Size(330, 400),
                Font = new Font("Consolas", 9)
            };

            // Fingerprint picture
            this.picIdentifyFingerprint = new PictureBox
            {
                Location = new Point(400, 20),
                Size = new Size(400, 400),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            // Status label
            this.lblIdentifyStatus = new Label
            {
                Location = new Point(400, 440),
                Size = new Size(400, 80),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Blue,
                TextAlign = ContentAlignment.TopCenter
            };

            this.tabIdentify.Controls.Add(this.btnIdentify);
            this.tabIdentify.Controls.Add(this.lblEnrolledCount);
            this.tabIdentify.Controls.Add(this.lstEnrolledUsers);
            this.tabIdentify.Controls.Add(this.picIdentifyFingerprint);
            this.tabIdentify.Controls.Add(this.lblIdentifyStatus);
        }
    }
}
