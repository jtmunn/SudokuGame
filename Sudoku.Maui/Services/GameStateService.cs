using System.Text.Json;
using Sudoku.Application.Models;
using Sudoku.Application.Services;

namespace Sudoku.Maui.Services
{
    /// <summary>
    /// Service for persisting and restoring game state using JSON file storage.
    /// </summary>
    public class GameStateService : IGameStateService
    {
        private const string GameStateFileName = "sudoku_gamestate.json";
        private readonly string _gameStateFilePath;
        private GameState? _cachedGameState;

        public GameStateService()
        {
            _gameStateFilePath = Path.Combine(FileSystem.AppDataDirectory, GameStateFileName);
        }

        public GameState? LoadGameState()
        {
            try
            {
                if (File.Exists(_gameStateFilePath))
                {
                    var json = File.ReadAllText(_gameStateFilePath);
                    _cachedGameState = JsonSerializer.Deserialize<GameState>(json);
                    return _cachedGameState;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GameStateService: Error loading game state: {ex.Message}");
            }

            return null;
        }

        public async Task SaveGameStateAsync(GameState gameState)
        {
            _cachedGameState = gameState;
            gameState.SavedAt = DateTime.UtcNow;

            try
            {
                var json = JsonSerializer.Serialize(gameState);
                await File.WriteAllTextAsync(_gameStateFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GameStateService: Failed to save game state: {ex.Message}");
            }
        }

        public async Task ClearGameStateAsync()
        {
            _cachedGameState = null;

            try
            {
                if (File.Exists(_gameStateFilePath))
                {
                    await Task.Run(() => File.Delete(_gameStateFilePath));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GameStateService: Failed to clear game state: {ex.Message}");
            }
        }

        public bool HasSavedGame()
        {
            return File.Exists(_gameStateFilePath);
        }
    }
}
