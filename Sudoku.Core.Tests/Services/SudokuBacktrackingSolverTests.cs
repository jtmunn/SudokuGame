using Sudoku.Core.Models;
using Sudoku.Core.Services;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Services
{
    public class SudokuBacktrackingSolverTests
    {
        private readonly SudokuBacktrackingSolver _solver;
        private readonly SudokuValidator _validator;

        public SudokuBacktrackingSolverTests()
        {
            _validator = new SudokuValidator();
            _solver = new SudokuBacktrackingSolver(_validator);
        }

        [Fact]
        public void Solve_ValidPuzzle_ReturnsTrueAndSolves()
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

            // Act
            var result = _solver.Solve(board);

            // Assert
            Assert.True(result);
            Assert.True(_validator.IsSolved(board));
        }

        [Fact]
        public void Solve_AlreadySolved_ReturnsTrue()
        {
            // Arrange
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

            // Act
            var result = _solver.Solve(board);

            // Assert
            Assert.True(result);
            Assert.True(_validator.IsSolved(board));
        }

        [Fact]
        public void Solve_InvalidPuzzle_ReturnsFalse()
        {
            // Arrange - Create an impossible puzzle
            var board = new SudokuBoard();
            board.SetCell(0, 0, 1, isGiven: true);
            board.SetCell(0, 1, 1, isGiven: true); // Duplicate in row

            // Act
            var result = _solver.Solve(board);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Solve_HardPuzzle_SolvesSuccessfully()
        {
            // Arrange - A harder puzzle
            var board = BoardBuilder.CreateFromString(
                "800000000" +
                "003600000" +
                "070090200" +
                "050007000" +
                "000045700" +
                "000100030" +
                "001000068" +
                "008500010" +
                "090000400"
            );

            // Act
            var result = _solver.Solve(board);

            // Assert
            Assert.True(result);
            Assert.True(_validator.IsSolved(board));
        }

        [Fact]
        public void GetHint_ValidPuzzle_ReturnsHint()
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

            // Act
            var hint = _solver.GetHint(board);

            // Assert
            Assert.NotNull(hint);
            Assert.InRange(hint.Value.row, 0, 8);
            Assert.InRange(hint.Value.col, 0, 8);
            Assert.InRange(hint.Value.value, 1, 9);
        }

        [Fact]
        public void GetHint_SolvedPuzzle_ReturnsNull()
        {
            // Arrange
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

            // Act
            var hint = _solver.GetHint(board);

            // Assert
            Assert.Null(hint);
        }

        [Fact]
        public void GetHint_HintIsValid_CanBePlaced()
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

            // Act
            var hint = _solver.GetHint(board);

            // Assert
            Assert.NotNull(hint);
            Assert.True(_validator.IsValidMove(board, hint.Value.row, hint.Value.col, hint.Value.value));
        }

        [Fact]
        public void GetHint_DoesNotModifyBoard()
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
            var originalSerialized = board.Serialize();

            // Act
            _solver.GetHint(board);

            // Assert
            Assert.Equal(originalSerialized, board.Serialize());
        }

        [Fact]
        public void Solve_PreservesGivenCells()
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
            var givenCells = board.GetAllCells().Where(c => c.IsGiven).ToList();

            // Act
            _solver.Solve(board);

            // Assert
            foreach (var givenCell in givenCells)
            {
                var solvedCell = board.GetCell(givenCell.Row, givenCell.Column);
                Assert.True(solvedCell.IsGiven);
            }
        }
    }
}


