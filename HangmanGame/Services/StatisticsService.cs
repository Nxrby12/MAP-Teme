using System;
using System.Collections.Generic;
using System.IO;
using HangmanGame.Models;
using Newtonsoft.Json;

namespace HangmanGame.Services
{
    public class StatisticsService
    {
        private readonly string _dataFolder;

        public StatisticsService(string dataFolder)
        {
            _dataFolder = dataFolder;
        }

        public UserStatistics GetStatistics(string userName)
        {
            string filePath = GetStatsFilePath(userName);
            if (!File.Exists(filePath))
                return new UserStatistics { UserName = userName };

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<UserStatistics>(json)
                    ?? new UserStatistics { UserName = userName };
            }
            catch
            {
                return new UserStatistics { UserName = userName };
            }
        }

        public void RecordGame(string userName, string category, bool won)
        {
            var stats = GetStatistics(userName);
            stats.TotalGamesPlayed++;
            if (won) stats.TotalGamesWon++;

            if (!stats.CategoryStats.ContainsKey(category))
                stats.CategoryStats[category] = new CategoryStats { Category = category };

            stats.CategoryStats[category].GamesPlayed++;
            if (won) stats.CategoryStats[category].GamesWon++;

            SaveStatistics(stats);
        }

        public List<UserStatistics> GetAllStatistics(IEnumerable<string> userNames)
        {
            var result = new List<UserStatistics>();
            foreach (var name in userNames)
                result.Add(GetStatistics(name));
            return result;
        }

        private void SaveStatistics(UserStatistics stats)
        {
            string filePath = GetStatsFilePath(stats.UserName);
            string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private string GetStatsFilePath(string userName)
        {
            return Path.Combine(_dataFolder, $"stats_{UserService.SanitizeFileName(userName)}.json");
        }
    }
}
