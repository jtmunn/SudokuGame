using System.Text;

namespace Sudoku.Core.Models
{
    /// <summary>
    /// Represents a 9x9 Sudoku board with all cells and game state.
    /// </summary>
    public class SudokuBoard
    {
        private readonly SudokuCell[,] _cells;
        public const int Size = 9;

        public SudokuBoard()
        {
            _cells = new SudokuCell[Size, Size];
            InitializeEmptyBoard();
        }

        private void InitializeEmptyBoard()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    _cells[row, col] = new SudokuCell(row, col);
                }
            }
        }

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        public SudokuCell GetCell(int row, int column)
        {
            if (row < 0 || row >= Size || column < 0 || column >= Size)
                throw new ArgumentOutOfRangeException("Row and column must be between 0 and 8.");

            return _cells[row, column];
        }

        /// <summary>
        /// Sets the value of a cell at the specified position.
        /// </summary>
        public void SetCell(int row, int column, int value, bool isGiven = false)
        {
            if (row < 0 || row >= Size || column < 0 || column >= Size)
                throw new ArgumentOutOfRangeException("Row and column must be between 0 and 8.");

            if (value < 0 || value > 9)
                throw new ArgumentOutOfRangeException("Value must be between 0 and 9.");

            _cells[row, column].Value = value;
            _cells[row, column].IsGiven = isGiven;
        }

        /// <summary>
        /// Gets all cells in the specified row.
        /// </summary>
        public IEnumerable<SudokuCell> GetRow(int row)
        {
            for (int col = 0; col < Size; col++)
            {
                yield return _cells[row, col];
            }
        }

        /// <summary>
        /// Gets all cells in the specified column.
        /// </summary>
        public IEnumerable<SudokuCell> GetColumn(int column)
        {
            for (int row = 0; row < Size; row++)
            {
                yield return _cells[row, column];
            }
        }

        /// <summary>
        /// Gets all cells in the specified 3x3 box (0-8).
        /// </summary>
        public IEnumerable<SudokuCell> GetBox(int boxIndex)
        {
            int startRow = (boxIndex / 3) * 3;
            int startCol = (boxIndex % 3) * 3;

            for (int row = startRow; row < startRow + 3; row++)
            {
                for (int col = startCol; col < startCol + 3; col++)
                {
                    yield return _cells[row, col];
                }
            }
        }

        /// <summary>
        /// Gets all cells on the board.
        /// </summary>
        public IEnumerable<SudokuCell> GetAllCells()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    yield return _cells[row, col];
                }
            }
        }

        /// <summary>
        /// Clears all non-given cells.
        /// </summary>
        public void ClearUserEntries()
        {
            foreach (var cell in GetAllCells())
            {
                if (!cell.IsGiven)
                {
                    cell.Value = 0;
                    cell.HasError = false;
                }
            }
        }

        /// <summary>
        /// Resets the entire board to empty.
        /// </summary>
        public void Clear()
        {
            foreach (var cell in GetAllCells())
            {
                cell.Value = 0;
                cell.IsGiven = false;
                cell.HasError = false;
            }
        }

        /// <summary>
        /// Creates a deep copy of the board.
        /// </summary>
        public SudokuBoard Clone()
        {
            var clone = new SudokuBoard();
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    var sourceCell = _cells[row, col];
                    var destCell = clone.GetCell(row, col);
                    destCell.Value = sourceCell.Value;
                    destCell.IsGiven = sourceCell.IsGiven;
                    destCell.HasError = sourceCell.HasError;
                    destCell.Candidates = new HashSet<int>(sourceCell.Candidates); // Deep copy candidates
                }
            }
            return clone;
        }

        /// <summary>
        /// Initializes candidate values for all empty cells on the board.
        /// Each empty cell starts with candidates {1,2,3,4,5,6,7,8,9},
        /// then invalid candidates are removed based on current board state.
        /// </summary>
        public void InitializeCandidates()
        {
            // First, initialize all cells with full candidate sets
            foreach (var cell in GetAllCells())
            {
                cell.InitializeCandidates();
            }

            // Then remove invalid candidates based on current board state
            UpdateAllCandidates();
        }

        /// <summary>
        /// Updates candidate values for all empty cells based on current board state.
        /// Removes candidates that conflict with filled cells in the same row, column, or box.
        /// </summary>
        public void UpdateAllCandidates()
        {
            foreach (var cell in GetAllCells())
            {
                if (cell.Value == 0)
                {
                    // Remove candidates that already exist in the same row, column, or box
                    for (int num = 1; num <= 9; num++)
                    {
                        if (!IsValidCandidate(cell.Row, cell.Column, num))
                        {
                            cell.RemoveCandidate(num);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a number is a valid candidate for a cell position.
        /// Returns false if the number already exists in the same row, column, or box.
        /// </summary>
        private bool IsValidCandidate(int row, int col, int num)
        {
            // Check row
            foreach (var cell in GetRow(row))
            {
                if (cell.Value == num)
                    return false;
            }

            // Check column
            foreach (var cell in GetColumn(col))
            {
                if (cell.Value == num)
                    return false;
            }

            // Check box
            int boxIndex = (row / 3) * 3 + (col / 3);
            foreach (var cell in GetBox(boxIndex))
            {
                if (cell.Value == num)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all empty cells that have exactly N candidates.
        /// Useful for finding naked pairs (N=2), naked triples (N=3), etc.
        /// </summary>
        public IEnumerable<SudokuCell> GetCellsWithNCandidates(int n)
        {
            return GetAllCells().Where(c => c.Value == 0 && c.CandidateCount == n);
        }

        /// <summary>
        /// Serializes the board to a string (for save/load functionality).
        /// Format: 81 digits where 0 represents empty cells, with 'G' suffix for given cells.
        /// </summary>
        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var cell in GetAllCells())
            {
                sb.Append(cell.Value);
            }
            sb.Append('|');
            foreach (var cell in GetAllCells())
            {
                sb.Append(cell.IsGiven ? '1' : '0');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Deserializes a board from a string.
        /// </summary>
        public static SudokuBoard Deserialize(string data)
        {
            var board = new SudokuBoard();
            var parts = data.Split('|');
            if (parts.Length != 2 || parts[0].Length != 81 || parts[1].Length != 81)
            {
                throw new ArgumentException("Invalid board data format.");
            }

            int index = 0;
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    int value = int.Parse(parts[0][index].ToString());
                    bool isGiven = parts[1][index] == '1';
                    board.SetCell(row, col, value, isGiven);
                    index++;
                }
            }
            return board;
        }
    }
}
