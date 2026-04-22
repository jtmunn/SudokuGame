using Sudoku.Application.Models;
using Sudoku.Application.Services;
using Sudoku.Core.Models;
using Sudoku.Core.Services;
using Sudoku.Maui.Controls;
using Sudoku.Maui.Helpers;
using CoreDifficulty = Sudoku.Core.Services.DifficultyLevel;

namespace Sudoku.Maui.Pages
{
    public partial class SudokuPage : ContentPage
    {
        private readonly ISettingsService _settingsService;
        private readonly IGameSession _session;
        private readonly CellHighlightManager _highlightManager;

        private readonly Button[,] _cellButtons;
        private Button? _selectedButton;
        private int _selectedRow = -1;
        private int _selectedCol = -1;
        private bool _isFirstAppearing = true;
        private bool _isProcessingInput;

        // Theme color accessors --------------------------------------------------
        private Color DefaultCellColor => GetThemeColor("CellDefaultColor");
        private Color GivenCellColor => GetThemeColor("CellGivenColor");
        private Color ErrorCellColor => GetThemeColor("CellErrorColor");
        private Color CellTextColor => GetThemeColor("CellUserTextColor");
        private Color GivenTextColor => GetThemeColor("CellGivenTextColor");

        private Color GetThemeColor(string key)
        {
            if (Microsoft.Maui.Controls.Application.Current?.Resources != null)
            {
                foreach (var dict in Microsoft.Maui.Controls.Application.Current.Resources.MergedDictionaries)
                {
                    if (dict.ContainsKey(key))
                        return (Color)dict[key];
                }
            }
            throw new InvalidOperationException($"Theme color '{key}' not found in any loaded theme. Ensure both LightTheme.xaml and DarkTheme.xaml define this color.");
        }

        // Construction ----------------------------------------------------------
        public SudokuPage(ISettingsService settingsService, IGameSession session)
        {
            InitializeComponent();

            _settingsService = settingsService;
            _session = session;
            _highlightManager = new CellHighlightManager((key, _) => GetThemeColor(key));

            _cellButtons = SudokuBoardControl.GetAllCellButtons();
            SudokuBoardControl.CellClicked += OnCellClicked;

            SizeChanged += OnPageSizeChanged;
            GridBorder.SizeChanged += OnGridBorderSizeChanged;
            UpdateButtonSizes();

            // Wire to session events. Page is a long-lived singleton in practice
            // (Shell caches it), so subscribe once for the lifetime of the page.
            _session.PhaseChanged += OnSessionPhaseChanged;
            _session.BoardChanged += OnSessionBoardChanged;
            _session.TimerTick += OnSessionTimerTick;
            _session.PuzzleSolved += OnSessionPuzzleSolved;

            UpdateGrid();
            ApplySettings();
        }

        private void ApplySettings()
        {
            var settings = _settingsService.LoadSettings();
            HintButton.IsVisible = settings.ShowHintButton;
            CheckButton.IsVisible = settings.ShowCheckButton;
            UpdateButtonSizes();
        }

        // Layout / sizing -------------------------------------------------------
        private void OnPageSizeChanged(object? sender, EventArgs e) => UpdateButtonSizes();

        private void OnGridBorderSizeChanged(object? sender, EventArgs e)
        {
            double gridSize = Math.Min(GridBorder.Width, GridBorder.Height);
            if (gridSize > 0 && !double.IsNaN(gridSize))
            {
                double cellFontSize = SudokuLayoutCalculator.CalculateCellFontSize(gridSize);
                UpdateCellFontSizes(cellFontSize);
            }
        }

