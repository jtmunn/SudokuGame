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
        }

        public GameSettings LoadSettings()
        {
            if (_cachedSettings != null)
                return _cachedSettings;

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
            catch
            {
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
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
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
