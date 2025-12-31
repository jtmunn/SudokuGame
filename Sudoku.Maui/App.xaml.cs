using Microsoft.Extensions.DependencyInjection;
using Sudoku.Maui.Services;
using Sudoku.Maui.Pages;
using System.IO;

namespace Sudoku.Maui
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private Window? _mainWindow;
        private System.Timers.Timer? _saveWindowSizeTimer;
        private const int SaveWindowSizeDelayMs = 500;
        private bool _shouldMaximizeOnCreated = false;
        
        private double _lastRestoredWidth = 800;
        private double _lastRestoredHeight = 800;
        
        private AppTheme _currentTheme = AppTheme.Unspecified;
        
        // Crash logging
        private static string? _logFilePath;
        
#if WINDOWS
        private bool _isMaximized = false;
#endif
        
        public App()
        {
            try
            {
                InitializeCrashLogging();
                LogMessage("=== APP CONSTRUCTOR START ===");
                
                InitializeComponent();
                _currentTheme = AppTheme.Light;
                
                LogMessage("App constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in App constructor: {ex}");
                throw;
            }
        }

        private static void InitializeCrashLogging()
        {
            try
            {
                var appDataDir = FileSystem.AppDataDirectory;
                _logFilePath = Path.Combine(appDataDir, "crash_log.txt");
                
                // Clear old log
                if (File.Exists(_logFilePath))
                    File.Delete(_logFilePath);
                
                File.WriteAllText(_logFilePath, $"Crash Log - {DateTime.Now}\n");
            }
            catch
            {
                // Can't log if logging fails
            }
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
                System.Diagnostics.Debug.WriteLine(message);
            }
            catch
            {
                // Ignore logging failures
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                LogMessage("=== CREATE WINDOW START ===");
                
                var settingsService = Handler?.MauiContext?.Services.GetService<ISettingsService>();
                LogMessage($"SettingsService: {(settingsService != null ? "Found" : "NULL")}");
                
                if (settingsService != null)
                {
                    try
                    {
                        LogMessage("Loading settings...");
                        var settings = settingsService.LoadSettings();
                        LogMessage($"Settings loaded - Theme: {settings.Theme}");
                        
                        UserAppTheme = settings.Theme;
                        LogMessage("UserAppTheme set");
                        
                        LoadTheme(settings.Theme);
                        LogMessage("LoadTheme completed");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"ERROR loading theme in CreateWindow: {ex}");
                    }
                }
                
                LogMessage("Creating Window with AppShell...");
                var window = new Window(new AppShell())
                {
                    Title = "Sudoku",
                    MinimumWidth = 600,
                    MinimumHeight = 700
                };
                LogMessage("Window created successfully");
                
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
                
                LogMessage("=== CREATE WINDOW COMPLETED ===");
                return window;
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in CreateWindow: {ex}");
                throw;
            }
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
            try
            {
                LogMessage($"LoadTheme called - Requested: {theme}, Current: {_currentTheme}");
                
                // Skip if already loaded
                if (_currentTheme == theme)
                {
                    LogMessage("Theme already loaded, skipping");
                    return;
                }

                LogMessage("Getting MergedDictionaries...");
                ICollection<ResourceDictionary> mergedDictionaries = Resources.MergedDictionaries;
                
                if (mergedDictionaries != null)
                {
                    LogMessage($"MergedDictionaries count: {mergedDictionaries.Count}");
                    
                    var themeDict = mergedDictionaries.FirstOrDefault(d => 
                        d.GetType().Name == "LightTheme" || d.GetType().Name == "DarkTheme");
                    
                    if (themeDict != null)
                    {
                        LogMessage($"Found existing theme: {themeDict.GetType().Name}");
                        LogMessage("Removing old theme...");
                        mergedDictionaries.Remove(themeDict);
                        LogMessage("Old theme removed");
                    }
                    
                    LogMessage($"Adding new theme: {theme}...");
                    if (theme == AppTheme.Dark)
                    {
                        mergedDictionaries.Add(new Resources.Styles.Themes.DarkTheme());
                    }
                    else
                    {
                        mergedDictionaries.Add(new Resources.Styles.Themes.LightTheme());
                    }
                    LogMessage("New theme added");
                    
                    _currentTheme = theme;
                    LogMessage($"Theme loaded successfully: {theme}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"CRASH in LoadTheme: {ex}");
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