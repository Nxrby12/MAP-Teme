using System;

namespace HangmanGame.Models
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User() { }

        public User(string name, string imagePath)
        {
            Name = name;
            ImagePath = imagePath;
            CreatedAt = DateTime.Now;
        }
    }
}
