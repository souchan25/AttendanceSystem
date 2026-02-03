namespace FingerprintUI
{
    partial class StudentEnrollmentForm : Form
    {
        private System.ComponentModel.IContainer components = null;

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
            this.lblTitle = new Label();
            this.grpStudentInfo = new GroupBox();
            this.txtStudentId = new TextBox();
            this.lblStudentId = new Label();
            this.txtName = new TextBox();
            this.lblName = new Label();
            this.txtProgram = new TextBox();
            this.lblProgram = new Label();
            this.cmbYearLevel = new ComboBox();
            this.lblYearLevel = new Label();
            this.grpFingerprint = new GroupBox();
            this.lblInstructions = new Label();
            this.picFingerprint = new PictureBox();
            this.progressBar = new ProgressBar();
            this.lblProgress = new Label();
            this.btnCapture = new Button();
            this.btnSaveEnrollment = new Button();
            this.btnCancel = new Button();
            this.lblStatus = new Label();
            this.grpStudentInfo.SuspendLayout();
            this.grpFingerprint.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFingerprint)).BeginInit();
            this.SuspendLayout();
            
            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.Location = new Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(260, 30);
            this.lblTitle.Text = "Student Enrollment";
            
            // grpStudentInfo
            this.grpStudentInfo.Controls.Add(this.txtStudentId);
            this.grpStudentInfo.Controls.Add(this.lblStudentId);
            this.grpStudentInfo.Controls.Add(this.txtName);
            this.grpStudentInfo.Controls.Add(this.lblName);
            this.grpStudentInfo.Controls.Add(this.txtProgram);
            this.grpStudentInfo.Controls.Add(this.lblProgram);
            this.grpStudentInfo.Controls.Add(this.cmbYearLevel);
            this.grpStudentInfo.Controls.Add(this.lblYearLevel);
            this.grpStudentInfo.Location = new Point(20, 60);
            this.grpStudentInfo.Name = "grpStudentInfo";
            this.grpStudentInfo.Size = new Size(400, 200);
            this.grpStudentInfo.Text = "Student Information";
            
            // txtStudentId
            this.txtStudentId.Location = new Point(120, 30);
            this.txtStudentId.Name = "txtStudentId";
            this.txtStudentId.Size = new Size(260, 27);
            
            // lblStudentId
            this.lblStudentId.AutoSize = true;
            this.lblStudentId.Location = new Point(20, 33);
            this.lblStudentId.Text = "Student ID:";
            
            // txtName
            this.txtName.Location = new Point(120, 70);
            this.txtName.Name = "txtName";
            this.txtName.Size = new Size(260, 27);
            
            // lblName
            this.lblName.AutoSize = true;
            this.lblName.Location = new Point(20, 73);
            this.lblName.Text = "Full Name:";
            
            // txtProgram
            this.txtProgram.Location = new Point(120, 110);
            this.txtProgram.Name = "txtProgram";
            this.txtProgram.Size = new Size(260, 27);
            this.txtProgram.PlaceholderText = "e.g., BSCS, BSIT, BSIS";
            
            // lblProgram
            this.lblProgram.AutoSize = true;
            this.lblProgram.Location = new Point(20, 113);
            this.lblProgram.Text = "Program:";
            
            // cmbYearLevel
            this.cmbYearLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbYearLevel.Items.AddRange(new object[] { "1", "2", "3", "4" });
            this.cmbYearLevel.Location = new Point(120, 150);
            this.cmbYearLevel.Name = "cmbYearLevel";
            this.cmbYearLevel.Size = new Size(100, 28);
            this.cmbYearLevel.SelectedIndex = 0;
            
            // lblYearLevel
            this.lblYearLevel.AutoSize = true;
            this.lblYearLevel.Location = new Point(20, 153);
            this.lblYearLevel.Text = "Year Level:";
            
            // grpFingerprint
            this.grpFingerprint.Controls.Add(this.lblInstructions);
            this.grpFingerprint.Controls.Add(this.picFingerprint);
            this.grpFingerprint.Controls.Add(this.progressBar);
            this.grpFingerprint.Controls.Add(this.lblProgress);
            this.grpFingerprint.Controls.Add(this.btnCapture);
            this.grpFingerprint.Location = new Point(440, 60);
            this.grpFingerprint.Name = "grpFingerprint";
            this.grpFingerprint.Size = new Size(400, 400);
            this.grpFingerprint.Text = "Fingerprint Capture (5 Samples Required)";
            
            // lblInstructions
            this.lblInstructions.Location = new Point(20, 30);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new Size(360, 60);
            this.lblInstructions.Text = "For reliable identification, we need 5 fingerprint samples.\n\n" +
                                      "Place the SAME finger at different angles for each capture:";
            
            // picFingerprint
            this.picFingerprint.BorderStyle = BorderStyle.FixedSingle;
            this.picFingerprint.Location = new Point(100, 100);
            this.picFingerprint.Name = "picFingerprint";
            this.picFingerprint.Size = new Size(200, 200);
            this.picFingerprint.SizeMode = PictureBoxSizeMode.Zoom;
            
            // progressBar
            this.progressBar.Location = new Point(20, 310);
            this.progressBar.Maximum = 5;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(360, 25);
            
            // lblProgress
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new Point(20, 340);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new Size(120, 20);
            this.lblProgress.Text = "Samples: 0 / 5";
            
            // btnCapture
            this.btnCapture.BackColor = Color.DodgerBlue;
            this.btnCapture.FlatStyle = FlatStyle.Flat;
            this.btnCapture.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnCapture.ForeColor = Color.White;
            this.btnCapture.Location = new Point(240, 335);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new Size(140, 50);
            this.btnCapture.Text = "Capture Sample";
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new EventHandler(this.BtnCapture_Click!);
            
            // btnSaveEnrollment
            this.btnSaveEnrollment.BackColor = Color.Green;
            this.btnSaveEnrollment.Enabled = false;
            this.btnSaveEnrollment.FlatStyle = FlatStyle.Flat;
            this.btnSaveEnrollment.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnSaveEnrollment.ForeColor = Color.White;
            this.btnSaveEnrollment.Location = new Point(440, 480);
            this.btnSaveEnrollment.Name = "btnSaveEnrollment";
            this.btnSaveEnrollment.Size = new Size(200, 50);
            this.btnSaveEnrollment.Text = "Save Enrollment";
            this.btnSaveEnrollment.UseVisualStyleBackColor = false;
            this.btnSaveEnrollment.Click += new EventHandler(this.BtnSaveEnrollment_Click!);
            
            // btnCancel
            this.btnCancel.BackColor = Color.Gray;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Location = new Point(660, 480);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(180, 50);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new EventHandler(this.BtnCancel_Click!);
            
            // lblStatus
            this.lblStatus.Location = new Point(20, 280);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(400, 60);
            this.lblStatus.Text = "Enter student information and capture fingerprint samples.";
            
            // StudentEnrollmentForm
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(870, 560);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSaveEnrollment);
            this.Controls.Add(this.grpFingerprint);
            this.Controls.Add(this.grpStudentInfo);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StudentEnrollmentForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Enroll New Student";
            this.grpStudentInfo.ResumeLayout(false);
            this.grpStudentInfo.PerformLayout();
            this.grpFingerprint.ResumeLayout(false);
            this.grpFingerprint.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFingerprint)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label lblTitle;
        private GroupBox grpStudentInfo;
        private TextBox txtStudentId;
        private Label lblStudentId;
        private TextBox txtName;
        private Label lblName;
        private TextBox txtProgram;
        private Label lblProgram;
        private ComboBox cmbYearLevel;
        private Label lblYearLevel;
        private GroupBox grpFingerprint;
        private Label lblInstructions;
        private PictureBox picFingerprint;
        private ProgressBar progressBar;
        private Label lblProgress;
        private Button btnCapture;
        private Button btnSaveEnrollment;
        private Button btnCancel;
        private Label lblStatus;
    }
}
