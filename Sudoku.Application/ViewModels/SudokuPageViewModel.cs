using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sudoku.Core.Models;
using CoreDifficulty = Sudoku.Core.Services.DifficultyLevel;
using Sudoku.Core.Services;
using Sudoku.Application.Services;
using Models = Sudoku.Application.Models;

namespace Sudoku.Application.ViewModels
{
    /// <summary>
    /// ViewModel for the Sudoku game page, managing game state, timer, and statistics.
    /// </summary>
    public class SudokuPageViewModel : INotifyPropertyChanged
    {
        private readonly SudokuGenerator _generator;
        private readonly SudokuValidator _validator;
        private readonly SudokuSolver _solver;
        private readonly ISettingsService _settingsService;
        private readonly IGameStateService _gameStateService;

        private SudokuBoard _currentBoard;
        private SudokuBoard? _solution;
        private System.Timers.Timer? _gameTimer;
        private int _elapsedSeconds;
        private string _currentDifficulty = "Easy";
        private bool _isPuzzleSolved;
        private int _mistakesCount;
        private int _hintsUsedCount;
        private bool _hasUserMadeEntries;
        private bool _isProcessingInput;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SudokuBoard CurrentBoard
        {
            get => _currentBoard;
            private set
            {
                _currentBoard = value;
                OnPropertyChanged();
            }
        }

        public SudokuBoard? Solution
        {
            get => _solution;
            private set
            {
                _solution = value;
                OnPropertyChanged();
            }
        }

