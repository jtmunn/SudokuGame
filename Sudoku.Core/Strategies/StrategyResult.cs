namespace Sudoku.Core.Strategies
{
    /// <summary>
    /// Represents the result of applying a solving strategy.
    /// Contains the changes that should be made to the board.
    /// </summary>
    public class StrategyResult
    {
        /// <summary>
        /// List of cells that had values placed by this strategy.
        /// </summary>
        public List<CellChange> PlacedValues { get; set; } = new();

        /// <summary>
        /// List of candidates that were eliminated (removed) by this strategy.
        /// </summary>
        public List<CandidateElimination> RemovedCandidates { get; set; } = new();

        /// <summary>
        /// Human-readable description of what the strategy found.
        /// Example: "Naked Single: R1C3 must be 7"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Returns true if this result contains any changes (placed values or removed candidates).
        /// </summary>
        public bool HasChanges => PlacedValues.Count > 0 || RemovedCandidates.Count > 0;
    }

    /// <summary>
    /// Represents a cell that should have a value placed.
    /// </summary>
    /// <param name="Row">Zero-based row index (0-8).</param>
    /// <param name="Col">Zero-based column index (0-8).</param>
    /// <param name="Value">The value to place (1-9).</param>
    public record CellChange(int Row, int Col, int Value);

    /// <summary>
    /// Represents a candidate that should be eliminated from a cell.
    /// </summary>
    /// <param name="Row">Zero-based row index (0-8).</param>
    /// <param name="Col">Zero-based column index (0-8).</param>
    /// <param name="Candidate">The candidate value to remove (1-9).</param>
    public record CandidateElimination(int Row, int Col, int Candidate);
}
