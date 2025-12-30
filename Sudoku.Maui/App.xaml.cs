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
        private bool _shouldMaximizeOnCreated = false;
        
        // Track last non-maximized size so we can save it even when closing maximized
        private double _lastRestoredWidth = 800;
        private double _lastRestoredHeight = 800;
        
#if WINDOWS
        // Cache maximized state to avoid querying it during app shutdown
        private bool _isMaximized = false;
#endif
        
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
                
                System.Diagnostics.Debug.WriteLine($"Loaded settings - IsMaximized: {settings.IsMaximized}");
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
                    // Set to restored size first so Windows knows what size to restore to
                    if (settings.RestoredWidth.HasValue && settings.RestoredHeight.HasValue)
                    {
                        window.Width = settings.RestoredWidth.Value;
                        window.Height = settings.RestoredHeight.Value;
                        _lastRestoredWidth = settings.RestoredWidth.Value;
                        _lastRestoredHeight = settings.RestoredHeight.Value;
                        System.Diagnostics.Debug.WriteLine($"Setting restored size: {window.Width}x{window.Height}");
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
                    _isMaximized = true; // Pre-set cached state
#endif
                    window.Created += OnWindowCreated;
                    System.Diagnostics.Debug.WriteLine("Will maximize window after creation");
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
            
            return window;
        }

        private void OnWindowCreated(object? sender, EventArgs e)
        {
            if (_shouldMaximizeOnCreated && sender is Window window)
            {
                System.Diagnostics.Debug.WriteLine("Window.Created event fired - attempting to maximize");
                window.Created -= OnWindowCreated;
                
                // Use Dispatcher to ensure window is fully ready
                window.Dispatcher.Dispatch(() =>
                {
                    RestoreMaximizedState(window);
                });
            }
            
#if WINDOWS
            // Subscribe to window state changes to track maximized/restored
            if (sender is Window win && win.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            {
                var appWindow = GetAppWindow(nativeWindow);
                if (appWindow != null)
                {
                    appWindow.Changed += OnAppWindowChanged;
                    System.Diagnostics.Debug.WriteLine("Subscribed to AppWindow.Changed event");
                }
            }
#endif
        }

#if WINDOWS
        private void OnAppWindowChanged(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            // Check if the window size or presenter state changed
            if (args.DidPresenterChange || args.DidSizeChange)
            {
                if (sender.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    bool isMax = presenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Maximized;
                    bool stateChanged = _isMaximized != isMax;
                    
                    System.Diagnostics.Debug.WriteLine($"OnAppWindowChanged: IsMaximized = {isMax}, StateChanged = {stateChanged}, DidPresenterChange = {args.DidPresenterChange}, DidSizeChange = {args.DidSizeChange}");
                    
                    if (stateChanged)
                    {
                        // Update cached state immediately
                        _isMaximized = isMax;
                        System.Diagnostics.Debug.WriteLine($"OnAppWindowChanged: Updated _isMaximized to {_isMaximized}");
                        
                        // Trigger save after state change
                        SaveWindowSize();
                    }
                }
            }
        }
#endif

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
        
        private void RestoreMaximizedState(Window window)
        {
#if WINDOWS
            try
            {
                System.Diagnostics.Debug.WriteLine("RestoreMaximizedState called");
                
                if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
                {
                    System.Diagnostics.Debug.WriteLine("Got native window");
                    
                    var appWindow = GetAppWindow(nativeWindow);
                    if (appWindow != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Got AppWindow");
                        
                        var presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
                        if (presenter != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Current state: {presenter.State}");
                            presenter.Maximize();
                            System.Diagnostics.Debug.WriteLine($"After maximize: {presenter.State}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Presenter is null or not OverlappedPresenter");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("AppWindow is null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PlatformView is not Microsoft.UI.Xaml.Window");
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAppWindow exception: {ex.Message}");
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
                
                // Always save current window size
                settings.WindowWidth = _mainWindow.Width;
                settings.WindowHeight = _mainWindow.Height;
                
#if WINDOWS
                // Use cached maximized state (updated by OnAppWindowChanged)
                settings.IsMaximized = _isMaximized;
                System.Diagnostics.Debug.WriteLine($"SaveWindowSize: Saving IsMaximized = {settings.IsMaximized}");
                
                // Update restored size based on maximization state
                if (!_isMaximized)
                {
                    // Window is not maximized - save current size as restored size
                    _lastRestoredWidth = _mainWindow.Width;
                    _lastRestoredHeight = _mainWindow.Height;
                    settings.RestoredWidth = _lastRestoredWidth;
                    settings.RestoredHeight = _lastRestoredHeight;
                    System.Diagnostics.Debug.WriteLine($"Saved restored size: {_lastRestoredWidth}x{_lastRestoredHeight}");
                }
                else
                {
                    // Window is maximized - preserve the last known restored size
                    settings.RestoredWidth = _lastRestoredWidth;
                    settings.RestoredHeight = _lastRestoredHeight;
                    System.Diagnostics.Debug.WriteLine($"Window maximized, preserving restored size: {_lastRestoredWidth}x{_lastRestoredHeight}");
                }
#else
                settings.IsMaximized = false;
                // On non-Windows platforms, always save as restored size
                _lastRestoredWidth = _mainWindow.Width;
                _lastRestoredHeight = _mainWindow.Height;
                settings.RestoredWidth = _lastRestoredWidth;
                settings.RestoredHeight = _lastRestoredHeight;
#endif
                
                // Save asynchronously
                _ = settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveWindowSize exception: {ex.Message}");
            }
        }
    }
}