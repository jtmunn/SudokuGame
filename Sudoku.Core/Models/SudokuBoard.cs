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
                }
            }
            return clone;
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
