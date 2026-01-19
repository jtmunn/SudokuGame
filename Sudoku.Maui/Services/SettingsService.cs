using System.Text.Json;
using AppModels = Sudoku.Application.Models;
using Sudoku.Application.Services;

namespace Sudoku.Maui.Services
{
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "sudoku_settings.json";
        private const string StatisticsFileName = "sudoku_statistics.json";
        private readonly string _settingsFilePath;
        private readonly string _statisticsFilePath;
        private AppModels.GameSettings? _cachedSettings;
        private AppModels.GameStatistics? _cachedStatistics;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
            _statisticsFilePath = Path.Combine(FileSystem.AppDataDirectory, StatisticsFileName);
        }

        public AppModels.GameSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _cachedSettings = JsonSerializer.Deserialize<AppModels.GameSettings>(json) ?? CreateDefaultSettings();
                }
                else
                {
                    _cachedSettings = CreateDefaultSettings();
                    // Don't save on first load - let it save naturally when settings change
                    // Calling .Wait() here causes deadlock during MAUI initialization
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                _cachedSettings = CreateDefaultSettings();
            }

            return _cachedSettings;
        }

        public async Task<AppModels.GameSettings> GetSettingsAsync()
        {
            return await Task.Run(() => LoadSettings());
        }

        public async Task SaveSettingsAsync(AppModels.GameSettings settings)
        {
            _cachedSettings = settings;
            
            try
            {
                var json = JsonSerializer.Serialize(settings);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Failed to save settings: {ex.Message}");
            }
        }

        public AppModels.GameStatistics LoadStatistics()
        {
            if (_cachedStatistics != null)
            {
                return _cachedStatistics;
            }

            try
            {
                if (File.Exists(_statisticsFilePath))
                {
                    var json = File.ReadAllText(_statisticsFilePath);
                    _cachedStatistics = JsonSerializer.Deserialize<AppModels.GameStatistics>(json) ?? new AppModels.GameStatistics();
                }
                else
                {
                    _cachedStatistics = new AppModels.GameStatistics();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error loading statistics: {ex.Message}");
                _cachedStatistics = new AppModels.GameStatistics();
            }

            return _cachedStatistics;
        }

        public async Task SaveStatisticsAsync(AppModels.GameStatistics statistics)
        {
            _cachedStatistics = statistics;
            
            try
            {
                var json = JsonSerializer.Serialize(statistics);
                await File.WriteAllTextAsync(_statisticsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Failed to save statistics: {ex.Message}");
            }
        }

        private AppModels.GameSettings CreateDefaultSettings()
        {
            return new AppModels.GameSettings
            {
                LastPlayedDifficulty = null,
                ShowHintButton = true,
                ShowCheckButton = true,
                Theme = AppModels.ThemeMode.Light
            };
        }
    }
}

