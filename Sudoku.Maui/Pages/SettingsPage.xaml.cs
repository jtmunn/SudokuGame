using Sudoku.Maui.Models;
using Sudoku.Maui.Services;

namespace Sudoku.Maui.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly ISettingsService _settingsService;
        private GameSettings _currentSettings;
        private bool _isLoadingSettings; // Guard flag to prevent event handlers during initialization

        public SettingsPage(ISettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _currentSettings = _settingsService.LoadSettings();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            _isLoadingSettings = true; // Set guard flag

            ThemePicker.SelectedIndex = _currentSettings.Theme == AppTheme.Light ? 0 : 1;
            ShowHintSwitch.IsToggled = _currentSettings.ShowHintButton;
            ShowCheckSwitch.IsToggled = _currentSettings.ShowCheckButton;

            _isLoadingSettings = false; // Clear guard flag
        }

        private async void OnThemeChanged(object? sender, EventArgs e)
        {
            if (_isLoadingSettings || ThemePicker.SelectedIndex == -1) // Check guard flag
                return;

            _currentSettings.Theme = ThemePicker.SelectedIndex == 0 ? AppTheme.Light : AppTheme.Dark;
            
            // Set UserAppTheme AND load the theme dictionary
            Application.Current!.UserAppTheme = _currentSettings.Theme;
            
            if (Application.Current is App app)
            {
                app.LoadTheme(_currentSettings.Theme);
            }
            
            await _settingsService.SaveSettingsAsync(_currentSettings);
        }

        private async void OnShowHintToggled(object? sender, ToggledEventArgs e)
        {
            if (_isLoadingSettings) // Check guard flag
                return;

            _currentSettings.ShowHintButton = e.Value;
            await _settingsService.SaveSettingsAsync(_currentSettings);
        }

        private async void OnShowCheckToggled(object? sender, ToggledEventArgs e)
        {
            if (_isLoadingSettings) // Check guard flag
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
