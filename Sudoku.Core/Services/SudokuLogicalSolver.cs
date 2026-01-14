using Sudoku.Core.Models;
using Sudoku.Core.Strategies;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Strategies.Tough;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Solves Sudoku puzzles using logical strategies (human-solvable techniques).
    /// Tracks which strategies are needed to solve the puzzle to calculate difficulty.
    /// </summary>
    public class SudokuLogicalSolver
    {
        private readonly List<ISolvingStrategy> _strategies;

        /// <summary>
        /// Creates a new logical solver with all implemented strategies.
        /// Strategies are registered in order of difficulty (easiest first).
        /// </summary>
        public SudokuLogicalSolver()
        {
            _strategies = new List<ISolvingStrategy>
            {
                // Basic strategies (Score: 5-50)
                new NakedSingleStrategy(),           // 5
                new HiddenSingleStrategy(),          // 10
                new PointingPairStrategy(),          // 25
                new BoxLineReductionStrategy(),      // 25
                new NakedPairStrategy(),             // 30
                new HiddenPairStrategy(),            // 35
                
                // Tough strategies (Score: 100-150)
                new XWingStrategy(),                 // 100
                new YWingStrategy(),                 // 130
                
                // Future: Add more strategies here as implemented
                // new SwordfishStrategy(),          // 140
                // new SimpleColouringStrategy(),    // 120
                // new XYChainStrategy(),            // 240
            };
        }

        /// <summary>
        /// Attempts to solve the puzzle using logical strategies.
        /// Returns a SolveResult with difficulty analysis.
        /// </summary>
        /// <param name="board">The puzzle to solve (will be cloned internally).</param>
        /// <returns>Results including whether puzzle was solved and difficulty score.</returns>
        public SolveResult Solve(SudokuBoard board)
        {
            var result = new SolveResult();
            var workingBoard = board.Clone();
            
            // Initialize candidates for all empty cells
            workingBoard.InitializeCandidates();

            bool madeProgress = true;
            int iterationLimit = 1000; // Safety limit to prevent infinite loops
            int iterations = 0;

            while (madeProgress && iterations < iterationLimit)
            {
                madeProgress = false;
                iterations++;

                // Try each strategy in order of difficulty
                foreach (var strategy in _strategies)
                {
                    var strategyResult = strategy.Apply(workingBoard);

                    if (strategyResult != null && strategyResult.HasChanges)
                    {
                        // Apply the changes to the board
                        ApplyStrategyResult(workingBoard, strategyResult);

                        // Record that this strategy was used
                        result.RecordStrategyUsage(strategy);

                        madeProgress = true;
                        
                        // After applying changes, restart from the easiest strategy
                        // (This mimics human solving - after placing a value, look for naked singles again)
                        break;
                    }
                }

                // Check if puzzle is solved
                if (IsSolved(workingBoard))
                {
                    result.IsSolved = true;
                    break;
                }
            }

            result.Iterations = iterations;
            result.CellsFilled = CountFilledCells(workingBoard) - CountFilledCells(board);
            result.DifficultyScore = CalculateDifficulty(result);
            result.CalculatedDifficulty = ScoreToDifficultyLevel(result.DifficultyScore);

            return result;
        }

        /// <summary>
        /// Applies the changes from a strategy result to the board.
        /// </summary>
        private void ApplyStrategyResult(SudokuBoard board, StrategyResult strategyResult)
        {
            // Apply placed values
            foreach (var change in strategyResult.PlacedValues)
            {
                board.SetCell(change.Row, change.Col, change.Value);
                
                // Clear candidates for this cell and update related cells
                var cell = board.GetCell(change.Row, change.Col);
                cell.Candidates.Clear();
                
                // Remove this value as candidate from all cells in same row, column, and box
                UpdateCandidatesAfterPlacement(board, change.Row, change.Col, change.Value);
            }

            // Apply candidate eliminations
            foreach (var elimination in strategyResult.RemovedCandidates)
            {
                board.GetCell(elimination.Row, elimination.Col).RemoveCandidate(elimination.Candidate);
            }
        }

        /// <summary>
        /// Updates candidates in all related cells after a value is placed.
        /// </summary>
        private void UpdateCandidatesAfterPlacement(SudokuBoard board, int row, int col, int value)
        {
            // Remove from row
            foreach (var cell in board.GetRow(row))
            {
                if (cell.Value == 0)
                    cell.RemoveCandidate(value);
            }

            // Remove from column
            foreach (var cell in board.GetColumn(col))
            {
                if (cell.Value == 0)
                    cell.RemoveCandidate(value);
            }

            // Remove from box
            int boxIndex = (row / 3) * 3 + (col / 3);
            foreach (var cell in board.GetBox(boxIndex))
            {
                if (cell.Value == 0)
                    cell.RemoveCandidate(value);
            }
        }

        /// <summary>
        /// Checks if the puzzle is completely solved.
        /// </summary>
        private bool IsSolved(SudokuBoard board)
        {
            return board.GetAllCells().All(c => c.Value != 0);
        }

        /// <summary>
        /// Counts how many cells have values.
        /// </summary>
        private int CountFilledCells(SudokuBoard board)
        {
            return board.GetAllCells().Count(c => c.Value != 0);
        }

        /// <summary>
        /// Calculates the overall difficulty score based on strategies used.
        /// Formula: Sum of (strategy score × times used) + (hardest strategy score × 2)
        /// </summary>
        private int CalculateDifficulty(SolveResult result)
        {
            if (result.StrategiesUsed.Count == 0)
                return 0;

            // Base score: sum of all strategy scores weighted by usage
            int totalScore = result.StrategiesUsed.Sum(s => s.Strategy.DifficultyScore * s.TimesUsed);

            // Bonus for the most difficult strategy required
            // This ensures puzzles requiring advanced techniques score higher
            var hardestStrategy = result.StrategiesUsed.MaxBy(s => s.Strategy.DifficultyScore);
            if (hardestStrategy != null)
            {
                totalScore += hardestStrategy.Strategy.DifficultyScore * 2;
            }

            return totalScore;
        }

        /// <summary>
        /// Converts a numeric difficulty score to a difficulty level.
        /// Based on SudokuWiki.org difficulty tiers.
        /// </summary>
        private DifficultyLevel ScoreToDifficultyLevel(int score)
        {
            return score switch
            {
                <= 100 => DifficultyLevel.Easy,      // Basic strategies only
                <= 300 => DifficultyLevel.Medium,    // Up to Tough strategies
                <= 600 => DifficultyLevel.Hard,      // Requires Diabolical strategies
                <= 1000 => DifficultyLevel.Expert,   // Requires Extreme strategies
                _ => DifficultyLevel.Evil            // Multiple Extreme strategies
            };
        }

        /// <summary>
        /// Gets the list of strategies this solver uses, in order of difficulty.
        /// Useful for debugging and understanding what the solver can handle.
        /// </summary>
        public IReadOnlyList<ISolvingStrategy> GetStrategies() => _strategies.AsReadOnly();
    }
}
