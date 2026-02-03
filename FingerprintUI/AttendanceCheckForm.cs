using FingerprintUI.Models;
using FingerprintUI.Services;

namespace FingerprintUI
{
    public partial class AttendanceCheckForm : Form
    {
        private readonly AttendanceDatabase _database;
        private readonly Event _currentEvent;
        private DataGridView _gridView = null!;

        public AttendanceCheckForm(AttendanceDatabase database, Event currentEvent)
        {
            _database = database;
            _currentEvent = currentEvent;
            
            InitializeComponent();
            LoadAttendanceData();
        }

        private void InitializeComponent()
        {
            var lblTitle = new Label
            {
                Text = $"Attendance for {_currentEvent.EventName}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(800, 30)
            };

            _gridView = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(960, 480),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            var btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(20, 560),
                Size = new Size(150, 40),
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadAttendanceData();

            var btnClose = new Button
            {
                Text = "Close",
                Location = new Point(830, 560),
                Size = new Size(150, 40),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            this.Text = "Check Attendance";
            this.ClientSize = new Size(1000, 620);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterParent;

            this.Controls.Add(lblTitle);
            this.Controls.Add(_gridView);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(btnClose);
        }

        private void LoadAttendanceData()
        {
            var attendanceList = _database.GetEventAttendance(_currentEvent.Id);

            _gridView.DataSource = null;
            _gridView.Columns.Clear();

            // Create columns
            _gridView.Columns.Add("StudentId", "Student ID");
            _gridView.Columns.Add("StudentName", "Name");
            _gridView.Columns.Add("Program", "Program");
            _gridView.Columns.Add("TimeIn", "Time In");
            _gridView.Columns.Add("TimeOut", "Time Out");
            _gridView.Columns.Add("Status", "Status");

            // Populate rows
            foreach (var info in attendanceList)
            {
                var timeInStr = info.Record.TimeIn?.ToString("hh:mm tt") ?? "-";
                var timeOutStr = info.Record.TimeOut?.ToString("hh:mm tt") ?? "-";
                var status = info.Record.TimeIn != null && info.Record.TimeOut != null ? "Complete" :
                             info.Record.TimeIn != null ? "Time In Only" : "No Record";

                _gridView.Rows.Add(
                    info.Student.StudentId,
                    info.Student.Name,
                    $"{info.Student.Program} Y{info.Student.YearLevel}",
                    timeInStr,
                    timeOutStr,
                    status
                );
            }

            // Color code status
            foreach (DataGridViewRow row in _gridView.Rows)
            {
                var status = row.Cells["Status"].Value?.ToString();
                if (status == "Complete")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else if (status == "Time In Only")
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }
        }
    }
}
