using ClosedXML.Excel;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using AttendanceWeb.Models;

namespace AttendanceWeb.Services
{
    public class ExcelImportExportService
    {
        public byte[] ExportDatabaseToExcel(AttendanceDatabase.DatabaseBackup data)
        {
            using var workbook = new XLWorkbook();

            // Students Sheet
            var wsStudents = workbook.Worksheets.Add("Students");
            if (data.Students != null && data.Students.Count > 0)
            {
                wsStudents.Cell(1, 1).Value = "Id";
                wsStudents.Cell(1, 2).Value = "StudentId";
                wsStudents.Cell(1, 3).Value = "Name";
                wsStudents.Cell(1, 4).Value = "Email";
                wsStudents.Cell(1, 5).Value = "Program";
                wsStudents.Cell(1, 6).Value = "YearLevel";
                wsStudents.Cell(1, 7).Value = "EnrolledDate";
                wsStudents.Cell(1, 8).Value = "IsActive";
                wsStudents.Cell(1, 9).Value = "Section";
                wsStudents.Cell(1, 10).Value = "IsVerified";

                for (int i = 0; i < data.Students.Count; i++)
                {
                    var student = data.Students[i];
                    wsStudents.Cell(i + 2, 1).Value = student.Id;
                    wsStudents.Cell(i + 2, 2).Value = student.StudentId;
                    wsStudents.Cell(i + 2, 3).Value = student.Name;
                    wsStudents.Cell(i + 2, 4).Value = student.Email;
                    wsStudents.Cell(i + 2, 5).Value = student.Program;
                    wsStudents.Cell(i + 2, 6).Value = student.YearLevel;
                    wsStudents.Cell(i + 2, 7).Value = student.EnrolledDate.ToString("yyyy-MM-dd");
                    wsStudents.Cell(i + 2, 8).Value = student.IsActive;
                    wsStudents.Cell(i + 2, 9).Value = student.Section;
                    wsStudents.Cell(i + 2, 10).Value = student.IsVerified;
                }
            }

            // Events Sheet
            var wsEvents = workbook.Worksheets.Add("Events");
            if (data.Events != null && data.Events.Count > 0)
            {
                wsEvents.Cell(1, 1).Value = "Id";
                wsEvents.Cell(1, 2).Value = "EventName";
                wsEvents.Cell(1, 3).Value = "EventDate";
                wsEvents.Cell(1, 4).Value = "Description";
                wsEvents.Cell(1, 5).Value = "IsActive";
                wsEvents.Cell(1, 6).Value = "TimeInStart";
                wsEvents.Cell(1, 7).Value = "TimeInEnd";
                wsEvents.Cell(1, 8).Value = "TimeOutStart";
                wsEvents.Cell(1, 9).Value = "TimeOutEnd";
                wsEvents.Cell(1, 10).Value = "Semester";
                wsEvents.Cell(1, 11).Value = "AcademicYear";

                for (int i = 0; i < data.Events.Count; i++)
                {
                    var ev = data.Events[i];
                    wsEvents.Cell(i + 2, 1).Value = ev.Id;
                    wsEvents.Cell(i + 2, 2).Value = ev.EventName;
                    wsEvents.Cell(i + 2, 3).Value = ev.EventDate.ToString("yyyy-MM-dd");
                    wsEvents.Cell(i + 2, 4).Value = ev.Description;
                    wsEvents.Cell(i + 2, 5).Value = ev.IsActive;
                    wsEvents.Cell(i + 2, 6).Value = ev.TimeInStart?.ToString("HH:mm:ss");
                    wsEvents.Cell(i + 2, 7).Value = ev.TimeInEnd?.ToString("HH:mm:ss");
                    wsEvents.Cell(i + 2, 8).Value = ev.TimeOutStart?.ToString("HH:mm:ss");
                    wsEvents.Cell(i + 2, 9).Value = ev.TimeOutEnd?.ToString("HH:mm:ss");
                    wsEvents.Cell(i + 2, 10).Value = ev.Semester;
                    wsEvents.Cell(i + 2, 11).Value = ev.AcademicYear;
                }
            }

            // Attendance Sheet
            var wsAttendance = workbook.Worksheets.Add("Attendance");
            if (data.Attendance != null && data.Attendance.Count > 0)
            {
                wsAttendance.Cell(1, 1).Value = "Id";
                wsAttendance.Cell(1, 2).Value = "EventId";
                wsAttendance.Cell(1, 3).Value = "StudentId";
                wsAttendance.Cell(1, 4).Value = "TimeIn";
                wsAttendance.Cell(1, 5).Value = "TimeOut";
                wsAttendance.Cell(1, 6).Value = "Status";
                wsAttendance.Cell(1, 7).Value = "RecordDate";
                wsAttendance.Cell(1, 8).Value = "IsDeleted";

                for (int i = 0; i < data.Attendance.Count; i++)
                {
                    var att = data.Attendance[i];
                    wsAttendance.Cell(i + 2, 1).Value = att.Id;
                    wsAttendance.Cell(i + 2, 2).Value = att.EventId;
                    wsAttendance.Cell(i + 2, 3).Value = att.StudentId;
                    wsAttendance.Cell(i + 2, 4).Value = att.TimeIn?.ToString("o");
                    wsAttendance.Cell(i + 2, 5).Value = att.TimeOut?.ToString("o");
                    wsAttendance.Cell(i + 2, 6).Value = att.Status;
                    wsAttendance.Cell(i + 2, 7).Value = att.RecordDate.ToString("yyyy-MM-dd");
                    wsAttendance.Cell(i + 2, 8).Value = att.IsDeleted;
                }
            }

            // Logs Sheet
            var wsLogs = workbook.Worksheets.Add("Logs");
            if (data.Logs != null && data.Logs.Count > 0)
            {
                wsLogs.Cell(1, 1).Value = "Id";
                wsLogs.Cell(1, 2).Value = "AdminId";
                wsLogs.Cell(1, 3).Value = "AdminUsername";
                wsLogs.Cell(1, 4).Value = "Action";
                wsLogs.Cell(1, 5).Value = "Target";
                wsLogs.Cell(1, 6).Value = "Details";
                wsLogs.Cell(1, 7).Value = "Timestamp";

                for (int i = 0; i < data.Logs.Count; i++)
                {
                    var log = data.Logs[i];
                    wsLogs.Cell(i + 2, 1).Value = log.Id;
                    wsLogs.Cell(i + 2, 2).Value = log.AdminId;
                    wsLogs.Cell(i + 2, 3).Value = log.AdminUsername;
                    wsLogs.Cell(i + 2, 4).Value = log.Action;
                    wsLogs.Cell(i + 2, 5).Value = log.Target;
                    wsLogs.Cell(i + 2, 6).Value = log.Details;
                    wsLogs.Cell(i + 2, 7).Value = log.Timestamp.ToString("o");
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public AttendanceDatabase.DatabaseBackup ImportDatabaseFromExcel(Stream stream)
        {
            var data = new AttendanceDatabase.DatabaseBackup
            {
                Students = new List<Student>(),
                Events = new List<Event>(),
                Attendance = new List<AttendanceRecord>(),
                Logs = new List<ActivityLog>()
            };

            using var workbook = new XLWorkbook(stream);

            if (workbook.TryGetWorksheet("Students", out var wsStudents))
            {
                var rows = wsStudents.RowsUsed().Skip(1); // Skip header
                foreach (var row in rows)
                {
                    data.Students.Add(new Student
                    {
                        Id = SafeGetInt(row.Cell(1)),
                        StudentId = row.Cell(2).GetString(),
                        Name = row.Cell(3).GetString(),
                        Email = row.Cell(4).GetString(),
                        Program = row.Cell(5).GetString(),
                        YearLevel = SafeGetInt(row.Cell(6)),
                        EnrolledDate = SafeGetDateTime(row.Cell(7)),
                        IsActive = SafeGetBool(row.Cell(8)),
                        Section = row.Cell(9).GetString(),
                        IsVerified = SafeGetBool(row.Cell(10))
                    });
                }
            }

            if (workbook.TryGetWorksheet("Events", out var wsEvents))
            {
                var rows = wsEvents.RowsUsed().Skip(1); // Skip header
                foreach (var row in rows)
                {
                    data.Events.Add(new Event
                    {
                        Id = SafeGetInt(row.Cell(1)),
                        EventName = row.Cell(2).GetString(),
                        EventDate = SafeGetDateTime(row.Cell(3)),
                        Description = row.Cell(4).GetString(),
                        IsActive = SafeGetBool(row.Cell(5)),
                        TimeInStart = SafeGetTimeOnly(row.Cell(6)),
                        TimeInEnd = SafeGetTimeOnly(row.Cell(7)),
                        TimeOutStart = SafeGetTimeOnly(row.Cell(8)),
                        TimeOutEnd = SafeGetTimeOnly(row.Cell(9)),
                        Semester = row.Cell(10).GetString(),
                        AcademicYear = row.Cell(11).GetString()
                    });
                }
            }

            if (workbook.TryGetWorksheet("Attendance", out var wsAttendance))
            {
                var rows = wsAttendance.RowsUsed().Skip(1); // Skip header
                foreach (var row in rows)
                {
                    data.Attendance.Add(new AttendanceRecord
                    {
                        Id = SafeGetInt(row.Cell(1)),
                        EventId = SafeGetInt(row.Cell(2)),
                        StudentId = SafeGetInt(row.Cell(3)),
                        TimeIn = SafeGetNullableDateTime(row.Cell(4)),
                        TimeOut = SafeGetNullableDateTime(row.Cell(5)),
                        Status = row.Cell(6).GetString(),
                        RecordDate = SafeGetDateTime(row.Cell(7)),
                        IsDeleted = SafeGetBool(row.Cell(8))
                    });
                }
            }

            if (workbook.TryGetWorksheet("Logs", out var wsLogs))
            {
                var rows = wsLogs.RowsUsed().Skip(1); // Skip header
                foreach (var row in rows)
                {
                    data.Logs.Add(new ActivityLog
                    {
                        Id = SafeGetInt(row.Cell(1)),
                        AdminId = SafeGetNullableInt(row.Cell(2)),
                        AdminUsername = row.Cell(3).GetString(),
                        Action = row.Cell(4).GetString(),
                        Target = row.Cell(5).GetString(),
                        Details = row.Cell(6).GetString(),
                        Timestamp = SafeGetDateTime(row.Cell(7))
                    });
                }
            }

            return data;
        }

        private int SafeGetInt(IXLCell cell)
        {
            if (cell.IsEmpty()) return 0;
            if (cell.DataType == XLDataType.Number) return (int)cell.GetDouble();
            if (int.TryParse(cell.GetString(), out int val)) return val;
            return 0;
        }

        private int? SafeGetNullableInt(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            if (cell.DataType == XLDataType.Number) return (int)cell.GetDouble();
            if (int.TryParse(cell.GetString(), out int val)) return val;
            return null;
        }

        private bool SafeGetBool(IXLCell cell)
        {
            if (cell.IsEmpty()) return false;
            if (cell.DataType == XLDataType.Boolean) return cell.GetBoolean();
            if (cell.DataType == XLDataType.Number) return cell.GetDouble() != 0;
            var str = cell.GetString().ToLower();
            return str == "true" || str == "1" || str == "yes";
        }

        private DateTime SafeGetDateTime(IXLCell cell)
        {
            if (cell.IsEmpty()) return DateTime.MinValue;
            try
            {
                if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
                if (DateTime.TryParse(cell.GetString(), out DateTime dt)) return dt;
            }
            catch { }
            return DateTime.MinValue;
        }

        private DateTime? SafeGetNullableDateTime(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            try
            {
                if (cell.DataType == XLDataType.DateTime) return cell.GetDateTime();
                if (DateTime.TryParse(cell.GetString(), out DateTime dt)) return dt;
            }
            catch { }
            return null;
        }

        private TimeOnly? SafeGetTimeOnly(IXLCell cell)
        {
            if (cell.IsEmpty()) return null;
            var str = cell.GetString();
            if (string.IsNullOrEmpty(str)) return null;
            
            // Handle ClosedXML/Excel weirdness with time values formatted as strings
            if (TimeOnly.TryParse(str, out TimeOnly result)) return result;
            
            // Handle potential DateTime strings
            if (DateTime.TryParse(str, out DateTime dt)) return TimeOnly.FromDateTime(dt);

            return null;
        }
    }
}
