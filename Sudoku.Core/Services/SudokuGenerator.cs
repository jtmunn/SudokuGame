using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Difficulty levels for generated puzzles.
    /// Ranges based on number of given cells (clues).
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,         // 46-50 given cells (clues)
        Medium,       // 36-45 given cells (clues)
        Hard,         // 32-35 given cells (clues)
        Expert,       // 28-31 given cells (clues)
        Master,       // 25-27 given cells (clues)
        GrandMaster   // 22-24 given cells (clues)
    }

    /// <summary>
    /// Generates valid Sudoku puzzles with varying difficulty levels.
    /// Uses backtracking algorithm to create a complete valid solution,
    /// then removes cells based on difficulty while ensuring unique solution.
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
        /// Uses uniqueness checking to ensure puzzle has exactly one solution.
        /// </summary>
        private void RemoveCells(SudokuBoard board, int targetCount)
        {
            var positions = new List<(int row, int col)>();
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    positions.Add((row, col));
                }
            }

            // Shuffle positions to randomize removal pattern
            positions = positions.OrderBy(x => _random.Next()).ToList();

            int removed = 0;
            int attempts = 0;
            int maxAttempts = positions.Count;

            foreach (var (row, col) in positions)
            {
                if (removed >= targetCount)
                    break;

                attempts++;
                if (attempts > maxAttempts)
                    break; // Safety: prevent infinite loop

                var cell = board.GetCell(row, col);
                if (cell.Value == 0)
                    continue; // Already empty

                int backup = cell.Value;
                
                // Temporarily remove the cell
                board.SetCell(row, col, 0);

                // Check if puzzle still has unique solution
                if (_solver.HasUniqueSolution(board))
                {
                    // Keep it removed - puzzle still valid
                    removed++;
                }
                else
                {
                    // Restore the cell - removal would create multiple solutions
                    board.SetCell(row, col, backup);
                }
            }

            // Note: Final removed count may be less than target if uniqueness cannot be maintained
            // This is acceptable - puzzle quality is more important than exact difficulty
        }

        /// <summary>
        /// Determines how many cells to remove based on difficulty.
        /// Total cells = 81, so cells to remove = 81 - desired clues
        /// </summary>
        private int GetCellsToRemove(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => _random.Next(31, 36),         // Remove 31-35 cells ? Leave 46-50 clues
                DifficultyLevel.Medium => _random.Next(36, 46),       // Remove 36-45 cells ? Leave 36-45 clues
                DifficultyLevel.Hard => _random.Next(46, 50),         // Remove 46-49 cells ? Leave 32-35 clues
                DifficultyLevel.Expert => _random.Next(50, 54),       // Remove 50-53 cells ? Leave 28-31 clues
                DifficultyLevel.Master => _random.Next(54, 57),       // Remove 54-56 cells ? Leave 25-27 clues
                DifficultyLevel.GrandMaster => _random.Next(57, 60),  // Remove 57-59 cells ? Leave 22-24 clues
                _ => 31  // Default to Easy
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
