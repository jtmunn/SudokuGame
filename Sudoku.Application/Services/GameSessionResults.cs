namespace Sudoku.Application.Services
{
    /// <summary>
    /// Outcome of attempting to place a number on the board.
    /// </summary>
    public enum PlacementOutcome
    {
        /// <summary>Move was rejected (out of range, given cell, board not editable).</summary>
        Rejected,

        /// <summary>Move conflicts with another visible value; counted as a mistake, value not placed.</summary>
        VisibleConflict,

        /// <summary>Value was placed and matches the solution.</summary>
        PlacedCorrect,

        /// <summary>Value was placed but does not match the solution; counted as a mistake.</summary>
        PlacedIncorrect
    }

    /// <summary>
    /// Result of a placement attempt.
    /// </summary>
    public readonly record struct PlacementResult(PlacementOutcome Outcome, bool PuzzleSolved);

    /// <summary>
    /// Outcome of requesting a hint.
    /// </summary>
    public enum HintOutcome
    {
        /// <summary>The session is not in a state that allows hints.</summary>
        Rejected,

        /// <summary>The board contains conflicts; resolve them before requesting a hint.</summary>
        BlockedByConflicts,

        /// <summary>No hint could be derived from the current board state.</summary>
        NoHintAvailable,

        /// <summary>A hint was applied to the board.</summary>
        Provided
    }

    /// <summary>
    /// Result of requesting a hint, including the placed value when <see cref="HintOutcome.Provided"/>.
    /// </summary>
    public readonly record struct HintResult(HintOutcome Outcome, int Row, int Col, int Value, bool PuzzleSolved);

    /// <summary>
    /// Outcome of a board-state check.
    /// </summary>
    public enum CheckOutcome
    {
        /// <summary>The session is not in a state that allows checks.</summary>
        Rejected,

        /// <summary>One or more conflicts are present.</summary>
        HasConflicts,

        /// <summary>The puzzle is solved.</summary>
        Solved,

        /// <summary>No conflicts and not yet solved.</summary>
        InProgress
    }

    /// <summary>
    /// Result of a board check, with how many cells match the solution when in progress.
    /// </summary>
    public readonly record struct CheckResult(CheckOutcome Outcome, int CorrectCellCount);
}
