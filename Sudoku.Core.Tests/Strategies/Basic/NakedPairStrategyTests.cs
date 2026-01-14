using Sudoku.Core.Models;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Basic
{
    public class NakedPairStrategyTests
    {
        [Fact]
        public void Apply_FindsNakedPairInRow_EliminatesCandidates()
        {
            // Arrange: Create a board where R0C0 and R0C1 both have only {3,7}
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Manually set up the naked pair
            BoardBuilder.SetCandidates(board, 0, 0, 3, 7);
            BoardBuilder.SetCandidates(board, 0, 1, 3, 7);
            
            // Add cells with {3,7} as part of their candidates
            BoardBuilder.SetCandidates(board, 0, 2, 3, 4, 5);  // Has 3
            BoardBuilder.SetCandidates(board, 0, 3, 7, 8, 9);  // Has 7
            
            var strategy = new NakedPairStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 3 from R0C2 and 7 from R0C3
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 2 && e.Candidate == 3);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 3 && e.Candidate == 7);
        }

        [Fact]
        public void Apply_NoNakedPairs_ReturnsNull()
        {
            // Arrange: Board with no naked pairs
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            var strategy = new NakedPairStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }
    }
}
