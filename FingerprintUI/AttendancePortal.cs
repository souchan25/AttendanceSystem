using FingerprintUI.Models;
using FingerprintUI.Services;

namespace FingerprintUI
{
    public partial class AttendancePortal : Form
    {
        private readonly FingerprintApiClient _apiClient;
        private readonly AttendanceDatabase _database;
        private Event? _currentEvent;
        private Student? _lastScannedStudent;
        private bool _isScanning = false;

        public AttendancePortal()
        {
            InitializeComponent();
            _apiClient = new FingerprintApiClient();
            _database = new AttendanceDatabase();
            
            this.Load += AttendancePortal_Load;
        }

        private async void AttendancePortal_Load(object? sender, EventArgs e)
        {
            await CheckConnectionAsync();
            await LoadCurrentEventAsync();
            
            // Show enrollment dialog if no students registered
            var students = _database.GetAllStudents();
            if (students.Count == 0)
            {
                var result = MessageBox.Show(
                    "No students are enrolled yet.\n\nWould you like to enroll the first student now?",
                    "No Students Enrolled",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                
                if (result == DialogResult.Yes)
                {
                    ShowEnrollmentDialog();
                }
            }
        }

        private async Task CheckConnectionAsync()
        {
            var connected = await _apiClient.CheckConnectionAsync();
            if (connected)
            {
                statusLabel.Text = "✓ Scanner connected and ready";
                statusLabel.ForeColor = Color.Green;
            }
            else
            {
                statusLabel.Text = "✗ Scanner not connected - Please start fingerprint middleware";
                statusLabel.ForeColor = Color.Red;
                MessageBox.Show(
                    "Cannot connect to fingerprint scanner service.\n\n" +
                    "Please ensure the middleware is running:\n" +
                    "cd fingerprintMiddleware && dotnet run",
                    "Scanner Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private async Task LoadCurrentEventAsync()
        {
            // Get or create today's event
            _currentEvent = _database.GetCurrentEvent();
            
            if (_currentEvent == null)
            {
                // Auto-create today's event
                var period = DateTime.Now.Month <= 6 ? "Final" : "Midterm";
                var academicYear = $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}";
                
                _currentEvent = new Event
                {
                    EventName = $"Daily Attendance - {DateTime.Now:MMMM dd, yyyy}",
                    Description = "Auto-generated daily attendance",
                    EventDate = DateTime.Now,
                    Period = period,
                    AcademicYear = academicYear,
                    IsActive = true
                };
                
                _currentEvent.Id = _database.AddEvent(_currentEvent);
            }
            
            lblCurrentEvent.Text = $"Event: {_currentEvent.EventName} ({_currentEvent.Period} - {_currentEvent.AcademicYear})";
        }

        private async void BtnTimeIn_Click(object? sender, EventArgs e)
        {
            await PerformAttendanceCheckAsync(isTimeIn: true);
        }

        private async void BtnTimeOut_Click(object? sender, EventArgs e)
        {
            await PerformAttendanceCheckAsync(isTimeIn: false);
        }

        private async Task PerformAttendanceCheckAsync(bool isTimeIn)
        {
            if (_isScanning) return;
            if (_currentEvent == null)
            {
                MessageBox.Show("No active event found. Please contact administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _isScanning = true;
            btnTimeIn.Enabled = false;
            btnTimeOut.Enabled = false;
            
            try
            {
                lblStatus.Text = "⏳ Please place your finger on the scanner...";
                lblStatus.ForeColor = Color.Blue;
                lblStudentInfo.Text = "";
                lblTimeInOut.Text = "";
                picFingerprint.Image = null;

                // Capture fingerprint
                var captureResponse = await _apiClient.CaptureAsync();

                if (!captureResponse.Success || string.IsNullOrEmpty(captureResponse.Template))
                {
                    lblStatus.Text = $"✗ Scan failed: {captureResponse.Message}";
                    lblStatus.ForeColor = Color.Red;
                    System.Media.SystemSounds.Hand.Play();
                    return;
                }

                // Display captured fingerprint image
                if (!string.IsNullOrEmpty(captureResponse.ImageData))
                {
                    var imageBytes = Convert.FromBase64String(captureResponse.ImageData);
                    using var ms = new MemoryStream(imageBytes);
                    picFingerprint.Image = Image.FromStream(ms);
                }

                lblStatus.Text = "🔍 Identifying student...";

                // Identify student by matching against all stored templates
                var identifiedStudent = await IdentifyStudentAsync(captureResponse.Template);

                if (identifiedStudent == null)
                {
                    lblStatus.Text = "✗ Student not recognized";
                    lblStatus.ForeColor = Color.Red;
                    lblStudentInfo.Text = "Not Enrolled";
                    lblStudentInfo.ForeColor = Color.Red;
                    System.Media.SystemSounds.Hand.Play();
                    
                    MessageBox.Show(
                        "Fingerprint not recognized.\n\nPlease enroll your fingerprint first.",
                        "Not Enrolled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _lastScannedStudent = identifiedStudent;

                // Record attendance
                if (isTimeIn)
                {
                    _database.RecordTimeIn(identifiedStudent.Id, _currentEvent.Id);
                    lblTimeInOut.Text = $"⏱️ TIME IN: {DateTime.Now:hh:mm:ss tt}";
                    lblTimeInOut.ForeColor = Color.Green;
                }
                else
                {
                    _database.RecordTimeOut(identifiedStudent.Id, _currentEvent.Id);
                    lblTimeInOut.Text = $"⏱️ TIME OUT: {DateTime.Now:hh:mm:ss tt}";
                    lblTimeInOut.ForeColor = Color.Red;
                }

                // Display student info
                lblStudentInfo.Text = $"✓ {identifiedStudent.Name}";
                lblStudentInfo.ForeColor = Color.Green;
                
                lblStatus.Text = $"✓ {identifiedStudent.StudentId} - {identifiedStudent.Program} Year {identifiedStudent.YearLevel}";
                lblStatus.ForeColor = Color.Green;

                System.Media.SystemSounds.Asterisk.Play();

                // Show today's attendance
                var todayRecord = _database.GetTodayAttendance(identifiedStudent.Id, _currentEvent.Id);
                if (todayRecord != null)
                {
                    var timeInStr = todayRecord.TimeIn?.ToString("hh:mm tt") ?? "Not yet";
                    var timeOutStr = todayRecord.TimeOut?.ToString("hh:mm tt") ?? "Not yet";
                    MessageBox.Show(
                        $"Attendance Recorded!\n\n" +
                        $"Student: {identifiedStudent.Name}\n" +
                        $"ID: {identifiedStudent.StudentId}\n" +
                        $"Event: {_currentEvent.EventName}\n\n" +
                        $"Time In: {timeInStr}\n" +
                        $"Time Out: {timeOutStr}",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
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
                _isScanning = false;
                btnTimeIn.Enabled = true;
                btnTimeOut.Enabled = true;
            }
        }

        private async Task<Student?> IdentifyStudentAsync(string capturedTemplate)
        {
            // Get all students with their templates
            var allStudentTemplates = _database.GetAllStudentTemplates();

            foreach (var (studentId, templates) in allStudentTemplates)
            {
                // Try to match the captured template against any of the student's 5 templates
                foreach (var template in templates)
                {
                    // Compare templates using the middleware's compare method
                    // We need to use a direct comparison method, not verify (which captures again)
                    if (await CompareTemplatesAsync(capturedTemplate, template.TemplateData))
                    {
                        // Match found!
                        return _database.GetStudent(studentId);
                    }
                }
            }

            return null; // No match found
        }

        private async Task<bool> CompareTemplatesAsync(string template1, string template2)
        {
            try
            {
                // Use the API client's compare method
                var response = await _apiClient.CompareTemplatesAsync(template1, template2);
                return response.Success;
            }
            catch
            {
                return false;
            }
        }

        private void BtnAdminDashboard_Click(object? sender, EventArgs e)
        {
            var adminForm = new AdminDashboard(_database, _apiClient);
            adminForm.ShowDialog();
        }

        private void BtnCheckAttendance_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_currentEvent == null)
                {
                    MessageBox.Show("No active event.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var checkForm = new AttendanceCheckForm(_database, _currentEvent);
                checkForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening records: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowEnrollmentDialog()
        {
            var enrollForm = new StudentEnrollmentForm(_database, _apiClient);
            enrollForm.ShowDialog();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _database.Dispose();
            base.OnFormClosing(e);
        }
    }
}
