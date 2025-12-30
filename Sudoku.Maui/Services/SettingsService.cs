using System.Text.Json;
using Sudoku.Maui.Models;

namespace Sudoku.Maui.Services
{
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "sudoku_settings.json";
        private readonly string _settingsFilePath;
        private GameSettings? _cachedSettings;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
            System.Diagnostics.Debug.WriteLine($"SettingsService: Settings file path: {_settingsFilePath}");
        }

        public GameSettings LoadSettings()
        {
            // ALWAYS reload from file to get latest saved state
            // Don't use cache on load - only cache after loading
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    System.Diagnostics.Debug.WriteLine($"SettingsService: Loaded settings from file: {json}");
                    _cachedSettings = JsonSerializer.Deserialize<GameSettings>(json) ?? CreateDefaultSettings();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SettingsService: No settings file found, creating defaults");
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
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                System.Diagnostics.Debug.WriteLine($"SettingsService: Saving settings: {json}");
                await File.WriteAllTextAsync(_settingsFilePath, json);
                System.Diagnostics.Debug.WriteLine("SettingsService: Settings saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsService: Failed to save settings: {ex.Message}");
            }
        }

        private GameSettings CreateDefaultSettings()
        {
            return new GameSettings
            {
                DefaultDifficulty = DifficultyLevel.Medium,
                ShowHintButton = true,
                ShowCheckButton = true,
                Theme = AppTheme.Light
            };
        }
    }
}
