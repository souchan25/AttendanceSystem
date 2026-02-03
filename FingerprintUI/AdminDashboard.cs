using FingerprintUI.Models;
using FingerprintUI.Services;

namespace FingerprintUI
{
    public partial class AdminDashboard : Form
    {
        private readonly AttendanceDatabase _database;
        private readonly FingerprintApiClient _apiClient;

        public AdminDashboard(AttendanceDatabase database, FingerprintApiClient apiClient)
        {
            _database = database;
            _apiClient = apiClient;
            
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            var lblTitle = new Label
            {
                Text = "Admin Dashboard",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            var btnEnrollStudent = new Button
            {
                Text = "Enroll New Student",
                Location = new Point(20, 70),
                Size = new Size(200, 50),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnEnrollStudent.Click += BtnEnrollStudent_Click;

            var btnManageEvents = new Button
            {
                Text = "Manage Events",
                Location = new Point(240, 70),
                Size = new Size(200, 50),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnManageEvents.Click += BtnManageEvents_Click;

            var btnViewReports = new Button
            {
                Text = "View Reports",
                Location = new Point(460, 70),
                Size = new Size(200, 50),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnViewReports.Click += BtnViewReports_Click;

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(680, 70),
                Size = new Size(150, 50),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Text = "Admin Dashboard";
            this.ClientSize = new Size(860, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            this.Controls.Add(lblTitle);
            this.Controls.Add(btnEnrollStudent);
            this.Controls.Add(btnManageEvents);
            this.Controls.Add(btnViewReports);
            this.Controls.Add(btnClose);
        }

        private void LoadData()
        {
            // Future: Add dashboard statistics
        }

        private void BtnEnrollStudent_Click(object? sender, EventArgs e)
        {
            var enrollForm = new StudentEnrollmentForm(_database, _apiClient);
            enrollForm.ShowDialog();
        }

        private void BtnManageEvents_Click(object? sender, EventArgs e)
        {
            var eventForm = new EventManagementForm(_database);
            eventForm.ShowDialog();
        }

        private void BtnViewReports_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Reports feature coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
