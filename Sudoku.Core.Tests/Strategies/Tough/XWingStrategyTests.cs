using Sudoku.Core.Models;
using Sudoku.Core.Strategies.Tough;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Tough
{
    public class XWingStrategyTests
    {
        [Fact]
        public void Apply_FindsXWingInRows_EliminatesCandidatesFromColumns()
        {
            // Arrange: Create an X-Wing pattern for digit 5
            // 5 appears in:
            // - Row 0: columns 2 and 6
            // - Row 4: columns 2 and 6
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Set up the X-Wing by limiting where 5 can go
            // In row 0, only allow 5 in columns 2 and 6
            for (int col = 0; col < 9; col++)
            {
                if (col != 2 && col != 6)
                {
                    var cell = board.GetCell(0, col);
                    cell.RemoveCandidate(5);
                }
            }
            
            // In row 4, only allow 5 in columns 2 and 6
            for (int col = 0; col < 9; col++)
            {
                if (col != 2 && col != 6)
                {
                    var cell = board.GetCell(4, col);
                    cell.RemoveCandidate(5);
                }
            }
            
            // Ensure some cells in columns 2 and 6 (other rows) have 5 as candidate
            // These should be eliminated
            Assert.True(board.GetCell(2, 2).HasCandidate(5));  // Should have 5 before strategy
            Assert.True(board.GetCell(7, 6).HasCandidate(5));  // Should have 5 before strategy
            
            var strategy = new XWingStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 5 from other rows in columns 2 and 6
            Assert.All(result.RemovedCandidates, e => Assert.Equal(5, e.Candidate));
            Assert.All(result.RemovedCandidates, e => Assert.True(e.Col == 2 || e.Col == 6));
            Assert.All(result.RemovedCandidates, e => Assert.True(e.Row != 0 && e.Row != 4));
        }

        [Fact]
        public void Apply_NoXWingPattern_ReturnsNull()
        {
            // Arrange: Empty board with no X-Wing patterns
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            var strategy = new XWingStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }
    }
}
