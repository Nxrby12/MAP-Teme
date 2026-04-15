using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using HangmanGame.Commands;
using HangmanGame.Models;
using HangmanGame.Services;

namespace HangmanGame.ViewModels
{
    public class LetterButton : BaseViewModel
    {
        private bool _isGuessed;
        private bool _isCorrect;

        public char Letter { get; set; }

        public bool IsGuessed
        {
            get => _isGuessed;
            set => SetProperty(ref _isGuessed, value);
        }

        public bool IsCorrect
        {
            get => _isCorrect;
            set => SetProperty(ref _isCorrect, value);
        }
    }

    public class GameViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly GameService _gameService;
        private readonly StatisticsService _statisticsService;
        private readonly Action _onLogout;
        private readonly DispatcherTimer _timer;

        private User _currentUser;
        private string _currentCategory = string.Empty;
        private string _currentWord = string.Empty;
        private List<char> _guessedLetters = new List<char>();
        private int _wrongGuesses = 0;
        private int _remainingSeconds = 30;
        private string _displayWord = string.Empty;
        private string _gameMessage = string.Empty;
        private bool _isGameActive = false;
        private bool _isGameOver = false;
        private string _selectedCategory = string.Empty;

        public const int MaxWrongGuesses = 6;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string CurrentCategory
        {
            get => _currentCategory;
            set => SetProperty(ref _currentCategory, value);
        }

        public string DisplayWord
        {
            get => _displayWord;
            private set => SetProperty(ref _displayWord, value);
        }

        public int WrongGuesses
        {
            get => _wrongGuesses;
            private set => SetProperty(ref _wrongGuesses, value);
        }

        public int RemainingSeconds
        {
            get => _remainingSeconds;
            private set => SetProperty(ref _remainingSeconds, value);
        }

        public string GameMessage
        {
            get => _gameMessage;
            set => SetProperty(ref _gameMessage, value);
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            set => SetProperty(ref _isGameActive, value);
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            set => SetProperty(ref _isGameOver, value);
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<LetterButton> Alphabet { get; } = new ObservableCollection<LetterButton>();
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        public ICommand NewGameCommand { get; }
        public ICommand SaveGameCommand { get; }
        public ICommand OpenGameCommand { get; }
        public ICommand ShowStatisticsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GuessLetterCommand { get; }

        public GameViewModel(User user, UserService userService, GameService gameService,
            StatisticsService statisticsService, Action onLogout)
        {
            _currentUser = user;
            _userService = userService;
            _gameService = gameService;
            _statisticsService = statisticsService;
            _onLogout = onLogout;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;

            NewGameCommand = new RelayCommand(StartNewGame);
            SaveGameCommand = new RelayCommand(SaveGame, () => IsGameActive);
            OpenGameCommand = new RelayCommand(OpenGame);
            ShowStatisticsCommand = new RelayCommand(ShowStatistics);
            LogoutCommand = new RelayCommand(Logout);
            GuessLetterCommand = new RelayCommand(o => GuessLetter((char)o!), o => IsGameActive && !IsGameOver);

            InitializeAlphabet();
            LoadCategories();
        }

        private void InitializeAlphabet()
        {
            Alphabet.Clear();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                Alphabet.Add(new LetterButton { Letter = c });
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();
            foreach (var cat in _gameService.GetCategories())
                Categories.Add(cat.Name);

            SelectedCategory = Categories.FirstOrDefault() ?? string.Empty;
        }

        private void StartNewGame()
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory))
            {
                MessageBox.Show("Please select a category first.", "No Category", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _timer.Stop();
            InitializeAlphabet();

            _currentWord = _gameService.GetRandomWord(SelectedCategory);
            _guessedLetters = new List<char>();
            WrongGuesses = 0;
            RemainingSeconds = 30;
            CurrentCategory = SelectedCategory;
            IsGameActive = true;
            IsGameOver = false;
            GameMessage = string.Empty;

            UpdateDisplayWord();
            _timer.Start();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            RemainingSeconds--;
            if (RemainingSeconds <= 0)
            {
                _timer.Stop();
                EndGame(false, "Time's up! The word was: " + _currentWord);
            }
        }

        private void GuessLetter(char letter)
        {
            if (_guessedLetters.Contains(letter)) return;

            _guessedLetters.Add(letter);
            var btn = Alphabet.FirstOrDefault(b => b.Letter == letter);

            if (_currentWord.Contains(letter))
            {
                if (btn != null) { btn.IsGuessed = true; btn.IsCorrect = true; }
                UpdateDisplayWord();

                if (!_displayWord.Contains('_'))
                {
                    _timer.Stop();
                    EndGame(true, $"Congratulations! You guessed the word: {_currentWord}");
                }
            }
            else
            {
                if (btn != null) { btn.IsGuessed = true; btn.IsCorrect = false; }
                WrongGuesses++;

                if (WrongGuesses >= MaxWrongGuesses)
                {
                    _timer.Stop();
                    EndGame(false, $"Game over! The word was: {_currentWord}");
                }
            }
        }

        private void UpdateDisplayWord()
        {
            var chars = _currentWord.Select(c =>
                _guessedLetters.Contains(c) ? c : '_').ToArray();
            DisplayWord = string.Join(" ", chars);
        }

        private void EndGame(bool won, string message)
        {
            IsGameActive = false;
            IsGameOver = true;
            GameMessage = message;
            _statisticsService.RecordGame(_currentUser.Name, CurrentCategory, won);
        }

        private void SaveGame()
        {
            var state = new GameState
            {
                UserName = _currentUser.Name,
                Category = CurrentCategory,
                Word = _currentWord,
                GuessedLetters = new List<char>(_guessedLetters),
                WrongGuesses = WrongGuesses,
                RemainingSeconds = RemainingSeconds,
                SavedAt = DateTime.Now,
                IsCompleted = false,
                IsWon = false
            };

            _timer.Stop();
            _gameService.SaveGame(state);
            MessageBox.Show("Game saved successfully!", "Save Game", MessageBoxButton.OK, MessageBoxImage.Information);
            _timer.Start();
        }

        private void OpenGame()
        {
            var savedGames = _gameService.GetSavedGames(_currentUser.Name);
            if (savedGames.Count == 0)
            {
                MessageBox.Show("No saved games found.", "Open Game", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Views.LoadGameDialog(savedGames);
            if (dialog.ShowDialog() == true && dialog.SelectedGame != null)
            {
                LoadGameState(dialog.SelectedGame);
            }
        }

        private void LoadGameState(GameState state)
        {
            _timer.Stop();
            InitializeAlphabet();

            _currentWord = state.Word;
            _guessedLetters = new List<char>(state.GuessedLetters);
            CurrentCategory = state.Category;
            WrongGuesses = state.WrongGuesses;
            RemainingSeconds = state.RemainingSeconds;
            IsGameActive = true;
            IsGameOver = false;
            GameMessage = string.Empty;

            // Mark guessed letters
            foreach (var letter in _guessedLetters)
            {
                var btn = Alphabet.FirstOrDefault(b => b.Letter == letter);
                if (btn != null)
                {
                    btn.IsGuessed = true;
                    btn.IsCorrect = _currentWord.Contains(letter);
                }
            }

            UpdateDisplayWord();
            _timer.Start();
        }

        private void ShowStatistics()
        {
            var allUsers = _userService.GetAllUsers().Select(u => u.Name).ToList();
            var dialog = new Views.StatisticsDialog(_statisticsService, allUsers);
            dialog.ShowDialog();
        }

        private void Logout()
        {
            _timer.Stop();
            _onLogout();
        }
    }
}
