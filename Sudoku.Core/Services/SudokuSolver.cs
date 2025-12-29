using Sudoku.Core.Models;

namespace Sudoku.Core.Services
{
    /// <summary>
    /// Solves Sudoku puzzles using backtracking algorithm.
    /// Can be used for hints, auto-solve, and solution validation.
    /// </summary>
    public class SudokuSolver
    {
        private readonly SudokuValidator _validator;

        public SudokuSolver(SudokuValidator validator)
        {
            _validator = validator;
        }

        /// <summary>
        /// Solves the given Sudoku board using backtracking.
        /// Returns true if a solution was found.
        /// </summary>
        public bool Solve(SudokuBoard board)
        {
            return SolveRecursive(board);
        }

        /// <summary>
        /// Gets a hint for the next move. Returns the cell position and value.
        /// </summary>
        public (int row, int col, int value)? GetHint(SudokuBoard board)
        {
            // Find an empty cell
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value == 0 && !cell.IsGiven)
                    {
                        // Try to solve and find the correct value
                        var clonedBoard = board.Clone();
                        if (Solve(clonedBoard))
                        {
                            var solvedCell = clonedBoard.GetCell(row, col);
                            return (row, col, solvedCell.Value);
                        }
                    }
                }
            }

            return null; // No hint available
        }

        /// <summary>
        /// Checks if the puzzle has a unique solution.
        /// </summary>
        public bool HasUniqueSolution(SudokuBoard board)
        {
            var clonedBoard = board.Clone();
            int solutionCount = 0;
            CountSolutions(clonedBoard, ref solutionCount, 2); // Stop after finding 2 solutions
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
        /// </summary>
        private bool SolveRecursive(SudokuBoard board)
        {
            // Find the next empty cell
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
                if (_validator.IsValidMove(board, row, col, num))
                {
                    board.SetCell(row, col, num);

                    if (SolveRecursive(board))
                    {
                        return true;
                    }

                    // Backtrack
                    board.SetCell(row, col, 0);
                }
            }

            return false; // No solution found
        }

        /// <summary>
        /// Finds the next empty cell in the board.
        /// Uses a strategy to find cells with fewer possibilities first (for optimization).
        /// </summary>
        private (int row, int col)? FindEmptyCell(SudokuBoard board)
        {
            // Simple approach: find first empty cell
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value == 0)
                    {
                        return (row, col);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Counts the number of solutions for the puzzle (up to maxCount).
        /// Used to verify uniqueness of solutions.
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
                if (_validator.IsValidMove(board, row, col, num))
                {
                    board.SetCell(row, col, num);
                    CountSolutions(board, ref count, maxCount);
                    board.SetCell(row, col, 0);

                    if (count >= maxCount)
                        return;
                }
            }
        }

        /// <summary>
        /// Gets the difficulty rating of a puzzle (0-100, higher is harder).
        /// Based on number of givens and solving complexity.
        /// </summary>
        public int GetDifficultyRating(SudokuBoard board)
        {
            int givenCount = board.GetAllCells().Count(c => c.IsGiven);
            
            // Simple difficulty based on number of givens
            // More sophisticated analysis could be added later
            if (givenCount >= 40)
                return 20; // Easy
            else if (givenCount >= 32)
                return 40; // Medium
            else if (givenCount >= 25)
                return 60; // Hard
            else
                return 80; // Expert
        }
    }
}
