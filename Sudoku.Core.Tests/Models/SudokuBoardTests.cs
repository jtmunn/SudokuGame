using Sudoku.Core.Models;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Models
{
    public class SudokuBoardTests
    {
        [Fact]
        public void Constructor_InitializesEmptyBoard()
        {
            // Act
            var board = new SudokuBoard();

            // Assert
            Assert.NotNull(board);
            Assert.All(board.GetAllCells(), cell => Assert.Equal(0, cell.Value));
            Assert.All(board.GetAllCells(), cell => Assert.False(cell.IsGiven));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(4, 4)]
        [InlineData(8, 8)]
        public void GetCell_ValidCoordinates_ReturnsCell(int row, int col)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var cell = board.GetCell(row, col);

            // Assert
            Assert.NotNull(cell);
            Assert.Equal(row, cell.Row);
            Assert.Equal(col, cell.Column);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(9, 0)]
        [InlineData(0, 9)]
        [InlineData(10, 10)]
        public void GetCell_InvalidCoordinates_ThrowsArgumentOutOfRangeException(int row, int col)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => board.GetCell(row, col));
        }

        [Theory]
        [InlineData(0, 0, 5, true)]
        [InlineData(4, 4, 9, false)]
        [InlineData(8, 8, 1, true)]
        [InlineData(3, 5, 0, false)]
        public void SetCell_ValidValues_SetsCellCorrectly(int row, int col, int value, bool isGiven)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            board.SetCell(row, col, value, isGiven);

            // Assert
            var cell = board.GetCell(row, col);
            Assert.Equal(value, cell.Value);
            Assert.Equal(isGiven, cell.IsGiven);
        }

        [Theory]
        [InlineData(0, 0, -1)]
        [InlineData(0, 0, 10)]
        [InlineData(5, 5, 11)]
        public void SetCell_InvalidValue_ThrowsArgumentOutOfRangeException(int row, int col, int value)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => board.SetCell(row, col, value));
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, 9)]
        public void SetCell_InvalidCoordinates_ThrowsArgumentOutOfRangeException(int row, int col)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => board.SetCell(row, col, 5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        [InlineData(8)]
        public void GetRow_ReturnsAllCellsInRow(int targetRow)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var cells = board.GetRow(targetRow).ToList();

            // Assert
            Assert.Equal(9, cells.Count);
            Assert.All(cells, cell => Assert.Equal(targetRow, cell.Row));
            Assert.Equal(Enumerable.Range(0, 9).ToList(), cells.Select(c => c.Column).ToList());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(8)]
        public void GetColumn_ReturnsAllCellsInColumn(int targetCol)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var cells = board.GetColumn(targetCol).ToList();

            // Assert
            Assert.Equal(9, cells.Count);
            Assert.All(cells, cell => Assert.Equal(targetCol, cell.Column));
            Assert.Equal(Enumerable.Range(0, 9).ToList(), cells.Select(c => c.Row).ToList());
        }

        [Theory]
        [InlineData(0, 0, 2, 0, 2)] // Top-left box
        [InlineData(4, 3, 5, 3, 5)] // Middle box
        [InlineData(8, 6, 8, 6, 8)] // Bottom-right box
        public void GetBox_ReturnsAllCellsInBox(int boxIndex, int minRow, int maxRow, int minCol, int maxCol)
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var cells = board.GetBox(boxIndex).ToList();

            // Assert
            Assert.Equal(9, cells.Count);
            Assert.All(cells, cell => Assert.InRange(cell.Row, minRow, maxRow));
            Assert.All(cells, cell => Assert.InRange(cell.Column, minCol, maxCol));
        }

        [Fact]
        public void GetAllCells_Returns81Cells()
        {
            // Arrange
            var board = new SudokuBoard();

            // Act
            var cells = board.GetAllCells().ToList();

            // Assert
            Assert.Equal(81, cells.Count);
        }

        [Fact]
        public void ClearUserEntries_RemovesOnlyNonGivenCells()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: false);
            board.SetCell(1, 0, 7, isGiven: false);

            // Act
            board.ClearUserEntries();

            // Assert
            Assert.Equal(5, board.GetCell(0, 0).Value);  // Given cell remains
            Assert.Equal(0, board.GetCell(0, 1).Value);  // User entry cleared
            Assert.Equal(0, board.GetCell(1, 0).Value);  // User entry cleared
        }

        [Fact]
        public void Clear_RemovesAllCells()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: false);

            // Act
            board.Clear();

            // Assert
            Assert.All(board.GetAllCells(), cell => Assert.Equal(0, cell.Value));
            Assert.All(board.GetAllCells(), cell => Assert.False(cell.IsGiven));
        }

        [Fact]
        public void Clone_CreatesDeepCopy()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(1, 1, 3, isGiven: false);
            board.InitializeCandidates();

            // Act
            var clone = board.Clone();

            // Assert
            Assert.Equal(5, clone.GetCell(0, 0).Value);
            Assert.Equal(3, clone.GetCell(1, 1).Value);
            Assert.True(clone.GetCell(0, 0).IsGiven);
            Assert.False(clone.GetCell(1, 1).IsGiven);

            // Verify independence
            board.SetCell(0, 0, 9);
            Assert.Equal(5, clone.GetCell(0, 0).Value);
        }

        [Fact]
        public void Clone_CopiesCandidates()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.InitializeCandidates();
            var originalCandidates = board.GetCell(0, 1).Candidates.ToList();

            // Act
            var clone = board.Clone();

            // Assert
            Assert.Equal(originalCandidates, clone.GetCell(0, 1).Candidates.ToList());

            // Verify independence
            clone.GetCell(0, 1).RemoveCandidate(1);
            Assert.Contains(1, board.GetCell(0, 1).Candidates);
        }

        [Fact]
        public void InitializeCandidates_SetsCandidatesForEmptyCells()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            board.InitializeCandidates();

            // Assert
            Assert.Empty(board.GetCell(0, 0).Candidates); // Filled cell has no candidates
            Assert.NotEmpty(board.GetCell(0, 1).Candidates); // Empty cell has candidates
        }

        [Fact]
        public void InitializeCandidates_RemovesInvalidCandidates()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            board.InitializeCandidates();

            // Assert - R0C1 should not have 5 as candidate (same row as filled 5)
            Assert.DoesNotContain(5, board.GetCell(0, 1).Candidates);
        }

        [Fact]
        public void UpdateAllCandidates_RemovesConflictingCandidates()
        {
            // Arrange
            var board = new SudokuBoard();
            board.InitializeCandidates();
            board.SetCell(0, 0, 5, isGiven: true);

            // Act
            board.UpdateAllCandidates();

            // Assert
            Assert.DoesNotContain(5, board.GetCell(0, 1).Candidates); // Same row
            Assert.DoesNotContain(5, board.GetCell(1, 0).Candidates); // Same column
            Assert.DoesNotContain(5, board.GetCell(1, 1).Candidates); // Same box
        }

        [Fact]
        public void GetCellsWithNCandidates_ReturnsCorrectCells()
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
            board.InitializeCandidates();

            // Act
            var cellsWith2Candidates = board.GetCellsWithNCandidates(2).ToList();

            // Assert
            Assert.NotEmpty(cellsWith2Candidates);
            Assert.All(cellsWith2Candidates, cell => Assert.Equal(2, cell.CandidateCount));
        }

        [Fact]
        public void Serialize_CreatesValidString()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: false);

            // Act
            var serialized = board.Serialize();

            // Assert
            Assert.Contains('|', serialized);
            var parts = serialized.Split('|');
            Assert.Equal(3, parts.Length);
            Assert.Equal(81, parts[0].Length);
            Assert.Equal(81, parts[1].Length);
            Assert.Equal(81, parts[2].Length);
        }

        [Fact]
        public void Deserialize_ReconstructsBoard()
        {
            // Arrange
            var original = new SudokuBoard();
            original.SetCell(0, 0, 5, isGiven: true);
            original.SetCell(0, 1, 3, isGiven: false);
            var serialized = original.Serialize();

            // Act
            var deserialized = SudokuBoard.Deserialize(serialized);

            // Assert
            Assert.Equal(5, deserialized.GetCell(0, 0).Value);
            Assert.Equal(3, deserialized.GetCell(0, 1).Value);
            Assert.True(deserialized.GetCell(0, 0).IsGiven);
            Assert.False(deserialized.GetCell(0, 1).IsGiven);
        }

        [Fact]
        public void Serialize_PreservesHasErrorState()
        {
            // Arrange
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: false);
            board.GetCell(0, 1).HasError = true;

            // Act
            var serialized = board.Serialize();
            var deserialized = SudokuBoard.Deserialize(serialized);

            // Assert
            Assert.False(deserialized.GetCell(0, 0).HasError);
            Assert.True(deserialized.GetCell(0, 1).HasError);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("123|456")]
        [InlineData("")]
        public void Deserialize_InvalidData_ThrowsArgumentException(string invalidData)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => SudokuBoard.Deserialize(invalidData));
        }
    }
}
