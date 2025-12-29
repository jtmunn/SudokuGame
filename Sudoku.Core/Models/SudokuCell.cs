namespace Sudoku.Core.Models
{
    /// <summary>
    /// Represents a single cell in the Sudoku grid.
    /// </summary>
    public class SudokuCell
    {
        /// <summary>
        /// The current value in the cell (1-9, or 0 if empty).
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Indicates whether this cell is part of the initial puzzle (fixed/given).
        /// </summary>
        public bool IsGiven { get; set; }

        /// <summary>
        /// Row position (0-8).
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// Column position (0-8).
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Optional notes/pencil marks for this cell (for future expansion).
        /// </summary>
        public HashSet<int> Notes { get; set; } = new HashSet<int>();

        /// <summary>
        /// Indicates if this cell has a validation error (conflict with row/column/box).
        /// </summary>
        public bool HasError { get; set; }

        public SudokuCell(int row, int column)
        {
            Row = row;
            Column = column;
            Value = 0;
            IsGiven = false;
            HasError = false;
        }

        /// <summary>
        /// Determines which 3x3 box this cell belongs to (0-8).
        /// </summary>
        public int GetBoxIndex()
        {
            return (Row / 3) * 3 + (Column / 3);
        }

        public SudokuCell Clone()
        {
            return new SudokuCell(Row, Column)
            {
                Value = Value,
                IsGiven = IsGiven,
                HasError = HasError,
                Notes = new HashSet<int>(Notes)
            };
        }
    }
}
