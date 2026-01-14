using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Tough
{
    /// <summary>
    /// Swordfish: A 3x3 pattern where a candidate appears in exactly 3 positions in 3 rows (or 3 columns),
    /// and those positions align in columns (or rows). Like X-Wing but 3x3 instead of 2x2.
    /// 
    /// Example: If digit 5 appears in:
    /// - Row 1: columns 2, 5, 8
    /// - Row 4: columns 2, 5, 8
    /// - Row 7: columns 2, 5, 8
    /// Then 5 can be eliminated from all other cells in columns 2, 5, and 8.
    /// </summary>
    public class SwordfishStrategy : ISolvingStrategy
    {
        public string Name => "Swordfish";
        public int DifficultyScore => 140;
        public StrategyCategory Category => StrategyCategory.Tough;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check for Swordfish in rows (eliminations in columns)
            for (int digit = 1; digit <= 9; digit++)
            {
                var result = FindSwordfishInRows(board, digit);
                if (result != null) return result;
            }

            // Check for Swordfish in columns (eliminations in rows)
            for (int digit = 1; digit <= 9; digit++)
            {
                var result = FindSwordfishInColumns(board, digit);
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? FindSwordfishInRows(SudokuBoard board, int digit)
        {
            // Find rows where the digit appears in 2 or 3 positions
            var rowsWithDigit = new List<(int row, List<int> cols)>();

            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var cols = board.GetRow(row)
                    .Where(c => c.Value == 0 && c.HasCandidate(digit))
                    .Select(c => c.Column)
                    .ToList();

                if (cols.Count >= 2 && cols.Count <= 3)
                {
                    rowsWithDigit.Add((row, cols));
                }
            }

            if (rowsWithDigit.Count < 3)
                return null;

            // Try all combinations of 3 rows
            for (int i = 0; i < rowsWithDigit.Count - 2; i++)
            {
                for (int j = i + 1; j < rowsWithDigit.Count - 1; j++)
                {
                    for (int k = j + 1; k < rowsWithDigit.Count; k++)
                    {
                        var (row1, cols1) = rowsWithDigit[i];
                        var (row2, cols2) = rowsWithDigit[j];
                        var (row3, cols3) = rowsWithDigit[k];

                        // Combine all columns
                        var allCols = new HashSet<int>(cols1);
                        allCols.UnionWith(cols2);
                        allCols.UnionWith(cols3);

                        // If exactly 3 columns span these 3 rows, we have a Swordfish
                        if (allCols.Count == 3)
                        {
                            var eliminations = new List<CandidateElimination>();
                            var colList = allCols.ToList();

                            // Eliminate from other rows in these columns
                            foreach (int col in colList)
                            {
                                foreach (var cell in board.GetColumn(col))
                                {
                                    if (cell.Row == row1 || cell.Row == row2 || cell.Row == row3)
                                        continue;

                                    if (cell.Value == 0 && cell.HasCandidate(digit))
                                    {
                                        eliminations.Add(new CandidateElimination(cell.Row, cell.Column, digit));
                                    }
                                }
                            }

                            if (eliminations.Count > 0)
                            {
                                return new StrategyResult
                                {
                                    RemovedCandidates = eliminations,
                                    Description = $"Swordfish: {digit} in rows {row1 + 1},{row2 + 1},{row3 + 1} columns {string.Join(",", colList.Select(c => c + 1))}"
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }

        private StrategyResult? FindSwordfishInColumns(SudokuBoard board, int digit)
        {
            // Find columns where the digit appears in 2 or 3 positions
            var colsWithDigit = new List<(int col, List<int> rows)>();

            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var rows = board.GetColumn(col)
                    .Where(c => c.Value == 0 && c.HasCandidate(digit))
                    .Select(c => c.Row)
                    .ToList();

                if (rows.Count >= 2 && rows.Count <= 3)
                {
                    colsWithDigit.Add((col, rows));
                }
            }

            if (colsWithDigit.Count < 3)
                return null;

            // Try all combinations of 3 columns
            for (int i = 0; i < colsWithDigit.Count - 2; i++)
            {
                for (int j = i + 1; j < colsWithDigit.Count - 1; j++)
                {
                    for (int k = j + 1; k < colsWithDigit.Count; k++)
                    {
                        var (col1, rows1) = colsWithDigit[i];
                        var (col2, rows2) = colsWithDigit[j];
                        var (col3, rows3) = colsWithDigit[k];

                        // Combine all rows
                        var allRows = new HashSet<int>(rows1);
                        allRows.UnionWith(rows2);
                        allRows.UnionWith(rows3);

                        // If exactly 3 rows span these 3 columns, we have a Swordfish
                        if (allRows.Count == 3)
                        {
                            var eliminations = new List<CandidateElimination>();
                            var rowList = allRows.ToList();

                            // Eliminate from other columns in these rows
                            foreach (int row in rowList)
                            {
                                foreach (var cell in board.GetRow(row))
                                {
                                    if (cell.Column == col1 || cell.Column == col2 || cell.Column == col3)
                                        continue;

                                    if (cell.Value == 0 && cell.HasCandidate(digit))
                                    {
                                        eliminations.Add(new CandidateElimination(cell.Row, cell.Column, digit));
                                    }
                                }
                            }

                            if (eliminations.Count > 0)
                            {
                                return new StrategyResult
                                {
                                    RemovedCandidates = eliminations,
                                    Description = $"Swordfish: {digit} in columns {col1 + 1},{col2 + 1},{col3 + 1} rows {string.Join(",", rowList.Select(r => r + 1))}"
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
