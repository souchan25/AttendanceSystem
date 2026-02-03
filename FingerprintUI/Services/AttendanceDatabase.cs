using Microsoft.Data.Sqlite;
using FingerprintUI.Models;

namespace FingerprintUI.Services
{
    public class AttendanceDatabase : IDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public AttendanceDatabase()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "FingerprintAttendance");
            Directory.CreateDirectory(appFolder);
            
            var dbPath = Path.Combine(appFolder, "attendance.db");
            _connectionString = $"Data Source={dbPath}";
            
            InitializeDatabase();
        }

        private SqliteConnection GetConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        private void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var createTables = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId TEXT UNIQUE NOT NULL,
                    Name TEXT NOT NULL,
                    Email TEXT,
                    Program TEXT,
                    YearLevel INTEGER,
                    EnrolledDate TEXT NOT NULL,
                    IsActive INTEGER DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS FingerprintTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    SampleNumber INTEGER NOT NULL,
                    TemplateData TEXT NOT NULL,
                    Quality INTEGER,
                    CapturedDate TEXT NOT NULL,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id),
                    UNIQUE(StudentId, SampleNumber)
                );

                CREATE TABLE IF NOT EXISTS Events (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EventName TEXT NOT NULL,
                    Description TEXT,
                    EventDate TEXT NOT NULL,
                    Period TEXT NOT NULL,
                    AcademicYear TEXT NOT NULL,
                    IsActive INTEGER DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS AttendanceRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    EventId INTEGER NOT NULL,
                    TimeIn TEXT,
                    TimeOut TEXT,
                    Status TEXT,
                    RecordDate TEXT NOT NULL,
                    FOREIGN KEY (StudentId) REFERENCES Students(Id),
                    FOREIGN KEY (EventId) REFERENCES Events(Id),
                    UNIQUE(StudentId, EventId, RecordDate)
                );

                CREATE TABLE IF NOT EXISTS Admins (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    Name TEXT NOT NULL,
                    TemplateData TEXT NOT NULL,
                    CreatedDate TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_student_id ON Students(StudentId);
                CREATE INDEX IF NOT EXISTS idx_attendance_date ON AttendanceRecords(RecordDate);
                CREATE INDEX IF NOT EXISTS idx_event_period ON Events(Period, AcademicYear);
            ";

            using var cmd = new SqliteCommand(createTables, conn);
            cmd.ExecuteNonQuery();
        }

        // Student Operations
        public int AddStudent(Student student)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO Students (StudentId, Name, Email, Program, YearLevel, EnrolledDate, IsActive)
                       VALUES (@StudentId, @Name, @Email, @Program, @YearLevel, @EnrolledDate, @IsActive);
                       SELECT last_insert_rowid();";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", student.StudentId);
            cmd.Parameters.AddWithValue("@Name", student.Name);
            cmd.Parameters.AddWithValue("@Email", student.Email ?? "");
            cmd.Parameters.AddWithValue("@Program", student.Program ?? "");
            cmd.Parameters.AddWithValue("@YearLevel", student.YearLevel);
            cmd.Parameters.AddWithValue("@EnrolledDate", student.EnrolledDate.ToString("o"));
            cmd.Parameters.AddWithValue("@IsActive", student.IsActive ? 1 : 0);
            
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public Student? GetStudent(int id)
        {
            using var conn = GetConnection();
            var sql = "SELECT * FROM Students WHERE Id = @Id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Student
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1),
                    Name = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Program = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    YearLevel = reader.GetInt32(5),
                    EnrolledDate = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1
                };
            }
            return null;
        }

        public Student? GetStudentByStudentId(string studentId)
        {
            using var conn = GetConnection();
            var sql = "SELECT * FROM Students WHERE StudentId = @StudentId";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Student
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1),
                    Name = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Program = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    YearLevel = reader.GetInt32(5),
                    EnrolledDate = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1
                };
            }
            return null;
        }

        public List<Student> GetAllStudents()
        {
            var students = new List<Student>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Students WHERE IsActive = 1 ORDER BY Name";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1),
                    Name = reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Program = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    YearLevel = reader.GetInt32(5),
                    EnrolledDate = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1
                });
            }
            return students;
        }

        // Fingerprint Template Operations
        public void AddFingerprintTemplate(FingerprintTemplate template)
        {
            using var conn = GetConnection();
            var sql = @"INSERT OR REPLACE INTO FingerprintTemplates 
                       (StudentId, SampleNumber, TemplateData, Quality, CapturedDate)
                       VALUES (@StudentId, @SampleNumber, @TemplateData, @Quality, @CapturedDate)";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", template.StudentId);
            cmd.Parameters.AddWithValue("@SampleNumber", template.SampleNumber);
            cmd.Parameters.AddWithValue("@TemplateData", template.TemplateData);
            cmd.Parameters.AddWithValue("@Quality", template.Quality);
            cmd.Parameters.AddWithValue("@CapturedDate", template.CapturedDate.ToString("o"));
            
            cmd.ExecuteNonQuery();
        }

        public List<FingerprintTemplate> GetStudentTemplates(int studentId)
        {
            var templates = new List<FingerprintTemplate>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM FingerprintTemplates WHERE StudentId = @StudentId ORDER BY SampleNumber";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                templates.Add(new FingerprintTemplate
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    SampleNumber = reader.GetInt32(2),
                    TemplateData = reader.GetString(3),
                    Quality = reader.GetInt32(4),
                    CapturedDate = DateTime.Parse(reader.GetString(5))
                });
            }
            return templates;
        }

        public List<(int StudentId, List<FingerprintTemplate> Templates)> GetAllStudentTemplates()
        {
            var result = new List<(int, List<FingerprintTemplate>)>();
            var students = GetAllStudents();
            
            foreach (var student in students)
            {
                var templates = GetStudentTemplates(student.Id);
                if (templates.Count > 0)
                {
                    result.Add((student.Id, templates));
                }
            }
            return result;
        }

        // Event Operations
        public int AddEvent(Event evt)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO Events (EventName, Description, EventDate, Period, AcademicYear, IsActive)
                       VALUES (@EventName, @Description, @EventDate, @Period, @AcademicYear, @IsActive);
                       SELECT last_insert_rowid();";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@EventName", evt.EventName);
            cmd.Parameters.AddWithValue("@Description", evt.Description ?? "");
            cmd.Parameters.AddWithValue("@EventDate", evt.EventDate.ToString("o"));
            cmd.Parameters.AddWithValue("@Period", evt.Period);
            cmd.Parameters.AddWithValue("@AcademicYear", evt.AcademicYear);
            cmd.Parameters.AddWithValue("@IsActive", evt.IsActive ? 1 : 0);
            
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Event> GetActiveEvents()
        {
            var events = new List<Event>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Events WHERE IsActive = 1 ORDER BY EventDate DESC";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                events.Add(new Event
                {
                    Id = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    EventDate = DateTime.Parse(reader.GetString(3)),
                    Period = reader.GetString(4),
                    AcademicYear = reader.GetString(5),
                    IsActive = reader.GetInt32(6) == 1
                });
            }
            return events;
        }

        public Event? GetCurrentEvent()
        {
            using var conn = GetConnection();
            var sql = @"SELECT * FROM Events 
                       WHERE IsActive = 1 AND date(EventDate) = date('now')
                       ORDER BY EventDate DESC LIMIT 1";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                return new Event
                {
                    Id = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    EventDate = DateTime.Parse(reader.GetString(3)),
                    Period = reader.GetString(4),
                    AcademicYear = reader.GetString(5),
                    IsActive = reader.GetInt32(6) == 1
                };
            }
            return null;
        }

        public List<Event> GetAllEvents()
        {
            var events = new List<Event>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Events ORDER BY EventDate DESC";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                events.Add(new Event
                {
                    Id = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    EventDate = DateTime.Parse(reader.GetString(3)),
                    Period = reader.GetString(4),
                    AcademicYear = reader.GetString(5),
                    IsActive = reader.GetInt32(6) == 1
                });
            }
            return events;
        }

        // Attendance Operations
        public void RecordTimeIn(int studentId, int eventId)
        {
            using var conn = GetConnection();
            var sql = @"INSERT OR REPLACE INTO AttendanceRecords 
                       (StudentId, EventId, TimeIn, RecordDate, Status)
                       VALUES (@StudentId, @EventId, @TimeIn, @RecordDate, @Status)";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            cmd.Parameters.AddWithValue("@TimeIn", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("@RecordDate", DateTime.Now.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Status", "Present");
            
            cmd.ExecuteNonQuery();
        }

        public void RecordTimeOut(int studentId, int eventId)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE AttendanceRecords 
                       SET TimeOut = @TimeOut
                       WHERE StudentId = @StudentId AND EventId = @EventId 
                       AND RecordDate = @RecordDate";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TimeOut", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            cmd.Parameters.AddWithValue("@RecordDate", DateTime.Now.ToString("yyyy-MM-dd"));
            
            cmd.ExecuteNonQuery();
        }

        public AttendanceRecord? GetTodayAttendance(int studentId, int eventId)
        {
            using var conn = GetConnection();
            var sql = @"SELECT * FROM AttendanceRecords 
                       WHERE StudentId = @StudentId AND EventId = @EventId 
                       AND RecordDate = @RecordDate";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            cmd.Parameters.AddWithValue("@RecordDate", DateTime.Now.ToString("yyyy-MM-dd"));
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new AttendanceRecord
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    EventId = reader.GetInt32(2),
                    TimeIn = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                    TimeOut = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                    Status = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    RecordDate = DateTime.Parse(reader.GetString(6))
                };
            }
            return null;
        }

        public List<AttendanceInfo> GetEventAttendance(int eventId)
        {
            var records = new List<AttendanceInfo>();
            using var conn = GetConnection();
            var sql = @"SELECT 
                        a.Id, a.StudentId, a.EventId, a.TimeIn, a.TimeOut, a.Status, a.RecordDate,
                        s.Id, s.StudentId, s.Name, s.Email, s.Program, s.YearLevel, s.EnrolledDate, s.IsActive,
                        e.Id, e.EventName
                       FROM AttendanceRecords a
                       JOIN Students s ON a.StudentId = s.Id
                       JOIN Events e ON a.EventId = e.Id
                       WHERE a.EventId = @EventId
                       ORDER BY a.TimeIn";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                records.Add(new AttendanceInfo
                {
                    Record = new AttendanceRecord
                    {
                        Id = reader.GetInt32(0),
                        StudentId = reader.GetInt32(1),
                        EventId = reader.GetInt32(2),
                        TimeIn = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                        TimeOut = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                        Status = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        RecordDate = DateTime.Parse(reader.GetString(6))
                    },
                    Student = new Student
                    {
                        Id = reader.GetInt32(7),
                        StudentId = reader.GetString(8),
                        Name = reader.GetString(9),
                        Email = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        Program = reader.IsDBNull(11) ? "" : reader.GetString(11),
                        YearLevel = reader.GetInt32(12),
                        EnrolledDate = DateTime.Parse(reader.GetString(13)),
                        IsActive = reader.GetInt32(14) == 1
                    },
                    Event = new Event
                    {
                        Id = reader.GetInt32(15),
                        EventName = reader.GetString(16)
                    }
                });
            }
            return records;
        }

        // Admin Operations
        public void AddAdmin(Admin admin)
        {
            using var conn = GetConnection();
            var sql = @"INSERT OR REPLACE INTO Admins (Username, Name, TemplateData, CreatedDate)
                       VALUES (@Username, @Name, @TemplateData, @CreatedDate)";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", admin.Username);
            cmd.Parameters.AddWithValue("@Name", admin.Name);
            cmd.Parameters.AddWithValue("@TemplateData", admin.TemplateData);
            cmd.Parameters.AddWithValue("@CreatedDate", admin.CreatedDate.ToString("o"));
            
            cmd.ExecuteNonQuery();
        }

        public List<Admin> GetAllAdmins()
        {
            var admins = new List<Admin>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Admins ORDER BY Name";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                admins.Add(new Admin
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Name = reader.GetString(2),
                    TemplateData = reader.GetString(3),
                    CreatedDate = DateTime.Parse(reader.GetString(4))
                });
            }
            return admins;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
