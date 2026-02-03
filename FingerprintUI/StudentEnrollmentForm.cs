using FingerprintUI.Models;
using FingerprintUI.Services;

namespace FingerprintUI
{
    public partial class StudentEnrollmentForm : Form
    {
        private readonly AttendanceDatabase _database;
        private readonly FingerprintApiClient _apiClient;
        private List<string> _capturedTemplates = new();
        private bool _isCapturing = false;

        public StudentEnrollmentForm(AttendanceDatabase database, FingerprintApiClient apiClient)
        {
            InitializeComponent();
            _database = database;
            _apiClient = apiClient;
        }

        private async void BtnCapture_Click(object? sender, EventArgs e)
        {
            if (_isCapturing) return;
            if (_capturedTemplates.Count >= 5)
            {
                MessageBox.Show("All 5 samples have been captured.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _isCapturing = true;
            btnCapture.Enabled = false;

            try
            {
                lblStatus.Text = $"⏳ Capturing sample {_capturedTemplates.Count + 1}/5... Place your finger on the scanner.";
                lblStatus.ForeColor = Color.Blue;

                var response = await _apiClient.CaptureAsync();

                if (!response.Success || string.IsNullOrEmpty(response.Template))
                {
                    lblStatus.Text = $"✗ Capture failed: {response.Message}";
                    lblStatus.ForeColor = Color.Red;
                    System.Media.SystemSounds.Hand.Play();
                    return;
                }

                // Display fingerprint image
                if (!string.IsNullOrEmpty(response.ImageData))
                {
                    var imageBytes = Convert.FromBase64String(response.ImageData);
                    using var ms = new MemoryStream(imageBytes);
                    picFingerprint.Image = Image.FromStream(ms);
                }

                // Store the template
                _capturedTemplates.Add(response.Template);

                // Update progress
                progressBar.Value = _capturedTemplates.Count;
                lblProgress.Text = $"Samples: {_capturedTemplates.Count} / 5";

                if (_capturedTemplates.Count >= 5)
                {
                    lblStatus.Text = "✓ All 5 samples captured successfully! Click 'Save Enrollment' to complete.";
                    lblStatus.ForeColor = Color.Green;
                    btnSaveEnrollment.Enabled = true;
                    btnCapture.Enabled = false;
                    System.Media.SystemSounds.Asterisk.Play();
                }
                else
                {
                    lblStatus.Text = $"✓ Sample {_capturedTemplates.Count}/5 captured. Lift finger, then place at a different angle.";
                    lblStatus.ForeColor = Color.Green;
                    System.Media.SystemSounds.Beep.Play();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"✗ Error: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isCapturing = false;
                if (_capturedTemplates.Count < 5)
                {
                    btnCapture.Enabled = true;
                }
            }
        }

        private void BtnSaveEnrollment_Click(object? sender, EventArgs e)
        {
            // Validate student information
            if (string.IsNullOrWhiteSpace(txtStudentId.Text))
            {
                MessageBox.Show("Please enter a Student ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentId.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the student's name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtProgram.Text))
            {
                MessageBox.Show("Please enter the program (e.g., BSCS, BSIT).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProgram.Focus();
                return;
            }

            if (_capturedTemplates.Count < 5)
            {
                MessageBox.Show("Please capture all 5 fingerprint samples before saving.", "Incomplete Enrollment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Check if student ID already exists
                var existingStudent = _database.GetStudentByStudentId(txtStudentId.Text.Trim());
                if (existingStudent != null)
                {
                    MessageBox.Show($"Student ID '{txtStudentId.Text.Trim()}' is already enrolled.", "Duplicate Student ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStudentId.Focus();
                    return;
                }

                // Create student record
                var student = new Student
                {
                    StudentId = txtStudentId.Text.Trim(),
                    Name = txtName.Text.Trim(),
                    Program = txtProgram.Text.Trim(),
                    YearLevel = int.Parse(cmbYearLevel.SelectedItem?.ToString() ?? "1"),
                    IsActive = true,
                    EnrolledDate = DateTime.Now
                };

                var studentId = _database.AddStudent(student);

                // Save all 5 fingerprint templates
                for (int i = 0; i < _capturedTemplates.Count; i++)
                {
                    var template = new FingerprintTemplate
                    {
                        StudentId = studentId,
                        SampleNumber = i + 1, // 1-5
                        TemplateData = _capturedTemplates[i],
                        CapturedDate = DateTime.Now,
                        Quality = 80 // Could be extracted from capture response
                    };

                    _database.AddFingerprintTemplate(template);
                }

                MessageBox.Show(
                    $"Student enrolled successfully!\n\n" +
                    $"Name: {student.Name}\n" +
                    $"ID: {student.StudentId}\n" +
                    $"Program: {student.Program} Year {student.YearLevel}\n" +
                    $"Fingerprint Samples: {_capturedTemplates.Count}",
                    "Enrollment Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save enrollment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (_capturedTemplates.Count > 0)
            {
                var result = MessageBox.Show(
                    "You have captured fingerprint samples.\n\nAre you sure you want to cancel enrollment?",
                    "Confirm Cancel",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
