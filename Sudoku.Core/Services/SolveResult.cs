using Sudoku.Core.Strategies;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Represents the result of attempting to solve a Sudoku puzzle using logical strategies.
    /// </summary>
    public class SolveResult
    {
        /// <summary>
        /// Whether the puzzle was completely solved using logical strategies.
        /// </summary>
        public bool IsSolved { get; set; }

        /// <summary>
        /// The calculated difficulty score based on strategies used.
        /// Formula: Sum of (strategy score × times used) + (hardest strategy × 2)
        /// </summary>
        public int DifficultyScore { get; set; }

        /// <summary>
        /// The difficulty level calculated from the score.
        /// </summary>
        public DifficultyLevel CalculatedDifficulty { get; set; }

        /// <summary>
        /// List of strategies that were used and how many times.
        /// </summary>
        public List<StrategyUsage> StrategiesUsed { get; set; } = new();

        /// <summary>
        /// Total number of cells filled during solving.
        /// </summary>
        public int CellsFilled { get; set; }

        /// <summary>
        /// Number of iterations the solver took.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Records that a strategy was used.
        /// If the strategy was already used, increments its count.
        /// </summary>
        public void RecordStrategyUsage(ISolvingStrategy strategy)
        {
            var existing = StrategiesUsed.FirstOrDefault(s => s.Strategy.Name == strategy.Name);

            if (existing != null)
            {
                existing.TimesUsed++;
            }
            else
            {
                StrategiesUsed.Add(new StrategyUsage
                {
                    Strategy = strategy,
                    TimesUsed = 1
                });
            }
        }

        /// <summary>
        /// Gets a human-readable summary of the solve attempt.
        /// </summary>
        public string GetSummary()
        {
            if (IsSolved)
            {
                return $"Solved! Difficulty: {CalculatedDifficulty} (Score: {DifficultyScore})\n" +
                       $"Filled {CellsFilled} cells in {Iterations} iterations\n" +
                       $"Strategies used: {string.Join(", ", StrategiesUsed.Select(s => $"{s.Strategy.Name}×{s.TimesUsed}"))}";
            }
            else
            {
                return $"Could not solve with logical strategies alone.\n" +
                       $"Filled {CellsFilled} cells in {Iterations} iterations\n" +
                       $"Current difficulty: {CalculatedDifficulty} (Score: {DifficultyScore})";
            }
        }
    }

    /// <summary>
    /// Tracks how many times a specific strategy was used during solving.
    /// </summary>
    public class StrategyUsage
    {
        /// <summary>
        /// The strategy that was used.
        /// </summary>
        public ISolvingStrategy Strategy { get; set; } = null!;

        /// <summary>
        /// Number of times this strategy was successfully applied.
        /// </summary>
        public int TimesUsed { get; set; }
    }
}
