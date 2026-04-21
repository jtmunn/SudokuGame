using Sudoku.Application.Models;
using Xunit;

namespace Sudoku.Application.Tests.Models
{
    public class GameStateTests
    {
        [Fact]
        public void IsSolved_WhenTrue_ShouldIndicateGameIsLocked()
        {
            // Arrange
            var state = new GameState { IsSolved = true };

            // Act & Assert
            Assert.True(state.IsSolved);
            // In the Maui UI, this would lock the board and disable actions
        }

        [Fact]
        public void IsSolved_WhenFalse_ShouldIndicateGameIsActive()
        {
            // Arrange
            var state = new GameState { IsSolved = false };

            // Act & Assert
            Assert.False(state.IsSolved);
            // In the Maui UI, this would allow board interaction and actions
        }
    }
}
