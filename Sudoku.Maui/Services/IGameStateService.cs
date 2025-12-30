using Sudoku.Maui.Models;

namespace Sudoku.Maui.Services
{
    /// <summary>
    /// Service for persisting and restoring game state.
    /// </summary>
    public interface IGameStateService
    {
        /// <summary>
        /// Loads the saved game state if it exists.
        /// </summary>
        /// <returns>GameState or null if no saved state exists.</returns>
        GameState? LoadGameState();
        
        /// <summary>
        /// Saves the current game state asynchronously.
        /// </summary>
        Task SaveGameStateAsync(GameState gameState);
        
        /// <summary>
        /// Clears any saved game state (called when puzzle is completed or abandoned).
        /// </summary>
        Task ClearGameStateAsync();
        
        /// <summary>
        /// Checks if a saved game state exists.
        /// </summary>
        bool HasSavedGame();
    }
}
