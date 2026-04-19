using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Solves Sudoku puzzles using backtracking algorithm.
    /// Can be used for hints, auto-solve, and solution validation.
    /// </summary>
    public class SudokuBacktrackingSolver
    {
        private readonly SudokuValidator _validator;

        public SudokuBacktrackingSolver(SudokuValidator validator)
        {
            _validator = validator;
        }

        /// <summary>
        /// Solves the given Sudoku board using backtracking.
        /// Returns true if a solution was found.
        /// </summary>
        public bool Solve(SudokuBoard board)
        {
            // Early detection: check if the initial board state is valid
            // This prevents trying to solve boards with conflicts (e.g., duplicate numbers in row)
            if (!_validator.IsValidState(board))
            {
                return false;
            }

            // Initialize bitsets for fast O(1) validation
            _validator.InitializeBitsets(board);
            bool result = SolveRecursive(board);
            _validator.ClearBitsets();
            return result;
        }

        /// <summary>
        /// Gets a hint for the next move. Returns the cell position and value.
        /// Optimized to solve the board once instead of cloning for each empty cell.
        /// </summary>
        public (int row, int col, int value)? GetHint(SudokuBoard board)
        {
            // Clone the board once and solve it
            var clonedBoard = board.Clone();
            if (!Solve(clonedBoard))
            {
                return null; // No solution available
            }

            // Find the first empty cell in the original board and return its solution
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value == 0 && !cell.IsGiven)
                    {
                        var solvedCell = clonedBoard.GetCell(row, col);
                        return (row, col, solvedCell.Value);
                    }
                }
            }

            return null; // No hint available (board already solved)
        }

        /// <summary>
        /// Checks if the puzzle has a unique solution.
        /// Uses bitsets for fast validation.
        /// </summary>
        public bool HasUniqueSolution(SudokuBoard board)
        {
            var clonedBoard = board.Clone();
            
            // Initialize bitsets for fast validation
            _validator.InitializeBitsets(clonedBoard);
            
            int solutionCount = 0;
            CountSolutions(clonedBoard, ref solutionCount, 2); // Stop after finding 2 solutions
            
            _validator.ClearBitsets();
            
            return solutionCount == 1;
        }

        /// <summary>
        /// Gets the complete solution for the puzzle.
        /// Returns null if no solution exists.
        /// </summary>
        public SudokuBoard? GetSolution(SudokuBoard board)
        {
            var clonedBoard = board.Clone();
            if (Solve(clonedBoard))
            {
                return clonedBoard;
            }
            return null;
        }

        /// <summary>
        /// Recursive backtracking algorithm to solve the puzzle.
        /// Uses bitsets for O(1) validation and MCV heuristic for cell selection.
        /// </summary>
        private bool SolveRecursive(SudokuBoard board)
        {
            // Find the next empty cell using MCV heuristic
            var emptyCell = FindEmptyCell(board);
            
            if (emptyCell == null)
            {
                // No empty cells, puzzle is solved
                return true;
            }

            int row = emptyCell.Value.row;
            int col = emptyCell.Value.col;

            // Try numbers 1-9
            for (int num = 1; num <= 9; num++)
            {
                if (_validator.IsValidMoveFast(row, col, num))
                {
                    board.SetCell(row, col, num);
                    _validator.SetBitsets(row, col, num);

                    if (SolveRecursive(board))
                    {
                        return true;
                    }

                    // Backtrack
                    board.SetCell(row, col, 0);
                    _validator.ClearBitsets(row, col, num);
                }
            }

            return false; // No solution found
        }

        /// <summary>
        /// Finds the next empty cell in the board using Most Constrained Variable (MCV) heuristic.
        /// Selects the empty cell with the fewest valid possibilities to minimize backtracking.
        /// This dramatically improves solving performance for difficult puzzles.
        /// </summary>
        private (int row, int col)? FindEmptyCell(SudokuBoard board)
        {
            int minPossibilities = 10;
            (int row, int col)? bestCell = null;

            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value == 0)
                    {
                        // Count valid possibilities for this cell
                        int possibilities = CountPossibilities(row, col);
                        
                        if (possibilities < minPossibilities)
                        {
                            minPossibilities = possibilities;
                            bestCell = (row, col);
                            
                            // Early exit: if only 1 possibility, can't do better
                            if (minPossibilities == 1)
                                return bestCell;
                        }
                    }
                }
            }

            return bestCell;
        }

        /// <summary>
        /// Counts how many valid numbers can be placed in a cell.
        /// Uses fast bitset validation.
        /// </summary>
        private int CountPossibilities(int row, int col)
        {
            int count = 0;
            for (int num = 1; num <= 9; num++)
            {
                if (_validator.IsValidMoveFast(row, col, num))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Counts the number of solutions for the puzzle (up to maxCount).
        /// Used to verify uniqueness of solutions.
        /// Optimized to use bitsets for fast validation.
        /// </summary>
        private void CountSolutions(SudokuBoard board, ref int count, int maxCount)
        {
            if (count >= maxCount)
                return;

            var emptyCell = FindEmptyCell(board);
            
            if (emptyCell == null)
            {
                // Found a solution
                count++;
                return;
            }

            int row = emptyCell.Value.row;
            int col = emptyCell.Value.col;

            for (int num = 1; num <= 9; num++)
            {
                if (_validator.IsValidMoveFast(row, col, num))
                {
                    board.SetCell(row, col, num);
                    _validator.SetBitsets(row, col, num);
                    
                    CountSolutions(board, ref count, maxCount);
                    
                    board.SetCell(row, col, 0);
                    _validator.ClearBitsets(row, col, num);

                    if (count >= maxCount)
                        return;
                }
            }
        }
    }
}
