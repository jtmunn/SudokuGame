namespace Sudoku.Maui.Models
{
    public class GameSettings
    {
        public DifficultyLevel DefaultDifficulty { get; set; } = DifficultyLevel.Easy;
        public bool ShowHintButton { get; set; } = true;
        public bool ShowCheckButton { get; set; } = true;
        public AppTheme Theme { get; set; } = AppTheme.Light;
        
        // Window size persistence
        public double? WindowWidth { get; set; }
        public double? WindowHeight { get; set; }
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }
}
