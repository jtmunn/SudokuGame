namespace Sudoku.Application.Services
{
    /// <summary>
    /// Data passed when a puzzle is solved. Contains everything needed to render the summary.
    /// </summary>
    public class PuzzleSolvedEventArgs : EventArgs
    {
        /// <summary>Display name of the difficulty (e.g., "Medium").</summary>
        public string DifficultyName { get; init; } = "";

        /// <summary>Total elapsed time when the puzzle was completed.</summary>
        public int ElapsedSeconds { get; init; }

        /// <summary>Best previously recorded time for this difficulty, or null if none.</summary>
        public int? PreviousBestTime { get; init; }

        /// <summary>Mistakes made during this game.</summary>
        public int MistakesCount { get; init; }

        /// <summary>Hints used during this game.</summary>
        public int HintsUsedCount { get; init; }

        /// <summary>True when this completion set a new best time.</summary>
        public bool IsNewRecord { get; init; }
    }
}
