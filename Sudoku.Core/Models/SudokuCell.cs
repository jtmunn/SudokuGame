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
        /// Indicates if this cell has a validation error (conflict with row/column/box).
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Set of possible candidate values for this cell (1-9).
        /// Used by logical solving strategies to track which numbers are still possible.
        /// Empty when the cell has a value.
        /// </summary>
        public HashSet<int> Candidates { get; set; } = new HashSet<int>();

        public SudokuCell(int row, int column)
        {
            Row = row;
            Column = column;
            Value = 0;
            IsGiven = false;
            HasError = false;
        }

        /// <summary>
        /// Initializes candidates for an empty cell with all possible values (1-9).
        /// Clears candidates if the cell has a value.
        /// </summary>
        public void InitializeCandidates()
        {
            if (Value == 0)
                Candidates = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            else
                Candidates.Clear();
        }

        /// <summary>
        /// Removes a candidate from this cell's possible values.
        /// </summary>
        public void RemoveCandidate(int candidate)
        {
            Candidates.Remove(candidate);
        }

        /// <summary>
        /// Adds a candidate to this cell's possible values.
        /// </summary>
        public void AddCandidate(int candidate)
        {
            if (candidate >= 1 && candidate <= 9)
                Candidates.Add(candidate);
        }

        /// <summary>
        /// Checks if this cell has a specific candidate.
        /// </summary>
        public bool HasCandidate(int candidate)
        {
            return Candidates.Contains(candidate);
        }

        /// <summary>
        /// Gets the number of remaining candidates in this cell.
        /// </summary>
        public int CandidateCount => Candidates.Count;

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
                Candidates = new HashSet<int>(Candidates) // Deep copy candidates
            };
        }
    }
}
