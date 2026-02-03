using System.Text.Json;
using FingerprintUI.Models;

namespace FingerprintUI.Services
{
    public class UserDatabase
    {
        private readonly string _dbPath;
        private List<EnrolledUser> _users;

        public UserDatabase()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "FingerprintUI");
            Directory.CreateDirectory(appFolder);
            _dbPath = Path.Combine(appFolder, "users.json");
            _users = LoadUsers();
        }

        private List<EnrolledUser> LoadUsers()
        {
            if (!File.Exists(_dbPath))
                return new List<EnrolledUser>();

            try
            {
                var json = File.ReadAllText(_dbPath);
                return JsonSerializer.Deserialize<List<EnrolledUser>>(json) ?? new List<EnrolledUser>();
            }
            catch
            {
                return new List<EnrolledUser>();
            }
        }

        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dbPath, json);
        }

        public void AddUser(EnrolledUser user)
        {
            // Remove existing user with same ID
            _users.RemoveAll(u => u.UserId == user.UserId);
            _users.Add(user);
            SaveUsers();
        }

        public EnrolledUser? GetUser(string userId)
        {
            return _users.FirstOrDefault(u => u.UserId == userId);
        }

        public List<EnrolledUser> GetAllUsers()
        {
            return _users.ToList();
        }

        public void DeleteUser(string userId)
        {
            _users.RemoveAll(u => u.UserId == userId);
            SaveUsers();
        }

        public int GetUserCount()
        {
            return _users.Count;
        }
    }
}
