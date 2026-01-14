using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Naked Pair: Two cells in a unit have the exact same two candidates.
    /// Those two digits must be in those two cells, so they can be eliminated from other cells in the unit.
    /// Example: If R1C1 and R1C4 both have only {3,7}, then 3 and 7 can be removed from all other cells in row 1.
    /// </summary>
    public class NakedPairStrategy : ISolvingStrategy
    {
        public string Name => "Naked Pair";
        public int DifficultyScore => 30;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check each row
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var result = FindNakedPairInUnit(board.GetRow(row), $"row {row + 1}");
                if (result != null) return result;
            }

            // Check each column
            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var result = FindNakedPairInUnit(board.GetColumn(col), $"column {col + 1}");
                if (result != null) return result;
            }

            // Check each box
            for (int box = 0; box < SudokuBoard.Size; box++)
            {
                var result = FindNakedPairInUnit(board.GetBox(box), $"box {box + 1}");
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? FindNakedPairInUnit(IEnumerable<SudokuCell> unit, string unitName)
        {
            var cellsWithTwoCandidates = unit.Where(c => c.Value == 0 && c.CandidateCount == 2).ToList();

            // Need at least 2 cells with 2 candidates to form a pair
            if (cellsWithTwoCandidates.Count < 2)
                return null;

            // Look for two cells with identical candidate sets
            for (int i = 0; i < cellsWithTwoCandidates.Count - 1; i++)
            {
                for (int j = i + 1; j < cellsWithTwoCandidates.Count; j++)
                {
                    var cell1 = cellsWithTwoCandidates[i];
                    var cell2 = cellsWithTwoCandidates[j];

                    // Check if they have the same two candidates
                    if (cell1.Candidates.SetEquals(cell2.Candidates))
                    {
                        // Found a naked pair! Now eliminate these candidates from other cells
                        var eliminations = new List<CandidateElimination>();
                        var pairDigits = cell1.Candidates.ToList();

                        foreach (var cell in unit)
                        {
                            // Skip the pair cells themselves and filled cells
                            if (cell == cell1 || cell == cell2 || cell.Value != 0)
                                continue;

                            foreach (int digit in pairDigits)
                            {
                                if (cell.HasCandidate(digit))
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
                                Description = $"Naked Pair: {{{string.Join(",", pairDigits)}}} at R{cell1.Row + 1}C{cell1.Column + 1} and R{cell2.Row + 1}C{cell2.Column + 1} in {unitName}"
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}
