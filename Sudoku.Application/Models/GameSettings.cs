using CoreDifficulty = Sudoku.Core.Services.DifficultyLevel;

namespace Sudoku.Application.Models
{
    /// <summary>
    /// Application theme modes (platform-agnostic).
    /// </summary>
    public enum ThemeMode
    {
        Light = 0,
        Dark = 1
    }

    public class GameSettings
    {
        public CoreDifficulty? LastPlayedDifficulty { get; set; }
        public bool ShowHintButton { get; set; } = true;
        public bool ShowCheckButton { get; set; } = true;
        public ThemeMode Theme { get; set; } = ThemeMode.Light;
        
        // Window size persistence
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }
        public bool? IsMaximized { get; set; }
        
        // Restored (non-maximized) window size
        public double? RestoredWidth { get; set; }
        public double? RestoredHeight { get; set; }
    }
}

