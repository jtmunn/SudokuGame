using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Hidden Pair: Two digits can only appear in the same two cells within a unit.
    /// All other candidates can be eliminated from those two cells.
    /// Example: If digits 3 and 7 can only go in cells R1C1 and R1C4 in row 1,
    /// then all other candidates can be removed from those two cells.
    /// </summary>
    public class HiddenPairStrategy : ISolvingStrategy
    {
        public string Name => "Hidden Pair";
        public int DifficultyScore => 35;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check each row
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var result = FindHiddenPairInUnit(board.GetRow(row), $"row {row + 1}");
                if (result != null) return result;
            }

            // Check each column
            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var result = FindHiddenPairInUnit(board.GetColumn(col), $"column {col + 1}");
                if (result != null) return result;
            }

            // Check each box
            for (int box = 0; box < SudokuBoard.Size; box++)
            {
                var result = FindHiddenPairInUnit(board.GetBox(box), $"box {box + 1}");
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? FindHiddenPairInUnit(IEnumerable<SudokuCell> unit, string unitName)
        {
            var emptyCells = unit.Where(c => c.Value == 0).ToList();

            if (emptyCells.Count < 2)
                return null;

            // Try all pairs of digits
            for (int digit1 = 1; digit1 <= 8; digit1++)
            {
                for (int digit2 = digit1 + 1; digit2 <= 9; digit2++)
                {
                    // Find cells that can contain digit1
                    var cellsWithDigit1 = emptyCells.Where(c => c.HasCandidate(digit1)).ToList();
                    // Find cells that can contain digit2
                    var cellsWithDigit2 = emptyCells.Where(c => c.HasCandidate(digit2)).ToList();

                    // If both digits appear in exactly the same 2 cells, we have a hidden pair
                    if (cellsWithDigit1.Count == 2 && 
                        cellsWithDigit2.Count == 2 &&
                        cellsWithDigit1.SequenceEqual(cellsWithDigit2))
                    {
                        var cell1 = cellsWithDigit1[0];
                        var cell2 = cellsWithDigit1[1];

                        // Eliminate all other candidates from these two cells
                        var eliminations = new List<CandidateElimination>();

                        foreach (var cell in new[] { cell1, cell2 })
                        {
                            foreach (int candidate in cell.Candidates.ToList())
                            {
                                if (candidate != digit1 && candidate != digit2)
                                {
                                    eliminations.Add(new CandidateElimination(cell.Row, cell.Column, candidate));
                                }
                            }
                        }

                        if (eliminations.Count > 0)
                        {
                            return new StrategyResult
                            {
                                RemovedCandidates = eliminations,
                                Description = $"Hidden Pair: {{{digit1},{digit2}}} locked to R{cell1.Row + 1}C{cell1.Column + 1} and R{cell2.Row + 1}C{cell2.Column + 1} in {unitName}"
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
