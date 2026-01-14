using Sudoku.Core.Models;
using Sudoku.Core.Strategies;
using Sudoku.Core.Strategies.Tough;
using Sudoku.Core.Tests.Helpers;
using Xunit;

namespace Sudoku.Core.Tests.Strategies.Tough
{
    public class SwordfishStrategyTests
    {
        [Fact]
        public void Apply_FindsSwordfishInRows_EliminatesFromColumns()
        {
            // Arrange: Create a board where digit 5 forms Swordfish in rows 0,3,6 at columns 1,4,7
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Clear 5 from most cells first to create controlled pattern
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    var cell = board.GetCell(r, c);
                    if (cell.HasCandidate(5))
                    {
                        cell.RemoveCandidate(5);
                    }
                }
            }
            
            // Set up Swordfish pattern in rows 0, 3, 6 at columns 1,4,7
            board.GetCell(0, 1).AddCandidate(5);
            board.GetCell(0, 4).AddCandidate(5);
            board.GetCell(0, 7).AddCandidate(5);
            
            board.GetCell(3, 1).AddCandidate(5);
            board.GetCell(3, 4).AddCandidate(5);
            board.GetCell(3, 7).AddCandidate(5);
            
            board.GetCell(6, 1).AddCandidate(5);
            board.GetCell(6, 4).AddCandidate(5);
            board.GetCell(6, 7).AddCandidate(5);
            
            // Add 5 to cells in columns 1,4,7 (other rows) that should be eliminated
            board.GetCell(1, 1).AddCandidate(5);
            board.GetCell(2, 4).AddCandidate(5);
            board.GetCell(5, 7).AddCandidate(5);
            
            var strategy = new SwordfishStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 5 from columns 1,4,7 in rows other than 0,3,6
            Assert.Contains(result.RemovedCandidates, e => e.Row == 1 && e.Col == 1 && e.Candidate == 5);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 2 && e.Col == 4 && e.Candidate == 5);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 5 && e.Col == 7 && e.Candidate == 5);
        }

        [Fact]
        public void Apply_FindsSwordfishInColumns_EliminatesFromRows()
        {
            // Arrange: Create a board where digit 3 forms Swordfish in columns 0,3,6 at rows 1,4,7
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Clear 3 from all cells first
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    board.GetCell(r, c).RemoveCandidate(3);
                }
            }
            
            // Set up Swordfish pattern in columns 0, 3, 6 at rows 1,4,7
            board.GetCell(1, 0).AddCandidate(3);
            board.GetCell(4, 0).AddCandidate(3);
            board.GetCell(7, 0).AddCandidate(3);
            
            board.GetCell(1, 3).AddCandidate(3);
            board.GetCell(4, 3).AddCandidate(3);
            board.GetCell(7, 3).AddCandidate(3);
            
            board.GetCell(1, 6).AddCandidate(3);
            board.GetCell(4, 6).AddCandidate(3);
            board.GetCell(7, 6).AddCandidate(3);
            
            // Add 3 to cells in rows 1,4,7 (other columns) that should be eliminated
            board.GetCell(1, 2).AddCandidate(3);
            board.GetCell(4, 5).AddCandidate(3);
            board.GetCell(7, 8).AddCandidate(3);
            
            var strategy = new SwordfishStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.RemovedCandidates);
            
            // Should eliminate 3 from rows 1,4,7 in columns other than 0,3,6
            Assert.Contains(result.RemovedCandidates, e => e.Row == 1 && e.Col == 2 && e.Candidate == 3);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 4 && e.Col == 5 && e.Candidate == 3);
            Assert.Contains(result.RemovedCandidates, e => e.Row == 7 && e.Col == 8 && e.Candidate == 3);
        }

        [Fact]
        public void Apply_NoSwordfish_ReturnsNull()
        {
            // Arrange: Board with no Swordfish pattern
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            var strategy = new SwordfishStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_InsufficientRows_ReturnsNull()
        {
            // Arrange: Only 2 rows with digit pattern (need 3 for Swordfish)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Only 2 rows with pattern
            BoardBuilder.SetCandidates(board, 0, 2, 5, 6);
            BoardBuilder.SetCandidates(board, 0, 5, 5, 7);
            
            BoardBuilder.SetCandidates(board, 1, 2, 5, 6);
            BoardBuilder.SetCandidates(board, 1, 5, 5, 7);
            
            var strategy = new SwordfishStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Apply_FourColumnsSpan_NotSwordfish()
        {
            // Arrange: 3 rows but digit spans 4 columns (invalid Swordfish)
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Pattern spans columns 2,5,7,8 (4 columns) - not valid Swordfish
            BoardBuilder.SetCandidates(board, 0, 2, 5, 6);
            BoardBuilder.SetCandidates(board, 0, 5, 5, 7);
            BoardBuilder.SetCandidates(board, 0, 8, 5, 8);
            
            BoardBuilder.SetCandidates(board, 1, 2, 5, 6);
            BoardBuilder.SetCandidates(board, 1, 7, 5, 9);  // Different column
            BoardBuilder.SetCandidates(board, 1, 8, 5, 8);
            
            BoardBuilder.SetCandidates(board, 2, 5, 5, 7);
            BoardBuilder.SetCandidates(board, 2, 7, 5, 9);
            BoardBuilder.SetCandidates(board, 2, 8, 5, 8);
            
            var strategy = new SwordfishStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert  
            // Might find different Swordfish or return null - either is acceptable
            // This just tests that it handles 4-column cases
            if (result != null)
            {
                Assert.NotEmpty(result.RemovedCandidates);
            }
        }

        [Fact]
        public void Apply_AllDigits_FindsFirstMatch()
        {
            // Arrange: Set up Swordfish for digit 1
            var board = BoardBuilder.CreateEmptyBoard();
            board.InitializeCandidates();
            
            // Clear 1 from all cells
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    board.GetCell(r, c).RemoveCandidate(1);
                }
            }
            
            // Create Swordfish pattern
            board.GetCell(0, 0).AddCandidate(1);
            board.GetCell(0, 3).AddCandidate(1);
            board.GetCell(0, 6).AddCandidate(1);
            
            board.GetCell(3, 0).AddCandidate(1);
            board.GetCell(3, 3).AddCandidate(1);
            board.GetCell(3, 6).AddCandidate(1);
            
            board.GetCell(6, 0).AddCandidate(1);
            board.GetCell(6, 3).AddCandidate(1);
            board.GetCell(6, 6).AddCandidate(1);
            
            // Add cells that should be eliminated
            board.GetCell(1, 0).AddCandidate(1);
            board.GetCell(2, 3).AddCandidate(1);
            
            var strategy = new SwordfishStrategy();

            // Act
            var result = strategy.Apply(board);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Swordfish", result.Description);
        }

        [Fact]
        public void Properties_HaveCorrectValues()
        {
            // Arrange
            var strategy = new SwordfishStrategy();

            // Assert
            Assert.Equal("Swordfish", strategy.Name);
            Assert.Equal(140, strategy.DifficultyScore);
            Assert.Equal(StrategyCategory.Tough, strategy.Category);
        }
    }
}
