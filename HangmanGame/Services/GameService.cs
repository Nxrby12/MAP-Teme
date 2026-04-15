using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HangmanGame.Models;
using Newtonsoft.Json;

namespace HangmanGame.Services
{
    public class GameService
    {
        private readonly string _savesFolder;
        private readonly List<WordCategory> _categories;

        public GameService(string dataFolder)
        {
            _savesFolder = Path.Combine(dataFolder, "saves");
            Directory.CreateDirectory(_savesFolder);
            _categories = InitializeCategories();
        }

        public IReadOnlyList<WordCategory> GetCategories() => _categories.AsReadOnly();

        public WordCategory? GetCategory(string name) =>
            _categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public string GetRandomWord(string categoryName)
        {
            var category = GetCategory(categoryName);
            if (category == null || category.Words.Count == 0)
                return "HANGMAN";

            var rng = new Random();
            return category.Words[rng.Next(category.Words.Count)].ToUpperInvariant();
        }

        public void SaveGame(GameState state)
        {
            string fileName = $"{UserService.SanitizeFileName(state.UserName)}_{DateTime.Now:yyyyMMddHHmmss}.json";
            string filePath = Path.Combine(_savesFolder, fileName);
            string json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public List<GameState> GetSavedGames(string userName)
        {
            var result = new List<GameState>();
            string prefix = UserService.SanitizeFileName(userName) + "_";

            foreach (var file in Directory.GetFiles(_savesFolder, $"{prefix}*.json")
                         .OrderByDescending(f => f))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var state = JsonConvert.DeserializeObject<GameState>(json);
                    if (state != null && !state.IsCompleted)
                        result.Add(state);
                }
                catch { /* skip corrupt files */ }
            }
            return result;
        }

        public void DeleteSave(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private List<WordCategory> InitializeCategories()
        {
            return new List<WordCategory>
            {
                new WordCategory("Animals", new List<string>
                {
                    "ELEPHANT", "GIRAFFE", "PENGUIN", "DOLPHIN", "KANGAROO",
                    "CHEETAH", "CROCODILE", "FLAMINGO", "JAGUAR", "OCTOPUS",
                    "RHINOCEROS", "CHIMPANZEE", "HAMSTER", "PLATYPUS", "WOLVERINE"
                }),
                new WordCategory("Countries", new List<string>
                {
                    "ROMANIA", "FRANCE", "GERMANY", "ITALY", "SPAIN",
                    "PORTUGAL", "BRAZIL", "ARGENTINA", "AUSTRALIA", "JAPAN",
                    "MEXICO", "CANADA", "SWEDEN", "NORWAY", "SWITZERLAND"
                }),
                new WordCategory("Sports", new List<string>
                {
                    "FOOTBALL", "BASKETBALL", "VOLLEYBALL", "SWIMMING", "GYMNASTICS",
                    "BADMINTON", "BASEBALL", "CRICKET", "ARCHERY", "WEIGHTLIFTING",
                    "SKATEBOARD", "SNOWBOARD", "WRESTLING", "FENCING", "TRIATHLON"
                }),
                new WordCategory("Technology", new List<string>
                {
                    "COMPUTER", "KEYBOARD", "SOFTWARE", "INTERNET", "DATABASE",
                    "ALGORITHM", "PROGRAMMING", "BLUETOOTH", "PROCESSOR", "FRAMEWORK",
                    "ENCRYPTION", "BANDWIDTH", "FIREWALL", "COMPILER", "DEBUGGING"
                }),
                new WordCategory("Movies", new List<string>
                {
                    "INCEPTION", "GLADIATOR", "TITANIC", "AVATAR", "INTERSTELLAR",
                    "PARASITE", "JOKER", "MATRIX", "TERMINATOR", "JURASSIC",
                    "GRAVITY", "MEMENTO", "SPOTLIGHT", "BRAVEHEART", "CASABLANCA"
                })
            };
        }
    }
}
