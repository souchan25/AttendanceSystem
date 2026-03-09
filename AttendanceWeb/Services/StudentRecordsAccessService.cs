namespace AttendanceWeb.Services
{
    public class StudentRecordsAccessService
    {
        private int? _authorizedStudentId;
        private DateTime _authorizedUntilUtc = DateTime.MinValue;

        public void GrantAccess(int studentId, TimeSpan? duration = null)
        {
            _authorizedStudentId = studentId;
            _authorizedUntilUtc = DateTime.UtcNow.Add(duration ?? TimeSpan.FromMinutes(2));
        }

        public bool HasAccess(int studentId)
        {
            return _authorizedStudentId == studentId && DateTime.UtcNow <= _authorizedUntilUtc;
        }

        public void ClearAccess()
        {
            _authorizedStudentId = null;
            _authorizedUntilUtc = DateTime.MinValue;
        }
    }
}
