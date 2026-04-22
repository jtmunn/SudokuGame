using Sudoku.Application.Models;
using Sudoku.Core.Models;
using Sudoku.Core.Services;

namespace Sudoku.Application.Services
{
    /// <summary>
    /// Default implementation of <see cref="IGameSession"/>. Owns the timer and all
    /// gameplay state. Methods are not internally synchronized; callers should invoke
    /// from a single thread (typically the UI thread) other than <see cref="TimerTick"/>
    /// which fires on the timer's thread-pool callback.
    /// </summary>
    public sealed class GameSession : IGameSession, IDisposable
    {
        private readonly SudokuGenerator _generator;
        private readonly SudokuValidator _validator;
        private readonly SudokuBacktrackingSolver _solver;
        private readonly IGameStateService _gameStateService;
        private readonly ISettingsService _settingsService;

        private readonly System.Timers.Timer _timer;
        private bool _disposed;

        private SudokuBoard _board = new();
        private SudokuBoard? _solution;
        private DifficultyLevel? _difficulty;
        private GamePhase _phase = GamePhase.NotStarted;
        private int _elapsedSeconds;
        private int _mistakesCount;
        private int _hintsUsedCount;
        private bool _hasUserMadeEntries;

        public GameSession(
            SudokuGenerator generator,
            SudokuValidator validator,
            SudokuBacktrackingSolver solver,
            IGameStateService gameStateService,
            ISettingsService settingsService)
        {
            _generator = generator;
            _validator = validator;
            _solver = solver;
            _gameStateService = gameStateService;
            _settingsService = settingsService;

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        public GamePhase Phase => _phase;
        public SudokuBoard Board => _board;
        public SudokuBoard? Solution => _solution;
        public DifficultyLevel? Difficulty => _difficulty;
        public string DifficultyName => _difficulty?.ToString() ?? "";
        public int ElapsedSeconds => _elapsedSeconds;
        public int MistakesCount => _mistakesCount;
        public int HintsUsedCount => _hintsUsedCount;
        public bool HasUserMadeEntries => _hasUserMadeEntries;

        public bool CanEditBoard => _phase == GamePhase.Playing;
        public bool CanUseGameActions => _phase == GamePhase.Playing;

        public event EventHandler<GamePhase>? PhaseChanged;
        public event EventHandler? BoardChanged;
        public event EventHandler? TimerTick;
        public event EventHandler<PuzzleSolvedEventArgs>? PuzzleSolved;

        // ---- Lifecycle ----

        public async Task StartNewAsync(DifficultyLevel difficulty)
        {
            SetPhase(GamePhase.Generating);

            var board = await Task.Run(() => _generator.Generate(difficulty));

            _board = board;
            _solution = _solver.GetSolution(_board);
            _difficulty = difficulty;
            _elapsedSeconds = 0;
            _mistakesCount = 0;
            _hintsUsedCount = 0;
            _hasUserMadeEntries = false;

            // Persist last-played difficulty for the next launch.
            var settings = _settingsService.LoadSettings();
            settings.LastPlayedDifficulty = difficulty;
            await _settingsService.SaveSettingsAsync(settings);

            // Discard any prior saved game; this one will save on its own cadence.
            await _gameStateService.ClearGameStateAsync();

            RaiseBoardChanged();
            SetPhase(GamePhase.Playing);
            StartTimerInternal();
        }

        public void Restart()
        {
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = _board.GetCell(row, col);
                    if (!cell.IsGiven)
                    {
                        cell.Value = 0;
                        cell.HasError = false;
                    }
                }
            }

            _elapsedSeconds = 0;
            _mistakesCount = 0;
            _hintsUsedCount = 0;
            _hasUserMadeEntries = false;

            RaiseBoardChanged();
            SetPhase(GamePhase.Playing);
            StartTimerInternal();
        }

        public bool TryRestore(GameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            if (string.IsNullOrEmpty(state.BoardData))
            {
                return false;
            }

            try
            {
                _board = SudokuBoard.Deserialize(state.BoardData);
                _solution = !string.IsNullOrEmpty(state.SolutionData)
                    ? SudokuBoard.Deserialize(state.SolutionData)
                    : null;

                _elapsedSeconds = state.ElapsedSeconds;
                _difficulty = ParseDifficulty(state.Difficulty);
                _mistakesCount = 0;
                _hintsUsedCount = 0;
                _hasUserMadeEntries = BoardHasUserEntries(_board);

                RaiseBoardChanged();

                if (state.IsSolved)
                {
                    SetPhase(GamePhase.Completed);
                }
                else
                {
                    SetPhase(GamePhase.Playing);
                    StartTimerInternal();
                }

                return true;
            }
            catch (Exception)
            {
                // Corrupt or incompatible saved state — caller should fall back to a fresh game.
                return false;
            }
        }

        public async Task SaveAsync()
        {
            // Don't persist a finished or empty game.
            if (_phase != GamePhase.Playing || _board.GetAllCells().All(c => c.Value == 0))
            {
                return;
            }

            var snapshot = new GameState
            {
                BoardData = _board.Serialize(),
                SolutionData = _solution?.Serialize(),
                ElapsedSeconds = _elapsedSeconds,
                Difficulty = DifficultyName,
                IsSolved = false
            };

            await _gameStateService.SaveGameStateAsync(snapshot);
        }

        public bool TryResumeSavedGame()
        {
            var saved = _gameStateService.LoadGameState();
            return saved is not null && TryRestore(saved);
        }

        public void PauseTimer() => _timer.Stop();

        public void ResumeTimer()
        {
            if (_phase == GamePhase.Playing)
            {
                _timer.Start();
            }
        }

        // ---- Input ----

