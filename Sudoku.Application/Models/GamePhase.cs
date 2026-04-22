namespace Sudoku.Application.Models
{
    /// <summary>
    /// Lifecycle phases of an active game session.
    /// Drives input authorization across the UI.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>No game has been loaded yet.</summary>
        NotStarted,

        /// <summary>A new puzzle is being generated.</summary>
        Generating,

        /// <summary>An active puzzle is being played.</summary>
        Playing,

        /// <summary>The current puzzle has been solved.</summary>
        Completed
    }
}
