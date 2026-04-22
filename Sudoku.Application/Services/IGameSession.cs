using Sudoku.Application.Models;
using Sudoku.Core.Models;
using Sudoku.Core.Services;

namespace Sudoku.Application.Services
{
    /// <summary>
    /// Owns the live state of the current Sudoku game: board, solution, timer,
    /// statistics, and lifecycle phase. UI layers subscribe to events and call
    /// the input methods; all gameplay rules and authorization live here.
    /// </summary>
    public interface IGameSession
    {
        // ---- State ----

        /// <summary>Current lifecycle phase.</summary>
        GamePhase Phase { get; }

        /// <summary>The active board. Always non-null; empty board before first game.</summary>
        SudokuBoard Board { get; }

        /// <summary>Solution for the current puzzle, if known.</summary>
        SudokuBoard? Solution { get; }

        /// <summary>Difficulty of the current puzzle, or null if no game has started.</summary>
        DifficultyLevel? Difficulty { get; }

        /// <summary>Display name of the current difficulty (empty if no game).</summary>
        string DifficultyName { get; }

        /// <summary>Elapsed seconds for the current game.</summary>
        int ElapsedSeconds { get; }

        /// <summary>Mistakes made in the current game.</summary>
        int MistakesCount { get; }

        /// <summary>Hints used in the current game.</summary>
        int HintsUsedCount { get; }

        /// <summary>True if the user has placed or cleared at least one cell since the game started.</summary>
        bool HasUserMadeEntries { get; }

        // ---- Authorization ----

        /// <summary>True when board edits (placement/clear) are allowed.</summary>
        bool CanEditBoard { get; }

        /// <summary>True when game-action buttons (Hint, Check, Clear) should be enabled.</summary>
        bool CanUseGameActions { get; }

        // ---- Events ----

        /// <summary>Raised when <see cref="Phase"/> changes.</summary>
        event EventHandler<GamePhase>? PhaseChanged;

        /// <summary>Raised when the board state changes (placement, clear, hint, restart, restore).</summary>
        event EventHandler? BoardChanged;

        /// <summary>Raised once per second while the timer is running. Fires on a non-UI thread.</summary>
        event EventHandler? TimerTick;

        /// <summary>Raised when the puzzle is solved, after statistics have been updated.</summary>
        event EventHandler<PuzzleSolvedEventArgs>? PuzzleSolved;

        // ---- Lifecycle ----

        /// <summary>Generates a new puzzle of the given difficulty and starts the timer.</summary>
        Task StartNewAsync(DifficultyLevel difficulty);

        /// <summary>Restarts the current puzzle, clearing user entries while keeping givens.</summary>
        void Restart();

        /// <summary>
        /// Restores a saved game. Returns false on failure (e.g., corrupt data); the
        /// session remains in <see cref="GamePhase.NotStarted"/> in that case.
        /// </summary>
        bool TryRestore(GameState state);

        /// <summary>
        /// Attempts to load and restore a previously persisted game from storage.
        /// Returns true if a game was successfully resumed.
        /// </summary>
        bool TryResumeSavedGame();

        /// <summary>Persists the current game state via <see cref="IGameStateService"/>.</summary>
        Task SaveAsync();

        /// <summary>Pauses the elapsed-time timer (e.g., when the window deactivates).</summary>
        void PauseTimer();

        /// <summary>Resumes the elapsed-time timer if the session is currently <see cref="GamePhase.Playing"/>.</summary>
        void ResumeTimer();

        // ---- Input ----

        /// <summary>Attempts to place a number on the board.</summary>
        PlacementResult TryPlaceNumber(int row, int col, int number);

        /// <summary>Attempts to clear a cell. Returns true if the cell was cleared.</summary>
        bool TryClearCell(int row, int col);

        /// <summary>Requests a hint and applies it to the board if available.</summary>
        HintResult TryGetHint();

        /// <summary>Performs a board check and returns the outcome.</summary>
        CheckResult Check();
    }
}
