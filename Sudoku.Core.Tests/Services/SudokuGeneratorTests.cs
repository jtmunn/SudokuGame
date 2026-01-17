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

        [Theory]
        [InlineData(DifficultyLevel.Easy, 36, 46)]
        [InlineData(DifficultyLevel.Medium, 32, 35)]
        [InlineData(DifficultyLevel.Hard, 28, 31)]
        [InlineData(DifficultyLevel.Expert, 24, 27)]
        [InlineData(DifficultyLevel.Evil, 22, 25)]
        public void Generate_PuzzleHasCorrectClueCountRange(DifficultyLevel difficulty, int minClues, int maxClues)
        {
            // Act
            var board = _generator.Generate(difficulty);

            // Assert
            var givenCells = board.GetAllCells().Count(c => c.IsGiven);
            Assert.InRange(givenCells, minClues, maxClues);
        }

        [Fact]
        public void Generate_EasyPuzzle_HasReasonableEmptyCellCount()
        {
            // Act - This test specifically addresses the bug where Easy had only 5 empty cells
            var board = _generator.Generate(DifficultyLevel.Easy);

            // Assert
            var emptyCells = board.GetAllCells().Count(c => c.Value == 0);
            var givenCells = board.GetAllCells().Count(c => c.IsGiven);
            
            // Easy should have 35-45 empty cells (36-46 given clues)
            Assert.InRange(emptyCells, 35, 45);
            Assert.InRange(givenCells, 36, 46);
            
            // Definitely NOT just 5 empty cells!
            Assert.True(emptyCells >= 30, $"Easy puzzle should have at least 30 empty cells, but had {emptyCells}");
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
