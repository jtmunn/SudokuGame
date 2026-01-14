using Sudoku.Core.Models;

namespace Sudoku.Core.Tests.Helpers
{
    /// <summary>
    /// Helper class to build test Sudoku boards with specific configurations.
    /// </summary>
    public static class BoardBuilder
    {
        /// <summary>
        /// Creates a board from a string representation.
        /// '0' or '.' represents empty cells, digits 1-9 represent filled cells.
        /// </summary>
        public static SudokuBoard CreateFromString(string boardString)
        {
            var board = new SudokuBoard();
            var digits = boardString.Replace("\n", "").Replace("\r", "").Replace(" ", "").Replace(".", "0");
            
            if (digits.Length != 81)
                throw new ArgumentException("Board string must contain exactly 81 characters");

            int index = 0;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int value = int.Parse(digits[index].ToString());
                    if (value != 0)
                    {
                        board.SetCell(row, col, value, isGiven: true);
                    }
                    index++;
                }
            }

            return board;
        }

        /// <summary>
        /// Creates an empty board and manually sets candidates for testing.
        /// </summary>
        public static SudokuBoard CreateEmptyBoard()
        {
            return new SudokuBoard();
        }

        /// <summary>
        /// Sets a cell's candidates manually (for testing strategies).
        /// </summary>
        public static void SetCandidates(SudokuBoard board, int row, int col, params int[] candidates)
        {
            var cell = board.GetCell(row, col);
            cell.Candidates.Clear();
            foreach (var candidate in candidates)
            {
                cell.AddCandidate(candidate);
            }
        }
    }
}
