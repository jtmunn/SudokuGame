using Sudoku.Core.Models;
using Xunit;

namespace Sudoku.Core.Tests.Models
{
    public class SudokuCellTests
    {
        [Fact]
        public void Constructor_InitializesEmptyCell()
        {
            // Act
            var cell = new SudokuCell(3, 5);

            // Assert
            Assert.Equal(3, cell.Row);
            Assert.Equal(5, cell.Column);
            Assert.Equal(0, cell.Value);
            Assert.False(cell.IsGiven);
            Assert.False(cell.HasError);
            Assert.Empty(cell.Candidates);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(5, false)]
        [InlineData(9, true)]
        public void ValueAndIsGiven_CanBeSet(int value, bool isGiven)
        {
            // Arrange
            var cell = new SudokuCell(0, 0);

            // Act
            cell.Value = value;
            cell.IsGiven = isGiven;

            // Assert
            Assert.Equal(value, cell.Value);
            Assert.Equal(isGiven, cell.IsGiven);
        }

        [Fact]
        public void InitializeCandidates_EmptyCell_SetsAllNineCandidates()
        {
            // Arrange
            var cell = new SudokuCell(0, 0);

            // Act
            cell.InitializeCandidates();

            // Assert
            Assert.Equal(9, cell.CandidateCount);
            Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, cell.Candidates.OrderBy(c => c));
        }

        [Fact]
        public void InitializeCandidates_FilledCell_ClearsCandidates()
        {
            // Arrange
            var cell = new SudokuCell(0, 0);
            cell.Candidates.Add(5);
            cell.Value = 3;

            // Act
            cell.InitializeCandidates();

            // Assert
            Assert.Empty(cell.Candidates);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void RemoveCandidate_RemovesSpecifiedCandidate(int candidateToRemove)
        {
            // Arrange
            var cell = new SudokuCell(0, 0);
            cell.InitializeCandidates();

            // Act
            cell.RemoveCandidate(candidateToRemove);

            // Assert
            Assert.DoesNotContain(candidateToRemove, cell.Candidates);
            Assert.Equal(8, cell.CandidateCount);
        }

        [Fact]
        public void RemoveCandidate_NonExistentCandidate_DoesNothing()
        {
            // Arrange
            var cell = new SudokuCell(0, 0);
            cell.Candidates.Add(5);

            // Act
            cell.RemoveCandidate(3);

            // Assert
            Assert.Single(cell.Candidates);
            Assert.Contains(5, cell.Candidates);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void AddCandidate_ValidValue_AddsCandidate(int candidate)
        {
            // Arrange
            var cell = new SudokuCell(0, 0);

            // Act
            cell.AddCandidate(candidate);

            // Assert
            Assert.Contains(candidate, cell.Candidates);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(-1)]
        public void AddCandidate_InvalidValue_DoesNotAdd(int candidate)
        {
            // Arrange
            var cell = new SudokuCell(0, 0);

            // Act
            cell.AddCandidate(candidate);

            // Assert
            Assert.Empty(cell.Candidates);
        }

        [Fact]
        public void AddCandidate_DuplicateValue_DoesNotAddTwice()
        {
            // Arrange
            var cell = new SudokuCell(0, 0);

            // Act
            cell.AddCandidate(5);
            cell.AddCandidate(5);

            // Assert
            Assert.Single(cell.Candidates);
            Assert.Contains(5, cell.Candidates);
        }

        [Theory]
        [InlineData(5, true)]
        [InlineData(3, false)]
        public void HasCandidate_ReturnsCorrectValue(int candidateToCheck, bool expectedResult)
        {
            // Arrange
            var cell = new SudokuCell(0, 0);
            cell.Candidates.Add(5);

            // Act
            var result = cell.HasCandidate(candidateToCheck);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(0, 9)] // Remove 0, expect 9 remaining
        [InlineData(5, 4)] // Remove 5, expect 4 remaining
        public void CandidateCount_ReturnsCorrectCount(int howMany, int expected)
        {
            // Arrange
            var cell = new SudokuCell(0, 0);
            cell.InitializeCandidates();

            // Act - Remove candidates to test count
            for (int i = 1; i <= howMany; i++)
            {
                cell.RemoveCandidate(i);
            }

            // Assert
            Assert.Equal(expected, cell.CandidateCount);
        }

        [Theory]
        [InlineData(0, 0, 0)] // Top-left box
        [InlineData(0, 3, 1)] // Top-middle box
        [InlineData(0, 6, 2)] // Top-right box
        [InlineData(3, 0, 3)] // Middle-left box
        [InlineData(4, 4, 4)] // Center box
        [InlineData(5, 8, 5)] // Middle-right box
        [InlineData(6, 0, 6)] // Bottom-left box
        [InlineData(7, 4, 7)] // Bottom-middle box
        [InlineData(8, 8, 8)] // Bottom-right box
        public void GetBoxIndex_ReturnsCorrectBox(int row, int col, int expectedBox)
        {
            // Arrange
            var cell = new SudokuCell(row, col);

            // Act
            var boxIndex = cell.GetBoxIndex();

            // Assert
            Assert.Equal(expectedBox, boxIndex);
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new SudokuCell(3, 5)
            {
                Value = 7,
                IsGiven = true,
                HasError = false
            };
            original.Candidates.Add(1);
            original.Candidates.Add(2);

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal(original.Row, clone.Row);
            Assert.Equal(original.Column, clone.Column);
            Assert.Equal(original.Value, clone.Value);
            Assert.Equal(original.IsGiven, clone.IsGiven);
            Assert.Equal(original.HasError, clone.HasError);
            Assert.Equal(original.Candidates, clone.Candidates);

            // Verify independence
            original.Value = 9;
            original.Candidates.Add(3);
            Assert.Equal(7, clone.Value);
            Assert.DoesNotContain(3, clone.Candidates);
        }

        [Fact]
        public void HasError_CanBeSetAndRead()
        {
            // Arrange
            var cell = new SudokuCell(0, 0);

            // Act
            cell.HasError = true;

            // Assert
            Assert.True(cell.HasError);
        }
    }
}
