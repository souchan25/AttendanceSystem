namespace AttendanceWeb.Services
{
    public class AuthService
    {
        private bool _isAdminAuthenticated = false;
        private string _adminUsername = "";
        
        public event Action? OnAuthStateChanged;

        public bool IsAdminAuthenticated => _isAdminAuthenticated;
        public string AdminUsername => _adminUsername;

        public void Login(string username)
        {
            _isAdminAuthenticated = true;
            _adminUsername = username;
            NotifyAuthStateChanged();
        }

        public void Logout()
        {
            _isAdminAuthenticated = false;
            _adminUsername = "";
            NotifyAuthStateChanged();
        }

        private void NotifyAuthStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }
    }
}
