using Sudoku.Maui.Models;
using Sudoku.Maui.Services;
using System.IO;

namespace Sudoku.Maui.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly ISettingsService _settingsService;
        private GameSettings _currentSettings;
        private bool _isLoadingSettings; // Guard flag to prevent event handlers during initialization
        private static string? _logFilePath;

        public SettingsPage(ISettingsService settingsService)
        {
            try
            {
                InitializeLogging();
                LogMessage("=== SETTINGSPAGE CONSTRUCTOR START ===");
                
                LogMessage("Calling InitializeComponent...");
                InitializeComponent();
                LogMessage("InitializeComponent completed");
                
                _settingsService = settingsService;
                LogMessage("Loading current settings...");
                _currentSettings = _settingsService.LoadSettings();
                LogMessage($"Settings loaded - Theme: {_currentSettings.Theme}");
                
                LogMessage("=== SETTINGSPAGE CONSTRUCTOR END ===");
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in SettingsPage constructor: {ex}");
                throw;
            }
        }

        private static void InitializeLogging()
        {
            try
            {
                var appDataDir = FileSystem.AppDataDirectory;
                _logFilePath = Path.Combine(appDataDir, "settings_crash_log.txt");
                
                if (File.Exists(_logFilePath))
                    File.Delete(_logFilePath);
                
                File.WriteAllText(_logFilePath, $"SettingsPage Crash Log - {DateTime.Now}\n");
            }
            catch { }
        }

        private static void LogMessage(string message)
        {
            try
            {
                if (_logFilePath != null)
                {
                    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                    File.AppendAllText(_logFilePath, $"[{timestamp}] {message}\n");
                }
                System.Diagnostics.Debug.WriteLine($"[SettingsPage] {message}");
            }
            catch { }
        }

        protected override void OnAppearing()
        {
            try
            {
                LogMessage("=== OnAppearing START ===");
                base.OnAppearing();
                LogMessage("base.OnAppearing completed");
                
                LoadCurrentSettings();
                LogMessage("=== OnAppearing END ===");
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in OnAppearing: {ex}");
                throw;
            }
        }

        private void LoadCurrentSettings()
        {
            try
            {
                LogMessage("=== LoadCurrentSettings START ===");
                _isLoadingSettings = true;
                LogMessage("Guard flag set to true");

                LogMessage($"Setting ThemePicker.SelectedIndex to {(_currentSettings.Theme == AppTheme.Light ? 0 : 1)}...");
                ThemePicker.SelectedIndex = _currentSettings.Theme == AppTheme.Light ? 0 : 1;
                LogMessage("ThemePicker.SelectedIndex set");
                
                LogMessage($"Setting DifficultyPicker.SelectedIndex to {(int)_currentSettings.DefaultDifficulty}...");
                DifficultyPicker.SelectedIndex = (int)_currentSettings.DefaultDifficulty;
                LogMessage("DifficultyPicker.SelectedIndex set");
                
                LogMessage($"Setting ShowHintSwitch.IsToggled to {_currentSettings.ShowHintButton}...");
                ShowHintSwitch.IsToggled = _currentSettings.ShowHintButton;
                LogMessage("ShowHintSwitch.IsToggled set");
                
                LogMessage($"Setting ShowCheckSwitch.IsToggled to {_currentSettings.ShowCheckButton}...");
                ShowCheckSwitch.IsToggled = _currentSettings.ShowCheckButton;
                LogMessage("ShowCheckSwitch.IsToggled set");

                _isLoadingSettings = false;
                LogMessage("Guard flag cleared");
                LogMessage("=== LoadCurrentSettings END ===");
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in LoadCurrentSettings: {ex}");
                throw;
            }
        }

        private async void OnThemeChanged(object? sender, EventArgs e)
        {
            try
            {
                LogMessage($"OnThemeChanged fired - Guard: {_isLoadingSettings}, SelectedIndex: {ThemePicker.SelectedIndex}");
                
                if (_isLoadingSettings || ThemePicker.SelectedIndex == -1)
                {
                    LogMessage("OnThemeChanged skipped (guard or invalid index)");
                    return;
                }

                LogMessage("Processing theme change...");
                _currentSettings.Theme = ThemePicker.SelectedIndex == 0 ? AppTheme.Light : AppTheme.Dark;
                LogMessage($"New theme: {_currentSettings.Theme}");
                
                Application.Current!.UserAppTheme = _currentSettings.Theme;
                LogMessage("UserAppTheme set");
                
                if (Application.Current is App app)
                {
                    LogMessage("Calling app.LoadTheme...");
                    app.LoadTheme(_currentSettings.Theme);
                    LogMessage("app.LoadTheme completed");
                }
                
                LogMessage("Saving settings...");
                await _settingsService.SaveSettingsAsync(_currentSettings);
                LogMessage("Settings saved");
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in OnThemeChanged: {ex}");
            }
        }

        private async void OnDifficultyChanged(object? sender, EventArgs e)
        {
            if (_isLoadingSettings || DifficultyPicker.SelectedIndex == -1)
                return;

            _currentSettings.DefaultDifficulty = (DifficultyLevel)DifficultyPicker.SelectedIndex;
            await _settingsService.SaveSettingsAsync(_currentSettings);
        }

        private async void OnShowHintToggled(object? sender, ToggledEventArgs e)
        {
            if (_isLoadingSettings)
                return;

            _currentSettings.ShowHintButton = e.Value;
            await _settingsService.SaveSettingsAsync(_currentSettings);
        }

        private async void OnShowCheckToggled(object? sender, ToggledEventArgs e)
        {
            if (_isLoadingSettings)
                return;

            _currentSettings.ShowCheckButton = e.Value;
            await _settingsService.SaveSettingsAsync(_currentSettings);
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SudokuPage");
        }
    }
}
