using System.Collections.Generic;

namespace HangmanGame.Models
{
    public class WordCategory
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Words { get; set; } = new List<string>();

        public WordCategory() { }

        public WordCategory(string name, List<string> words)
        {
            Name = name;
            Words = words;
        }
    }
}
