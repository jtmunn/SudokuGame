using Microsoft.Extensions.DependencyInjection;
using Sudoku.Maui.Services;

namespace Sudoku.Maui
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        // Window sizing constants
        private const int MinGridSize = 360; // Minimum size for the Sudoku grid
        private const double BaseGridSize = 450.0; // Reference grid size for calculations
        private const double BaseButtonSize = 45.0; // Base size for action buttons
        private const int ActionButtonMargin = 20; // Left margin for action buttons
        private const int GameAreaPadding = 10; // Padding around game area (per side)
        private const int NumberButtonMargin = 6; // Margin around each number button
        private const int MinSpacerWidth = 50; // Minimum width for centering spacers
        private const int MinWindowHeight = 700; // Minimum window height
        
        private Window? _mainWindow;
        private System.Timers.Timer? _saveWindowSizeTimer;
        private const int SaveWindowSizeDelayMs = 500; // Debounce delay for saving window size
        
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Load and apply theme from settings FIRST, before creating window
            var settingsService = Handler?.MauiContext?.Services.GetService<ISettingsService>();
            if (settingsService != null)
            {
                var settings = settingsService.LoadSettings();
                UserAppTheme = settings.Theme;
                LoadTheme(settings.Theme);
            }
            
            // NOW create the window - theme is already loaded
            var window = new Window(new AppShell())
            {
                Title = "Sudoku",
                MinimumWidth = 600,
                MinimumHeight = 700
                // No MaximumWidth or MaximumHeight - allow unlimited
            };
            
            // Restore saved window size or use defaults
            if (settingsService != null)
            {
                var settings = settingsService.LoadSettings();
                
                if (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
                {
                    window.Width = settings.WindowWidth.Value;
                    window.Height = settings.WindowHeight.Value;
                }
                else
                {
                    // Default size
                    window.Width = 800;
                    window.Height = 800;
                }
            }
            else
            {
                // Default size if settings service not available
                window.Width = 800;
                window.Height = 800;
            }
            
            // Center window on screen
            CenterWindowOnScreen(window);
            
            _mainWindow = window;
            
            // Subscribe to window size changes
            window.SizeChanged += OnWindowSizeChanged;
            
            return window;
        }

        private Color? GetThemeColor(string key)
        {
            try
            {
                if (Application.Current?.Resources != null)
                {
                    // Search through merged dictionaries
                    foreach (var dict in Application.Current.Resources.MergedDictionaries)
                    {
                        if (dict.ContainsKey(key))
                            return (Color)dict[key];
                    }
                }
            }
            catch
            {
                // Resource not available
            }
            return null;
        }
        
        public void LoadTheme(AppTheme theme)
        {
            // Official Microsoft pattern from documentation
            ICollection<ResourceDictionary> mergedDictionaries = Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                // Remove only the theme dictionary, keep Colors.xaml and Styles.xaml
                var themeDict = mergedDictionaries.FirstOrDefault(d => 
                    d.GetType().Name == "LightTheme" || d.GetType().Name == "DarkTheme");
                
                if (themeDict != null)
                {
                    mergedDictionaries.Remove(themeDict);
                }
                
                // Add the selected theme by instantiating the class
                if (theme == AppTheme.Dark)
                {
                    mergedDictionaries.Add(new Resources.Styles.Themes.DarkTheme());
                }
                else
                {
                    mergedDictionaries.Add(new Resources.Styles.Themes.LightTheme());
                }
            }
        }
        
        private void CenterWindowOnScreen(Window window)
        {
            try
            {
                // Get the main display info
                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
                
                // Calculate center position
                // Note: Density is the scaling factor (e.g., 1.5 for 150% scaling)
                var screenWidth = displayInfo.Width / displayInfo.Density;
                var screenHeight = displayInfo.Height / displayInfo.Density;
                
                var x = (screenWidth - window.Width) / 2;
                var y = (screenHeight - window.Height) / 2;
                
                // Ensure window is not positioned off-screen
                window.X = Math.Max(0, x);
                window.Y = Math.Max(0, y);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to center window: {ex.Message}");
                // If centering fails, let the OS handle window placement
            }
        }
        
        private void OnWindowSizeChanged(object? sender, EventArgs e)
        {
            // Debounce: Reset timer on each size change
            // This ensures we only save after the user stops resizing
            _saveWindowSizeTimer?.Stop();
            _saveWindowSizeTimer?.Dispose();
            
            _saveWindowSizeTimer = new System.Timers.Timer(SaveWindowSizeDelayMs);
            _saveWindowSizeTimer.AutoReset = false; // Only fire once
            _saveWindowSizeTimer.Elapsed += (s, args) =>
            {
                SaveWindowSize();
            };
            _saveWindowSizeTimer.Start();
        }
        
        private void SaveWindowSize()
        {
            if (_mainWindow is null)
                return;
                
            var settingsService = Handler?.MauiContext?.Services.GetService<ISettingsService>();
            if (settingsService is null)
                return;
                
            try
            {
                var settings = settingsService.LoadSettings();
                
                // Save current window size
                settings.WindowWidth = _mainWindow.Width;
                settings.WindowHeight = _mainWindow.Height;
                
                // Save asynchronously (we're debouncing, so no rush)
                _ = settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save window size: {ex.Message}");
            }
        }
    }
}