using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HangmanGame.Commands;
using HangmanGame.Models;
using HangmanGame.Services;
using Microsoft.Win32;

namespace HangmanGame.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly Action<User> _onLogin;

        private string _newUserName = string.Empty;
        private string? _selectedImagePath;
        private User? _selectedUser;
        private string _statusMessage = string.Empty;
        private bool _isError;

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        public string NewUserName
        {
            get => _newUserName;
            set => SetProperty(ref _newUserName, value);
        }

        public string? SelectedImagePath
        {
            get => _selectedImagePath;
            set => SetProperty(ref _selectedImagePath, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public ICommand BrowseImageCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public LoginViewModel(UserService userService, Action<User> onLogin)
        {
            _userService = userService;
            _onLogin = onLogin;

            BrowseImageCommand = new RelayCommand(BrowseImage);
            CreateUserCommand = new RelayCommand(CreateUser);
            LoginCommand = new RelayCommand(Login, CanLogin);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanDeleteUser);

            RefreshUsers();
        }

        private void RefreshUsers()
        {
            Users.Clear();
            foreach (var user in _userService.GetAllUsers())
                Users.Add(user);
        }

        private void BrowseImage()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Profile Picture",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
                SelectedImagePath = dialog.FileName;
        }

        private void CreateUser()
        {
            var (success, error) = _userService.AddUser(NewUserName.Trim(), SelectedImagePath);
            if (success)
            {
                SetStatus("User created successfully!", false);
                NewUserName = string.Empty;
                SelectedImagePath = null;
                RefreshUsers();
            }
            else
            {
                SetStatus(error, true);
            }
        }

        private bool CanLogin() => SelectedUser != null;

        private void Login()
        {
            if (SelectedUser != null)
                _onLogin(SelectedUser);
        }

        private bool CanDeleteUser() => SelectedUser != null;

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{SelectedUser.Name}'?\nThis will also remove all saved games and statistics.",
                "Delete User",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _userService.DeleteUser(SelectedUser.Name);
                SelectedUser = null;
                RefreshUsers();
                SetStatus("User deleted.", false);
            }
        }

        private void SetStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsError = isError;
        }
    }
}
