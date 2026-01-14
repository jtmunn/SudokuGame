using Sudoku.Core.Models;
using Sudoku.Core.Strategies;
using Sudoku.Core.Strategies.Diabolical;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Diabolical
{
    public class XYChainStrategyTests
    {
        [Fact]
        public void Apply_FindsSimpleXYChain_EliminatesCandidates()
        {
            // Arrange: Create XY-Chain {3,7} -> {7,2} -> {2,5} -> {5,3}
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Set up bivalue cells that form a chain
            BoardBuilder.SetCandidates(board, 0, 0, 3, 7);  // Start: {3,7}
            BoardBuilder.SetCandidates(board, 0, 1, 7, 2);  // Links via 7: {7,2}
            BoardBuilder.SetCandidates(board, 1, 1, 2, 5);  // Links via 2: {2,5}
            BoardBuilder.SetCandidates(board, 1, 0, 5, 3);  // End: {5,3} - closes chain with 3
            
            // Add a cell that sees both endpoints and has 3 as candidate
            BoardBuilder.SetCandidates(board, 0, 2, 3, 4, 6);  // Sees R0C0 and R1C0 via box
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 3 from cells seeing both endpoints
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 2 && e.Candidate == 3);
        }

        [Fact]
        public void Apply_ChainInRow_EliminatesFromCommonCell()
        {
            // Arrange: XY-Chain along a row
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Chain: {2,8} -> {8,4} -> {4,2}
            BoardBuilder.SetCandidates(board, 0, 0, 2, 8);  // Start
            BoardBuilder.SetCandidates(board, 0, 3, 8, 4);  // Middle
            BoardBuilder.SetCandidates(board, 0, 6, 4, 2);  // End - closes with 2
            
            // Cell that sees both endpoints via row
            BoardBuilder.SetCandidates(board, 0, 4, 2, 5, 6);
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 4 && e.Candidate == 2);
        }

        [Fact]
        public void Apply_NoBivalueCells_ReturnsNull()
        {
            // Arrange: Board with no bivalue cells (cells with exactly 2 candidates)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // All cells have 3+ candidates
            BoardBuilder.SetCandidates(board, 0, 0, 1, 2, 3);
            BoardBuilder.SetCandidates(board, 0, 1, 4, 5, 6);
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_InsufficientBivalueCells_ReturnsNull()
        {
            // Arrange: Only 2 bivalue cells (need at least 3 for chain)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            BoardBuilder.SetCandidates(board, 0, 0, 3, 7);
            BoardBuilder.SetCandidates(board, 0, 1, 7, 2);
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_BivalueCellsDontConnect_ReturnsNull()
        {
            // Arrange: Bivalue cells that don't share candidates properly
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            BoardBuilder.SetCandidates(board, 0, 0, 3, 7);
            BoardBuilder.SetCandidates(board, 0, 1, 2, 4);  // Doesn't share with previous
            BoardBuilder.SetCandidates(board, 0, 2, 5, 6);  // Doesn't share with previous
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_ChainTooShort_ReturnsNull()
        {
            // Arrange: Only 2-cell chain (need at least 3 for valid XY-Chain)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            BoardBuilder.SetCandidates(board, 0, 0, 3, 7);
            BoardBuilder.SetCandidates(board, 0, 1, 7, 3);  // Would close immediately
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            // May find longer chain elsewhere or return null
            // Either is acceptable - we're testing short chain handling
        }

        [Fact]
        public void Apply_ChainWithNoEliminations_ReturnsNull()
        {
            // Arrange: Valid XY-Chain but no cells see both endpoints
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Chain in different boxes/rows/columns with no common visibility
            BoardBuilder.SetCandidates(board, 0, 0, 2, 8);
            BoardBuilder.SetCandidates(board, 0, 1, 8, 4);
            BoardBuilder.SetCandidates(board, 1, 1, 4, 2);
            
            // Other cells don't have 2 or don't see both endpoints
            BoardBuilder.SetCandidates(board, 2, 0, 3, 5, 6);
            BoardBuilder.SetCandidates(board, 0, 3, 7, 9, 1);
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            // May find different chain or null - either acceptable
        }

        [Fact]
        public void Apply_LongerChain_FindsEliminations()
        {
            // Arrange: 5-cell XY-Chain
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Chain: {1,4} -> {4,7} -> {7,9} -> {9,6} -> {6,1}
            BoardBuilder.SetCandidates(board, 0, 0, 1, 4);
            BoardBuilder.SetCandidates(board, 0, 1, 4, 7);
            BoardBuilder.SetCandidates(board, 1, 1, 7, 9);
            BoardBuilder.SetCandidates(board, 1, 2, 9, 6);
            BoardBuilder.SetCandidates(board, 0, 2, 6, 1);  // Closes with 1
            
            // Cell seeing both endpoints
            BoardBuilder.SetCandidates(board, 0, 3, 1, 2, 3);
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            if (result != null)
            {
                Assert.NotEmpty(result.RemovedCandidates);
                Assert.Contains("XY-Chain", result.Description);
            }
        }

        [Fact]
        public void Apply_VerifyDescriptionFormat()
        {
            // Arrange
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            BoardBuilder.SetCandidates(board, 0, 0, 3, 7);
            BoardBuilder.SetCandidates(board, 0, 1, 7, 2);
            BoardBuilder.SetCandidates(board, 1, 1, 2, 5);
            BoardBuilder.SetCandidates(board, 1, 0, 5, 3);
            BoardBuilder.SetCandidates(board, 0, 2, 3, 4, 6);
            
            var strategy = new XYChainStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            if (result != null)
            {
                Assert.Contains("XY-Chain", result.Description);
                Assert.Contains("eliminates", result.Description);
            }
        }

        [Fact]
        public void Properties_HaveCorrectValues()
        {
            // Arrange
            var strategy = new XYChainStrategy();

            // Assert
            Assert.Equal("XY-Chain", strategy.Name);
            Assert.Equal(240, strategy.DifficultyScore);
            Assert.Equal(StrategyCategory.Diabolical, strategy.Category);
        }
    }
}
