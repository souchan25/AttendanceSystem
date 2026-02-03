using FingerprintUI.Models;
using FingerprintUI.Services;

namespace FingerprintUI
{
    public partial class EventManagementForm : Form
    {
        private readonly AttendanceDatabase _database;
        private DataGridView _gridView = null!;

        public EventManagementForm(AttendanceDatabase database)
        {
            _database = database;
            InitializeComponent();
            LoadEvents();
        }

        private void InitializeComponent()
        {
            var lblTitle = new Label
            {
                Text = "Event Management",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };

            _gridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(960, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            var btnAddEvent = new Button
            {
                Text = "Add Event",
                Location = new Point(20, 480),
                Size = new Size(150, 40),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnAddEvent.Click += BtnAddEvent_Click;

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(830, 480),
                Size = new Size(150, 40),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Text = "Event Management";
            this.ClientSize = new Size(1000, 540);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(lblTitle);
            this.Controls.Add(_gridView);
            this.Controls.Add(btnAddEvent);
            this.Controls.Add(btnClose);
        }

        private void LoadEvents()
        {
            var events = _database.GetAllEvents();

            _gridView.DataSource = null;
            _gridView.Columns.Clear();

            _gridView.Columns.Add("EventName", "Event Name");
            _gridView.Columns.Add("EventDate", "Date");
            _gridView.Columns.Add("Period", "Period");
            _gridView.Columns.Add("AcademicYear", "Academic Year");
            _gridView.Columns.Add("Active", "Active");

            foreach (var evt in events)
            {
                _gridView.Rows.Add(
                    evt.EventName,
                    evt.EventDate.ToString("MMMM dd, yyyy"),
                    evt.Period,
                    evt.AcademicYear,
                    evt.IsActive ? "Yes" : "No"
                );
            }
        }

        private void BtnAddEvent_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Add event feature coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
