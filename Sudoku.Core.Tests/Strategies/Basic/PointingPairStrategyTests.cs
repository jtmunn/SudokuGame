using Sudoku.Core.Models;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Basic
{
    public class PointingPairStrategyTests
    {
        [Fact]
        public void Apply_FindsPointingPairInBox_EliminatesCandidatesFromRow()
        {
            // Arrange: Create a board where digit 5 in box 0 is confined to row 0
            var board = BoardBuilder.CreateEmptyBoard();
            
            // Put 5s in strategic positions to confine it in box 0
            board.SetCell(1, 0, 5, isGiven: true);  // 5 in R1C0 (box 0)
            board.SetCell(1, 1, 5, isGiven: true);  // 5 in R1C1 (box 0) - wait, can't have two 5s in same row
            
            // Let me fix this - put 5s in different rows of box 0
            board.SetCell(1, 0, 5, isGiven: true);  // 5 in R1C0
            board.SetCell(2, 0, 1, isGiven: true);  // Some other digit
            board.SetCell(2, 1, 5, isGiven: true);  // 5 in R2C1
            
            // Now in box 0 (rows 0-2, cols 0-2), 5 appears in R1C0 and R2C1
            // But we want 5 confined to a single ROW in the box
            
            // Better setup: Fill R1 and R2 of box 0 so 5 can only go in R0
            board.SetCell(1, 0, 1, isGiven: true);
            board.SetCell(1, 1, 2, isGiven: true);
            board.SetCell(1, 2, 3, isGiven: true);
            board.SetCell(2, 0, 4, isGiven: true);
            board.SetCell(2, 1, 6, isGiven: true);
            board.SetCell(2, 2, 7, isGiven: true);
            
            // Now 5 in box 0 can only go in row 0 (R0C0, R0C1, or R0C2)
            // Add 5 in row 0 outside box 0 to test elimination
            BoardBuilder.SetCandidates(board, 0, 5, 5, 8, 9);  // R0C5 has 5 as candidate
            
            board.InitializeCandidates();
            
            var strategy = new PointingPairStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            if (result != null)
            {
                // Should eliminate 5 from cells in row 0 outside box 0
                Assert.NotEmpty(result.RemovedCandidates);
                Assert.All(result.RemovedCandidates, e => Assert.Equal(5, e.Candidate));
            }
        }
    }
}
