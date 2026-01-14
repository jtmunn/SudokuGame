using Sudoku.Core.Models;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Basic
{
    public class NakedSingleStrategyTests
    {
        [Fact]
        public void Apply_FindsCellWithOneCandidate_PlacesValue()
        {
            // Arrange: Create a simple board where R0C0 has only candidate 7
            var board = BoardBuilder.CreateEmptyBoard();
            board.SetCell(0, 1, 1, isGiven: true);  // Force R0C0 to not have candidate 1
            board.SetCell(0, 2, 2, isGiven: true);
            board.SetCell(0, 3, 3, isGiven: true);
            board.SetCell(0, 4, 4, isGiven: true);
            board.SetCell(0, 5, 5, isGiven: true);
            board.SetCell(0, 6, 6, isGiven: true);
            board.SetCell(0, 8, 8, isGiven: true);
            board.SetCell(1, 0, 9, isGiven: true);
            
            board.InitializeCandidates();  // R0C0 should now only have candidate 7
            
            var strategy = new NakedSingleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.PlacedValues);
            Assert.Equal(7, result.PlacedValues[0].Value);
            Assert.Equal(0, result.PlacedValues[0].Row);
            Assert.Equal(0, result.PlacedValues[0].Col);
        }

        [Fact]
        public void Apply_NoCellsWithOneCandidate_ReturnsNull()
        {
            // Arrange: Empty board with all candidates available
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            var strategy = new NakedSingleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }
    }
}
