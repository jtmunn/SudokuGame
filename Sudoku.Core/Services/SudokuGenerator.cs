using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Difficulty levels for generated puzzles.
    /// Based on logical solving strategies required (not clue count).
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,      // Score: 40-60 (Target: 50, Basic strategies)
        Medium,    // Score: 160-240 (Target: 200, Basic + X-Wing/Y-Wing)
        Hard,      // Score: 280-420 (Target: 350, Add Swordfish)
        Expert,    // Score: 400-600 (Target: 500, Add XY-Chain)
        Evil       // Score: 560+ (Target: 700, Multiple advanced strategies)
    }

    /// <summary>
    /// Generates valid Sudoku puzzles with varying difficulty levels.
    /// Uses backtracking algorithm to create a complete valid solution,
    /// then removes cells based on logical difficulty (not clue count).
    /// </summary>
    public class SudokuGenerator
    {
        private readonly Random _random;
        private readonly SudokuSolver _solver;
        private readonly SudokuLogicalSolver _logicalSolver;

        public SudokuGenerator(SudokuSolver solver, SudokuLogicalSolver logicalSolver)
        {
            _random = new Random();
            _solver = solver;
            _logicalSolver = logicalSolver;
        }

        /// <summary>
        /// Generates a new Sudoku puzzle with the specified difficulty.
        /// Uses clue count (industry standard) as primary driver, with logical difficulty as secondary validation.
        /// Based on SudokuWiki.org research: "usually leaves between twenty and thirty clues behind".
        /// </summary>
        public SudokuBoard Generate(DifficultyLevel difficulty = DifficultyLevel.Easy)
        {            
            // Create a complete valid board
            var board = new SudokuBoard();
            FillBoard(board);

            // Remove cells until we hit target clue count range (primary criterion)
            var (minClues, maxClues) = GetTargetClueCountRange(difficulty);
            int targetScore = GetTargetDifficultyScore(difficulty);
            RemoveCellsForDifficulty(board, minClues, maxClues, targetScore);

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
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    if (board.GetCell(row, col).Value == 0)
                    {
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
        /// Optimized version using direct array access to avoid IEnumerable allocations.
        /// </summary>
        private bool IsValidPlacement(SudokuBoard board, int row, int col, int num)
        {
            // Check row - direct array access
            for (int c = 0; c < SudokuBoard.Size; c++)
            {
                if (board.GetCell(row, c).Value == num)
                    return false;
            }

            // Check column - direct array access
            for (int r = 0; r < SudokuBoard.Size; r++)
            {
                if (board.GetCell(r, col).Value == num)
                    return false;
            }

            // Check 3x3 box - direct array access
            int boxRow = (row / 3) * 3;
            int boxCol = (col / 3) * 3;
            for (int r = boxRow; r < boxRow + 3; r++)
            {
                for (int c = boxCol; c < boxCol + 3; c++)
                {
                    if (board.GetCell(r, c).Value == num)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes cells from the board to achieve target clue count (primary) and reasonable difficulty score (secondary).
        /// Based on SudokuWiki.org standards: clue count determines difficulty, not score alone.
        /// Maintains unique solution constraint throughout removal process.
        /// </summary>
        private void RemoveCellsForDifficulty(SudokuBoard board, int minClues, int maxClues, int targetScore)
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

            int attempts = 0;
            int maxAttempts = positions.Count * 2; // Allow multiple passes if needed
            int currentClues = 81; // Start with full board

            foreach (var (row, col) in positions)
            {
                attempts++;
                if (attempts > maxAttempts)
                    break; // Safety: prevent infinite loop

                var cell = board.GetCell(row, col);
                if (cell.Value == 0)
                    continue; // Already empty

                // PRIMARY CRITERION: Check if we've reached target clue count range
                if (currentClues <= maxClues)
                {
                    // We're in the acceptable clue range, verify difficulty score
                    var testBoard = board.Clone();
                    var solveResult = _logicalSolver.Solve(testBoard);
                    
                    // SECONDARY CRITERION: Check if difficulty score is reasonable (within 50% to 150% of target)
                    if (solveResult.IsSolved && 
                        solveResult.DifficultyScore >= targetScore * 0.5 && 
                        solveResult.DifficultyScore <= targetScore * 1.5)
                    {
                        // Both criteria met - stop removing cells
                        break;
                    }
                    
                    // If we're at minimum clues, stop even if score isn't perfect
                    if (currentClues <= minClues)
                        break;
                }

                int backup = cell.Value;
                
                // Temporarily remove the cell
                board.SetCell(row, col, 0);
                currentClues--;

                // Check if puzzle still has unique solution
                if (!_solver.HasUniqueSolution(board))
                {
                    // Restore - removal would create multiple solutions
                    board.SetCell(row, col, backup);
                    currentClues++;
                    continue;
                }

                // Test if puzzle is still logically solvable
                var logicalTest = board.Clone();
                var logicalResult = _logicalSolver.Solve(logicalTest);

                if (!logicalResult.IsSolved)
                {
                    // Can't be solved with logic alone - restore and continue
                    board.SetCell(row, col, backup);
                    currentClues++;
                    continue;
                }

                // Cell successfully removed, continue to next position
            }
        }

        /// <summary>
        /// Gets the target clue count range based on desired difficulty level.
        /// Based on SudokuWiki.org industry standards and puzzle analysis.
        /// Returns (minClues, maxClues) where minClues is minimum acceptable givens.
        /// </summary>
        private (int minClues, int maxClues) GetTargetClueCountRange(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => (36, 46),     // 35-45 empty cells
                DifficultyLevel.Medium => (32, 35),    // 46-49 empty cells
                DifficultyLevel.Hard => (28, 31),      // 50-53 empty cells
                DifficultyLevel.Expert => (24, 27),    // 54-57 empty cells
                DifficultyLevel.Evil => (22, 25),      // 56-59 empty cells (minimum viable: 17)
                _ => (36, 46)
            };
        }

        /// <summary>
        /// Gets the target difficulty score based on desired difficulty level.
        /// Used as SECONDARY validation criterion after clue count is achieved.
        /// Adjusted to match currently implemented strategies.
        /// </summary>
        private int GetTargetDifficultyScore(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => 50,       // Basic strategies only
                DifficultyLevel.Medium => 200,    // Basic + X-Wing/Y-Wing
                DifficultyLevel.Hard => 350,      // Add Swordfish
                DifficultyLevel.Expert => 500,    // Add XY-Chain
                DifficultyLevel.Evil => 700,      // Multiple advanced strategies
                _ => 50
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
