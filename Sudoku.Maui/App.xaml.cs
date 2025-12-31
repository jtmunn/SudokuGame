using Microsoft.Extensions.DependencyInjection;
using Sudoku.Maui.Services;
using Sudoku.Maui.Pages;

namespace Sudoku.Maui
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private Window? _mainWindow;
        private System.Timers.Timer? _saveWindowSizeTimer;
        private const int SaveWindowSizeDelayMs = 500; // Debounce delay for saving window size
        private bool _shouldMaximizeOnCreated = false;
        
        // Track last non-maximized size so we can save it even when closing maximized
        private double _lastRestoredWidth = 800;
        private double _lastRestoredHeight = 800;
        
        // Track currently loaded theme to avoid unnecessary reloads
        private AppTheme _currentTheme = AppTheme.Unspecified;
        
#if WINDOWS
        // Cache maximized state to avoid querying it during app shutdown
        private bool _isMaximized = false;
#endif
        
        public App()
        {
            InitializeComponent();
            // App.xaml has default LightTheme loaded
            _currentTheme = AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Load and apply theme from settings FIRST, before creating window
            var settingsService = Handler?.MauiContext?.Services.GetService<ISettingsService>();
            if (settingsService != null)
            {
                try
                {
                    var settings = settingsService.LoadSettings();
                    UserAppTheme = settings.Theme;
                    LoadTheme(settings.Theme);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load theme in CreateWindow: {ex.Message}");
                    // Continue with default theme from App.xaml
                }
            }
            
            // NOW create the window - theme is already loaded
            var window = new Window(new AppShell())
            {
                Title = "Sudoku",
                MinimumWidth = 600,
                MinimumHeight = 700
            };
            
            // Restore saved window size or use defaults
            if (settingsService != null)
            {
                var settings = settingsService.LoadSettings();
                
                // If window was maximized, use restored size (or fall back to saved size)
                if (settings.IsMaximized == true)
                {
                    if (settings.RestoredWidth.HasValue && settings.RestoredHeight.HasValue)
                    {
                        window.Width = settings.RestoredWidth.Value;
                        window.Height = settings.RestoredHeight.Value;
                        _lastRestoredWidth = settings.RestoredWidth.Value;
                        _lastRestoredHeight = settings.RestoredHeight.Value;
                    }
                    else if (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
                    {
                        window.Width = settings.WindowWidth.Value;
                        window.Height = settings.WindowHeight.Value;
                        _lastRestoredWidth = settings.WindowWidth.Value;
                        _lastRestoredHeight = settings.WindowHeight.Value;
                    }
                    else
                    {
                        window.Width = 800;
                        window.Height = 800;
                    }
                }
                else
                {
                    // Not maximized - use current saved size
                    if (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
                    {
                        window.Width = settings.WindowWidth.Value;
                        window.Height = settings.WindowHeight.Value;
                        _lastRestoredWidth = settings.WindowWidth.Value;
                        _lastRestoredHeight = settings.WindowHeight.Value;
                    }
                    else
                    {
                        window.Width = 800;
                        window.Height = 800;
                    }
                }
                
                // Center window on screen
                CenterWindowOnScreen(window);
                
                // Check if we should maximize - do it after window is fully created
                if (settings.IsMaximized == true)
                {
                    _shouldMaximizeOnCreated = true;
#if WINDOWS
                    _isMaximized = true;
#endif
                    window.Created += OnWindowCreated;
                }
            }
            else
            {
                window.Width = 800;
                window.Height = 800;
                CenterWindowOnScreen(window);
            }
            
            _mainWindow = window;
            
            // Subscribe to window size changes
            window.SizeChanged += OnWindowSizeChanged;
            
            // Subscribe to window creation to set up state tracking
            window.Created += OnWindowCreated;
            
            // Subscribe to window destruction for cleanup
            window.Destroying += OnWindowDestroying;
            
            return window;
        }

        private void OnWindowCreated(object? sender, EventArgs e)
        {
            if (_shouldMaximizeOnCreated && sender is Window window)
            {
                window.Created -= OnWindowCreated;
                
                window.Dispatcher.Dispatch(() =>
                {
                    RestoreMaximizedState(window);
                });
            }
            
#if WINDOWS
            if (sender is Window win && win.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            {
                var appWindow = GetAppWindow(nativeWindow);
                if (appWindow != null)
                {
                    appWindow.Changed += OnAppWindowChanged;
                }
            }
#endif
        }

        private void OnWindowDestroying(object? sender, EventArgs e)
        {
            // Save game state when app is closing
            try
            {
                var services = Handler?.MauiContext?.Services;
                if (services == null)
                    return;

                var gameStateService = services.GetService<IGameStateService>();
                if (gameStateService == null)
                    return;
                
                // Note: Don't try to access Shell.Current or CurrentPage during window destruction
                // The window is already being torn down and UI elements may be deactivated
                // Game state should have been saved by SudokuPage.OnDisappearing() already
                
                System.Diagnostics.Debug.WriteLine("OnWindowDestroying: Window closing, game state already saved by page lifecycle");
            }
            catch (Exception ex)
            {
                // Fail silently during shutdown to avoid crash
                System.Diagnostics.Debug.WriteLine($"OnWindowDestroying: Error during cleanup: {ex.Message}");
            }
        }

#if WINDOWS
        private void OnAppWindowChanged(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange || args.DidSizeChange)
            {
                if (sender.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    bool isMax = presenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized;
                    bool stateChanged = _isMaximized != isMax;
                    
                    if (stateChanged)
                    {
                        _isMaximized = isMax;
                        SaveWindowSize();
                    }
                }
            }
        }
#endif

        public void LoadTheme(AppTheme theme)
        {
            // Skip if already loaded
            if (_currentTheme == theme)
            {
                System.Diagnostics.Debug.WriteLine($"Theme {theme} already loaded, skipping reload");
                return;
            }

            try
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
                    
                    _currentTheme = theme;
                    System.Diagnostics.Debug.WriteLine($"Successfully loaded theme: {theme}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme {theme}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't crash - keep current theme
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
            catch (Exception)
            {
                // If centering fails, let the OS handle window placement
            }
        }
        
        private void RestoreMaximizedState(Window window)
        {
#if WINDOWS
            try
            {
                if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
                {
                    var appWindow = GetAppWindow(nativeWindow);
                    if (appWindow != null)
                    {
                        var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
                        presenter?.Maximize();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore maximized state: {ex.Message}");
            }
#endif
        }

#if WINDOWS
        private Microsoft.UI.Windowing.AppWindow? GetAppWindow(Microsoft.UI.Xaml.Window window)
        {
            try
            {
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            }
            catch (Exception)
            {
                return null;
            }
        }
#endif
        
        private void OnWindowSizeChanged(object? sender, EventArgs e)
        {
            // Debounce: Reset timer on each size change
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
            {
                return;
            }
                
            var settingsService = Handler?.MauiContext?.Services.GetService<ISettingsService>();
            if (settingsService is null)
            {
                return;
            }
                
            try
            {
                var settings = settingsService.LoadSettings();
                
                settings.WindowWidth = _mainWindow.Width;
                settings.WindowHeight = _mainWindow.Height;
                
#if WINDOWS
                settings.IsMaximized = _isMaximized;
                
                if (!_isMaximized)
                {
                    _lastRestoredWidth = _mainWindow.Width;
                    _lastRestoredHeight = _mainWindow.Height;
                    settings.RestoredWidth = _lastRestoredWidth;
                    settings.RestoredHeight = _lastRestoredHeight;
                }
                else
                {
                    settings.RestoredWidth = _lastRestoredWidth;
                    settings.RestoredHeight = _lastRestoredHeight;
                }
#else
                settings.IsMaximized = false;
                _lastRestoredWidth = _mainWindow.Width;
                _lastRestoredHeight = _mainWindow.Height;
                settings.RestoredWidth = _lastRestoredWidth;
                settings.RestoredHeight = _lastRestoredHeight;
#endif
                
                _ = settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveWindowSize exception: {ex.Message}");
            }
        }
    }
}