using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Hidden Single: A digit can only go in one place within a row, column, or box.
    /// Example: If 7 can only appear in one cell of a row, it must go there.
    /// </summary>
    public class HiddenSingleStrategy : ISolvingStrategy
    {
        public string Name => "Hidden Single";
        public int DifficultyScore => 10;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check each row
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var result = FindHiddenSingleInUnit(board.GetRow(row), $"row {row + 1}");
                if (result != null) return result;
            }

            // Check each column
            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var result = FindHiddenSingleInUnit(board.GetColumn(col), $"column {col + 1}");
                if (result != null) return result;
            }

            // Check each box
            for (int box = 0; box < SudokuBoard.Size; box++)
            {
                var result = FindHiddenSingleInUnit(board.GetBox(box), $"box {box + 1}");
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? FindHiddenSingleInUnit(IEnumerable<SudokuCell> unit, string unitName)
        {
            // For each digit 1-9, count how many cells can contain it
            for (int digit = 1; digit <= 9; digit++)
            {
                var cellsWithDigit = unit.Where(c => c.Value == 0 && c.HasCandidate(digit)).ToList();

                if (cellsWithDigit.Count == 1)
                {
                    // This digit can only go in one cell - hidden single!
                    var cell = cellsWithDigit[0];
                    
                    return new StrategyResult
                    {
                        PlacedValues = new List<CellChange>
                        {
                            new CellChange(cell.Row, cell.Column, digit)
                        },
                        Description = $"Hidden Single: R{cell.Row + 1}C{cell.Column + 1} is the only place for {digit} in {unitName}"
                    };
                }
            }

            return null;
        }
    }
}