        private void UpdateButtonSizes()
        {
            var layout = SudokuLayoutCalculator.Calculate(Width, Height);

            UpdateButtonsInContainer(NumberPadRow1, layout);
            UpdateButtonsInContainer(NumberPadRow2, layout);
            UpdateCircularButton(ClearButton, layout);

            double actionHeight = Math.Round(layout.ButtonSize * 0.45);
            double actionFontSize = Math.Round(layout.ButtonSize * 0.22);
            double actionIconSize = Math.Round(layout.ButtonSize * 0.22);
            HintButton.HeightRequest = actionHeight;
            HintButton.CornerRadius = (int)(actionHeight / 2);
            HintButton.FontSize = actionFontSize;
            CheckButton.HeightRequest = actionHeight;
            CheckButton.CornerRadius = (int)(actionHeight / 2);
            CheckButton.FontSize = actionFontSize;

            if (HintButton.ImageSource is FontImageSource hintIcon)
                hintIcon.Size = actionIconSize;
            if (CheckButton.ImageSource is FontImageSource checkIcon)
                checkIcon.Size = actionIconSize;
        }

        private static void UpdateButtonsInContainer(Microsoft.Maui.Controls.Layout container, LayoutMetrics layout)
        {
            foreach (var child in container.Children)
            {
                if (child is Controls.NumberPadButton numPadBtn)
                {
                    numPadBtn.WidthRequest = layout.ButtonSize;
                    numPadBtn.HeightRequest = layout.ButtonSize;
                    numPadBtn.MainFontSize = layout.FontSize;
                    numPadBtn.CountFontSize = layout.CountFontSize;
                    numPadBtn.CountMargin = layout.CountMargin;
                }
            }
        }

        private static void UpdateCircularButton(Button button, LayoutMetrics layout)
        {
            button.WidthRequest = layout.ButtonSize;
            button.HeightRequest = layout.ButtonSize;
            button.CornerRadius = (int)(layout.ButtonSize / 2);
            button.FontSize = layout.FontSize;
        }

