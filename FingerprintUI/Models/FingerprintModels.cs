namespace FingerprintUI.Models
{
    public class FingerprintData
    {
        public string? UserId { get; set; }
        public string? Template { get; set; }
    }

    public class FingerprintResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? Template { get; set; }
        public string? ImageData { get; set; }
        public int Quality { get; set; }
        public string? Error { get; set; }
    }

    public class EnrolledUser
    {
        public string UserId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Template { get; set; } = "";
        public DateTime EnrolledDate { get; set; }
    }
}
