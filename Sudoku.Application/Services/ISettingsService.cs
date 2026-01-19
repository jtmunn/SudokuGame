using Sudoku.Application.Models;

namespace Sudoku.Application.Services
{
    public interface ISettingsService
    {
        GameSettings LoadSettings();
        Task SaveSettingsAsync(GameSettings settings);
        Task<GameSettings> GetSettingsAsync();
        
        GameStatistics LoadStatistics();
        Task SaveStatisticsAsync(GameStatistics statistics);
    }
}