        private void UpdateCellFontSizes(double fontSize)
        {
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    _cellButtons[row, col].FontSize = fontSize;
                }
            }
        }

        // Session event handlers (always dispatch to UI thread) -----------------
        private void OnSessionPhaseChanged(object? sender, GamePhase phase)
        {
            Dispatcher.Dispatch(() =>
            {
                UpdateActionButtonsEnabled();
                UpdateHeaderLabels();
            });
        }

        private void OnSessionBoardChanged(object? sender, EventArgs e) =>
            Dispatcher.Dispatch(UpdateGrid);

        private void OnSessionTimerTick(object? sender, EventArgs e) =>
            Dispatcher.Dispatch(() => TimerLabel.Text = $"Time: {FormatTime(_session.ElapsedSeconds)}");

        private async void OnSessionPuzzleSolved(object? sender, PuzzleSolvedEventArgs e)
        {
            await SummaryOverlay.ShowAsync(
                e.DifficultyName,
                e.ElapsedSeconds,
                e.PreviousBestTime,
                e.MistakesCount,
                e.HintsUsedCount);
        }

        // Header button handlers ------------------------------------------------
        private async void OnNewGameClicked(object? sender, EventArgs e)
        {
            if (_isProcessingInput) return;
            _isProcessingInput = true;
            try
            {
                if (_session.Phase == GamePhase.Playing && _session.HasUserMadeEntries)
                {
                    bool answer = await DisplayAlertAsync("Abandon Puzzle?", "All progress will be lost. Start a new game?", "Yes", "No");
                    if (!answer) return;
                }
                await ShowDifficultySelectionAsync();
            }
            finally
            {
                _isProcessingInput = false;
            }
        }

        private async void OnRestartClicked(object? sender, EventArgs e)
        {
            if (_isProcessingInput) return;
            _isProcessingInput = true;
            try
            {
                if (_session.Phase == GamePhase.Playing && _session.HasUserMadeEntries)
                {
                    bool answer = await DisplayAlertAsync("Restart Puzzle?", "All progress will be lost. Restart this puzzle?", "Yes", "No");
                    if (!answer) return;
                }
                _session.Restart();
                ClearSelection();
            }
            finally
            {
                _isProcessingInput = false;
            }
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            if (_isProcessingInput) return;
            _isProcessingInput = true;
            try
            {
                _session.PauseTimer();
                await _session.SaveAsync();
                await Shell.Current.GoToAsync(nameof(SettingsPage));
            }
            finally
            {
                _isProcessingInput = false;
            }
        }

        // Game action handlers --------------------------------------------------
        private async void OnHintClicked(object? sender, EventArgs e)
        {
            if (_isProcessingInput) return;
            _isProcessingInput = true;
            try
            {
                var result = _session.TryGetHint();
                switch (result.Outcome)
                {
                    case HintOutcome.Rejected:
                        return;
                    case HintOutcome.BlockedByConflicts:
                        await DisplayAlertAsync("Fix Conflicts First", "Resolve highlighted conflicts before requesting a hint.", "OK");
                        return;
                    case HintOutcome.NoHintAvailable:
                        await DisplayAlertAsync("No Hint Available", "No valid hints are available right now.", "OK");
                        return;
                    case HintOutcome.Provided:
                        _selectedRow = result.Row;
                        _selectedCol = result.Col;
                        _selectedButton = _cellButtons[result.Row, result.Col];
                        HighlightSelection(result.Row, result.Col);
                        if (_selectedButton != null)
                        {
                            await _selectedButton.ScaleToAsync(1.08, 120, Easing.CubicOut);
                            await _selectedButton.ScaleToAsync(1.0, 120, Easing.CubicIn);
                        }
                        return;
                }
            }
            finally
            {
                _isProcessingInput = false;
            }
        }

        private async void OnCheckClicked(object? sender, EventArgs e)
        {
            if (_isProcessingInput) return;
            _isProcessingInput = true;
            try
            {
                var result = _session.Check();
                switch (result.Outcome)
                {
                    case CheckOutcome.Rejected:
                        return;
                    case CheckOutcome.HasConflicts:
                        await DisplayAlertAsync("Conflicts Found", "There are conflicts highlighted in red. Please fix them.", "OK");
                        return;
                    case CheckOutcome.Solved:
                        // PuzzleSolved event handler will show the summary overlay.
                        return;
                    case CheckOutcome.InProgress:
                        await DisplayAlertAsync("Progress Check", $"No conflicts found. {result.CorrectCellCount}/81 cells are correct so far.", "OK");
                        return;
                }
            }
            finally
            {
                _isProcessingInput = false;
            }
        }

        private void OnClearClicked(object? sender, EventArgs e) => ClearSelectedCell();

        // Grid rendering --------------------------------------------------------
        private void UpdateActionButtonsEnabled()
        {
            HintButton.IsEnabled = CheckButton.IsEnabled = ClearButton.IsEnabled = _session.CanUseGameActions;
        }

        private void UpdateGrid()
        {
            UpdateActionButtonsEnabled();

            var board = _session.Board;
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    var button = _cellButtons[row, col];

                    button.Text = cell.Value == 0 ? "" : cell.Value.ToString();

                    if (cell.HasError)
                    {
                        button.BackgroundColor = ErrorCellColor;
                        button.FontAttributes = FontAttributes.Bold;
                        button.TextColor = Colors.White;
                    }
                    else if (cell.IsGiven)
                    {
                        button.BackgroundColor = GivenCellColor;
                        button.FontAttributes = FontAttributes.Bold;
                        button.TextColor = GivenTextColor;
                    }
                    else
                    {
                        button.BackgroundColor = DefaultCellColor;
                        button.FontAttributes = FontAttributes.Bold;
                        button.TextColor = CellTextColor;
                    }
                }
            }

            if (_selectedRow >= 0 && _selectedCol >= 0)
            {
                HighlightSelection(_selectedRow, _selectedCol);
            }

            UpdateNumberPadCounts();
        }

        private void UpdateNumberPadCounts()
        {
            var remaining = new int[10];
            for (int i = 1; i <= 9; i++) remaining[i] = 9;

            var board = _session.Board;
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    if (cell.Value >= 1 && cell.Value <= 9)
                        remaining[cell.Value]--;
                }
            }

            UpdateNumPadCounts(NumberPadRow1, remaining);
            UpdateNumPadCounts(NumberPadRow2, remaining);
        }

        private static void UpdateNumPadCounts(Microsoft.Maui.Controls.Layout container, int[] remaining)
        {
            foreach (var child in container.Children)
            {
                if (child is Controls.NumberPadButton numPadBtn)
                {
                    numPadBtn.RemainingCount = remaining[numPadBtn.Number];
                    numPadBtn.IsEnabled = remaining[numPadBtn.Number] > 0;
                }
            }
        }

        // Selection / highlighting ---------------------------------------------
        private void OnCellClicked(object? sender, CellClickedEventArgs e)
        {
            if (!_session.CanEditBoard) return;

            _selectedRow = e.Row;
            _selectedCol = e.Col;
            _selectedButton = _cellButtons[e.Row, e.Col];
            HighlightSelection(e.Row, e.Col);
        }

        private void HighlightSelection(int row, int col)
        {
            var colorMap = _highlightManager.CalculateColors(_session.Board, row, col);
            for (int r = 0; r < SudokuBoard.Size; r++)
            {
                for (int c = 0; c < SudokuBoard.Size; c++)
                {
                    var (backgroundColor, textColor) = colorMap.GetColors(r, c);
                    var button = _cellButtons[r, c];
                    button.BackgroundColor = backgroundColor;
                    button.TextColor = textColor;
                }
            }
        }

        private void ClearSelection()
        {
            _selectedRow = -1;
            _selectedCol = -1;
            _selectedButton = null;
            UpdateGrid();
        }

        // Number input ---------------------------------------------------------
        private async void OnNumPadButtonTapped(object? sender, EventArgs e)
        {
            if (sender is not Controls.NumberPadButton numPadButton) return;
            await ApplyNumberInputAsync(numPadButton.Number);
        }

        private async Task ApplyNumberInputAsync(int number)
        {
            if (_isProcessingInput) return;
            if (_selectedRow < 0 || _selectedCol < 0) return;

            _isProcessingInput = true;
            try
            {
                var result = _session.TryPlaceNumber(_selectedRow, _selectedCol, number);
                if (result.Outcome == PlacementOutcome.VisibleConflict)
                {
                    await ShowConflictFeedbackAsync();
                }
            }
            finally
            {
                _isProcessingInput = false;
            }
        }

        private async Task ShowConflictFeedbackAsync()
        {
            if (_selectedButton != null)
            {
                var originalColor = _selectedButton.BackgroundColor;
                _selectedButton.BackgroundColor = ErrorCellColor;
                await Task.Delay(300);
                _selectedButton.BackgroundColor = originalColor;
            }
        }

        private void ClearSelectedCell()
        {
            if (_selectedRow < 0 || _selectedCol < 0) return;
            _session.TryClearCell(_selectedRow, _selectedCol);
        }

        // Header / time formatting ---------------------------------------------
        private static string FormatTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        private void UpdateHeaderLabels()
        {
            DifficultyLabel.Text = $"Difficulty: {_session.DifficultyName}";
            TimerLabel.Text = $"Time: {FormatTime(_session.ElapsedSeconds)}";
        }

        // Window event wiring --------------------------------------------------
        private void AttachWindowKeyHandler()
        {
#if WINDOWS
            var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow &&
                nativeWindow.Content is Microsoft.UI.Xaml.UIElement rootElement)
            {
                rootElement.KeyUp -= OnNativeWindowKeyUp;
                rootElement.KeyUp += OnNativeWindowKeyUp;
            }
