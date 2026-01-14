using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Pointing Pair (Box/Line Reduction): When a candidate in a box is confined to a single row or column,
    /// that candidate can be eliminated from the same row/column outside the box.
    /// Example: If digit 5 in box 1 can only be in row 1, then 5 can be removed from row 1 in boxes 2 and 3.
    /// </summary>
    public class PointingPairStrategy : ISolvingStrategy
    {
        public string Name => "Pointing Pair";
        public int DifficultyScore => 25;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check each box
            for (int box = 0; box < SudokuBoard.Size; box++)
            {
                var boxCells = board.GetBox(box).ToList();

                // Check each digit 1-9
                for (int digit = 1; digit <= 9; digit++)
                {
                    var cellsWithDigit = boxCells.Where(c => c.Value == 0 && c.HasCandidate(digit)).ToList();

                    if (cellsWithDigit.Count < 2 || cellsWithDigit.Count > 3)
                        continue; // Need 2-3 cells for pointing pair/triple

                    // Check if all cells are in the same row
                    if (cellsWithDigit.All(c => c.Row == cellsWithDigit[0].Row))
                    {
                        int row = cellsWithDigit[0].Row;
                        var result = EliminateFromRowOutsideBox(board, row, box, digit);
                        if (result != null) return result;
                    }

                    // Check if all cells are in the same column
                    if (cellsWithDigit.All(c => c.Column == cellsWithDigit[0].Column))
                    {
                        int col = cellsWithDigit[0].Column;
                        var result = EliminateFromColumnOutsideBox(board, col, box, digit);
                        if (result != null) return result;
                    }
                }
            }

            return null;
        }

        private StrategyResult? EliminateFromRowOutsideBox(SudokuBoard board, int row, int box, int digit)
        {
            var eliminations = new List<CandidateElimination>();

            foreach (var cell in board.GetRow(row))
            {
                // Skip cells in the same box
                if (cell.GetBoxIndex() == box)
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
                    Description = $"Pointing Pair: {digit} in box {box + 1} confined to row {row + 1}"
                };
            }

            return null;
        }

        private StrategyResult? EliminateFromColumnOutsideBox(SudokuBoard board, int col, int box, int digit)
        {
            var eliminations = new List<CandidateElimination>();

            foreach (var cell in board.GetColumn(col))
            {
                // Skip cells in the same box
                if (cell.GetBoxIndex() == box)
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
                    Description = $"Pointing Pair: {digit} in box {box + 1} confined to column {col + 1}"
                };
            }

            return null;
        }
    }
}
