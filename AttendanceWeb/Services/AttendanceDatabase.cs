using Microsoft.Data.Sqlite;
using AttendanceWeb.Models;

namespace AttendanceWeb.Services
{
    public class AttendanceDatabase
    {
        private readonly string _connectionString;

        public AttendanceDatabase()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "FingerprintAttendance");
            Directory.CreateDirectory(appFolder);
            
            var dbPath = Path.Combine(appFolder, "attendance.db");
            _connectionString = $"Data Source={dbPath}";
            Console.WriteLine($"[AttendanceDatabase] Initialized with path: {dbPath}");

            InitializeDatabase();
        }

        private SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
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
                    IsActive INTEGER DEFAULT 1,
                    TimeInStart TEXT,
                    TimeInEnd TEXT,
                    TimeOutStart TEXT,
                    TimeOutEnd TEXT,
                    IsDeleted INTEGER DEFAULT 0
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
                    Password TEXT DEFAULT '',
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

            // Migrations for Events table to support time windows
            try { using var c = new SqliteCommand("ALTER TABLE Events ADD COLUMN TimeInStart TEXT", conn); c.ExecuteNonQuery(); } catch { }
            try { using var c = new SqliteCommand("ALTER TABLE Events ADD COLUMN TimeInEnd TEXT", conn); c.ExecuteNonQuery(); } catch { }
            try { using var c = new SqliteCommand("ALTER TABLE Events ADD COLUMN TimeOutStart TEXT", conn); c.ExecuteNonQuery(); } catch { }
            try { using var c = new SqliteCommand("ALTER TABLE Events ADD COLUMN TimeOutEnd TEXT", conn); c.ExecuteNonQuery(); } catch { }
            try { using var c = new SqliteCommand("ALTER TABLE Events ADD COLUMN IsDeleted INTEGER DEFAULT 0", conn); c.ExecuteNonQuery(); } catch { }
            try { using var c = new SqliteCommand("ALTER TABLE Admins ADD COLUMN Password TEXT DEFAULT ''", conn); c.ExecuteNonQuery(); } catch { }
            try { using var c = new SqliteCommand("ALTER TABLE Students ADD COLUMN Section TEXT DEFAULT ''", conn); c.ExecuteNonQuery(); } catch { }

            // Ensure default admin has a password if it's missing (Fix for legacy/broken users)
            try { 
                using var c = new SqliteCommand("UPDATE Admins SET Password = 'admin' WHERE Username = 'admin' AND (Password IS NULL OR Password = '')", conn); 
                c.ExecuteNonQuery(); 
            } catch { }
        }

        // Student Operations
        public int AddStudent(Student student)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO Students (StudentId, Name, Email, Program, YearLevel, Section, EnrolledDate, IsActive)
                       VALUES (@StudentId, @Name, @Email, @Program, @YearLevel, @Section, @EnrolledDate, @IsActive);
                       SELECT last_insert_rowid();";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", student.StudentId);
            cmd.Parameters.AddWithValue("@Name", student.Name);
            cmd.Parameters.AddWithValue("@Email", student.Email ?? "");
            cmd.Parameters.AddWithValue("@Program", student.Program ?? "");
            cmd.Parameters.AddWithValue("@YearLevel", student.YearLevel);
            cmd.Parameters.AddWithValue("@Section", student.Section ?? "");
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
                    YearLevel = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    EnrolledDate = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1,
                    // Handle Section if column exists (it might not in old queries without strict select)
                    // But we used SELECT *, so check field count
                    Section = reader.FieldCount > 8 && !reader.IsDBNull(8) ? reader.GetString(8) : ""
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
                    YearLevel = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    EnrolledDate = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1,
                    Section = reader.FieldCount > 8 && !reader.IsDBNull(8) ? reader.GetString(8) : ""
                };
            }
            return null;
        }

        public List<Student> GetAllStudents()
        {
            var students = new List<Student>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Students ORDER BY Name";
            
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
                    YearLevel = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    EnrolledDate = DateTime.Parse(reader.GetString(6)),
                    IsActive = reader.GetInt32(7) == 1,
                    Section = reader.FieldCount > 8 && !reader.IsDBNull(8) ? reader.GetString(8) : ""
                });
            }
            return students;
        }

        public void UpdateStudent(Student student)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE Students 
                       SET StudentId = @studentId, Name = @name, Email = @email, 
                           Program = @program, YearLevel = @yearLevel, IsActive = @isActive
                       WHERE Id = @id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", student.Id);
            cmd.Parameters.AddWithValue("@studentId", student.StudentId);
            cmd.Parameters.AddWithValue("@name", student.Name);
            cmd.Parameters.AddWithValue("@email", student.Email ?? "");
            cmd.Parameters.AddWithValue("@program", student.Program ?? "");
            cmd.Parameters.AddWithValue("@yearLevel", student.YearLevel);
            cmd.Parameters.AddWithValue("@isActive", student.IsActive ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public void DeleteStudent(int studentId)
        {
            using var conn = GetConnection();
            
            // Delete fingerprint templates first
            var sql1 = "DELETE FROM FingerprintTemplates WHERE StudentId = @studentId";
            using var cmd1 = new SqliteCommand(sql1, conn);
            cmd1.Parameters.AddWithValue("@studentId", studentId);
            cmd1.ExecuteNonQuery();
            
            // Delete attendance records
            var sql2 = "DELETE FROM AttendanceRecords WHERE StudentId = @studentId";
            using var cmd2 = new SqliteCommand(sql2, conn);
            cmd2.Parameters.AddWithValue("@studentId", studentId);
            cmd2.ExecuteNonQuery();
            
            // Delete student
            var sql3 = "DELETE FROM Students WHERE Id = @studentId";
            using var cmd3 = new SqliteCommand(sql3, conn);
            cmd3.Parameters.AddWithValue("@studentId", studentId);
            cmd3.ExecuteNonQuery();
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
        // Removed duplicate AddEvent here; prefer the updated void version below.

        public List<Event> GetActiveEvents()
        {
            // Auto-deactivate expired events first
            AutoDeactivateExpiredEvents();
            
            var events = new List<Event>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Events WHERE IsActive = 1 AND IsDeleted = 0 ORDER BY EventDate DESC";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                events.Add(MapEvent(reader));
            }
            return events;
        }

        public Event? GetCurrentEvent()
        {
            // Auto-deactivate expired events first
            AutoDeactivateExpiredEvents();
            
            using var conn = GetConnection();
            var sql = @"SELECT * FROM Events 
                       WHERE IsActive = 1 AND IsDeleted = 0 AND date(EventDate) = date('now')
                       ORDER BY EventDate DESC LIMIT 1";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                return MapEvent(reader);
            }
            return null;
        }

        public List<Event> GetAllEvents(bool includeDeleted = false)
        {
            var events = new List<Event>();
            using var conn = GetConnection();
            var sql = includeDeleted 
                ? "SELECT * FROM Events ORDER BY EventDate DESC" 
                : "SELECT * FROM Events WHERE IsDeleted = 0 ORDER BY EventDate DESC";
            
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                events.Add(MapEvent(reader));
            }
            return events;
        }

        public Event? GetEvent(int eventId)
        {
            using var conn = GetConnection();
            var sql = "SELECT * FROM Events WHERE Id = @Id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", eventId);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapEvent(reader);
            }
            return null;
        }

        public void UpdateEvent(Event evt)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE Events 
                       SET EventName = @eventName, Description = @description, 
                           EventDate = @eventDate, Period = @period, 
                           AcademicYear = @academicYear, IsActive = @isActive,
                           TimeInStart = @TimeInStart, TimeInEnd = @TimeInEnd,
                           TimeOutStart = @TimeOutStart, TimeOutEnd = @TimeOutEnd,
                           IsDeleted = @IsDeleted
                       WHERE Id = @id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", evt.Id);
            cmd.Parameters.AddWithValue("@eventName", evt.EventName);
            cmd.Parameters.AddWithValue("@description", evt.Description ?? "");
            cmd.Parameters.AddWithValue("@eventDate", evt.EventDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@period", evt.Period);
            cmd.Parameters.AddWithValue("@academicYear", evt.AcademicYear);
            cmd.Parameters.AddWithValue("@isActive", evt.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@TimeInStart", evt.TimeInStart ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TimeInEnd", evt.TimeInEnd ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TimeOutStart", evt.TimeOutStart ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TimeOutEnd", evt.TimeOutEnd ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsDeleted", evt.IsDeleted ? 1 : 0);
            cmd.ExecuteNonQuery();
        }


        // DeleteEvent moved to support soft delete with specific implementation

        private Event MapEvent(SqliteDataReader reader)
        {
            TimeOnly? ParseTime(string? input)
            {
                if (string.IsNullOrWhiteSpace(input)) return null;
                // Fix for possible typo 'o' instead of '0'
                input = input.Replace("o", "0").Replace("O", "0");
                if (TimeOnly.TryParse(input, out var t)) return t;
                return null;
            }

            return new Event
            {
                Id = reader.GetInt32(0),
                EventName = reader.GetString(1),
                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                EventDate = DateTime.Parse(reader.GetString(3)),
                Period = reader.GetString(4),
                AcademicYear = reader.GetString(5),
                IsActive = reader.GetInt32(6) == 1,
                TimeInStart = reader.FieldCount > 7 && !reader.IsDBNull(7) ? ParseTime(reader.GetString(7)) : null,
                TimeInEnd = reader.FieldCount > 8 && !reader.IsDBNull(8) ? ParseTime(reader.GetString(8)) : null,
                TimeOutStart = reader.FieldCount > 9 && !reader.IsDBNull(9) ? ParseTime(reader.GetString(9)) : null,
                TimeOutEnd = reader.FieldCount > 10 && !reader.IsDBNull(10) ? ParseTime(reader.GetString(10)) : null,
                IsDeleted = reader.FieldCount > 11 && !reader.IsDBNull(11) && reader.GetInt32(11) == 1
            };
        }
        public (bool Success, string Message) RecordTimeIn(int studentId, int eventId)
        {
            Console.WriteLine($"[RecordTimeIn] Attempting Time In for Student {studentId}, Event {eventId}");
            var evt = GetEvent(eventId);
            if (evt == null) 
            {
                Console.WriteLine("[RecordTimeIn] Event not found!");
                return (false, "Event not found.");
            }

            var now = DateTime.Now;
            
            // Strict Time Validation for Time In
            string recordStatus = "Present";
            if (evt.TimeInStart.HasValue)
            {
                var startDate = evt.EventDate.Date.Add(evt.TimeInStart.Value.ToTimeSpan());
                if (now < startDate) return (false, $"Too early for Time In. Starts at {evt.TimeInStart}");
            }
            
            if (evt.TimeInEnd.HasValue)
            {
                var endDate = evt.EventDate.Date.Add(evt.TimeInEnd.Value.ToTimeSpan());
                if (now > endDate) 
                {
                    recordStatus = "Late";
                }
            }

            try 
            {
                using var conn = GetConnection();
                
                // Try Update First (Upsert logic to preserve TimeOut if it exists)
                var updateSql = @"UPDATE AttendanceRecords 
                                 SET TimeIn = @TimeIn, Status = @Status
                                 WHERE StudentId = @StudentId AND EventId = @EventId AND RecordDate = @RecordDate";
                
                using var updateCmd = new SqliteCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@StudentId", studentId);
                updateCmd.Parameters.AddWithValue("@EventId", eventId);
                updateCmd.Parameters.AddWithValue("@RecordDate", now.ToString("yyyy-MM-dd"));
                updateCmd.Parameters.AddWithValue("@TimeIn", now.ToString("o"));
                updateCmd.Parameters.AddWithValue("@Status", recordStatus);
                
                int rows = updateCmd.ExecuteNonQuery();
                Console.WriteLine($"[RecordTimeIn] Update affected {rows} rows.");

                if (rows == 0)
                {
                    // Update failed (no record), so Insert
                    var insertSql = @"INSERT INTO AttendanceRecords 
                           (StudentId, EventId, TimeIn, RecordDate, Status)
                           VALUES (@StudentId, @EventId, @TimeIn, @RecordDate, @Status)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@StudentId", studentId);
                    insertCmd.Parameters.AddWithValue("@EventId", eventId);
                    insertCmd.Parameters.AddWithValue("@TimeIn", now.ToString("o"));
                    insertCmd.Parameters.AddWithValue("@RecordDate", now.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@Status", recordStatus);
                    
                    insertCmd.ExecuteNonQuery();
                    Console.WriteLine("[RecordTimeIn] Inserted new record.");
                    
                    // Count as 1 row change for the check below
                    rows = 1;
                }
                
                if (rows == 0)
                {
                     return (false, "Database insert/update failed (0 rows affected).");
                }

                // Verification
                using var verifyCmd = new SqliteCommand("SELECT COUNT(*) FROM AttendanceRecords", conn);
                var count = Convert.ToInt64(verifyCmd.ExecuteScalar());
                Console.WriteLine($"[RecordTimeIn] Total records in DB: {count}");
                
                if (count == 0)
                {
                    return (false, "Critical Error: Record was not saved to database.");
                }

                return (true, recordStatus == "Late" ? "Time In Recorded (Late)" : "Time In Recorded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecordTimeIn] Exception: {ex}");
                return (false, $"Error: {ex.Message}");
            }
        }

        public (bool Success, string Message) RecordTimeOut(int studentId, int eventId)
        {
            Console.WriteLine($"[RecordTimeOut] Attempting Time Out for Student {studentId}, Event {eventId}");
            var evt = GetEvent(eventId);
            if (evt == null) return (false, "Event not found.");

            var now = DateTime.Now;
            string recordStatus = "Present"; // Default, only used if inserting new or updating status logic

            // Strict Time Validation for Time Out
            if (evt.TimeOutStart.HasValue)
            {
                var startDate = evt.EventDate.Date.Add(evt.TimeOutStart.Value.ToTimeSpan());
                if (now < startDate) return (false, $"Too early for Time Out. Starts at {evt.TimeOutStart}");
            }
            
            bool isLate = false;
            if (evt.TimeOutEnd.HasValue)
            {
                var endDate = evt.EventDate.Date.Add(evt.TimeOutEnd.Value.ToTimeSpan());
                if (now > endDate) 
                {
                    isLate = true;
                    recordStatus = "Late";
                }
            }

            try
            {
                using var conn = GetConnection();
                
                // Try to UPDATE existing record first (if Time In exists)
                // If Late, update status? Maybe. Let's say if TimeOut is late, status becomes Late.
                var updateSql = @"UPDATE AttendanceRecords 
                           SET TimeOut = @TimeOut " + (isLate ? ", Status = 'Late'" : "") + @"
                           WHERE StudentId = @StudentId AND EventId = @EventId 
                           AND RecordDate = @RecordDate";
                
                using var updateCmd = new SqliteCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@TimeOut", now.ToString("o"));
                updateCmd.Parameters.AddWithValue("@StudentId", studentId);
                updateCmd.Parameters.AddWithValue("@EventId", eventId);
                updateCmd.Parameters.AddWithValue("@RecordDate", now.ToString("yyyy-MM-dd"));
                
                int rows = updateCmd.ExecuteNonQuery();
                Console.WriteLine($"[RecordTimeOut] Update affected {rows} rows.");

                if (rows == 0)
                {
                    // No record exists (Student didn't Time In), so INSERT new record
                    Console.WriteLine("[RecordTimeOut] No existing record found. Inserting new record.");
                    
                    var insertSql = @"INSERT INTO AttendanceRecords 
                               (StudentId, EventId, TimeOut, RecordDate, Status)
                               VALUES (@StudentId, @EventId, @TimeOut, @RecordDate, @Status)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@StudentId", studentId);
                    insertCmd.Parameters.AddWithValue("@EventId", eventId);
                    insertCmd.Parameters.AddWithValue("@TimeOut", now.ToString("o"));
                    insertCmd.Parameters.AddWithValue("@RecordDate", now.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@Status", recordStatus);
                    
                    insertCmd.ExecuteNonQuery();
                }
                
                return (true, isLate ? "Time Out Recorded (Late)" : "Time Out Recorded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RecordTimeOut] Error: {ex}");
                return (false, $"Error: {ex.Message}");
            }
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


        public (int Present, int Absent) GetStudentAttendanceStats(int studentId)
        {
            using var conn = GetConnection();
            
            // Get student's enrollment date to filter events
            var enrollDateSql = @"SELECT EnrolledDate FROM Students WHERE Id = @StudentId";
            using var enrollCmd = new SqliteCommand(enrollDateSql, conn);
            enrollCmd.Parameters.AddWithValue("@StudentId", studentId);
            var enrollDateStr = enrollCmd.ExecuteScalar()?.ToString();
            var enrolledDate = !string.IsNullOrEmpty(enrollDateStr) ? DateTime.Parse(enrollDateStr) : DateTime.MinValue;

            // Count Present (Records with TimeIn OR Status='Present'/'Late')
            // Only count attendance for events on or after enrollment date
            var countPresentSql = @"SELECT COUNT(*) FROM AttendanceRecords ar
                                    JOIN Events e ON ar.EventId = e.Id
                                    WHERE ar.StudentId = @StudentId 
                                    AND (ar.TimeIn IS NOT NULL OR ar.Status IN ('Present', 'Late'))
                                    AND e.EventDate >= @EnrolledDate";
            
            using var cmdPresent = new SqliteCommand(countPresentSql, conn);
            cmdPresent.Parameters.AddWithValue("@StudentId", studentId);
            cmdPresent.Parameters.AddWithValue("@EnrolledDate", enrolledDate.ToString("yyyy-MM-dd"));
            int present = Convert.ToInt32(cmdPresent.ExecuteScalar());

            // Count Total Events that occurred on or after the student's enrollment date
            var countEventsSql = @"SELECT COUNT(*) FROM Events 
                                   WHERE IsDeleted = 0 
                                   AND EventDate < date('now')
                                   AND EventDate >= @EnrolledDate";
            
            using var cmdEvents = new SqliteCommand(countEventsSql, conn);
            cmdEvents.Parameters.AddWithValue("@EnrolledDate", enrolledDate.ToString("yyyy-MM-dd"));
            int totalPastEvents = Convert.ToInt32(cmdEvents.ExecuteScalar());
            
            int absent = Math.Max(0, totalPastEvents - present);
            
            return (present, absent);
        }

        public List<AttendanceInfo> GetStudentAttendanceHistory(int studentId)
        {
            var history = new List<AttendanceInfo>();
            using var conn = GetConnection();
            
            // Get student's enrollment date to filter events
            var enrollDateSql = @"SELECT EnrolledDate FROM Students WHERE Id = @StudentId";
            using var enrollCmd = new SqliteCommand(enrollDateSql, conn);
            enrollCmd.Parameters.AddWithValue("@StudentId", studentId);
            var enrollDateStr = enrollCmd.ExecuteScalar()?.ToString();
            var enrolledDate = !string.IsNullOrEmpty(enrollDateStr) ? DateTime.Parse(enrollDateStr) : DateTime.MinValue;

            var sql = @"SELECT 
                        a.Id, a.StudentId, a.EventId, a.TimeIn, a.TimeOut, a.Status, a.RecordDate,
                        e.Id, e.EventName, e.EventDate, e.Period, e.AcademicYear
                       FROM AttendanceRecords a
                       JOIN Events e ON a.EventId = e.Id
                       WHERE a.StudentId = @StudentId
                       AND e.EventDate >= @EnrolledDate
                       ORDER BY e.EventDate DESC";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@EnrolledDate", enrolledDate.ToString("yyyy-MM-dd"));
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                history.Add(new AttendanceInfo
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
                     Event = new Event 
                     {
                        Id = reader.GetInt32(7),
                        EventName = reader.GetString(8),
                        EventDate = DateTime.Parse(reader.GetString(9)),
                        Period = reader.GetString(10),
                        AcademicYear = reader.GetString(11)
                     }
                });
            }
            return history;
        }

        public List<Event> GetEvents(bool includeDeleted = false)
        {
            var events = new List<Event>();
            using var conn = GetConnection();
            var sql = "SELECT * FROM Events";
            if (!includeDeleted)
            {
                sql += " WHERE IsDeleted = 0";
            }
            sql += " ORDER BY EventDate DESC";

            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                events.Add(MapEvent(reader));
            }
            return events;
        }

        public void DeleteEvent(int eventId)
        {
             // Soft Delete
             using var conn = GetConnection();
             var sql = "UPDATE Events SET IsDeleted = 1 WHERE Id = @Id";
             using var cmd = new SqliteCommand(sql, conn);
             cmd.Parameters.AddWithValue("@Id", eventId);
             cmd.ExecuteNonQuery();
        }

        public void AddEvent(Event evt)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO Events 
                        (EventName, Description, EventDate, Period, AcademicYear, IsActive, 
                         TimeInStart, TimeInEnd, TimeOutStart, TimeOutEnd, IsDeleted)
                        VALUES 
                        (@Name, @Desc, @Date, @Period, @Year, @Active, 
                         @TIS, @TIE, @TOS, @TOE, 0)";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", evt.EventName);
            cmd.Parameters.AddWithValue("@Desc", evt.Description ?? "");
            cmd.Parameters.AddWithValue("@Date", evt.EventDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@Period", evt.Period);
            cmd.Parameters.AddWithValue("@Year", evt.AcademicYear);
            cmd.Parameters.AddWithValue("@Active", evt.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("@TIS", evt.TimeInStart?.ToString("HH:mm") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TIE", evt.TimeInEnd?.ToString("HH:mm") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TOS", evt.TimeOutStart?.ToString("HH:mm") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TOE", evt.TimeOutEnd?.ToString("HH:mm") ?? (object)DBNull.Value);
            
            cmd.ExecuteNonQuery();
        }

        public List<AttendanceInfo> GetEventAttendance(int eventId)
        {
            var records = new List<AttendanceInfo>();
            using var conn = GetConnection();
            var sql = @"SELECT 
                        a.Id, a.StudentId, a.EventId, a.TimeIn, a.TimeOut, a.Status, a.RecordDate,
                        s.Id, s.StudentId, s.Name, s.Email, s.Program, s.YearLevel, s.EnrolledDate, s.IsActive, s.Section,
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
                        YearLevel = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                        EnrolledDate = DateTime.Parse(reader.GetString(13)),
                        IsActive = reader.GetInt32(14) == 1,
                        Section = reader.IsDBNull(15) ? "" : reader.GetString(15)
                    },
                    Event = new Event
                    {
                        Id = reader.GetInt32(16),
                        EventName = reader.GetString(17)
                    }
                });
            }
            return records;
        }

        public List<AttendanceRecord> GetAllAttendanceRecords()
        {
            var records = new List<AttendanceRecord>();
            try 
            {
                using var conn = GetConnection();
                var sql = "SELECT * FROM AttendanceRecords ORDER BY RecordDate DESC";
                
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    records.Add(new AttendanceRecord
                    {
                        Id = reader.GetInt32(0),
                        StudentId = reader.GetInt32(1),
                        EventId = reader.GetInt32(2),
                        TimeIn = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                        TimeOut = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                        Status = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        RecordDate = DateTime.Parse(reader.GetString(6))
                    });
                }
                Console.WriteLine($"[GetAllAttendanceRecords] Returning {records.Count} records");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllAttendanceRecords] Error: {ex.Message}");
            }
            return records;
        }

        // Admin Operations
        public void AddAdmin(Admin admin)
        {
            using var conn = GetConnection();
            var sql = @"INSERT INTO Admins (Username, Password, Name, TemplateData, CreatedDate)
                       VALUES (@Username, @Password, @Name, @TemplateData, @CreatedDate)";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", admin.Username);
            cmd.Parameters.AddWithValue("@Password", admin.Password);
            cmd.Parameters.AddWithValue("@Name", admin.Name);
            cmd.Parameters.AddWithValue("@TemplateData", admin.TemplateData);
            cmd.Parameters.AddWithValue("@CreatedDate", admin.CreatedDate.ToString("o"));
            
            cmd.ExecuteNonQuery();
        }

        public void UpdateAdmin(Admin admin)
        {
            using var conn = GetConnection();
            var sql = @"UPDATE Admins 
                       SET Username = @Username, Password = @Password, Name = @Name, TemplateData = @TemplateData
                       WHERE Id = @Id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", admin.Id);
            cmd.Parameters.AddWithValue("@Username", admin.Username);
            cmd.Parameters.AddWithValue("@Password", admin.Password);
            cmd.Parameters.AddWithValue("@Name", admin.Name);
            cmd.Parameters.AddWithValue("@TemplateData", admin.TemplateData);
            
            cmd.ExecuteNonQuery();
        }

        public void DeleteAdmin(int adminId)
        {
            using var conn = GetConnection();
            var sql = "DELETE FROM Admins WHERE Id = @Id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", adminId);
            
            cmd.ExecuteNonQuery();
        }

        public Admin? GetAdminById(int id)
        {
            using var conn = GetConnection();
            var sql = "SELECT * FROM Admins WHERE Id = @Id";
            
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            
            using var reader = cmd.ExecuteReader();
            
            if (reader.Read())
            {
                var admin = new Admin
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"].ToString() ?? "",
                    Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() ?? "" : "",
                    TemplateData = reader["TemplateData"] != DBNull.Value ? reader["TemplateData"].ToString() ?? "" : ""
                };
                
                var createdStr = reader["CreatedDate"] != DBNull.Value ? reader["CreatedDate"].ToString() : null;
                admin.CreatedDate = createdStr != null ? DateTime.Parse(createdStr) : DateTime.Now;

                try 
                {
                    var pwdIdx = reader.GetOrdinal("Password");
                    if (!reader.IsDBNull(pwdIdx))
                    {
                        admin.Password = reader.GetString(pwdIdx);
                    }
                }
                catch { }
                
                return admin;
            }
            
            return null;
        }

        public async Task<List<Admin>> GetAllAdminsAsync()
        {
            return await Task.Run(() => GetAllAdmins());
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
                var admin = new Admin();
                // Safe property mapping using name lookups to avoid index issues with schema changes
                admin.Id = Convert.ToInt32(reader["Id"]);
                admin.Username = reader["Username"].ToString() ?? "";
                admin.Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() ?? "" : "";
                admin.TemplateData = reader["TemplateData"] != DBNull.Value ? reader["TemplateData"].ToString() ?? "" : "";
                
                var createdStr = reader["CreatedDate"] != DBNull.Value ? reader["CreatedDate"].ToString() : null;
                admin.CreatedDate = createdStr != null ? DateTime.Parse(createdStr) : DateTime.Now;

                // Handle Password column robustly
                try 
                {
                    var pwdIdx = reader.GetOrdinal("Password");
                    if (!reader.IsDBNull(pwdIdx))
                    {
                        admin.Password = reader.GetString(pwdIdx);
                    }
                }
                catch { /* Ignore if column missing */ }

                admins.Add(admin);
            }
            return admins;
        }

        // Auto-deactivate events where TimeOut has passed
        private void AutoDeactivateExpiredEvents()
        {
            using var conn = GetConnection();
            
            // Get all active events
            var sql = "SELECT * FROM Events WHERE IsActive = 1";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            
            var expiredEventIds = new List<int>();
            
            while (reader.Read())
            {
                var eventId = reader.GetInt32(0);
                var eventDate = DateTime.Parse(reader.GetString(3));
                var timeOutEnd = reader.FieldCount > 10 && !reader.IsDBNull(10) ? reader.GetString(10) : null;
                
                // If event has a TimeOutEnd, check if it has passed
                if (!string.IsNullOrEmpty(timeOutEnd))
                {
                    DateTime eventEndDateTime;
                    bool parsed = false;

                    // Try combining Date + Time string (handles both "HH:mm" and "hh:mm tt")
                    string combinedString = $"{eventDate:yyyy-MM-dd} {timeOutEnd}";
                    
                    if (DateTime.TryParse(combinedString, out eventEndDateTime))
                    {
                        parsed = true;
                    }
                    else if (TimeSpan.TryParse(timeOutEnd, out var timeOut))
                    {
                        // Fallback for simple time formats
                        eventEndDateTime = eventDate.Date.Add(timeOut);
                        parsed = true;
                    }

                    if (parsed)
                    {
                        // If current time is past the event's timeout, mark for deactivation
                        if (DateTime.Now > eventEndDateTime)
                        {
                            expiredEventIds.Add(eventId);
                        }
                    }
                }
                // If no TimeOutEnd is set, deactivate if event date has passed
                else if (eventDate.Date < DateTime.Now.Date)
                {
                    expiredEventIds.Add(eventId);
                }
            }
            
            reader.Close();
            
            // Deactivate expired events
            foreach (var eventId in expiredEventIds)
            {
                var updateSql = "UPDATE Events SET IsActive = 0 WHERE Id = @id";
                using var updateCmd = new SqliteCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@id", eventId);
                updateCmd.ExecuteNonQuery();
            }
        }

        private bool TryGetEventTime(DateTime eventDate, string? timeString, out DateTime result)
        {
            result = DateTime.MinValue;
            if (string.IsNullOrEmpty(timeString)) return false;

            // Try combining Date + Time string (handles both "HH:mm" and "hh:mm tt")
            string combinedString = $"{eventDate:yyyy-MM-dd} {timeString}";
            
            if (DateTime.TryParse(combinedString, out result))
            {
                return true;
            }
            
            if (TimeSpan.TryParse(timeString, out var timeSpan))
            {
                result = eventDate.Date.Add(timeSpan);
                return true;
            }

            return false;
        }


        // Removed duplicate GetStudentAttendanceHistory
    }
}
