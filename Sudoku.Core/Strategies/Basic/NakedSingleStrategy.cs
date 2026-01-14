using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Naked Single: A cell has only one candidate remaining, so that must be its value.
    /// This is the simplest and most fundamental solving technique.
    /// Example: If a cell can only be 7, then it must be 7.
    /// </summary>
    public class NakedSingleStrategy : ISolvingStrategy
    {
        public string Name => "Naked Single";
        public int DifficultyScore => 5;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Look for any cell with exactly 1 candidate
            foreach (var cell in board.GetAllCells())
            {
                if (cell.Value == 0 && cell.CandidateCount == 1)
                {
                    int value = cell.Candidates.First();
                    
                    return new StrategyResult
                    {
                        PlacedValues = new List<CellChange>
                        {
                            new CellChange(cell.Row, cell.Column, value)
                        },
                        Description = $"Naked Single: R{cell.Row + 1}C{cell.Column + 1} must be {value}"
                    };
                }
            }

            return null; // No naked singles found
        }
    }
}
