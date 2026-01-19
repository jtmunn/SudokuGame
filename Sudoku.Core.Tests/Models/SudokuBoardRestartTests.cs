using Sudoku.Core.Models;
using Xunit;

namespace Sudoku.Core.Tests.Models
{
    /// <summary>
    /// Tests for restart game functionality - verifying that user entries can be cleared while preserving given cells.
    /// </summary>
    public class SudokuBoardRestartTests
    {
        [Fact]
        public void RestartGame_ClearsUserEntries_PreservesGivenCells()
        {
            // Arrange - Create a board with some given cells and user entries
            var board = new SudokuBoard();
            
            // Set up given cells (original puzzle)
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: true);
            board.SetCell(1, 0, 6, isGiven: true);
            
            // Set up user entries
            board.SetCell(0, 2, 7, isGiven: false);
            board.SetCell(1, 1, 8, isGiven: false);
            board.SetCell(2, 2, 9, isGiven: false);
            
            // Act - Simulate restart by clearing all non-given cells
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (!cell.IsGiven)
                    {
                        cell.Value = 0;
                        cell.HasError = false;
                    }
                }
            }
            
            // Assert - Given cells should still have values, user entries should be cleared
            Assert.Equal(5, board.GetCell(0, 0).Value);
            Assert.True(board.GetCell(0, 0).IsGiven);
            
            Assert.Equal(3, board.GetCell(0, 1).Value);
            Assert.True(board.GetCell(0, 1).IsGiven);
            
            Assert.Equal(6, board.GetCell(1, 0).Value);
            Assert.True(board.GetCell(1, 0).IsGiven);
            
            // User entries should be cleared
            Assert.Equal(0, board.GetCell(0, 2).Value);
            Assert.False(board.GetCell(0, 2).IsGiven);
            
            Assert.Equal(0, board.GetCell(1, 1).Value);
            Assert.False(board.GetCell(1, 1).IsGiven);
            
            Assert.Equal(0, board.GetCell(2, 2).Value);
            Assert.False(board.GetCell(2, 2).IsGiven);
        }
        
        [Fact]
        public void RestartGame_ClearsErrorFlags_OnUserCells()
        {
            // Arrange - Create a board with errors on user entries
            var board = new SudokuBoard();
            
            // Set up given cells
            board.SetCell(0, 0, 5, isGiven: true);
            
            // Set up user entry with error
            board.SetCell(0, 1, 7, isGiven: false);
            var userCell = board.GetCell(0, 1);
            userCell.HasError = true;
            
            // Act - Simulate restart
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (!cell.IsGiven)
                    {
                        cell.Value = 0;
                        cell.HasError = false;
                    }
                }
            }
            
            // Assert - User cell should be cleared and error flag removed
            Assert.Equal(0, board.GetCell(0, 1).Value);
            Assert.False(board.GetCell(0, 1).HasError);
            
            // Given cell should be unchanged
            Assert.Equal(5, board.GetCell(0, 0).Value);
            Assert.True(board.GetCell(0, 0).IsGiven);
        }
        
        [Fact]
        public void RestartGame_AllGivenCells_NoChange()
        {
            // Arrange - Create a board where all filled cells are given (no user entries)
            var board = new SudokuBoard();
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: true);
            board.SetCell(1, 0, 6, isGiven: true);
            
            // Act - Simulate restart
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (!cell.IsGiven)
                    {
                        cell.Value = 0;
                        cell.HasError = false;
                    }
                }
            }
            
            // Assert - All given cells should remain unchanged
            Assert.Equal(5, board.GetCell(0, 0).Value);
            Assert.True(board.GetCell(0, 0).IsGiven);
            
            Assert.Equal(3, board.GetCell(0, 1).Value);
            Assert.True(board.GetCell(0, 1).IsGiven);
            
            Assert.Equal(6, board.GetCell(1, 0).Value);
            Assert.True(board.GetCell(1, 0).IsGiven);
        }
        
        [Fact]
        public void RestartGame_EmptyBoard_RemainsEmpty()
        {
            // Arrange - Empty board
            var board = new SudokuBoard();
            
            // Act - Simulate restart
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (!cell.IsGiven)
                    {
                        cell.Value = 0;
                        cell.HasError = false;
                    }
                }
            }
            
            // Assert - Board should still be empty
            Assert.All(board.GetAllCells(), cell => Assert.Equal(0, cell.Value));
            Assert.All(board.GetAllCells(), cell => Assert.False(cell.IsGiven));
        }
        
        [Fact]
        public void RestartGame_MixedBoard_PreservesOnlyGiven()
        {
            // Arrange - Create a realistic scenario with many cells
            var board = new SudokuBoard();
            
            // Set up a row with mixed given and user entries
            board.SetCell(0, 0, 5, isGiven: true);
            board.SetCell(0, 1, 3, isGiven: false); // User
            board.SetCell(0, 2, 7, isGiven: true);
            board.SetCell(0, 3, 2, isGiven: false); // User
            board.SetCell(0, 4, 9, isGiven: true);
            board.SetCell(0, 5, 4, isGiven: false); // User
            board.SetCell(0, 6, 8, isGiven: true);
            board.SetCell(0, 7, 1, isGiven: false); // User
            board.SetCell(0, 8, 6, isGiven: true);
            
            // Act - Simulate restart
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (!cell.IsGiven)
                    {
                        cell.Value = 0;
                        cell.HasError = false;
                    }
                }
            }
            
            // Assert - Only given cells should have values
            Assert.Equal(5, board.GetCell(0, 0).Value);
            Assert.Equal(0, board.GetCell(0, 1).Value); // Cleared
            Assert.Equal(7, board.GetCell(0, 2).Value);
            Assert.Equal(0, board.GetCell(0, 3).Value); // Cleared
            Assert.Equal(9, board.GetCell(0, 4).Value);
            Assert.Equal(0, board.GetCell(0, 5).Value); // Cleared
            Assert.Equal(8, board.GetCell(0, 6).Value);
            Assert.Equal(0, board.GetCell(0, 7).Value); // Cleared
            Assert.Equal(6, board.GetCell(0, 8).Value);
            
            // Verify IsGiven flags are preserved correctly
            Assert.True(board.GetCell(0, 0).IsGiven);
            Assert.False(board.GetCell(0, 1).IsGiven);
            Assert.True(board.GetCell(0, 2).IsGiven);
            Assert.False(board.GetCell(0, 3).IsGiven);
            Assert.True(board.GetCell(0, 4).IsGiven);
            Assert.False(board.GetCell(0, 5).IsGiven);
            Assert.True(board.GetCell(0, 6).IsGiven);
            Assert.False(board.GetCell(0, 7).IsGiven);
            Assert.True(board.GetCell(0, 8).IsGiven);
        }
    }
}
