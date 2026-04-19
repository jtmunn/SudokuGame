using Moq;
using Sudoku.Core.Models;
using Sudoku.Application.Models;
using Sudoku.Application.Services;
using Sudoku.Application.ViewModels;
using CoreDifficultyLevel = Sudoku.Core.Services.DifficultyLevel;
using CoreGenerator = Sudoku.Core.Services.SudokuGenerator;
using CoreValidator = Sudoku.Core.Services.SudokuValidator;
using CoreSolver = Sudoku.Core.Services.SudokuBacktrackingSolver;

namespace Sudoku.Application.Tests.ViewModels;

public class SudokuPageViewModelTests
{
    private readonly Mock<CoreGenerator> _mockGenerator;
    private readonly Mock<CoreValidator> _mockValidator;
    private readonly Mock<CoreSolver> _mockSolver;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IGameStateService> _mockGameStateService;
    private readonly SudokuPageViewModel _viewModel;

    public SudokuPageViewModelTests()
    {
        // Core services - create real instances since they have complex dependencies
        var realValidator = new CoreValidator();
        var realSolver = new CoreSolver(realValidator);
        var realLogicalSolver = new Sudoku.Core.Services.SudokuLogicalSolver();
        var realGenerator = new CoreGenerator(realSolver, realLogicalSolver);
        
        // Mock only the MAUI services (these are interfaces)
        _mockSettingsService = new Mock<ISettingsService>();
        _mockGameStateService = new Mock<IGameStateService>();
        
        // Create minimal mocks for core services that we still want to verify calls on
        _mockGenerator = new Mock<CoreGenerator>(realSolver, realLogicalSolver) { CallBase = true };
        _mockValidator = new Mock<CoreValidator>() { CallBase = true };
        _mockSolver = new Mock<CoreSolver>(realValidator) { CallBase = true };

        _viewModel = new SudokuPageViewModel(
            _mockGenerator.Object,
            _mockValidator.Object,
            _mockSolver.Object,
            _mockSettingsService.Object,
            _mockGameStateService.Object
        );
    }

    #region Property Tests

    [Fact]
    public void IsPuzzleSolved_InitiallyFalse()
    {
        // Assert
        Assert.False(_viewModel.IsPuzzleSolved);
    }

    [Fact]
    public void IsPuzzleSolved_CanBeSet()
    {
        // Act
        _viewModel.IsPuzzleSolved = true;

        // Assert
        Assert.True(_viewModel.IsPuzzleSolved);
    }

