using Sudoku.Core.Models;
using Sudoku.Core.Strategies;
using Sudoku.Core.Strategies.Basic;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Basic
{
    public class NakedTripleStrategyTests
    {
        [Fact]
        public void Apply_FindsNakedTripleInRow_EliminatesCandidates()
        {
            // Arrange: Create a board where R0C0={2,5,8}, R0C1={2,8}, R0C2={5,8}
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Set up the naked triple
            BoardBuilder.SetCandidates(board, 0, 0, 2, 5, 8);
            BoardBuilder.SetCandidates(board, 0, 1, 2, 8);
            BoardBuilder.SetCandidates(board, 0, 2, 5, 8);
            
            // Add cells with 2,5,8 as part of their candidates (should be eliminated)
            BoardBuilder.SetCandidates(board, 0, 3, 2, 3, 4);  // Has 2
            BoardBuilder.SetCandidates(board, 0, 4, 5, 6, 7);  // Has 5
            BoardBuilder.SetCandidates(board, 0, 5, 8, 9, 1);  // Has 8
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 2,5,8 from other cells in row 0
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 3 && e.Candidate == 2);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 4 && e.Candidate == 5);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 0 && e.Col == 5 && e.Candidate == 8);
        }

        [Fact]
        public void Apply_FindsNakedTripleInColumn_EliminatesCandidates()
        {
            // Arrange: Create a board where column 0 has a naked triple {1,4,9}
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Set up the naked triple in column 0
            BoardBuilder.SetCandidates(board, 0, 0, 1, 4, 9);
            BoardBuilder.SetCandidates(board, 1, 0, 1, 4);
            BoardBuilder.SetCandidates(board, 2, 0, 4, 9);
            
            // Add cells with 1,4,9 as part of their candidates
            BoardBuilder.SetCandidates(board, 3, 0, 1, 2, 3);  // Has 1
            BoardBuilder.SetCandidates(board, 4, 0, 4, 5, 6);  // Has 4
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 1,4,9 from other cells in column 0
            Assert.Contains(result.RemovedCandidates, e => e.Row == 3 && e.Col == 0 && e.Candidate == 1);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 4 && e.Col == 0 && e.Candidate == 4);
        }

        [Fact]
        public void Apply_FindsNakedTripleInBox_EliminatesCandidates()
        {
            // Arrange: Create a board where box 0 has a naked triple {3,6,7}
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Set up the naked triple in box 0 (rows 0-2, cols 0-2)
            BoardBuilder.SetCandidates(board, 0, 0, 3, 6);
            BoardBuilder.SetCandidates(board, 0, 1, 3, 7);
            BoardBuilder.SetCandidates(board, 1, 0, 6, 7);
            
            // Add cells with 3,6,7 as part of their candidates
            BoardBuilder.SetCandidates(board, 1, 1, 3, 4, 5);  // Has 3
            BoardBuilder.SetCandidates(board, 2, 2, 6, 8, 9);  // Has 6
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 3,6,7 from other cells in box 0
            Assert.Contains(result.RemovedCandidates, e => e.Row == 1 && e.Col == 1 && e.Candidate == 3);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 2 && e.Col == 2 && e.Candidate == 6);
        }

        [Fact]
        public void Apply_NoNakedTriple_ReturnsNull()
        {
            // Arrange: Board with no naked triples
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_InsufficientCells_ReturnsNull()
        {
            // Arrange: Board where row has only 2 cells with 2-3 candidates (need 3 for naked triple)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Only two cells with limited candidates
            BoardBuilder.SetCandidates(board, 0, 0, 2, 5);
            BoardBuilder.SetCandidates(board, 0, 1, 2, 8);
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_FourCandidatesInThreeCells_ReturnsNull()
        {
            // Arrange: Three cells but with 4 combined candidates (not a naked triple)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Combined candidates = {2,5,8,9} - not a valid naked triple
            BoardBuilder.SetCandidates(board, 0, 0, 2, 5, 8);
            BoardBuilder.SetCandidates(board, 0, 1, 2, 8, 9);
            BoardBuilder.SetCandidates(board, 0, 2, 5, 9);
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_VerifyDescriptionFormat()
        {
            // Arrange
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            BoardBuilder.SetCandidates(board, 0, 0, 2, 5, 8);
            BoardBuilder.SetCandidates(board, 0, 1, 2, 8);
            BoardBuilder.SetCandidates(board, 0, 2, 5, 8);
            BoardBuilder.SetCandidates(board, 0, 3, 2, 3, 4);
            
            var strategy = new NakedTripleStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Naked Triple", result.Description);
            Assert.Contains("row 1", result.Description);
        }

        [Fact]
        public void Properties_HaveCorrectValues()
        {
            // Arrange
            var strategy = new NakedTripleStrategy();

            // Assert
            Assert.Equal("Naked Triple", strategy.Name);
            Assert.Equal(40, strategy.DifficultyScore);
            Assert.Equal(StrategyCategory.Basic, strategy.Category);
        }
    }
}
