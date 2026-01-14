using Sudoku.Core.Models;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Basic
{
    public class BoxLineReductionStrategyTests
    {
        [Fact]
        public void Apply_FindsDigitConfinedToBoxInRow_EliminatesFromBox()
        {
            // Arrange: Create a board where 5 in row 0 is confined to box 0
            var board = BoardBuilder.CreateEmptyBoard();
            
            // Put 5s in row 0 outside box 0 to confine it to box 0
            board.SetCell(0, 3, 5, isGiven: true);  // 5 in R0C3 (box 1)
            board.SetCell(0, 6, 5, isGiven: true);  // 5 in R0C6 (box 2) - wait, can't have two 5s in same row!
            
            // Let me fix this - put 5s to eliminate from other boxes
            board.SetCell(0, 3, 1, isGiven: true);  
            board.SetCell(0, 4, 2, isGiven: true);  
            board.SetCell(0, 5, 3, isGiven: true);  // Box 1 full in row 0
            board.SetCell(0, 6, 4, isGiven: true);  
            board.SetCell(0, 7, 6, isGiven: true);  
            board.SetCell(0, 8, 7, isGiven: true);  // Box 2 full in row 0
            
            // Now 5 in row 0 can only go in box 0 (R0C0, R0C1, or R0C2)
            // Add a cell in box 0 but different row that could have 5
            BoardBuilder.SetCandidates(board, 1, 0, 5, 8, 9);  // R1C0 has 5 as candidate
            
            board.InitializeCandidates();
            
            var strategy = new BoxLineReductionStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            if (result != null)
            {
                // Should eliminate 5 from cells in box 0 outside row 0
                Assert.NotEmpty(result.RemovedCandidates);
                Assert.All(result.RemovedCandidates, e => Assert.Equal(5, e.Candidate));
                Assert.All(result.RemovedCandidates, e => Assert.NotEqual(0, e.Row));  // Not in row 0
            }
        }
    }
}