#endif
        }

        private void AttachWindowFocusHandler()
        {
            var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                window.Activated -= OnWindowActivated;
                window.Deactivated -= OnWindowDeactivated;
                window.Activated += OnWindowActivated;
                window.Deactivated += OnWindowDeactivated;
            }
        }

        private void DetachWindowKeyHandler()
        {
#if WINDOWS
            var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow &&
                nativeWindow.Content is Microsoft.UI.Xaml.UIElement rootElement)
            {
                rootElement.KeyUp -= OnNativeWindowKeyUp;
            }
#endif
        }

        private void DetachWindowFocusHandler()
        {
            var window = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                window.Activated -= OnWindowActivated;
                window.Deactivated -= OnWindowDeactivated;
            }
        }

#if WINDOWS
        private async void OnNativeWindowKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (!TryGetDigitFromVirtualKey(e.Key, out var number))
            {
                if (e.Key == Windows.System.VirtualKey.Back || e.Key == Windows.System.VirtualKey.Delete)
                {
                    ClearSelectedCell();
                    e.Handled = true;
                }
                return;
            }

            await ApplyNumberInputAsync(number);
            e.Handled = true;
        }

        private static bool TryGetDigitFromVirtualKey(Windows.System.VirtualKey key, out int number)
        {
            number = 0;
            if (key >= Windows.System.VirtualKey.Number1 && key <= Windows.System.VirtualKey.Number9)
            {
                number = (int)key - (int)Windows.System.VirtualKey.Number0;
                return true;
            }
            if (key >= Windows.System.VirtualKey.NumberPad1 && key <= Windows.System.VirtualKey.NumberPad9)
            {
                number = (int)key - (int)Windows.System.VirtualKey.NumberPad0;
                return true;
            }
            return false;
        }
