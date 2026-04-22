using Sudoku.Application.Models;
using Sudoku.Application.Services;
using Sudoku.Core.Models;
using Sudoku.Core.Services;

namespace Sudoku.Application.Tests.Services
{
    /// <summary>
    /// Tests for <see cref="GameSession"/>. Uses real <see cref="SudokuValidator"/> and
    /// <see cref="SudokuBacktrackingSolver"/> (both stateless and fast). Generation is
    /// avoided in most tests by restoring a known serialized board via <see cref="GameSession.TryRestore"/>.
    /// </summary>
    public class GameSessionTests
    {
        // A valid solved board, used to construct near-complete restore states.
        private const string SolvedBoardValues =
            "534678912" +
            "672195348" +
            "198342567" +
            "859761423" +
            "426853791" +
            "713924856" +
            "961537284" +
            "287419635" +
            "345286179";

        // ---- Helpers ----

        private static GameSession CreateSession(
            FakeGameStateService? stateService = null,
            FakeSettingsService? settingsService = null)
        {
            var validator = new SudokuValidator();
            var solver = new SudokuBacktrackingSolver(validator);
            var logical = new SudokuLogicalSolver();
            var generator = new SudokuGenerator(solver, logical);
            return new GameSession(
                generator, validator, solver,
                stateService ?? new FakeGameStateService(),
                settingsService ?? new FakeSettingsService());
        }

        private static SudokuBoard MakeBoardFromValues(string digits, bool allGiven)
        {
            var board = new SudokuBoard();
            for (int i = 0; i < 81; i++)
            {
                int value = digits[i] - '0';
                if (value != 0)
                {
                    board.SetCell(i / 9, i % 9, value, isGiven: allGiven);
                }
            }
            return board;
        }

        /// <summary>
        /// Builds a near-complete puzzle: all cells from <see cref="SolvedBoardValues"/> are
        /// givens except the one at <paramref name="missingRow"/>, <paramref name="missingCol"/>
        /// which is empty. The missing value is returned for placement.
        /// </summary>
        private static (GameState state, int missingValue) CreateOneMissingState(
            int missingRow, int missingCol, int elapsedSeconds = 30)
        {
            var board = MakeBoardFromValues(SolvedBoardValues, allGiven: true);
            int missingValue = board.GetCell(missingRow, missingCol).Value;
            board.GetCell(missingRow, missingCol).Value = 0;
            board.GetCell(missingRow, missingCol).IsGiven = false;

            var solution = MakeBoardFromValues(SolvedBoardValues, allGiven: true);

            return (new GameState
            {
                BoardData = board.Serialize(),
                SolutionData = solution.Serialize(),
                ElapsedSeconds = elapsedSeconds,
                Difficulty = nameof(DifficultyLevel.Medium),
                IsSolved = false
            }, missingValue);
        }

        // ---- Initial state ----

        [Fact]
        public void NewSession_StartsInNotStarted_AndDisallowsAllInput()
        {
            var session = CreateSession();

            Assert.Equal(GamePhase.NotStarted, session.Phase);
            Assert.False(session.CanEditBoard);
            Assert.False(session.CanUseGameActions);
            Assert.Equal(0, session.ElapsedSeconds);
            Assert.Equal(0, session.MistakesCount);
            Assert.Equal(0, session.HintsUsedCount);
            Assert.False(session.HasUserMadeEntries);
        }

        [Fact]
        public void TryPlaceNumber_WhenNotStarted_IsRejected()
        {
            var session = CreateSession();

            var result = session.TryPlaceNumber(0, 0, 5);

            Assert.Equal(PlacementOutcome.Rejected, result.Outcome);
            Assert.False(result.PuzzleSolved);
        }

        [Fact]
        public void TryClearCell_WhenNotStarted_ReturnsFalse()
        {
            var session = CreateSession();

            Assert.False(session.TryClearCell(0, 0));
        }

        [Fact]
        public void TryGetHint_WhenNotStarted_IsRejected()
        {
            var session = CreateSession();

            var result = session.TryGetHint();

            Assert.Equal(HintOutcome.Rejected, result.Outcome);
        }

