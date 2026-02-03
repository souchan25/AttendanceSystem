namespace FingerprintMiddleware.Models
{
    public class FingerprintData
    {
        public string Action { get; set; } = "";
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

    public class DeviceInfo
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsConnected { get; set; }
    }
}