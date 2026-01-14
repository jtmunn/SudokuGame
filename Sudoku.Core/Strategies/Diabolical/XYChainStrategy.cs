using Sudoku.Core.Models;

namespace Sudoku.Core.Strategies.Diabolical
{
    /// <summary>
    /// XY-Chain: A chain of bivalue cells (cells with exactly 2 candidates) where each consecutive pair shares one candidate.
    /// If the chain starts and ends with the same candidate in both endpoints, that candidate can be eliminated
    /// from cells that see both endpoints.
    /// 
    /// Example chain: {3,7} -> {7,2} -> {2,5} -> {5,3}
    /// The chain starts with 3 and ends with 3, so any cell seeing both endpoints can't be 3.
    /// </summary>
    public class XYChainStrategy : ISolvingStrategy
    {
        public string Name => "XY-Chain";
        public int DifficultyScore => 240;
        public StrategyCategory Category => StrategyCategory.Diabolical;

        private const int MaxChainLength = 8; // Limit chain length for performance

        public StrategyResult? Apply(SudokuBoard board)
        {
            var bivalueCells = board.GetCellsWithNCandidates(2).ToList();

            if (bivalueCells.Count < 3)
                return null;

            // Try starting from each bivalue cell
            foreach (var startCell in bivalueCells)
            {
                var startCandidates = startCell.Candidates.ToList();

                // Try each candidate as the starting digit
                foreach (int startDigit in startCandidates)
                {
                    var otherDigit = startCandidates.First(c => c != startDigit);
                    
                    // Build chain starting with this cell and digit
                    var visited = new HashSet<SudokuCell> { startCell };
                    var result = FindChain(board, bivalueCells, startCell, otherDigit, startDigit, visited, 1);
                    
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private StrategyResult? FindChain(
            SudokuBoard board,
            List<SudokuCell> bivalueCells,
            SudokuCell currentCell,
            int currentDigit,
            int targetDigit,
            HashSet<SudokuCell> visited,
            int depth)
        {
            if (depth > MaxChainLength)
                return null;

            // Find next cell in chain: must be bivalue, see current cell, and share currentDigit
            foreach (var nextCell in bivalueCells)
            {
                if (visited.Contains(nextCell))
                    continue;

                if (!CellsSeeEachOther(currentCell, nextCell))
                    continue;

                if (!nextCell.HasCandidate(currentDigit))
                    continue;

                // Get the other digit in nextCell
                var nextCandidates = nextCell.Candidates.ToList();
                var otherDigit = nextCandidates.First(c => c != currentDigit);

                // Check if we've found a chain: other end has targetDigit
                if (otherDigit == targetDigit && depth >= 2)
                {
                    // Found chain! Eliminate targetDigit from cells seeing both ends
                    var startCell = visited.First();
                    var eliminations = new List<CandidateElimination>();

                    foreach (var cell in board.GetAllCells())
                    {
                        if (cell.Value == 0 &&
                            cell != startCell &&
                            cell != nextCell &&
                            cell.HasCandidate(targetDigit) &&
                            CellsSeeEachOther(cell, startCell) &&
                            CellsSeeEachOther(cell, nextCell))
                        {
                            eliminations.Add(new CandidateElimination(cell.Row, cell.Column, targetDigit));
                        }
                    }

                    if (eliminations.Count > 0)
                    {
                        return new StrategyResult
                        {
                            RemovedCandidates = eliminations,
                            Description = $"XY-Chain: Chain of length {depth + 1} eliminates {targetDigit}"
                        };
                    }
                }

                // Continue building chain
                visited.Add(nextCell);
                var result = FindChain(board, bivalueCells, nextCell, otherDigit, targetDigit, visited, depth + 1);
                visited.Remove(nextCell);

                if (result != null)
                    return result;
            }

            return null;
        }

        private bool CellsSeeEachOther(SudokuCell cell1, SudokuCell cell2)
        {
            return cell1.Row == cell2.Row ||
                   cell1.Column == cell2.Column ||
                   cell1.GetBoxIndex() == cell2.GetBoxIndex();
        }
    }
}