        public int ElapsedSeconds
        {
            get => _elapsedSeconds;
            set
            {
                _elapsedSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerText));
            }
        }

        public string CurrentDifficulty
        {
            get => _currentDifficulty;
            set
            {
                _currentDifficulty = value;
                OnPropertyChanged();
            }
        }

        public bool IsPuzzleSolved
        {
            get => _isPuzzleSolved;
            set
            {
                _isPuzzleSolved = value;
                OnPropertyChanged();
            }
        }

        public int MistakesCount
        {
            get => _mistakesCount;
            set
            {
                _mistakesCount = value;
                OnPropertyChanged();
            }
        }

        public int HintsUsedCount
        {
            get => _hintsUsedCount;
            set
            {
                _hintsUsedCount = value;
                OnPropertyChanged();
            }
        }

        public bool HasUserMadeEntries
        {
            get => _hasUserMadeEntries;
            set
            {
                _hasUserMadeEntries = value;
                OnPropertyChanged();
            }
        }

        public bool IsProcessingInput
        {
            get => _isProcessingInput;
            set
            {
                _isProcessingInput = value;
                OnPropertyChanged();
            }
        }

        public string TimerText
        {
            get
            {
                int minutes = _elapsedSeconds / 60;
                int seconds = _elapsedSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }

        public SudokuPageViewModel(
            SudokuGenerator generator,
            SudokuValidator validator,
            SudokuSolver solver,
            ISettingsService settingsService,
            IGameStateService gameStateService)
        {
            _generator = generator;
            _validator = validator;
            _solver = solver;
            _settingsService = settingsService;
            _gameStateService = gameStateService;
            _currentBoard = new SudokuBoard();
        }

        /// <summary>
        /// Starts a new game with the specified difficulty.
        /// </summary>
        public async Task<SudokuBoard> StartNewGameAsync(CoreDifficulty difficulty)
        {
            var coreDifficulty = MapDifficulty(difficulty);
            var board = await Task.Run(() => _generator.Generate(coreDifficulty));

            CurrentBoard = board;
            Solution = _solver.GetSolution(CurrentBoard);

            ResetTimer();
            StartTimer();

            IsPuzzleSolved = false;
            MistakesCount = 0;
            HintsUsedCount = 0;
            HasUserMadeEntries = false;
            CurrentDifficulty = difficulty.ToString();

            // Save last played difficulty
            var settings = _settingsService.LoadSettings();
            settings.LastPlayedDifficulty = difficulty;
            await _settingsService.SaveSettingsAsync(settings);

            // Clear any saved game state
            await _gameStateService.ClearGameStateAsync();

            return CurrentBoard;
        }

        /// <summary>
        /// Restores a game from saved state.
        /// </summary>
        public void RestoreGame(Models.GameState gameState)
        {
            if (!string.IsNullOrEmpty(gameState.BoardData))
            {
                CurrentBoard = SudokuBoard.Deserialize(gameState.BoardData);
            }

            if (!string.IsNullOrEmpty(gameState.SolutionData))
            {
                Solution = SudokuBoard.Deserialize(gameState.SolutionData);
            }

            ElapsedSeconds = gameState.ElapsedSeconds;
            CurrentDifficulty = gameState.Difficulty ?? "Medium";
            IsPuzzleSolved = gameState.IsSolved;
            HasUserMadeEntries = BoardHasUserEntries(CurrentBoard);

            if (!IsPuzzleSolved)
            {
                StartTimer();
            }
        }

        /// <summary>
        /// Saves the current game state.
        /// </summary>
        public async Task SaveCurrentGameStateAsync()
        {
            if (IsPuzzleSolved || CurrentBoard.GetAllCells().All(c => c.Value == 0))
            {
                return;
            }

            try
            {
                var gameState = new Models.GameState
                {
                    BoardData = CurrentBoard.Serialize(),
                    SolutionData = Solution?.Serialize(),
                    ElapsedSeconds = ElapsedSeconds,
                    Difficulty = CurrentDifficulty,
                    IsSolved = IsPuzzleSolved
                };

                await _gameStateService.SaveGameStateAsync(gameState);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save game state: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a hint for the current board state.
        /// </summary>
        public (int row, int col, int value)? GetHint()
        {
            _validator.UpdateErrorFlags(CurrentBoard);
            if (!_validator.IsValidState(CurrentBoard))
            {
                return null;
            }

            Solution ??= _solver.GetSolution(CurrentBoard);
            var hint = _solver.GetHint(CurrentBoard);
            
            if (hint.HasValue)
            {
                var (row, col, value) = hint.Value;
                CurrentBoard.SetCell(row, col, value);
                HintsUsedCount++;
                _validator.UpdateErrorFlags(CurrentBoard);
            }

            return hint;
        }

        /// <summary>
        /// Validates the current board state.
        /// </summary>
        public bool ValidateBoard()
        {
            _validator.UpdateErrorFlags(CurrentBoard);
            return _validator.IsValidState(CurrentBoard);
        }

        /// <summary>
        /// Checks if the puzzle is solved.
        /// </summary>
        public bool CheckSolved()
        {
            return _validator.IsSolved(CurrentBoard);
        }

        /// <summary>
        /// Counts correct cells compared to solution.
        /// </summary>
        public int CountCorrectCells()
        {
            Solution ??= _solver.GetSolution(CurrentBoard);
            return Solution != null ? _validator.CountCorrectCells(CurrentBoard, Solution) : 0;
        }

        /// <summary>
        /// Applies a number to a cell and validates it.
        /// </summary>
        public bool ApplyNumber(int row, int col, int number)
        {
            var cell = CurrentBoard.GetCell(row, col);
            if (cell.IsGiven)
                return false;

            if (!_validator.IsValidMove(CurrentBoard, row, col, number))
            {
                return false;
            }

            CurrentBoard.SetCell(row, col, number);
            HasUserMadeEntries = true;
            _validator.UpdateErrorFlags(CurrentBoard);

            // Check against solution if available
            if (Solution != null)
            {
                var solutionCell = Solution.GetCell(row, col);
                if (cell.Value != solutionCell.Value)
                {
                    cell.HasError = true;
                    MistakesCount++;
                }
            }

            return true;
        }

        /// <summary>
        /// Clears a cell value.
        /// </summary>
        public bool ClearCell(int row, int col)
        {
            var cell = CurrentBoard.GetCell(row, col);
            if (cell.IsGiven || cell.Value == 0)
                return false;

            cell.Value = 0;
            cell.HasError = false;
            _validator.UpdateErrorFlags(CurrentBoard);
            HasUserMadeEntries = true;

            return true;
        }

        /// <summary>
        /// Handles puzzle completion.
        /// </summary>
        public async Task<(int? previousBestTime, CoreDifficulty difficulty)> OnPuzzleSolvedAsync()
        {
            IsPuzzleSolved = true;
            StopTimer();
            await _gameStateService.ClearGameStateAsync();

            var stats = _settingsService.LoadStatistics();
            var settings = _settingsService.LoadSettings();
            var currentDifficultyEnum = settings.LastPlayedDifficulty ?? CoreDifficulty.Medium;
            var previousBestTime = stats.GetBestTime(currentDifficultyEnum);

            if (!previousBestTime.HasValue || ElapsedSeconds < previousBestTime.Value)
            {
                stats.SetBestTime(currentDifficultyEnum, ElapsedSeconds);
                await _settingsService.SaveStatisticsAsync(stats);
            }

            return (previousBestTime, currentDifficultyEnum);
        }

        // Timer management
        public void StartTimer()
        {
            _gameTimer?.Stop();
            _gameTimer = new System.Timers.Timer(1000);
            _gameTimer.Elapsed += (s, e) =>
            {
                ElapsedSeconds++;
            };
            _gameTimer.Start();
        }

        public void StopTimer()
        {
            _gameTimer?.Stop();
        }

        public void ResetTimer()
        {
            ElapsedSeconds = 0;
        }

        private Core.Services.DifficultyLevel MapDifficulty(CoreDifficulty coreDifficulty)
        {
            return coreDifficulty switch
            {
                CoreDifficulty.Easy => Core.Services.DifficultyLevel.Easy,
                CoreDifficulty.Medium => Core.Services.DifficultyLevel.Medium,
                CoreDifficulty.Hard => Core.Services.DifficultyLevel.Hard,
                CoreDifficulty.Expert => Core.Services.DifficultyLevel.Expert,
                CoreDifficulty.Evil => Core.Services.DifficultyLevel.Evil,
                _ => Core.Services.DifficultyLevel.Easy
            };
        }

        private bool BoardHasUserEntries(SudokuBoard board)
        {
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value != 0 && !cell.IsGiven)
                        return true;
                }
            }
            return false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


