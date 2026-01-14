using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Box/Line Reduction: When a candidate in a row/column is confined to a single box,
    /// that candidate can be eliminated from other cells in the box.
    /// Example: If digit 5 in row 1 can only be in box 1, then 5 can be removed from other cells in box 1.
    /// This is the inverse of Pointing Pair.
    /// </summary>
    public class BoxLineReductionStrategy : ISolvingStrategy
    {
        public string Name => "Box/Line Reduction";
        public int DifficultyScore => 25;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check each row
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var result = CheckRow(board, row);
                if (result != null) return result;
            }

            // Check each column
            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var result = CheckColumn(board, col);
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? CheckRow(SudokuBoard board, int row)
        {
            // Check each digit 1-9
            for (int digit = 1; digit <= 9; digit++)
            {
                var cellsWithDigit = board.GetRow(row)
                    .Where(c => c.Value == 0 && c.HasCandidate(digit))
                    .ToList();

                if (cellsWithDigit.Count < 2 || cellsWithDigit.Count > 3)
                    continue;

                // Check if all cells with this digit are in the same box
                int firstBox = cellsWithDigit[0].GetBoxIndex();
                if (cellsWithDigit.All(c => c.GetBoxIndex() == firstBox))
                {
                    // Eliminate digit from other cells in this box
                    var eliminations = new List<CandidateElimination>();

                    foreach (var cell in board.GetBox(firstBox))
                    {
                        // Skip cells in the same row
                        if (cell.Row == row)
                            continue;

                        if (cell.Value == 0 && cell.HasCandidate(digit))
                        {
                            eliminations.Add(new CandidateElimination(cell.Row, cell.Column, digit));
                        }
                    }

                    if (eliminations.Count > 0)
                    {
                        return new StrategyResult
                        {
                            RemovedCandidates = eliminations,
                            Description = $"Box/Line Reduction: {digit} in row {row + 1} confined to box {firstBox + 1}"
                        };
                    }
                }
            }

            return null;
        }

        private StrategyResult? CheckColumn(SudokuBoard board, int col)
        {
            // Check each digit 1-9
            for (int digit = 1; digit <= 9; digit++)
            {
                var cellsWithDigit = board.GetColumn(col)
                    .Where(c => c.Value == 0 && c.HasCandidate(digit))
                    .ToList();

                if (cellsWithDigit.Count < 2 || cellsWithDigit.Count > 3)
                    continue;

                // Check if all cells with this digit are in the same box
                int firstBox = cellsWithDigit[0].GetBoxIndex();
                if (cellsWithDigit.All(c => c.GetBoxIndex() == firstBox))
                {
                    // Eliminate digit from other cells in this box
                    var eliminations = new List<CandidateElimination>();

                    foreach (var cell in board.GetBox(firstBox))
                    {
                        // Skip cells in the same column
                        if (cell.Column == col)
                            continue;

                        if (cell.Value == 0 && cell.HasCandidate(digit))
                        {
                            eliminations.Add(new CandidateElimination(cell.Row, cell.Column, digit));
                        }
                    }

                    if (eliminations.Count > 0)
                    {
                        return new StrategyResult
                        {
                            RemovedCandidates = eliminations,
                            Description = $"Box/Line Reduction: {digit} in column {col + 1} confined to box {firstBox + 1}"
                        };
                    }
                }
            }

            return null;
        }
    }
}
