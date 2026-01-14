using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Tough
{
    /// <summary>
    /// Y-Wing: Three cells with bivalue candidates (XY, XZ, YZ) where:
    /// - Pivot cell has candidates {X,Y}
    /// - Wing 1 has candidates {X,Z} and sees the pivot
    /// - Wing 2 has candidates {Y,Z} and sees the pivot
    /// Then Z can be eliminated from cells that see both wings.
    /// 
    /// Example:
    /// - R1C1: {3,7} (pivot)
    /// - R1C5: {3,9} (wing 1, shares row with pivot)
    /// - R4C1: {7,9} (wing 2, shares column with pivot)
    /// Then 9 can be eliminated from any cell that sees both R1C5 and R4C1.
    /// </summary>
    public class YWingStrategy : ISolvingStrategy
    {
        public string Name => "Y-Wing";
        public int DifficultyScore => 130;
        public StrategyCategory Category => StrategyCategory.Tough;

        public StrategyResult? Apply(SudokuBoard board)
        {
            var bivalueCells = board.GetCellsWithNCandidates(2).ToList();

            if (bivalueCells.Count < 3)
                return null;

            // Try each bivalue cell as the pivot
            foreach (var pivot in bivalueCells)
            {
                var pivotCandidates = pivot.Candidates.ToList();
                int x = pivotCandidates[0];
                int y = pivotCandidates[1];

                // Find potential wings
                foreach (var wing1 in bivalueCells)
                {
                    if (wing1 == pivot || !CellsSeeEachOther(pivot, wing1))
                        continue;

                    var wing1Candidates = wing1.Candidates.ToList();

                    // Wing1 must share exactly one candidate with pivot
                    if (!wing1Candidates.Contains(x) && !wing1Candidates.Contains(y))
                        continue;

                    bool wing1SharesX = wing1Candidates.Contains(x);
                    int z1 = wing1SharesX ? wing1Candidates.First(c => c != x) : wing1Candidates.First(c => c != y);
                    int sharedWithPivot1 = wing1SharesX ? x : y;
                    int otherPivotCandidate = wing1SharesX ? y : x;

                    // Now find wing2 that shares the OTHER pivot candidate and has the same Z
                    foreach (var wing2 in bivalueCells)
                    {
                        if (wing2 == pivot || wing2 == wing1 || !CellsSeeEachOther(pivot, wing2))
                            continue;

                        var wing2Candidates = wing2.Candidates.ToList();

                        // Wing2 must have: the OTHER pivot candidate and the same Z as wing1
                        if (wing2Candidates.Contains(otherPivotCandidate) && wing2Candidates.Contains(z1))
                        {
                            // Found a Y-Wing! Eliminate Z from cells that see both wings
                            var eliminations = new List<CandidateElimination>();

                            foreach (var cell in board.GetAllCells())
                            {
                                if (cell.Value == 0 && 
                                    cell != pivot && cell != wing1 && cell != wing2 &&
                                    cell.HasCandidate(z1) &&
                                    CellsSeeEachOther(cell, wing1) && 
                                    CellsSeeEachOther(cell, wing2))
                                {
                                    eliminations.Add(new CandidateElimination(cell.Row, cell.Column, z1));
                                }
                            }

                            if (eliminations.Count > 0)
                            {
                                return new StrategyResult
                                {
                                    RemovedCandidates = eliminations,
                                    Description = $"Y-Wing: Pivot R{pivot.Row + 1}C{pivot.Column + 1} {{{x},{y}}}, " +
                                                $"Wings R{wing1.Row + 1}C{wing1.Column + 1} and R{wing2.Row + 1}C{wing2.Column + 1}, eliminates {z1}"
                                };
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if two cells "see" each other (share a row, column, or box).
        /// </summary>
        private bool CellsSeeEachOther(SudokuCell cell1, SudokuCell cell2)
        {
            return cell1.Row == cell2.Row ||
                   cell1.Column == cell2.Column ||
                   cell1.GetBoxIndex() == cell2.GetBoxIndex();
        }
    }
}
