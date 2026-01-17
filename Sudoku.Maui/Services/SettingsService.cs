using System.Text.Json;
using Sudoku.Maui.Models;

namespace Sudoku.Maui.Services
{
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "sudoku_settings.json";
        private const string StatisticsFileName = "sudoku_statistics.json";
        private readonly string _settingsFilePath;
        private readonly string _statisticsFilePath;
        private GameSettings? _cachedSettings;
        private GameStatistics? _cachedStatistics;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
            _statisticsFilePath = Path.Combine(FileSystem.AppDataDirectory, StatisticsFileName);
        }

        public GameSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _cachedSettings = JsonSerializer.Deserialize<GameSettings>(json) ?? CreateDefaultSettings();
                }
                else
                {
                    _cachedSettings = CreateDefaultSettings();
                    SaveSettingsAsync(_cachedSettings).Wait();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error loading settings: {ex.Message}");
                _cachedSettings = CreateDefaultSettings();
            }

            return _cachedSettings;
        }

        public async Task<GameSettings> GetSettingsAsync()
        {
            return await Task.Run(() => LoadSettings());
        }

        public async Task SaveSettingsAsync(GameSettings settings)
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

        public GameStatistics LoadStatistics()
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
                    _cachedStatistics = JsonSerializer.Deserialize<GameStatistics>(json) ?? new GameStatistics();
                }
                else
                {
                    _cachedStatistics = new GameStatistics();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Error loading statistics: {ex.Message}");
                _cachedStatistics = new GameStatistics();
            }

            return _cachedStatistics;
        }

        public async Task SaveStatisticsAsync(GameStatistics statistics)
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

        private GameSettings CreateDefaultSettings()
        {
            return new GameSettings
            {
                LastPlayedDifficulty = null,
                ShowHintButton = true,
                ShowCheckButton = true,
                Theme = AppTheme.Light
            };
        }
    }
}
