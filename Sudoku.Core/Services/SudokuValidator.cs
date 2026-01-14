using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Validates Sudoku moves and checks puzzle completion.
    /// Includes bitset-based fast validation for solver performance.
    /// </summary>
    public class SudokuValidator
    {
        // Bitset arrays for O(1) validation during solving
        // Each int represents a 9-bit set (bits 1-9) indicating which numbers are used
        private int[]? _rowUsed;
        private int[]? _colUsed;
        private int[]? _boxUsed;
        private bool _useBitsets = false;

        /// <summary>
        /// Initializes bitsets from the current board state for fast validation.
        /// Call this before running the solver for 50-100x faster validation.
        /// </summary>
        public void InitializeBitsets(SudokuBoard board)
        {
            _rowUsed = new int[9];
            _colUsed = new int[9];
            _boxUsed = new int[9];

            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value != 0)
                    {
                        int bit = 1 << cell.Value;
                        int boxIndex = (row / 3) * 3 + (col / 3);
                        _rowUsed[row] |= bit;
                        _colUsed[col] |= bit;
                        _boxUsed[boxIndex] |= bit;
                    }
                }
            }

            _useBitsets = true;
        }

        /// <summary>
        /// Clears bitsets. Call after solving is complete.
        /// </summary>
        public void ClearBitsets()
        {
            _rowUsed = null;
            _colUsed = null;
            _boxUsed = null;
            _useBitsets = false;
        }

        /// <summary>
        /// Sets a value in the bitsets when placing a number during solving.
        /// </summary>
        public void SetBitsets(int row, int col, int value)
        {
            if (!_useBitsets || _rowUsed == null || _colUsed == null || _boxUsed == null)
                return;

            int bit = 1 << value;
            int boxIndex = (row / 3) * 3 + (col / 3);
            _rowUsed[row] |= bit;
            _colUsed[col] |= bit;
            _boxUsed[boxIndex] |= bit;
        }

        /// <summary>
        /// Clears a value in the bitsets when backtracking during solving.
        /// </summary>
        public void ClearBitsets(int row, int col, int value)
        {
            if (!_useBitsets || _rowUsed == null || _colUsed == null || _boxUsed == null)
                return;

            int bit = 1 << value;
            int boxIndex = (row / 3) * 3 + (col / 3);
            _rowUsed[row] &= ~bit;
            _colUsed[col] &= ~bit;
            _boxUsed[boxIndex] &= ~bit;
        }

        /// <summary>
        /// Fast O(1) validation using bitsets. Only works when bitsets are initialized.
        /// </summary>
        public bool IsValidMoveFast(int row, int col, int value)
        {
            if (!_useBitsets || _rowUsed == null || _colUsed == null || _boxUsed == null)
                return false;

            if (value < 1 || value > 9)
                return false;

            int bit = 1 << value;
            int boxIndex = (row / 3) * 3 + (col / 3);

            return (_rowUsed[row] & bit) == 0 &&
                   (_colUsed[col] & bit) == 0 &&
                   (_boxUsed[boxIndex] & bit) == 0;
        }
        /// <summary>
        /// Checks if a value can be legally placed at the specified position.
        /// </summary>
        public bool IsValidMove(SudokuBoard board, int row, int col, int value)
        {
            if (value < 1 || value > 9)
                return false;

            var cell = board.GetCell(row, col);
            
            // Can't modify given cells
            if (cell.IsGiven)
                return false;

            // Temporarily set the value to check conflicts
            int originalValue = cell.Value;
            cell.Value = value;

            bool isValid = !HasConflict(board, row, col);

            // Restore original value
            cell.Value = originalValue;

            return isValid;
        }

        /// <summary>
        /// Checks if a cell has any conflicts with its row, column, or box.
        /// Optimized version using direct array access instead of IEnumerable to avoid allocations.
        /// </summary>
        public bool HasConflict(SudokuBoard board, int row, int col)
        {
            var cell = board.GetCell(row, col);
            
            if (cell.Value == 0)
                return false;

            int value = cell.Value;

            // Check row conflicts - direct array access
            for (int c = 0; c < SudokuBoard.Size; c++)
            {
                if (c != col && board.GetCell(row, c).Value == value)
                    return true;
            }

            // Check column conflicts - direct array access
            for (int r = 0; r < SudokuBoard.Size; r++)
            {
                if (r != row && board.GetCell(r, col).Value == value)
                    return true;
            }

            // Check box conflicts - direct array access
            int boxRow = (row / 3) * 3;
            int boxCol = (col / 3) * 3;
            for (int r = boxRow; r < boxRow + 3; r++)
            {
                for (int c = boxCol; c < boxCol + 3; c++)
                {
                    if ((r != row || c != col) && board.GetCell(r, c).Value == value)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates error flags for all cells based on conflicts.
        /// </summary>
        public void UpdateErrorFlags(SudokuBoard board)
        {
            foreach (var cell in board.GetAllCells())
            {
                cell.HasError = HasConflict(board, cell.Row, cell.Column);
            }
        }

        /// <summary>
        /// Checks if the puzzle is completely and correctly solved.
        /// </summary>
        public bool IsSolved(SudokuBoard board)
        {
            // Check if all cells are filled
            foreach (var cell in board.GetAllCells())
            {
                if (cell.Value == 0)
                    return false;
            }

            // Check if there are any conflicts
            foreach (var cell in board.GetAllCells())
            {
                if (HasConflict(board, cell.Row, cell.Column))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all valid numbers that can be placed in a cell.
        /// </summary>
        public List<int> GetValidNumbers(SudokuBoard board, int row, int col)
        {
            var validNumbers = new List<int>();
            var cell = board.GetCell(row, col);

            // Can't modify given cells
            if (cell.IsGiven || cell.Value != 0)
                return validNumbers;

            for (int num = 1; num <= 9; num++)
            {
                if (IsValidMove(board, row, col, num))
                {
                    validNumbers.Add(num);
                }
            }

            return validNumbers;
        }

        /// <summary>
        /// Checks if the puzzle is in a valid state (no conflicts).
        /// </summary>
        public bool IsValidState(SudokuBoard board)
        {
            foreach (var cell in board.GetAllCells())
            {
                if (cell.Value != 0 && HasConflict(board, cell.Row, cell.Column))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Counts how many cells are correctly filled.
        /// </summary>
        public int CountCorrectCells(SudokuBoard board, SudokuBoard solution)
        {
            int count = 0;
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    var solutionCell = solution.GetCell(row, col);
                    
                    if (cell.Value != 0 && cell.Value == solutionCell.Value)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
