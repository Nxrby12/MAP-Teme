using System.Collections.Generic;
using System.Windows;
using HangmanGame.Models;

namespace HangmanGame.Views
{
    public partial class LoadGameDialog : Window
    {
        public GameState? SelectedGame { get; private set; }

        public LoadGameDialog(List<GameState> savedGames)
        {
            InitializeComponent();
            SavedGamesList.ItemsSource = savedGames;
            if (savedGames.Count > 0)
                SavedGamesList.SelectedIndex = 0;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGame = SavedGamesList.SelectedItem as GameState;
            if (SelectedGame == null)
            {
                MessageBox.Show("Please select a saved game.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
