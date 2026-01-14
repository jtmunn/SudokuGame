using Sudoku.Core.Services;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Services
{
    public class SudokuLogicalSolverTests
    {
        [Fact]
        public void Solve_EmptyBoard_ReturnsUnsolvedResult()
        {
            // Arrange: Empty board cannot be solved without guessing
            var board = BoardBuilder.CreateEmptyBoard();
            var solver = new SudokuLogicalSolver();

            // Act
            var result = solver.Solve(board);

            // Assert
            Assert.False(result.IsSolved);
            Assert.Equal(0, result.DifficultyScore);
        }

        [Fact]
        public void Solve_EasyPuzzle_SolvesAndCalculatesDifficulty()
        {
            // Arrange: A very easy puzzle (from SudokuGenerator hardcoded example)
            var board = BoardBuilder.CreateFromString(
                "530070000" +
                "600195000" +
                "098000060" +
                "800060003" +
                "400803001" +
                "700020006" +
                "060000280" +
                "000419005" +
                "000080079"
            );
            
            var solver = new SudokuLogicalSolver();

            // Act
            var result = solver.Solve(board);

            // Assert
            Assert.True(result.IsSolved, "Puzzle should be solvable with logical strategies");
            Assert.True(result.DifficultyScore > 0, "Should have a difficulty score");
            Assert.True(result.CalculatedDifficulty <= DifficultyLevel.Medium, 
                "Should be Easy or Medium difficulty");
            Assert.NotEmpty(result.StrategiesUsed);
            Assert.True(result.Iterations > 0);
            Assert.True(result.CellsFilled >= 49 && result.CellsFilled <= 51, 
                $"Should fill approximately 50 cells, but filled {result.CellsFilled}");
        }

        [Fact]
        public void Solve_TracksStrategyUsage()
        {
            // Arrange: Easy puzzle
            var board = BoardBuilder.CreateFromString(
                "530070000" +
                "600195000" +
                "098000060" +
                "800060003" +
                "400803001" +
                "700020006" +
                "060000280" +
                "000419005" +
                "000080079"
            );
            
            var solver = new SudokuLogicalSolver();

            // Act
            var result = solver.Solve(board);

            // Assert
            Assert.NotEmpty(result.StrategiesUsed);
            
            // Easy puzzles should primarily use basic strategies
            var basicStrategiesUsed = result.StrategiesUsed
                .Where(s => s.Strategy.Category == Sudoku.Core.Strategies.StrategyCategory.Basic)
                .ToList();
            
            Assert.NotEmpty(basicStrategiesUsed);
            
            // Verify at least one strategy was used multiple times
            Assert.Contains(result.StrategiesUsed, s => s.TimesUsed > 1);
        }

        [Fact]
        public void Solve_AlreadySolvedPuzzle_ReturnsImmediately()
        {
            // Arrange: A complete valid solution
            var board = BoardBuilder.CreateFromString(
                "534678912" +
                "672195348" +
                "198342567" +
                "859761423" +
                "426853791" +
                "713924856" +
                "961537284" +
                "287419635" +
                "345286179"
            );
            
            var solver = new SudokuLogicalSolver();

            // Act
            var result = solver.Solve(board);

            // Assert
            Assert.True(result.IsSolved);
            Assert.Empty(result.StrategiesUsed); // No strategies needed
            Assert.Equal(0, result.CellsFilled);  // No cells filled
            Assert.Equal(0, result.DifficultyScore); // No difficulty
        }

        [Fact]
        public void GetStrategies_ReturnsOrderedList()
        {
            // Arrange
            var solver = new SudokuLogicalSolver();

            // Act
            var strategies = solver.GetStrategies();

            // Assert
            Assert.NotEmpty(strategies);
            
            // Verify strategies are ordered by difficulty (ascending)
            for (int i = 0; i < strategies.Count - 1; i++)
            {
                Assert.True(strategies[i].DifficultyScore <= strategies[i + 1].DifficultyScore,
                    $"Strategies should be ordered by difficulty score: {strategies[i].Name} ({strategies[i].DifficultyScore}) " +
                    $"should be <= {strategies[i + 1].Name} ({strategies[i + 1].DifficultyScore})");
            }
        }

        [Fact]
        public void Solve_GetSummary_ReturnsReadableString()
        {
            // Arrange
            var board = BoardBuilder.CreateFromString(
                "530070000" +
                "600195000" +
                "098000060" +
                "800060003" +
                "400803001" +
                "700020006" +
                "060000280" +
                "000419005" +
                "000080079"
            );
            
            var solver = new SudokuLogicalSolver();

            // Act
            var result = solver.Solve(board);
            var summary = result.GetSummary();

            // Assert
            Assert.NotEmpty(summary);
            Assert.Contains("Solved", summary);
            Assert.Contains("Difficulty", summary);
            Assert.Contains("iterations", summary);
        }

        [Fact]
        public void Solve_PuzzleRequiringAdvancedStrategy_UsesToughStrategies()
        {
            // This test would require a puzzle that needs X-Wing or Y-Wing
            // For now, we'll skip it and document that we need harder test puzzles
            // TODO: Add test puzzles that require Tough strategies
            Assert.True(true, "Placeholder - need puzzle database with known difficulties");
        }
    }
}
