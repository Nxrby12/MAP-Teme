using System;
using HangmanGame.Commands;
using HangmanGame.Models;
using HangmanGame.Services;
using HangmanGame.ViewModels;

namespace HangmanGame.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly GameService _gameService;
        private readonly StatisticsService _statisticsService;

        private BaseViewModel _currentView;

        public BaseViewModel CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainViewModel()
        {
            _userService = new UserService();
            _gameService = new GameService(_userService.GetDataFolder());
            _statisticsService = new StatisticsService(_userService.GetDataFolder());

            _currentView = new LoginViewModel(_userService, OnLogin);
        }

        private void OnLogin(User user)
        {
            CurrentView = new GameViewModel(user, _userService, _gameService, _statisticsService, OnLogout);
        }

        private void OnLogout()
        {
            CurrentView = new LoginViewModel(_userService, OnLogin);
        }
    }
}
