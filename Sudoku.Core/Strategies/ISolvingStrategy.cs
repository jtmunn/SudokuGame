using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies
{
    /// <summary>
    /// Interface for all Sudoku solving strategies.
    /// Each strategy attempts to make progress on the puzzle by placing values
    /// or eliminating candidates based on logical deduction.
    /// </summary>
    public interface ISolvingStrategy
    {
        /// <summary>
        /// Human-readable name of the strategy (e.g., "Naked Single", "X-Wing").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Difficulty score for this strategy based on SudokuWiki.org classification.
        /// Used to calculate overall puzzle difficulty.
        /// </summary>
        int DifficultyScore { get; }

        /// <summary>
        /// Category this strategy belongs to (Basic, Tough, Diabolical, Extreme).
        /// </summary>
        StrategyCategory Category { get; }

        /// <summary>
        /// Attempts to apply this strategy to the board.
        /// Returns a StrategyResult if the strategy found something, null otherwise.
        /// The strategy should NOT modify the board directly - it returns what changes to make.
        /// </summary>
        /// <param name="board">The Sudoku board to analyze.</param>
        /// <returns>StrategyResult with changes to apply, or null if strategy found nothing.</returns>
        StrategyResult? Apply(SudokuBoard board);
    }
}
