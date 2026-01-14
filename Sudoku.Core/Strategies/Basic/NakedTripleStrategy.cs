using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Basic
{
    /// <summary>
    /// Naked Triple: Three cells in a unit have the same three candidates between them.
    /// Those three digits must be in those three cells, so they can be eliminated from other cells.
    /// Example: If R1C1={2,5,8}, R1C3={2,8}, R1C7={5,8}, then 2,5,8 can be removed from other cells in row 1.
    /// </summary>
    public class NakedTripleStrategy : ISolvingStrategy
    {
        public string Name => "Naked Triple";
        public int DifficultyScore => 40;
        public StrategyCategory Category => StrategyCategory.Basic;

        public StrategyResult? Apply(SudokuBoard board)
        {
            // Check each row
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                var result = FindNakedTripleInUnit(board.GetRow(row), $"row {row + 1}");
                if (result != null) return result;
            }

            // Check each column
            for (int col = 0; col < SudokuBoard.Size; col++)
            {
                var result = FindNakedTripleInUnit(board.GetColumn(col), $"column {col + 1}");
                if (result != null) return result;
            }

            // Check each box
            for (int box = 0; box < SudokuBoard.Size; box++)
            {
                var result = FindNakedTripleInUnit(board.GetBox(box), $"box {box + 1}");
                if (result != null) return result;
            }

            return null;
        }

        private StrategyResult? FindNakedTripleInUnit(IEnumerable<SudokuCell> unit, string unitName)
        {
            // Get cells with 2 or 3 candidates
            var cellsWithTwoOrThree = unit.Where(c => c.Value == 0 && (c.CandidateCount == 2 || c.CandidateCount == 3)).ToList();

            if (cellsWithTwoOrThree.Count < 3)
                return null;

            // Try all combinations of 3 cells
            for (int i = 0; i < cellsWithTwoOrThree.Count - 2; i++)
            {
                for (int j = i + 1; j < cellsWithTwoOrThree.Count - 1; j++)
                {
                    for (int k = j + 1; k < cellsWithTwoOrThree.Count; k++)
                    {
                        var cell1 = cellsWithTwoOrThree[i];
                        var cell2 = cellsWithTwoOrThree[j];
                        var cell3 = cellsWithTwoOrThree[k];

                        // Combine all candidates from these 3 cells
                        var combinedCandidates = new HashSet<int>(cell1.Candidates);
                        combinedCandidates.UnionWith(cell2.Candidates);
                        combinedCandidates.UnionWith(cell3.Candidates);

                        // If combined candidates = exactly 3, we have a naked triple
                        if (combinedCandidates.Count == 3)
                        {
                            var tripleDigits = combinedCandidates.ToList();
                            var eliminations = new List<CandidateElimination>();

                            // Eliminate these digits from other cells in the unit
                            foreach (var cell in unit)
                            {
                                if (cell == cell1 || cell == cell2 || cell == cell3 || cell.Value != 0)
                                    continue;

                                foreach (int digit in tripleDigits)
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
                                    Description = $"Naked Triple: {{{string.Join(",", tripleDigits)}}} at R{cell1.Row + 1}C{cell1.Column + 1}, R{cell2.Row + 1}C{cell2.Column + 1}, R{cell3.Row + 1}C{cell3.Column + 1} in {unitName}"
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