        [Fact]
        public void Check_WhenNotStarted_IsRejected()
        {
            var session = CreateSession();

            var result = session.Check();

            Assert.Equal(CheckOutcome.Rejected, result.Outcome);
        }

        // ---- Restore + Playing-state input authorization ----

        [Fact]
        public void TryRestore_WithValidUnsolvedState_TransitionsToPlaying()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);

            bool restored = session.TryRestore(state);

            Assert.True(restored);
            Assert.Equal(GamePhase.Playing, session.Phase);
            Assert.True(session.CanEditBoard);
            Assert.True(session.CanUseGameActions);
            Assert.Equal(30, session.ElapsedSeconds);
            Assert.Equal(DifficultyLevel.Medium, session.Difficulty);
        }

        [Fact]
        public void TryRestore_WithSolvedState_TransitionsToCompleted_AndDisallowsBoardInput()
        {
            var session = CreateSession();
            var solved = MakeBoardFromValues(SolvedBoardValues, allGiven: true);
            var state = new GameState
            {
                BoardData = solved.Serialize(),
                SolutionData = solved.Serialize(),
                ElapsedSeconds = 120,
                Difficulty = nameof(DifficultyLevel.Easy),
                IsSolved = true
            };

            Assert.True(session.TryRestore(state));
            Assert.Equal(GamePhase.Completed, session.Phase);
            Assert.False(session.CanEditBoard);
            Assert.False(session.CanUseGameActions);
        }

        [Fact]
        public void TryRestore_WithEmptyBoardData_ReturnsFalse_AndStaysNotStarted()
        {
            var session = CreateSession();

            bool restored = session.TryRestore(new GameState { BoardData = "" });

            Assert.False(restored);
            Assert.Equal(GamePhase.NotStarted, session.Phase);
        }

        [Fact]
        public void TryRestore_WithCorruptBoardData_ReturnsFalse_AndStaysNotStarted()
        {
            var session = CreateSession();

            bool restored = session.TryRestore(new GameState { BoardData = "garbage" });

            Assert.False(restored);
            Assert.Equal(GamePhase.NotStarted, session.Phase);
        }

        // ---- Placement ----

        [Fact]
        public void TryPlaceNumber_OnGivenCell_IsRejected()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            // (0,1) is a given in our state — only (0,0) is empty.
            var result = session.TryPlaceNumber(0, 1, 5);

            Assert.Equal(PlacementOutcome.Rejected, result.Outcome);
        }

        [Fact]
        public void TryPlaceNumber_OutOfRange_IsRejected()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            Assert.Equal(PlacementOutcome.Rejected, session.TryPlaceNumber(0, 0, 0).Outcome);
            Assert.Equal(PlacementOutcome.Rejected, session.TryPlaceNumber(0, 0, 10).Outcome);
            Assert.Equal(PlacementOutcome.Rejected, session.TryPlaceNumber(-1, 0, 5).Outcome);
            Assert.Equal(PlacementOutcome.Rejected, session.TryPlaceNumber(0, 9, 5).Outcome);
        }

        [Fact]
        public void TryPlaceNumber_VisibleConflict_CountsMistakeAndDoesNotPlace()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            // Row 0 already contains 3,4,6,7,8,9,1,2 (only (0,0)=5 is missing).
            // Try placing 3 — visible conflict with same-row cell.
            var result = session.TryPlaceNumber(0, 0, 3);

            Assert.Equal(PlacementOutcome.VisibleConflict, result.Outcome);
            Assert.Equal(0, session.Board.GetCell(0, 0).Value);
            Assert.Equal(1, session.MistakesCount);
            Assert.False(session.HasUserMadeEntries);
        }

        [Fact]
        public void TryPlaceNumber_CorrectFinalValue_SolvesPuzzle_AndRaisesPuzzleSolved()
        {
            var session = CreateSession();
            var (state, missingValue) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            PuzzleSolvedEventArgs? raised = null;
            session.PuzzleSolved += (_, e) => raised = e;

            var result = session.TryPlaceNumber(0, 0, missingValue);

            Assert.Equal(PlacementOutcome.PlacedCorrect, result.Outcome);
            Assert.True(result.PuzzleSolved);
            Assert.Equal(GamePhase.Completed, session.Phase);
            Assert.NotNull(raised);
            Assert.Equal(0, raised!.MistakesCount);
            Assert.Equal(0, raised.HintsUsedCount);
            Assert.True(raised.IsNewRecord); // No previous best in fake settings.
        }

        // ---- The original bug class: completed games must reject gameplay input ----

        [Fact]
        public void AfterCompletion_AllGameplayInputIsRejected()
        {
            var session = CreateSession();
            var (state, missingValue) = CreateOneMissingState(0, 0);
            session.TryRestore(state);
            session.TryPlaceNumber(0, 0, missingValue); // Solves puzzle.

            Assert.Equal(GamePhase.Completed, session.Phase);
            Assert.False(session.CanEditBoard);
            Assert.False(session.CanUseGameActions);

            Assert.Equal(PlacementOutcome.Rejected, session.TryPlaceNumber(0, 0, 1).Outcome);
            Assert.False(session.TryClearCell(0, 0));
            Assert.Equal(HintOutcome.Rejected, session.TryGetHint().Outcome);
            Assert.Equal(CheckOutcome.Rejected, session.Check().Outcome);
        }

        // ---- Clear ----

        [Fact]
        public void TryClearCell_OnGivenCell_ReturnsFalse()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            Assert.False(session.TryClearCell(0, 1));
        }

        [Fact]
        public void TryClearCell_OnEmptyCell_ReturnsFalse()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            Assert.False(session.TryClearCell(0, 0));
        }

        [Fact]
        public void TryClearCell_OnUserPlacedCell_ClearsAndMarksUserEntries()
        {
            var session = CreateSession();
            var board = new SudokuBoard();
            // Place a single given so the board isn't entirely empty for save logic.
            board.SetCell(8, 8, 9, isGiven: true);
            // Pre-populate (0,0) as a non-given user value to clear later.
            board.GetCell(0, 0).Value = 5;
            board.GetCell(0, 0).IsGiven = false;

            session.TryRestore(new GameState
            {
                BoardData = board.Serialize(),
                SolutionData = null,
                ElapsedSeconds = 0,
                Difficulty = nameof(DifficultyLevel.Easy),
                IsSolved = false
            });

            Assert.True(session.TryClearCell(0, 0));
            Assert.Equal(0, session.Board.GetCell(0, 0).Value);
            Assert.True(session.HasUserMadeEntries);
        }

        // ---- Hint ----

        [Fact]
        public void TryGetHint_FillsTheMissingCellAndIncrementsHintCount()
        {
            var session = CreateSession();
            var (state, missingValue) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            var result = session.TryGetHint();

            Assert.Equal(HintOutcome.Provided, result.Outcome);
            Assert.Equal(0, result.Row);
            Assert.Equal(0, result.Col);
            Assert.Equal(missingValue, result.Value);
            Assert.True(result.PuzzleSolved);
            Assert.Equal(1, session.HintsUsedCount);
        }

        // ---- Check ----

        [Fact]
        public void Check_OnInProgressBoardWithNoConflicts_ReturnsInProgressWithCount()
        {
            var session = CreateSession();
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            var result = session.Check();

            Assert.Equal(CheckOutcome.InProgress, result.Outcome);
            Assert.Equal(80, result.CorrectCellCount);
        }

        // ---- Restart ----

        [Fact]
        public void Restart_ClearsUserEntries_KeepsGivens_ResetsTimerAndStats()
        {
            var session = CreateSession();
            var (state, missingValue) = CreateOneMissingState(0, 0, elapsedSeconds: 99);
            session.TryRestore(state);
            // Make a wrong placement that registers as a mistake (not the correct value).
            // We need a value not conflicting visibly with row/col/box but still wrong.
            // Easier: just call TryGetHint to bump hints, then restart.
            session.TryGetHint();
            // After hint above the puzzle is solved (only 1 cell was missing), so we
            // restore again and bump only mistake count via wrong-but-non-conflicting placement.
            session.TryRestore(state);
            // (0,0) row contains 3,4,6,7,8,9,1,2; col 0 contains 5,6,1,8,4,7,9,2,3; box 0 has 3,4,6,7,1,9,1,8.
            // So 5 is the only valid value. Try placing 5 — that will solve again. We'll instead
            // bump mistakes by pre-incrementing through a guaranteed visible conflict path:
            session.TryPlaceNumber(0, 0, 3); // visible conflict, +1 mistake, value not placed.
            Assert.Equal(1, session.MistakesCount);

            session.Restart();

            Assert.Equal(GamePhase.Playing, session.Phase);
            Assert.Equal(0, session.MistakesCount);
            Assert.Equal(0, session.HintsUsedCount);
            Assert.Equal(0, session.ElapsedSeconds);
            Assert.False(session.HasUserMadeEntries);
            // (0,0) was a non-given in the restored state, so it remains empty after restart.
            Assert.Equal(0, session.Board.GetCell(0, 0).Value);
            // (0,1) was a given — preserved.
            Assert.NotEqual(0, session.Board.GetCell(0, 1).Value);
            Assert.True(session.Board.GetCell(0, 1).IsGiven);
        }

        // ---- Phase events ----

        [Fact]
        public void PhaseChanged_FiresOnEachTransition()
        {
            var session = CreateSession();
            var transitions = new List<GamePhase>();
            session.PhaseChanged += (_, p) => transitions.Add(p);

            var (state, missingValue) = CreateOneMissingState(0, 0);
            session.TryRestore(state);
            session.TryPlaceNumber(0, 0, missingValue);

            Assert.Equal(new[] { GamePhase.Playing, GamePhase.Completed }, transitions);
        }

        // ---- Save ----

        [Fact]
        public async Task SaveAsync_DoesNotPersist_WhenPhaseIsCompleted()
        {
            var stateService = new FakeGameStateService();
            var session = CreateSession(stateService);

            var solved = MakeBoardFromValues(SolvedBoardValues, allGiven: true);
            session.TryRestore(new GameState
            {
                BoardData = solved.Serialize(),
                SolutionData = solved.Serialize(),
                ElapsedSeconds = 0,
                Difficulty = nameof(DifficultyLevel.Easy),
                IsSolved = true
            });

            await session.SaveAsync();

            Assert.Null(stateService.LastSaved);
        }

        [Fact]
        public async Task SaveAsync_PersistsWhenPlaying()
        {
            var stateService = new FakeGameStateService();
            var session = CreateSession(stateService);
            var (state, _) = CreateOneMissingState(0, 0);
            session.TryRestore(state);

            await session.SaveAsync();

            Assert.NotNull(stateService.LastSaved);
            Assert.False(stateService.LastSaved!.IsSolved);
        }

        // ---- Fakes ----

        private sealed class FakeGameStateService : IGameStateService
        {
            public GameState? Stored { get; set; }
            public GameState? LastSaved { get; private set; }
            public int ClearCount { get; private set; }

            public GameState? LoadGameState() => Stored;

            public Task SaveGameStateAsync(GameState gameState)
            {
                LastSaved = gameState;
                Stored = gameState;
                return Task.CompletedTask;
            }

            public Task ClearGameStateAsync()
            {
                ClearCount++;
                Stored = null;
                return Task.CompletedTask;
            }

            public bool HasSavedGame() => Stored is not null;
        }

        private sealed class FakeSettingsService : ISettingsService
        {
            public GameSettings Settings { get; set; } = new();
            public GameStatistics Statistics { get; set; } = new();

            public GameSettings LoadSettings() => Settings;

            public Task SaveSettingsAsync(GameSettings settings)
            {
                Settings = settings;
                return Task.CompletedTask;
            }

            public Task<GameSettings> GetSettingsAsync() => Task.FromResult(Settings);

            public GameStatistics LoadStatistics() => Statistics;

            public Task SaveStatisticsAsync(GameStatistics statistics)
            {
                Statistics = statistics;
                return Task.CompletedTask;
            }
        }
    }
}
