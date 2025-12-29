using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Validates Sudoku moves and checks puzzle completion.
    /// </summary>
    public class SudokuValidator
    {
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
        /// </summary>
        public bool HasConflict(SudokuBoard board, int row, int col)
        {
            var cell = board.GetCell(row, col);
            
            if (cell.Value == 0)
                return false;

            int value = cell.Value;

            // Check row conflicts
            foreach (var otherCell in board.GetRow(row))
            {
                if (otherCell != cell && otherCell.Value == value)
                    return true;
            }

            // Check column conflicts
            foreach (var otherCell in board.GetColumn(col))
            {
                if (otherCell != cell && otherCell.Value == value)
                    return true;
            }

            // Check box conflicts
            int boxIndex = cell.GetBoxIndex();
            foreach (var otherCell in board.GetBox(boxIndex))
            {
                if (otherCell != cell && otherCell.Value == value)
                    return true;
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
