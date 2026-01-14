using Sudoku.Core.Models;
using Sudoku.Core.Services;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Services
{
    public class SudokuValidatorTests
    {
        private readonly SudokuValidator _validator;

        public SudokuValidatorTests()
        {
            _validator = new SudokuValidator();
        }

        [Fact]
        public void IsValidMove_ValidMove_ReturnsTrue()
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var result = _validator.IsValidMove(board, 0, 0, 5);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(-1)]
        public void IsValidMove_InvalidValue_ReturnsFalse(int invalidValue)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var result = _validator.IsValidMove(board, 0, 0, invalidValue);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidMove_GivenCell_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var result = _validator.IsValidMove(board, 0, 0, 3);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidMove_ConflictInRow_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var result = _validator.IsValidMove(board, 0, 3, 5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidMove_ConflictInColumn_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var result = _validator.IsValidMove(board, 3, 0, 5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidMove_ConflictInBox_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var result = _validator.IsValidMove(board, 1, 1, 5);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidMove_NoConflict_ReturnsTrue()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var result = _validator.IsValidMove(board, 3, 3, 5);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasConflict_NoConflict_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var result = _validator.HasConflict(board, 0, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasConflict_EmptyCell_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var result = _validator.HasConflict(board, 0, 0);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasConflict_RowConflict_ReturnsTrue()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 3, 5, isGiven: false);

            // Act
            var result = _validator.HasConflict(board, 0, 3);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasConflict_ColumnConflict_ReturnsTrue()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(3, 0, 5, isGiven: false);

            // Act
            var result = _validator.HasConflict(board, 3, 0);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasConflict_BoxConflict_ReturnsTrue()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(1, 1, 5, isGiven: false);

            // Act
            var result = _validator.HasConflict(board, 1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UpdateErrorFlags_SetsErrorsForConflictingCells()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 3, 5, isGiven: false);

            // Act
            _validator.UpdateErrorFlags(board);

            // Assert
            // Both cells have conflicts with each other
            Assert.True(board.GetCell(0, 0).HasError);
            Assert.True(board.GetCell(0, 3).HasError);
        }

        [Fact]
        public void UpdateErrorFlags_ClearsErrorsWhenResolved()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 3, 5, isGiven: false);
            _validator.UpdateErrorFlags(board);
            board.SetCell(0, 3, 7, isGiven: false); // Fix the conflict

            // Act
            _validator.UpdateErrorFlags(board);

            // Assert
            Assert.False(board.GetCell(0, 3).HasError);
        }

        [Fact]
        public void IsSolved_EmptyBoard_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var result = _validator.IsSolved(board);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSolved_PartiallyFilledBoard_ReturnsFalse()
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
            var result = _validator.IsSolved(board);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSolved_ValidSolution_ReturnsTrue()
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
            var result = _validator.IsSolved(board);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSolved_CompleteButInvalid_ReturnsFalse()
        {
            // Arrange
            var board = BoardBuilder.CreateFromString(
                "111111111" +
                "222222222" +
                "333333333" +
                "444444444" +
                "555555555" +
                "666666666" +
                "777777777" +
                "888888888" +
                "999999999"
            );

            // Act
            var result = _validator.IsSolved(board);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetValidNumbers_EmptyCell_ReturnsValidOptions()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var validNumbers = _validator.GetValidNumbers(board, 0, 1);

            // Assert
            Assert.DoesNotContain(5, validNumbers); // 5 is already in row
            Assert.Equal(8, validNumbers.Count);
        }

        [Fact]
        public void GetValidNumbers_GivenCell_ReturnsEmptyList()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            var validNumbers = _validator.GetValidNumbers(board, 0, 0);

            // Assert
            Assert.Empty(validNumbers);
        }

        [Fact]
        public void GetValidNumbers_FilledCell_ReturnsEmptyList()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: false);

            // Act
            var validNumbers = _validator.GetValidNumbers(board, 0, 0);

            // Assert
            Assert.Empty(validNumbers);
        }

        [Fact]
        public void GetValidNumbers_HighlyConstrainedCell_ReturnsLimitedOptions()
        {
            // Arrange
            var board = new SudokuBoard();
            // Fill row with 1-8
            for (int i = 0; i < 8; i++)
            {
                board.SetCell(0, i, i + 1, isGiven: true);
            }

            // Act
            var validNumbers = _validator.GetValidNumbers(board, 0, 8);

            // Assert
            Assert.Single(validNumbers);
            Assert.Contains(9, validNumbers);
        }

        [Fact]
        public void IsValidState_EmptyBoard_ReturnsTrue()
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var result = _validator.IsValidState(board);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidState_ValidPartialBoard_ReturnsTrue()
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
            var result = _validator.IsValidState(board);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidState_InvalidBoard_ReturnsFalse()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 3, 5, isGiven: false);

            // Act
            var result = _validator.IsValidState(board);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CountCorrectCells_MatchesSolution_ReturnsCorrectCount()
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

            var solution = BoardBuilder.CreateFromString(
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
            var correctCount = _validator.CountCorrectCells(board, solution);

            // Assert
            Assert.True(correctCount >= 30); // At least the given cells should be correct
        }

        [Fact]
        public void CountCorrectCells_WrongValues_ReturnsLowerCount()
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
            board.SetCell(0, 2, 9, isGiven: false); // Wrong value

            var solution = BoardBuilder.CreateFromString(
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
            var correctCount = _validator.CountCorrectCells(board, solution);

            // Assert
            Assert.True(correctCount < 81); // Not all cells match
        }
    }
}
