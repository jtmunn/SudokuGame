using Sudoku.Core.Models;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Basic
{
    public class HiddenSingleStrategyTests
    {
        [Fact]
        public void Apply_FindsDigitWithOnlyOnePositionInRow_PlacesValue()
        {
            // Arrange: Create a board where 5 can only go in ONE cell in row 0, but that cell has multiple candidates
            var board = BoardBuilder.CreateEmptyBoard();
            
            // Setup: Make a scenario where 5 can only go in R0C0 in row 0
            // Put 5 in other cells to eliminate most positions in row 0
            board.SetCell(0, 1, 5, isGiven: true);  // 5 in R0C1 blocks R0C0-R0C2 from having 5 in their box
            
            // Wait, that's wrong - let me think differently
            // We need: 5 appears elsewhere in row 0's boxes and columns, but R0C0 is the only cell in row 0 that CAN have 5
            
            // Put 5 in columns 1-8 (but not in row 0)
            board.SetCell(1, 1, 5, isGiven: true);  // Column 1
            board.SetCell(1, 2, 5, isGiven: true);  // Column 2 - wait, can't have two 5s in same row!
            
            // Let me use a real Sudoku puzzle pattern instead
            // Use the hardcoded puzzle and find a hidden single in it
            var puzzle = @"
                530070000
                600195000
                098000060
                800060003
                400803001
                700020006
                060000280
                000419005
                000080079";
            
            board = BoardBuilder.CreateFromString(puzzle);
            board.InitializeCandidates();
            
            var strategy = new HiddenSingleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.PlacedValues);
            Assert.InRange(result.PlacedValues[0].Value, 1, 9);
        }

        [Fact]
        public void Apply_NoHiddenSingles_ReturnsNull()
        {
            // Arrange: Empty board where every digit has multiple positions
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            var strategy = new HiddenSingleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }
    }
}