#endif

        private void OnWindowActivated(object? sender, EventArgs e) => _session.ResumeTimer();
        private void OnWindowDeactivated(object? sender, EventArgs e) => _session.PauseTimer();

        // Page lifecycle -------------------------------------------------------
        protected override void OnAppearing()
        {
            base.OnAppearing();
            AttachWindowKeyHandler();
            AttachWindowFocusHandler();

            if (_isFirstAppearing)
            {
                _isFirstAppearing = false;
                if (!_session.TryResumeSavedGame())
                {
                    _ = ShowDifficultySelectionAsync(canDismiss: false);
                }
            }
            else
            {
                ApplySettings();
                UpdateGrid();
                _session.ResumeTimer();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DetachWindowKeyHandler();
            DetachWindowFocusHandler();
            _session.PauseTimer();
            _ = _session.SaveAsync();
        }

        // Difficulty / summary overlays ----------------------------------------
        private async Task ShowDifficultySelectionAsync(bool canDismiss = true)
        {
            _session.PauseTimer();

            var settings = _settingsService.LoadSettings();
            var statistics = _settingsService.LoadStatistics();
            CoreDifficulty? lastPlayed = settings.LastPlayedDifficulty;

            await DifficultyOverlay.ShowAsync(lastPlayed, statistics, canDismiss);
        }

        private async void OnDifficultySelected(object? sender, CoreDifficulty difficulty)
        {
            CancellationTokenSource? spinnerCts = null;
            try
            {
                spinnerCts = new CancellationTokenSource();
                var spinnerToken = spinnerCts.Token;

                _ = Task.Run(async () =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (spinnerToken.IsCancellationRequested) return;
                        await Task.Delay(50);
                    }
                    if (!spinnerToken.IsCancellationRequested)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LoadingOverlay.IsVisible = true;
                            LoadingMessage.Text = "Generating puzzle...";
                        });
                    }
                });

                ClearSelection();
                await _session.StartNewAsync(difficulty);
            }
            finally
            {
                spinnerCts?.Cancel();
                spinnerCts?.Dispose();
                LoadingOverlay.IsVisible = false;
            }
        }

        private void OnDifficultyOverlayDismissed(object? sender, EventArgs e) => _session.ResumeTimer();

        private void OnSummaryDoneRequested(object? sender, EventArgs e)
        {
            // No-op: overlay hides itself; lock state is owned by the session.
        }

        private async void OnSummaryPlayAgainRequested(object? sender, EventArgs e) =>
            await ShowDifficultySelectionAsync();
    }
}
