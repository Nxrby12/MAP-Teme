using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HangmanGame.Models;
using Newtonsoft.Json;

namespace HangmanGame.Services
{
    public class UserService
    {
        private readonly string _dataFolder;
        private readonly string _usersFile;
        private readonly string _imagesFolder;
        private List<User> _users = new List<User>();

        public UserService()
        {
            _dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HangmanGame");
            _usersFile = Path.Combine(_dataFolder, "users.json");
            _imagesFolder = Path.Combine(_dataFolder, "images");

            Directory.CreateDirectory(_dataFolder);
            Directory.CreateDirectory(_imagesFolder);

            LoadUsers();
        }

        public IReadOnlyList<User> GetAllUsers() => _users.AsReadOnly();

        public User? GetUser(string name) =>
            _users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public bool UserExists(string name) =>
            _users.Any(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public string GetImagesFolder() => _imagesFolder;
        public string GetDataFolder() => _dataFolder;

        public (bool success, string error) AddUser(string name, string? sourceImagePath)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Name cannot be empty.");

            if (UserExists(name))
                return (false, $"User '{name}' already exists.");

            string imagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(sourceImagePath) && File.Exists(sourceImagePath))
            {
                string ext = Path.GetExtension(sourceImagePath);
                string destFileName = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                string destPath = Path.Combine(_imagesFolder, destFileName);
                File.Copy(sourceImagePath, destPath, true);
                imagePath = destPath;
            }

            var user = new User(name, imagePath);
            _users.Add(user);
            SaveUsers();
            return (true, string.Empty);
        }

        public bool DeleteUser(string name)
        {
            var user = GetUser(name);
            if (user == null) return false;

            // Delete user image
            if (!string.IsNullOrWhiteSpace(user.ImagePath) && File.Exists(user.ImagePath))
                File.Delete(user.ImagePath);

            // Delete saved games
            string savedGamesFolder = Path.Combine(_dataFolder, "saves");
            if (Directory.Exists(savedGamesFolder))
            {
                foreach (var file in Directory.GetFiles(savedGamesFolder, $"{SanitizeFileName(name)}_*.json"))
                    File.Delete(file);
            }

            // Delete statistics
            string statsFile = Path.Combine(_dataFolder, $"stats_{SanitizeFileName(name)}.json");
            if (File.Exists(statsFile))
                File.Delete(statsFile);

            _users.Remove(user);
            SaveUsers();
            return true;
        }

        private void LoadUsers()
        {
            if (!File.Exists(_usersFile))
            {
                _users = new List<User>();
                return;
            }
            try
            {
                string json = File.ReadAllText(_usersFile);
                _users = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            catch
            {
                _users = new List<User>();
            }
        }

        private void SaveUsers()
        {
            string json = JsonConvert.SerializeObject(_users, Formatting.Indented);
            File.WriteAllText(_usersFile, json);
        }

        public static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
