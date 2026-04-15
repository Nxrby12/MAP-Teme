using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class UserStatistics
    {
        public string UserName { get; set; } = string.Empty;
        public int TotalGamesPlayed { get; set; } = 0;
        public int TotalGamesWon { get; set; } = 0;
        public Dictionary<string, CategoryStats> CategoryStats { get; set; } = new Dictionary<string, CategoryStats>();
    }

    public class CategoryStats
    {
        public string Category { get; set; } = string.Empty;
        public int GamesPlayed { get; set; } = 0;
        public int GamesWon { get; set; } = 0;
    }
}