        public PlacementResult TryPlaceNumber(int row, int col, int number)
        {
            if (!CanEditBoard) return new PlacementResult(PlacementOutcome.Rejected, false);
            if (number < 1 || number > 9) return new PlacementResult(PlacementOutcome.Rejected, false);
            if (!IsInRange(row, col)) return new PlacementResult(PlacementOutcome.Rejected, false);

            var cell = _board.GetCell(row, col);
            if (cell.IsGiven) return new PlacementResult(PlacementOutcome.Rejected, false);

            // Reject moves that conflict with another visible value; count the attempt as a mistake.
            if (!_validator.IsValidMove(_board, row, col, number))
            {
                _mistakesCount++;
                return new PlacementResult(PlacementOutcome.VisibleConflict, false);
            }

            _board.SetCell(row, col, number);
            _hasUserMadeEntries = true;

            _validator.UpdateErrorFlags(_board);

            var outcome = PlacementOutcome.PlacedCorrect;
            if (_solution is not null)
            {
                var solutionCell = _solution.GetCell(row, col);
                if (cell.Value != solutionCell.Value)
                {
                    cell.HasError = true;
                    _mistakesCount++;
                    outcome = PlacementOutcome.PlacedIncorrect;
                }
            }

            RaiseBoardChanged();

            bool solved = _validator.IsSolved(_board);
            if (solved)
            {
                _ = HandlePuzzleSolvedAsync();
            }

            return new PlacementResult(outcome, solved);
        }

        public bool TryClearCell(int row, int col)
        {
            if (!CanEditBoard) return false;
            if (!IsInRange(row, col)) return false;

            var cell = _board.GetCell(row, col);
            if (cell.IsGiven || cell.Value == 0) return false;

            cell.Value = 0;
            cell.HasError = false;
            _validator.UpdateErrorFlags(_board);
            _hasUserMadeEntries = true;

            RaiseBoardChanged();
            return true;
        }

        public HintResult TryGetHint()
        {
            if (!CanUseGameActions)
                return new HintResult(HintOutcome.Rejected, 0, 0, 0, false);

            _validator.UpdateErrorFlags(_board);
            if (!_validator.IsValidState(_board))
            {
                RaiseBoardChanged();
                return new HintResult(HintOutcome.BlockedByConflicts, 0, 0, 0, false);
            }

            _solution ??= _solver.GetSolution(_board);

            var hint = _solver.GetHint(_board);
            if (hint == null)
            {
                return new HintResult(HintOutcome.NoHintAvailable, 0, 0, 0, false);
            }

            var (row, col, value) = hint.Value;
            _board.SetCell(row, col, value);
            _hintsUsedCount++;

            _validator.UpdateErrorFlags(_board);
            RaiseBoardChanged();

            bool solved = _validator.IsSolved(_board);
            if (solved)
            {
                _ = HandlePuzzleSolvedAsync();
            }

            return new HintResult(HintOutcome.Provided, row, col, value, solved);
        }

        public CheckResult Check()
        {
            if (!CanUseGameActions)
                return new CheckResult(CheckOutcome.Rejected, 0);

            _validator.UpdateErrorFlags(_board);
            RaiseBoardChanged();

            if (!_validator.IsValidState(_board))
            {
                return new CheckResult(CheckOutcome.HasConflicts, 0);
            }

            if (_validator.IsSolved(_board))
            {
                _ = HandlePuzzleSolvedAsync();
                return new CheckResult(CheckOutcome.Solved, 81);
            }

            _solution ??= _solver.GetSolution(_board);
            int correct = _solution is not null
                ? _validator.CountCorrectCells(_board, _solution)
                : 0;

            return new CheckResult(CheckOutcome.InProgress, correct);
        }

        // ---- Internals ----

        private async Task HandlePuzzleSolvedAsync()
        {
            if (_phase == GamePhase.Completed) return;

            StopTimerInternal();

            await _gameStateService.ClearGameStateAsync();

            var stats = _settingsService.LoadStatistics();
            var difficultyForStats = _difficulty ?? DifficultyLevel.Medium;
            var previousBest = stats.GetBestTime(difficultyForStats);

            bool isNewRecord = !previousBest.HasValue || _elapsedSeconds < previousBest.Value;
            if (isNewRecord)
            {
                stats.SetBestTime(difficultyForStats, _elapsedSeconds);
                await _settingsService.SaveStatisticsAsync(stats);
            }

            SetPhase(GamePhase.Completed);

            PuzzleSolved?.Invoke(this, new PuzzleSolvedEventArgs
            {
                DifficultyName = DifficultyName,
                ElapsedSeconds = _elapsedSeconds,
                PreviousBestTime = previousBest,
                MistakesCount = _mistakesCount,
                HintsUsedCount = _hintsUsedCount,
                IsNewRecord = isNewRecord
            });
        }

        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _elapsedSeconds++;
            TimerTick?.Invoke(this, EventArgs.Empty);
        }

        private void StartTimerInternal()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void StopTimerInternal()
        {
            _timer.Stop();
        }

        private void SetPhase(GamePhase next)
        {
            if (_phase == next) return;
            _phase = next;
            PhaseChanged?.Invoke(this, _phase);
        }

        private void RaiseBoardChanged() => BoardChanged?.Invoke(this, EventArgs.Empty);

        private static bool IsInRange(int row, int col) =>
            row >= 0 && row < SudokuBoard.Size && col >= 0 && col < SudokuBoard.Size;

        private static bool BoardHasUserEntries(SudokuBoard board)
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

        private static DifficultyLevel? ParseDifficulty(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return Enum.TryParse<DifficultyLevel>(name, ignoreCase: true, out var parsed)
                ? parsed
                : null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
        }
    }
}