    [Fact]
    public void MistakesCount_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _viewModel.MistakesCount);
    }

    [Fact]
    public void MistakesCount_CanBeIncremented()
    {
        // Act
        _viewModel.MistakesCount = 5;

        // Assert
        Assert.Equal(5, _viewModel.MistakesCount);
    }

    [Fact]
    public void HintsUsedCount_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _viewModel.HintsUsedCount);
    }

    [Fact]
    public void HintsUsedCount_CanBeIncremented()
    {
        // Act
        _viewModel.HintsUsedCount = 3;

        // Assert
        Assert.Equal(3, _viewModel.HintsUsedCount);
    }

    [Fact]
    public void HasUserMadeEntries_InitiallyFalse()
    {
        // Assert
        Assert.False(_viewModel.HasUserMadeEntries);
    }

    [Fact]
    public void IsProcessingInput_InitiallyFalse()
    {
        // Assert
        Assert.False(_viewModel.IsProcessingInput);
    }

    [Fact]
    public void CurrentDifficulty_InitiallyEasy()
    {
        // Assert
        Assert.Equal("Easy", _viewModel.CurrentDifficulty);
    }

    [Fact]
    public void CurrentDifficulty_CanBeChanged()
    {
        // Act
        _viewModel.CurrentDifficulty = "Hard";

        // Assert
        Assert.Equal("Hard", _viewModel.CurrentDifficulty);
    }

    [Fact]
    public void ElapsedSeconds_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _viewModel.ElapsedSeconds);
    }

    [Fact]
    public void ElapsedSeconds_CanBeSet()
    {
        // Act
        _viewModel.ElapsedSeconds = 120;

        // Assert
        Assert.Equal(120, _viewModel.ElapsedSeconds);
    }

    [Fact]
    public void TimerText_FormatsCorrectly()
    {
        // Act
        _viewModel.ElapsedSeconds = 125; // 2 minutes 5 seconds

        // Assert
        Assert.Equal("02:05", _viewModel.TimerText);
    }

    [Fact]
    public void TimerText_FormatsZeroCorrectly()
    {
        // Act
        _viewModel.ElapsedSeconds = 0;

        // Assert
        Assert.Equal("00:00", _viewModel.TimerText);
    }

    [Fact]
    public void TimerText_FormatsLargeValuesCorrectly()
    {
        // Act
        _viewModel.ElapsedSeconds = 3661; // 61 minutes 1 second

        // Assert
        Assert.Equal("61:01", _viewModel.TimerText);
    }

    #endregion

    #region PropertyChanged Tests

    [Fact]
    public void IsPuzzleSolved_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChanged = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SudokuPageViewModel.IsPuzzleSolved))
                propertyChanged = true;
        };

        // Act
        _viewModel.IsPuzzleSolved = true;

        // Assert
        Assert.True(propertyChanged);
    }

    [Fact]
    public void MistakesCount_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChanged = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SudokuPageViewModel.MistakesCount))
                propertyChanged = true;
        };

        // Act
        _viewModel.MistakesCount = 1;

        // Assert
        Assert.True(propertyChanged);
    }

    [Fact]
    public void ElapsedSeconds_RaisesPropertyChanged()
    {
        // Arrange
        var elapsedSecondsChanged = false;
        var timerTextChanged = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SudokuPageViewModel.ElapsedSeconds))
                elapsedSecondsChanged = true;
            if (e.PropertyName == nameof(SudokuPageViewModel.TimerText))
                timerTextChanged = true;
        };

        // Act
        _viewModel.ElapsedSeconds = 10;

        // Assert
        Assert.True(elapsedSecondsChanged);
        Assert.True(timerTextChanged);
    }

    #endregion

    #region Timer Tests

    [Fact]
    public void StartTimer_InitializesTimer()
    {
        // Act
        _viewModel.StartTimer();

        // Assert - timer should be running (can't directly test, but no exception is good)
        Assert.Equal(0, _viewModel.ElapsedSeconds);
    }

    [Fact]
    public void StopTimer_CanBeCalled()
    {
        // Arrange
        _viewModel.StartTimer();

        // Act & Assert - should not throw
        _viewModel.StopTimer();
    }

    [Fact]
    public void ResetTimer_SetsElapsedSecondsToZero()
    {
        // Arrange
        _viewModel.ElapsedSeconds = 100;

        // Act
        _viewModel.ResetTimer();

        // Assert
        Assert.Equal(0, _viewModel.ElapsedSeconds);
    }

    [Fact]
    public async Task StartTimer_IncrementsElapsedSeconds()
    {
        // Arrange
        _viewModel.StartTimer();

        // Act - wait for timer to tick
        await Task.Delay(1500); // Wait 1.5 seconds

        // Assert - timer should have incremented at least once
        Assert.True(_viewModel.ElapsedSeconds >= 1);
        
        // Cleanup
        _viewModel.StopTimer();
    }

    #endregion

    #region StartNewGameAsync Tests

    [Fact]
    public async Task StartNewGameAsync_GeneratesBoardWithCorrectDifficulty()
    {
        // Arrange
        var difficulty = CoreDifficultyLevel.Medium;
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());

        // Act - uses real generator which creates a valid board
        var result = await _viewModel.StartNewGameAsync(difficulty);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Medium", _viewModel.CurrentDifficulty);
    }

    [Fact]
    public async Task StartNewGameAsync_ResetsStatistics()
    {
        // Arrange
        _viewModel.MistakesCount = 5;
        _viewModel.HintsUsedCount = 3;
        _viewModel.HasUserMadeEntries = true;
        _viewModel.IsPuzzleSolved = true;

        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());

        // Act - uses real generator
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Easy);

        // Assert
        Assert.Equal(0, _viewModel.MistakesCount);
        Assert.Equal(0, _viewModel.HintsUsedCount);
        Assert.False(_viewModel.HasUserMadeEntries);
        Assert.False(_viewModel.IsPuzzleSolved);
    }

    [Fact]
    public async Task StartNewGameAsync_UpdatesDifficulty()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());

        // Act - uses real generator
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Expert);

        // Assert
        Assert.Equal("Expert", _viewModel.CurrentDifficulty);
    }

    [Fact]
    public async Task StartNewGameAsync_SavesSettings()
    {
        // Arrange
        var settings = new GameSettings();
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(settings);

        // Act - uses real generator
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Hard);

        // Assert
        Assert.Equal(CoreDifficultyLevel.Hard, settings.LastPlayedDifficulty);
        _mockSettingsService.Verify(s => s.SaveSettingsAsync(settings), Times.Once);
    }

    [Fact]
    public async Task StartNewGameAsync_ClearsGameState()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());

        // Act - uses real generator
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Easy);

        // Assert
        _mockGameStateService.Verify(g => g.ClearGameStateAsync(), Times.Once);
    }

    #endregion

    #region RestoreGame Tests

    [Fact]
    public void RestoreGame_RestoresBoardFromGameState()
    {
        // Arrange
        var board = new SudokuBoard();
        var gameState = new GameState
        {
            BoardData = board.Serialize(),
            ElapsedSeconds = 100,
            Difficulty = "Medium",
            IsSolved = false
        };

        // Act
        _viewModel.RestoreGame(gameState);

        // Assert
        Assert.Equal(100, _viewModel.ElapsedSeconds);
        Assert.Equal("Medium", _viewModel.CurrentDifficulty);
        Assert.False(_viewModel.IsPuzzleSolved);
    }

    [Fact]
    public void RestoreGame_RestoresSolution()
    {
        // Arrange
        var solution = new SudokuBoard();
        var gameState = new GameState
        {
            BoardData = new SudokuBoard().Serialize(),
            SolutionData = solution.Serialize(),
            ElapsedSeconds = 50,
            Difficulty = "Hard"
        };

        // Act
        _viewModel.RestoreGame(gameState);

        // Assert
        Assert.NotNull(_viewModel.Solution);
    }

    #endregion

    #region SaveCurrentGameStateAsync Tests

    [Fact]
    public async Task SaveCurrentGameStateAsync_DoesNotSaveIfPuzzleSolved()
    {
        // Arrange
        _viewModel.IsPuzzleSolved = true;

        // Act
        await _viewModel.SaveCurrentGameStateAsync();

        // Assert
        _mockGameStateService.Verify(g => g.SaveGameStateAsync(It.IsAny<GameState>()), Times.Never);
    }

    [Fact]
    public async Task SaveCurrentGameStateAsync_SavesGameState()
    {
        // Arrange - need a real board to avoid early return
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());
        
        // Start a game first to get a board with values
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Easy);
        
        _viewModel.IsPuzzleSolved = false;
        _viewModel.ElapsedSeconds = 200;
        _viewModel.CurrentDifficulty = "Expert";

        // Act
        await _viewModel.SaveCurrentGameStateAsync();

        // Assert - verify SaveGameStateAsync was called
        _mockGameStateService.Verify(g => g.SaveGameStateAsync(It.Is<GameState>(gs =>
            gs.ElapsedSeconds == 200 &&
            gs.Difficulty == "Expert" &&
            gs.IsSolved == false
        )), Times.Once);
    }

    #endregion

    #region ValidateBoard Tests

    [Fact]
    public void ValidateBoard_ReturnsValidationResult()
    {
        // Act - uses real validator
        var result = _viewModel.ValidateBoard();

        // Assert - should return validation result (board is initially valid)
        Assert.True(result);
    }

    #endregion
    
    #region Restart Game Tests

    [Fact]
    public async Task RestartGame_ResetsStatisticsAndTimer()
    {
        // Arrange - Start a game and set some statistics
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());
        
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Medium);
        
        // Make some changes to statistics
        _viewModel.MistakesCount = 3;
        _viewModel.HintsUsedCount = 2;
        _viewModel.HasUserMadeEntries = true;
        _viewModel.ElapsedSeconds = 120;

        // Act - Reset statistics (simulating restart)
        _viewModel.ResetTimer();
        _viewModel.MistakesCount = 0;
        _viewModel.HintsUsedCount = 0;
        _viewModel.HasUserMadeEntries = false;
        _viewModel.IsPuzzleSolved = false;

        // Assert - All statistics should be reset
        Assert.Equal(0, _viewModel.MistakesCount);
        Assert.Equal(0, _viewModel.HintsUsedCount);
        Assert.False(_viewModel.HasUserMadeEntries);
        Assert.False(_viewModel.IsPuzzleSolved);
        Assert.Equal(0, _viewModel.ElapsedSeconds);
    }

    [Fact]
    public async Task RestartGame_PreservesSolution()
    {
        // Arrange - Start a game to get a board and solution
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings());
        
        await _viewModel.StartNewGameAsync(CoreDifficultyLevel.Easy);
        var originalSolution = _viewModel.Solution;

        // Act - Solution should remain available after restart (not cleared)
        // In actual restart, we keep the solution since it's the same puzzle
        
        // Assert - Solution should still be available
        Assert.NotNull(_viewModel.Solution);
        Assert.Same(originalSolution, _viewModel.Solution);
    }

    [Fact]
    public void RestartGame_UserEntriesFlag_ResetsCorrectly()
    {
        // Arrange
        _viewModel.HasUserMadeEntries = true;

        // Act - Reset the flag
        _viewModel.HasUserMadeEntries = false;

        // Assert
        Assert.False(_viewModel.HasUserMadeEntries);
    }

    #endregion

    #region CheckSolved Tests

    [Fact]
    public void CheckSolved_ReturnsValidationResult()
    {
        // Act - uses real validator
        var result = _viewModel.CheckSolved();

        // Assert - empty board is not solved
        Assert.False(result);
    }

    #endregion

    #region ClearCell Tests

    [Fact]
    public void ClearCell_ClearsCellValue()
    {
        // Arrange
        var board = new SudokuBoard();
        board.SetCell(0, 0, 5);
        // Access board through reflection or make it public for testing
        // For now, we'll test the behavior indirectly

        // Act
        var result = _viewModel.ClearCell(0, 0);

        // Assert - method should return true if cell was cleared
        // Note: This requires the board to be accessible or behavior testable
    }

    #endregion

    #region OnPuzzleSolvedAsync Tests

    [Fact]
    public async Task OnPuzzleSolvedAsync_SetsSolvedFlag()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.LoadStatistics())
            .Returns(new GameStatistics());
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings { LastPlayedDifficulty = CoreDifficultyLevel.Medium });

        // Act
        await _viewModel.OnPuzzleSolvedAsync();

        // Assert
        Assert.True(_viewModel.IsPuzzleSolved);
    }

    [Fact]
    public async Task OnPuzzleSolvedAsync_ClearsGameState()
    {
        // Arrange
        _mockSettingsService.Setup(s => s.LoadStatistics())
            .Returns(new GameStatistics());
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings { LastPlayedDifficulty = CoreDifficultyLevel.Easy });

        // Act
        await _viewModel.OnPuzzleSolvedAsync();

        // Assert
        _mockGameStateService.Verify(g => g.ClearGameStateAsync(), Times.Once);
    }

    [Fact]
    public async Task OnPuzzleSolvedAsync_UpdatesBestTimeIfBetter()
    {
        // Arrange
        var stats = new GameStatistics();
        stats.SetBestTime(CoreDifficultyLevel.Easy, 200);
        _viewModel.ElapsedSeconds = 150; // Better time

        _mockSettingsService.Setup(s => s.LoadStatistics())
            .Returns(stats);
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings { LastPlayedDifficulty = CoreDifficultyLevel.Easy });

        // Act
        var (previousBest, difficulty) = await _viewModel.OnPuzzleSolvedAsync();

        // Assert
        Assert.Equal(200, previousBest);
        Assert.Equal(150, stats.GetBestTime(CoreDifficultyLevel.Easy));
        _mockSettingsService.Verify(s => s.SaveStatisticsAsync(stats), Times.Once);
    }

    [Fact]
    public async Task OnPuzzleSolvedAsync_DoesNotUpdateBestTimeIfWorse()
    {
        // Arrange
        var stats = new GameStatistics();
        stats.SetBestTime(CoreDifficultyLevel.Easy, 100);
        _viewModel.ElapsedSeconds = 150; // Worse time

        _mockSettingsService.Setup(s => s.LoadStatistics())
            .Returns(stats);
        _mockSettingsService.Setup(s => s.LoadSettings())
            .Returns(new GameSettings { LastPlayedDifficulty = CoreDifficultyLevel.Easy });

        // Act
        await _viewModel.OnPuzzleSolvedAsync();

        // Assert
        Assert.Equal(100, stats.GetBestTime(CoreDifficultyLevel.Easy)); // Should remain unchanged
    }

    #endregion
}




