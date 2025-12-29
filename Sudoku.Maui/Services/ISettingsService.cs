using Sudoku.Maui.Models;

namespace Sudoku.Maui.Services
{
    public interface ISettingsService
    {
        GameSettings LoadSettings();
        Task SaveSettingsAsync(GameSettings settings);
        Task<GameSettings> GetSettingsAsync();
    }
}
