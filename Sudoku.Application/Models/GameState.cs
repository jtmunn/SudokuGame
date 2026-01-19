namespace Sudoku.Application.Models
{
    /// <summary>
    /// Represents the complete state of an active Sudoku game.
    /// </summary>
    public class GameState
    {
        /// <summary>
        /// Serialized board data from SudokuBoard.Serialize().
        /// </summary>
        public string? BoardData { get; set; }
        
        /// <summary>
        /// Serialized solution data from SudokuBoard.Serialize().
        /// </summary>
        public string? SolutionData { get; set; }
        
        /// <summary>
        /// Elapsed time in seconds.
        /// </summary>
        public int ElapsedSeconds { get; set; }
        
        /// <summary>
        /// Current difficulty level as string.
        /// </summary>
        public string? Difficulty { get; set; }
        
        /// <summary>
        /// Numeric difficulty score calculated by the logical solver.
        /// Used to determine actual puzzle difficulty based on solving techniques required.
        /// </summary>
        public int DifficultyScore { get; set; }
        
        /// <summary>
        /// Whether the puzzle is solved.
        /// </summary>
        public bool IsSolved { get; set; }
        
        /// <summary>
        /// Timestamp when the game state was saved.
        /// </summary>
        public DateTime SavedAt { get; set; }
    }
}

