using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Difficulty levels for generated puzzles.
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,       // 40-45 given cells
        Medium,     // 32-39 given cells
        Hard,       // 25-31 given cells
        Expert      // 22-24 given cells (for future expansion)
    }

    /// <summary>
    /// Generates valid Sudoku puzzles with varying difficulty levels.
    /// Uses backtracking algorithm to create a complete valid solution,
    /// then removes cells based on difficulty.
    /// </summary>
    public class SudokuGenerator
    {
        private readonly Random _random;
        private readonly SudokuSolver _solver;

        public SudokuGenerator(SudokuSolver solver)
        {
            _random = new Random();
            _solver = solver;
        }

        /// <summary>
        /// Generates a new Sudoku puzzle with the specified difficulty.
        /// </summary>
        public SudokuBoard Generate(DifficultyLevel difficulty = DifficultyLevel.Easy)
        {
            // Create a complete valid board
            var board = new SudokuBoard();
            FillBoard(board);

            // Remove cells based on difficulty
            int cellsToRemove = GetCellsToRemove(difficulty);
            RemoveCells(board, cellsToRemove);

            // Mark remaining cells as given
            foreach (var cell in board.GetAllCells())
            {
                if (cell.Value != 0)
                {
                    cell.IsGiven = true;
                }
            }

            return board;
        }

        /// <summary>
        /// Fills the board with a complete valid solution using backtracking.
        /// </summary>
        private bool FillBoard(SudokuBoard board)
        {
            // Find empty cell
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    if (board.GetCell(row, col).Value == 0)
                    {
                        // Try random numbers 1-9
                        var numbers = Enumerable.Range(1, 9).OrderBy(x => _random.Next()).ToList();
                        
                        foreach (int num in numbers)
                        {
                            if (IsValidPlacement(board, row, col, num))
                            {
                                board.SetCell(row, col, num);
                                
                                if (FillBoard(board))
                                {
                                    return true;
                                }
                                
                                board.SetCell(row, col, 0);
                            }
                        }
                        
                        return false;
                    }
                }
            }
            
            return true; // Board is complete
        }

        /// <summary>
        /// Checks if placing a number at the given position is valid.
        /// </summary>
        private bool IsValidPlacement(SudokuBoard board, int row, int col, int num)
        {
            // Check row
            foreach (var cell in board.GetRow(row))
            {
                if (cell.Value == num)
                    return false;
            }

            // Check column
            foreach (var cell in board.GetColumn(col))
            {
                if (cell.Value == num)
                    return false;
            }

            // Check 3x3 box
            int boxIndex = (row / 3) * 3 + (col / 3);
            foreach (var cell in board.GetBox(boxIndex))
            {
                if (cell.Value == num)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes cells from the board to create the puzzle.
        /// </summary>
        private void RemoveCells(SudokuBoard board, int count)
        {
            var positions = new List<(int row, int col)>();
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    positions.Add((row, col));
                }
            }

            // Shuffle positions
            positions = positions.OrderBy(x => _random.Next()).ToList();

            int removed = 0;
            foreach (var (row, col) in positions)
            {
                if (removed >= count)
                    break;

                var cell = board.GetCell(row, col);
                int backup = cell.Value;
                
                // Remove the cell
                board.SetCell(row, col, 0);

                // Check if puzzle still has unique solution (simplified version)
                // For now, just remove cells without checking uniqueness
                // TODO: Add proper uniqueness check for higher quality puzzles
                
                removed++;
            }
        }

        /// <summary>
        /// Determines how many cells to remove based on difficulty.
        /// </summary>
        private int GetCellsToRemove(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => _random.Next(36, 41),    // Leave 40-45 cells
                DifficultyLevel.Medium => _random.Next(42, 49),  // Leave 32-39 cells
                DifficultyLevel.Hard => _random.Next(50, 56),    // Leave 25-31 cells
                DifficultyLevel.Expert => _random.Next(57, 59),  // Leave 22-24 cells
                _ => 36
            };
        }

        /// <summary>
        /// Generates a hardcoded easy puzzle (stub for testing).
        /// This can be used as a fallback or for quick testing.
        /// </summary>
        public static SudokuBoard GenerateHardcodedPuzzle()
        {
            var board = new SudokuBoard();
            
            // A valid easy puzzle
            int[,] puzzle = new int[,]
            {
                {5, 3, 0, 0, 7, 0, 0, 0, 0},
                {6, 0, 0, 1, 9, 5, 0, 0, 0},
                {0, 9, 8, 0, 0, 0, 0, 6, 0},
                {8, 0, 0, 0, 6, 0, 0, 0, 3},
                {4, 0, 0, 8, 0, 3, 0, 0, 1},
                {7, 0, 0, 0, 2, 0, 0, 0, 6},
                {0, 6, 0, 0, 0, 0, 2, 8, 0},
                {0, 0, 0, 4, 1, 9, 0, 0, 5},
                {0, 0, 0, 0, 8, 0, 0, 7, 9}
            };

            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    int value = puzzle[row, col];
                    board.SetCell(row, col, value, value != 0);
                }
            }

            return board;
        }
    }
}
