namespace AttendanceWeb.Models
{
    // Database Models
    public class Student
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Program { get; set; } = "";
        public int YearLevel { get; set; }
        public DateTime EnrolledDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FingerprintTemplate
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SampleNumber { get; set; } // 1-5
        public string TemplateData { get; set; } = ""; // XML template
        public int Quality { get; set; }
        public DateTime CapturedDate { get; set; }
    }

    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int EventId { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public string Status { get; set; } = ""; // Present, Late, Absent
        public DateTime RecordDate { get; set; }
    }

    public class Event
    {
        public int Id { get; set; }
        public string EventName { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Period { get; set; } = ""; // Midterm, Final
        public string AcademicYear { get; set; } = "";
        
        // Time windows for attendance (stored as strings in HH:mm format)
        public string? TimeInStart { get; set; }
        public string? TimeInEnd { get; set; }
        public string? TimeOutStart { get; set; }
        public string? TimeOutEnd { get; set; }
        
        public bool IsActive { get; set; } = true;
    }

    public class Admin
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string TemplateData { get; set; } = ""; // Admin fingerprint
        public DateTime CreatedDate { get; set; }
    }

    // View Models
    public class StudentEnrollmentInfo
    {
        public Student Student { get; set; } = new();
        public List<FingerprintTemplate> Templates { get; set; } = new();
        public int CompletedSamples => Templates.Count;
        public bool IsComplete => CompletedSamples >= 5;
    }

    public class AttendanceInfo
    {
        public Student Student { get; set; } = new();
        public AttendanceRecord Record { get; set; } = new();
        public Event Event { get; set; } = new();
    }
}
