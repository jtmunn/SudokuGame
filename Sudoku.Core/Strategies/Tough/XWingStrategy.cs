using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Tough
{
    /// <summary>
    /// X-Wing: A 2x2 pattern where a candidate appears in exactly 2 positions in 2 rows (or 2 columns),
    /// and those positions align in columns (or rows). The candidate can be eliminated from other cells
    /// in those columns (or rows).
    /// 
    /// Example: If digit 5 appears in:
    /// - Row 1: columns 3 and 7
    /// - Row 5: columns 3 and 7
    /// Then 5 can be eliminated from all other cells in columns 3 and 7.
    /// </summary>
    public class XWingStrategy : ISolvingStrategy
    {
        public string Name => "X-Wing";
        public int DifficultyScore => 100;
        public StrategyCategory Category => StrategyCategory.Tough;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check for X-Wing in rows (eliminations in columns)
            for (int digit = 1; digit <= 9; digit++)
            {
                var result = FindXWingInRows(board, digit);
                if (result != null) return result;
            }

            // Check for X-Wing in columns (eliminations in rows)
            for (int digit = 1; digit <= 9; digit++)
            {
                var result = FindXWingInColumns(board, digit);
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? FindXWingInRows(SudokuBoard board, int digit)
        {
            // Find rows where the digit appears in exactly 2 positions
            var rowsWithTwo = new List<(int row, List<int> cols)>();

            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var cols = board.GetRow(row)
                    .Where(c => c.Value == 0 && c.HasCandidate(digit))
                    .Select(c => c.Column)
                    .ToList();

                if (cols.Count == 2)
                {
                    rowsWithTwo.Add((row, cols));
                }
            }

            // Need at least 2 rows to form an X-Wing
            if (rowsWithTwo.Count < 2)
                return null;

            // Look for two rows with the same column positions
            for (int i = 0; i < rowsWithTwo.Count - 1; i++)
            {
                for (int j = i + 1; j < rowsWithTwo.Count; j++)
                {
                    var (row1, cols1) = rowsWithTwo[i];
                    var (row2, cols2) = rowsWithTwo[j];

                    if (cols1[0] == cols2[0] && cols1[1] == cols2[1])
                    {
                        // Found X-Wing! Eliminate digit from other cells in these columns
                        var eliminations = new List<CandidateElimination>();
                        int col1 = cols1[0];
                        int col2 = cols1[1];

                        foreach (var cell in board.GetColumn(col1).Concat(board.GetColumn(col2)))
                        {
                            // Skip the X-Wing cells
                            if (cell.Row == row1 || cell.Row == row2)
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
                                Description = $"X-Wing: {digit} in rows {row1 + 1},{row2 + 1} columns {col1 + 1},{col2 + 1}"
                            };
                        }
                    }
                }
            }

            return null;
        }

        private StrategyResult? FindXWingInColumns(SudokuBoard board, int digit)
        {
            // Find columns where the digit appears in exactly 2 positions
            var colsWithTwo = new List<(int col, List<int> rows)>();

            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var rows = board.GetColumn(col)
                    .Where(c => c.Value == 0 && c.HasCandidate(digit))
                    .Select(c => c.Row)
                    .ToList();

                if (rows.Count == 2)
                {
                    colsWithTwo.Add((col, rows));
                }
            }

            // Need at least 2 columns to form an X-Wing
            if (colsWithTwo.Count < 2)
                return null;

            // Look for two columns with the same row positions
            for (int i = 0; i < colsWithTwo.Count - 1; i++)
            {
                for (int j = i + 1; j < colsWithTwo.Count; j++)
                {
                    var (col1, rows1) = colsWithTwo[i];
                    var (col2, rows2) = colsWithTwo[j];

                    if (rows1[0] == rows2[0] && rows1[1] == rows2[1])
                    {
                        // Found X-Wing! Eliminate digit from other cells in these rows
                        var eliminations = new List<CandidateElimination>();
                        int row1 = rows1[0];
                        int row2 = rows1[1];

                        foreach (var cell in board.GetRow(row1).Concat(board.GetRow(row2)))
                        {
                            // Skip the X-Wing cells
                            if (cell.Column == col1 || cell.Column == col2)
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
                                Description = $"X-Wing: {digit} in columns {col1 + 1},{col2 + 1} rows {row1 + 1},{row2 + 1}"
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
