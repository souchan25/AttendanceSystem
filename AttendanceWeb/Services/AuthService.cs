namespace AttendanceWeb.Services
{
    public class AuthService
    {
        private bool _isAdminAuthenticated = false;
        private string _adminUsername = "";
        private int _adminId = 0;
        
        public event Action? OnAuthStateChanged;

        public bool IsAdminAuthenticated => _isAdminAuthenticated;
        public string AdminUsername => _adminUsername;
        public int AdminId => _adminId;

        public void Login(string username, int adminId = 0)
        {
            _isAdminAuthenticated = true;
            _adminUsername = username;
            _adminId = adminId;
            NotifyAuthStateChanged();
        }

        public void Logout()
        {
            _isAdminAuthenticated = false;
            _adminUsername = "";
            _adminId = 0;
            NotifyAuthStateChanged();
        }

        private void NotifyAuthStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }
    }
}
