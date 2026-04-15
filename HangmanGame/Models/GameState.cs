using System;
using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class GameState
    {
        public string UserName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Word { get; set; } = string.Empty;
        public List<char> GuessedLetters { get; set; } = new List<char>();
        public int WrongGuesses { get; set; } = 0;
        public int RemainingSeconds { get; set; } = 30;
        public DateTime SavedAt { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; } = false;
        public bool IsWon { get; set; } = false;
    }
}
