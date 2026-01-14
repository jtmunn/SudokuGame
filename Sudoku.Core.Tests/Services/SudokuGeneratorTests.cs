using Sudoku.Core.Models;
using Sudoku.Core.Services;
using Xunit;

namespace Sudoku.Core.Tests.Services
{
    public class SudokuGeneratorTests
    {
        private readonly SudokuGenerator _generator;
        private readonly SudokuValidator _validator;
        private readonly SudokuSolver _solver;

        public SudokuGeneratorTests()
        {
            _validator = new SudokuValidator();
            _solver = new SudokuSolver(_validator);
            var logicalSolver = new SudokuLogicalSolver();
            _generator = new SudokuGenerator(_solver, logicalSolver);
        }

        [Fact]
        public void Generate_CreatesValidPuzzle()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            Assert.NotNull(board);
            Assert.True(_validator.IsValidState(board));
        }

        [Fact]
        public void Generate_PuzzleHasGivenCells()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            var givenCells = board.GetAllCells().Count(c => c.IsGiven);
            Assert.True(givenCells > 0, "Generated puzzle should have given cells");
            Assert.True(givenCells < 81, "Generated puzzle should have some empty cells");
        }

        [Fact]
        public void Generate_PuzzleIsSolvable()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);
            var clonedBoard = board.Clone();

            // Assert
            var result = _solver.Solve(clonedBoard);
            Assert.True(result, "Generated puzzle should be solvable");
            Assert.True(_validator.IsSolved(clonedBoard));
        }

        [Fact]
        public void Generate_PuzzleHasUniqueSolution()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            Assert.True(_solver.HasUniqueSolution(board), "Generated puzzle should have unique solution");
        }

        [Theory]
        [InlineData(DifficultyLevel.Easy)]
        [InlineData(DifficultyLevel.Medium)]
        [InlineData(DifficultyLevel.Hard)]
        [InlineData(DifficultyLevel.Expert)]
        [InlineData(DifficultyLevel.Evil)]
        public void Generate_AllDifficulties_CreateValidPuzzles(DifficultyLevel difficulty)
        {
            // Act
            var board = _generator.Generate(difficulty);

            // Assert
            Assert.NotNull(board);
            Assert.True(_validator.IsValidState(board));
            var givenCells = board.GetAllCells().Count(c => c.IsGiven);
            Assert.True(givenCells > 0);
            Assert.True(givenCells < 81);
        }

        [Fact]
        public void Generate_DifferentCallsProduceDifferentPuzzles()
        {
            // Act
            var board1 = _generator.Generate(DifficultyLevel.Easy);
            var board2 = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            var board1String = board1.Serialize();
            var board2String = board2.Serialize();
            
            Assert.NotEqual(board1String, board2String);
        }

        [Fact]
        public void Generate_AllGivenCellsAreMarkedCorrectly()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            foreach (var cell in board.GetAllCells())
            {
                if (cell.Value != 0)
                {
                    Assert.True(cell.IsGiven, $"Cell at R{cell.Row}C{cell.Column} has value but IsGiven=false");
                }
                else
                {
                    Assert.False(cell.IsGiven, $"Empty cell at R{cell.Row}C{cell.Column} should not be marked as given");
                }
            }
        }

        [Fact]
        public void Generate_NoConflictsInInitialState()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            _validator.UpdateErrorFlags(board);
            var cellsWithErrors = board.GetAllCells().Count(c => c.HasError);
            Assert.Equal(0, cellsWithErrors);
        }

        [Fact]
        public void Generate_EasyPuzzle_HasReasonableNumberOfGivens()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            var givenCells = board.GetAllCells().Count(c => c.IsGiven);
            // Puzzles should have some given cells and some empty cells
            // The generator aims for difficulty score rather than clue count
            Assert.InRange(givenCells, 17, 80);  // 17 is theoretical minimum for unique solution
        }

        [Fact]
        public void GenerateHardcodedPuzzle_ReturnsValidPuzzle()
        {
            // Act
            var board = SudokuGenerator.GenerateHardcodedPuzzle();

            // Assert
            Assert.NotNull(board);
            Assert.True(_validator.IsValidState(board));
        }

        [Fact]
        public void GenerateHardcodedPuzzle_IsSolvable()
        {
            // Act
            var board = SudokuGenerator.GenerateHardcodedPuzzle();
            var clonedBoard = board.Clone();

            // Assert
            var result = _solver.Solve(clonedBoard);
            Assert.True(result);
            Assert.True(_validator.IsSolved(clonedBoard));
        }

        [Fact]
        public void GenerateHardcodedPuzzle_AlwaysReturnsSamePuzzle()
        {
            // Act
            var board1 = SudokuGenerator.GenerateHardcodedPuzzle();
            var board2 = SudokuGenerator.GenerateHardcodedPuzzle();

            // Assert
            Assert.Equal(board1.Serialize(), board2.Serialize());
        }

        [Fact]
        public void Generate_SolutionIsComplete()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);
            var solution = _solver.GetSolution(board);

            // Assert
            Assert.NotNull(solution);
            Assert.True(_validator.IsSolved(solution!));
            
            // Verify solution has all 81 cells filled
            var filledCells = solution!.GetAllCells().Count(c => c.Value != 0);
            Assert.Equal(81, filledCells);
        }

        [Fact]
        public void Generate_PreservesGivenCellsInSolution()
        {
            // Act
            var board = _generator.Generate(DifficultyLevel.Easy);
            var solution = _solver.GetSolution(board);

            // Assert
            Assert.NotNull(solution);
            
            // All given cells should have same values in solution
            foreach (var cell in board.GetAllCells())
            {
                if (cell.IsGiven)
                {
                    var solutionCell = solution!.GetCell(cell.Row, cell.Column);
                    Assert.Equal(cell.Value, solutionCell.Value);
                }
            }
        }
    }
}
