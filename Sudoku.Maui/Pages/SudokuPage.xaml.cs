using Sudoku.Core.Models;
using Sudoku.Core.Services;
using CoreDifficulty = Sudoku.Core.Services.DifficultyLevel;
using Sudoku.Application.Services;
using Sudoku.Maui.Controls;
using Sudoku.Maui.Helpers;
using Sudoku.Application.ViewModels;
using AppModels = Sudoku.Application.Models;

namespace Sudoku.Maui.Pages
{
    public partial class SudokuPage : ContentPage
    {
        private readonly SudokuGenerator _generator;
        private readonly SudokuValidator _validator;
        private readonly SudokuSolver _solver;
        private readonly ISettingsService _settingsService;
        private readonly IGameStateService _gameStateService;
        private readonly SudokuPageViewModel _viewModel;
        private readonly CellHighlightManager _highlightManager;

        private SudokuBoard _currentBoard;
        private SudokuBoard? _solution;
        private Button[,] _cellButtons;
        private Button? _selectedButton;
        private int _selectedRow = -1;
        private int _selectedCol = -1;
        
        // Layout constants now in SudokuLayoutManager
        private double _currentGridSize = SudokuLayoutManager.BaseGridSize;
        
        // Timer/state/statistics now in ViewModel
        private bool _isFirstAppearing = true;

        // Colors for visual feedback - use safe access with fallback
        private Color DefaultCellColor => GetThemeColor("CellDefaultColor", Colors.White);
        private Color GivenCellColor => GetThemeColor("CellGivenColor", Colors.White);
        private Color SelectedCellColor => GetThemeColor("CellSelectedColor", Colors.LightBlue);
        private Color ErrorCellColor => GetThemeColor("CellErrorColor", Colors.Red);
        private Color HighlightCellColor => GetThemeColor("CellHighlightColor", Colors.LightGray);
        private Color LightHighlightCellColor => GetThemeColor("CellLightHighlightColor", Colors.LightGray);
        private Color MatchingNumberColor => GetThemeColor("CellMatchingNumberColor", Colors.Gray);
        private Color CellTextColor => GetThemeColor("CellUserTextColor", Colors.Black);
        private Color GivenTextColor => GetThemeColor("CellGivenTextColor", Colors.Black);
        private Color BorderColor => GetThemeColor("GridBorderColor", Colors.Gray);
        private Color ThickBorderColor => GetThemeColor("GridThickBorderColor", Colors.DarkBlue);

        private Color GetThemeColor(string key, Color fallback)
        {
            try
            {
                if (Microsoft.Maui.Controls.Application.Current?.Resources != null)
                {
                    // Search through merged dictionaries like SudokuGridView does
                    foreach (var dict in Microsoft.Maui.Controls.Application.Current.Resources.MergedDictionaries)
                    {
                        if (dict.ContainsKey(key))
                            return (Color)dict[key];
                    }
                }
            }
            catch
            {
                // Resource not available
            }
            return fallback;
        }

        public SudokuPage(SudokuGenerator generator, SudokuValidator validator, 
                         SudokuSolver solver, ISettingsService settingsService,
                         IGameStateService gameStateService, SudokuPageViewModel viewModel)
        {
            InitializeComponent();
            
            _generator = generator;
            _validator = validator;
            _solver = solver;
            _settingsService = settingsService;
            _gameStateService = gameStateService;
            _viewModel = viewModel;
            _highlightManager = new CellHighlightManager(GetThemeColor);
            
            // Set BindingContext for XAML data binding
            BindingContext = _viewModel;
            
            _currentBoard = new SudokuBoard();

            // Get cell buttons from the custom control
            _cellButtons = SudokuGridView.GetAllCellButtons();
            
            // Subscribe to cell click events
            SudokuGridView.CellClicked += OnCellClicked;

            // Grid lines are handled by DynamicResource in SudokuGridView
            
            SizeChanged += OnPageSizeChanged;
            UpdateGridSize();
            
            // Initialize all cells with default white backgrounds
            UpdateGrid();
            
            // Don't start game here - wait for OnAppearing when theme is loaded
            ApplySettings();
        }

        private void ApplySettings()
        {
            var settings = _settingsService.LoadSettings();
            // Only hide buttons if settings explicitly says so, otherwise show them
            HintButton.IsVisible = settings.ShowHintButton;
            CheckButton.IsVisible = settings.ShowCheckButton;
            
            // Trigger full layout update which includes button sizing
            UpdateGridSize();
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            UpdateGridSize();
        }

        private void UpdateGridSize()
        {
            var settings = _settingsService.LoadSettings();
            bool showActionButtons = settings.ShowHintButton || settings.ShowCheckButton;
            
            // Use LayoutManager to calculate all sizes and positions
            var layout = SudokuLayoutManager.Calculate(Width, Height, showActionButtons);
            
            _currentGridSize = layout.GridSize;
            GridBorder.WidthRequest = layout.GridSize;
            GridBorder.HeightRequest = layout.GridSize;
            
            // Update font sizes for all cell buttons
            UpdateCellFontSizes(layout.CellFontSize);
            
            // Update button sizes
            UpdateButtonSizes(layout);
            
            // Position action buttons if there's space
            if (layout.HasSpaceForActionButtons && showActionButtons)
            {
                AbsoluteLayout.SetLayoutBounds(ActionButtonStack, layout.ActionButtonBounds);
                ActionButtonStack.IsVisible = true;
            }
            else
            {
                ActionButtonStack.IsVisible = false;
            }
        }
        
        private void UpdateButtonSizes(LayoutCalculations layout)
        {
            // Update number pad buttons using calculated layout
            foreach (var child in NumberPad.Children)
            {
                if (child is Button btn)
                {
                    btn.WidthRequest = layout.ButtonSize;
                    btn.HeightRequest = layout.ButtonSize;
                    btn.CornerRadius = (int)(layout.ButtonSize / 2);
                    btn.FontSize = layout.FontSize;
                }
                else if (child is Controls.NumPadButton numPadBtn)
                {
                    numPadBtn.WidthRequest = layout.ButtonSize;
                    numPadBtn.HeightRequest = layout.ButtonSize;
                    numPadBtn.MainFontSize = layout.FontSize;
                    numPadBtn.CountFontSize = layout.CountFontSize;
                    numPadBtn.CountMargin = layout.CountMargin;
                }
            }
            
            // Action buttons - same sizing
            if (HintButton.IsVisible)
            {
                HintButton.WidthRequest = layout.ButtonSize;
                HintButton.HeightRequest = layout.ButtonSize;
                HintButton.CornerRadius = (int)(layout.ButtonSize / 2);
                HintButton.FontSize = layout.FontSize;
            }
            
            if (CheckButton.IsVisible)
            {
                CheckButton.WidthRequest = layout.ButtonSize;
                CheckButton.HeightRequest = layout.ButtonSize;
                CheckButton.CornerRadius = (int)(layout.ButtonSize / 2);
                CheckButton.FontSize = layout.FontSize;
            }
        }

        private void UpdateCellFontSizes(double fontSize)
        {
            // Apply calculated font size to all cell buttons
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    _cellButtons[row, col].FontSize = fontSize;
                }
            }
        }

        /// <summary>
        /// Starts a new game with a fresh puzzle.
        /// </summary>
        private async Task StartNewGameAsync(Sudoku.Core.Services.DifficultyLevel difficulty)
        {
            CancellationTokenSource? spinnerCts = null;
            
            try
            {
                // Map MAUI DifficultyLevel to Core DifficultyLevel
                var coreDifficulty = MapDifficulty(difficulty);
                
                // Setup delayed spinner (only show if generation takes >500ms)
                spinnerCts = new CancellationTokenSource();
                var spinnerToken = spinnerCts.Token;
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Check cancellation every 50ms instead of relying on exception
                        // This avoids TaskCanceledException breaking the debugger
                        for (int i = 0; i < 10; i++) // 10 * 50ms = 500ms
                        {
                            if (spinnerToken.IsCancellationRequested)
                                return; // Exit without showing spinner
                            
                            await Task.Delay(50);
                        }
                        
                        // If we reach here, generation is taking >500ms, show spinner
                        if (!spinnerToken.IsCancellationRequested)
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                LoadingOverlay.IsVisible = true;
                                LoadingMessage.Text = "Generating puzzle...";
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log unexpected errors but don't crash
                        System.Diagnostics.Debug.WriteLine($"Spinner task error: {ex.Message}");
                    }
                });  // ✅ Token only used inside the task
                
                // Generate new puzzle on background thread
                var board = await Task.Run(() => _generator.Generate(coreDifficulty));
                
                // Cancel spinner task if it hasn't shown yet
                spinnerCts?.Cancel();
                
                _currentBoard = board;
                
                // Get solution
                _solution = _solver.GetSolution(_currentBoard);

                // Update UI
                UpdateGrid();
                ClearSelection();
                
                // Reset and start timer
                _viewModel.ResetTimer();
                _viewModel.StartTimer();
                
                // Reset solved state and statistics in ViewModel
                _viewModel.IsPuzzleSolved = false;
                _viewModel.MistakesCount = 0;
                _viewModel.HintsUsedCount = 0;
                _viewModel.HasUserMadeEntries = false;
                
                // Update difficulty
                _viewModel.CurrentDifficulty = difficulty.ToString();
                
                // Save last played difficulty to settings
                var settings = _settingsService.LoadSettings();
                settings.LastPlayedDifficulty = difficulty;
                await _settingsService.SaveSettingsAsync(settings);
                
                // Clear any saved game state since we're starting fresh
                await _gameStateService.ClearGameStateAsync();
            }
            finally
            {
                // Cancel and dispose spinner task
                spinnerCts?.Cancel();
                spinnerCts?.Dispose();
                
                // Always hide loading overlay
                LoadingOverlay.IsVisible = false;
            }
        }
        
        /// <summary>
        /// Restores a game from saved state.
        /// </summary>
        private void RestoreGame(AppModels.GameState gameState)
        {
            try
            {
                // Deserialize board
                if (!string.IsNullOrEmpty(gameState.BoardData))
                {
                    _currentBoard = SudokuBoard.Deserialize(gameState.BoardData);
                }
                else
                {
                    throw new InvalidOperationException("Board data is missing");
                }
                
                // Deserialize solution if available
                if (!string.IsNullOrEmpty(gameState.SolutionData))
                {
                    _solution = SudokuBoard.Deserialize(gameState.SolutionData);
                }
                
                // Restore timer and state
                _viewModel.ElapsedSeconds = gameState.ElapsedSeconds;
                _viewModel.CurrentDifficulty = gameState.Difficulty ?? "Medium";
                _viewModel.IsPuzzleSolved = gameState.IsSolved;
                _viewModel.HasUserMadeEntries = BoardHasUserEntries(_currentBoard);
                
                // Update UI
                UpdateGrid();
                ClearSelection();
                
                // Start timer if puzzle not solved
                if (!_viewModel.IsPuzzleSolved)
                {
                    _viewModel.StartTimer();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SudokuPage: Failed to restore game state: {ex.Message}");
                // Fall back to showing difficulty selection (cannot be dismissed since there's no valid game)
                _ = ShowDifficultySelectionAsync(canDismiss: false);
            }
        }
        
        /// <summary>
        /// Saves the current game state.
        /// </summary>
        public async Task SaveCurrentGameStateAsync()
        {
            // Don't save if puzzle is solved or board is empty
            if (_viewModel.IsPuzzleSolved || _currentBoard.GetAllCells().All(c => c.Value == 0))
            {
                return;
            }
            
            try
            {
                var gameState = new AppModels.GameState
                {
                    BoardData = _currentBoard.Serialize(),
                    SolutionData = _solution?.Serialize(),
                    ElapsedSeconds = _viewModel.ElapsedSeconds,
                    Difficulty = _viewModel.CurrentDifficulty,
                    IsSolved = _viewModel.IsPuzzleSolved
                };
                
                await _gameStateService.SaveGameStateAsync(gameState);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SudokuPage: Failed to save game state: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Maps MAUI DifficultyLevel enum to Core DifficultyLevel enum.
        /// </summary>
        private Core.Services.DifficultyLevel MapDifficulty(CoreDifficulty mauiDifficulty)
        {
            return mauiDifficulty switch
            {
                CoreDifficulty.Easy => Core.Services.DifficultyLevel.Easy,
                CoreDifficulty.Medium => Core.Services.DifficultyLevel.Medium,
                CoreDifficulty.Hard => Core.Services.DifficultyLevel.Hard,
                CoreDifficulty.Expert => Core.Services.DifficultyLevel.Expert,
                CoreDifficulty.Evil => Core.Services.DifficultyLevel.Evil,
                _ => Core.Services.DifficultyLevel.Easy
            };
        }

        
        
        
        
        
        
        private async void OnNewGameClicked(object? sender, EventArgs e)
        {
            if (_viewModel.IsProcessingInput)
                return;
            
            _viewModel.IsProcessingInput = true;
            
            try
            {
                // Only prompt if puzzle is not solved AND user has made any entries
                if (!_viewModel.IsPuzzleSolved && _viewModel.HasUserMadeEntries)
                {
                    bool answer = await DisplayAlertAsync("Abandon Puzzle?", "All progress will be lost. Start a new game?", "Yes", "No");
                    if (!answer)
                    {
                        return;
                    }
                }
                
                // Show difficulty selection modal
                await ShowDifficultySelectionAsync();
            }
            finally
            {
                _viewModel.IsProcessingInput = false;
            }
        }
        
        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            if (_viewModel.IsProcessingInput)
                return;
            
            _viewModel.IsProcessingInput = true;
            
            try
            {
                _viewModel.StopTimer();
                
                // Save game state before navigating away
                await SaveCurrentGameStateAsync();
                
                await Shell.Current.GoToAsync(nameof(SettingsPage));
            }
            finally
            {
                _viewModel.IsProcessingInput = false;
            }
        }

        private async void OnHintClicked(object? sender, EventArgs e)
        {
            if (_viewModel.IsProcessingInput)
                return;
            
            _viewModel.IsProcessingInput = true;
            
            try
            {
                // Block hints if there are conflicts
                _validator.UpdateErrorFlags(_currentBoard);
                if (!_validator.IsValidState(_currentBoard))
                {
                    UpdateGrid();
                    await DisplayAlertAsync("Fix Conflicts First", "Resolve highlighted conflicts before requesting a hint.", "OK");
                    return;
                }

                _solution ??= _solver.GetSolution(_currentBoard);
                var hint = _solver.GetHint(_currentBoard);
                if (hint == null)
                {
                    await DisplayAlertAsync("No Hint Available", "No valid hints are available right now.", "OK");
                    return;
                }

                var (row, col, value) = hint.Value;
                _currentBoard.SetCell(row, col, value);
                
                // Increment hints used counter in ViewModel
                _viewModel.HintsUsedCount++;

                _selectedRow = row;
                _selectedCol = col;
                _selectedButton = _cellButtons[row, col];

                // Update error flags AFTER setting the cell
                _validator.UpdateErrorFlags(_currentBoard);
                UpdateGrid();

                if (_selectedButton != null)
                {
                    await _selectedButton.ScaleToAsync(1.08, 120, Easing.CubicOut);
                    await _selectedButton.ScaleToAsync(1.0, 120, Easing.CubicIn);
                }

                // Check if solved AFTER all animations and updates complete
                if (_validator.IsSolved(_currentBoard))
                {
                    await OnPuzzleSolvedAsync();
                }
            }
            finally
            {
                _viewModel.IsProcessingInput = false;
            }
        }

        private async void OnCheckClicked(object? sender, EventArgs e)
        {
            if (_viewModel.IsProcessingInput)
                return;
            
            _viewModel.IsProcessingInput = true;
            
            try
            {
                _validator.UpdateErrorFlags(_currentBoard);
                UpdateGrid();

                if (!_validator.IsValidState(_currentBoard))
                {
                    await DisplayAlertAsync("Conflicts Found", "There are conflicts highlighted in red. Please fix them.", "OK");
                    return;
                }

                if (_validator.IsSolved(_currentBoard))
                {
                    await OnPuzzleSolvedAsync();
                    return;
                }

                _solution ??= _solver.GetSolution(_currentBoard);
                if (_solution != null)
                {
                    int correct = _validator.CountCorrectCells(_currentBoard, _solution);
                    await DisplayAlertAsync("Progress Check", $"No conflicts found. {correct}/81 cells are correct so far.", "OK");
                }
                else
                {
                    await DisplayAlertAsync("Progress Check", "No conflicts found so far.", "OK");
                }
            }
            finally
            {
                _viewModel.IsProcessingInput = false;
            }
        }

        /// <summary>
        /// Updates all cell buttons to reflect the current board state.
        /// </summary>
        private void UpdateGrid()
        {
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = _currentBoard.GetCell(row, col);
                    var button = _cellButtons[row, col];

                    // Update text
                    button.Text = cell.Value == 0 ? "" : cell.Value.ToString();

                    // Update styling - differentiate given vs user-entered cells
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

            // Re-apply selection highlight if any
            if (_selectedRow >= 0 && _selectedCol >= 0)
            {
                HighlightSelection(_selectedRow, _selectedCol);
            }

            // Update number pad remaining counts
            UpdateNumberPadCounts();
        }

        /// <summary>
        /// Calculates and updates the remaining count for each number (1-9) on the number pad.
        /// </summary>
        private void UpdateNumberPadCounts()
        {
            // Count how many of each number (1-9) still need to be placed
            var remainingCounts = new int[10]; // Index 0 unused, 1-9 for numbers

            // Each number should appear exactly 9 times in a solved puzzle
            for (int i = 1; i <= 9; i++)
            {
                remainingCounts[i] = 9;
            }

            // Subtract the numbers already on the board
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = _currentBoard.GetCell(row, col);
                    if (cell.Value >= 1 && cell.Value <= 9)
                    {
                        remainingCounts[cell.Value]--;
                    }
                }
            }

            // Update each NumPadButton
            foreach (var child in NumberPad.Children)
            {
                if (child is Controls.NumPadButton numPadBtn)
                {
                    numPadBtn.RemainingCount = remainingCounts[numPadBtn.Number];
                    numPadBtn.IsEnabled = remainingCounts[numPadBtn.Number] > 0;
                }
            }
        }

        /// <summary>
        /// Handles cell selection, including given cells.
        /// </summary>
        private void OnCellClicked(object? sender, CellClickedEventArgs e)
        {
            var cell = _currentBoard.GetCell(e.Row, e.Col);
            
            _selectedRow = e.Row;
            _selectedCol = e.Col;
            _selectedButton = _cellButtons[e.Row, e.Col];

            HighlightSelection(e.Row, e.Col);
        }

        /// <summary>
        /// Highlights the selected cell and related row/column/subgrid.
        /// </summary>
        private void HighlightSelection(int row, int col)
        {
            // Use CellHighlightManager to calculate colors
            var colorMap = _highlightManager.CalculateColors(_currentBoard, row, col);
            
            // Apply colors to all cell buttons
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

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        private void ClearSelection()
        {
            _selectedRow = -1;
            _selectedCol = -1;
            _selectedButton = null;
            UpdateGrid();
        }

        /// <summary>
        /// Handles number button clicks from the number pad.
        /// </summary>
        private async void OnNumPadButtonTapped(object? sender, EventArgs e)
        {
            if (sender is not Controls.NumPadButton numPadButton)
                return;

            await ApplyNumberInputAsync(numPadButton.Number);
        }

        private async Task ApplyNumberInputAsync(int number)
        {
            if (_viewModel.IsProcessingInput) return; _viewModel.IsProcessingInput = true;
            
            try
            {
                if (number < 1 || number > 9)
                    return;

                if (_selectedRow < 0 || _selectedCol < 0)
                    return;

                var cell = _currentBoard.GetCell(_selectedRow, _selectedCol);
                if (cell.IsGiven)
                    return;

                // Check if move conflicts with visible numbers
                if (!_validator.IsValidMove(_currentBoard, _selectedRow, _selectedCol, number))
                {
                    // Show conflict feedback - temporarily highlight cell as error
                    await ShowConflictFeedbackAsync();
                    return;
                }

                // Move is valid (no visible conflicts), place it
                _currentBoard.SetCell(_selectedRow, _selectedCol, number);
                
                // Mark that user has made an entry
                _viewModel.HasUserMadeEntries = true;

                // Update error flags first to check for any conflicts across the board
                _validator.UpdateErrorFlags(_currentBoard);

                // Then check if the placed number matches the solution (this overrides conflict check)
                if (_solution != null)
                {
                    var solutionCell = _solution.GetCell(_selectedRow, _selectedCol);
                    if (cell.Value != solutionCell.Value)
                    {
                        cell.HasError = true;
                        _viewModel.MistakesCount++;
                    }
                }

                // Update grid display
                UpdateGrid();

                // Check if solved
                if (_validator.IsSolved(_currentBoard))
                {
                    await OnPuzzleSolvedAsync();
                }
            }
            finally
            {
                _viewModel.IsProcessingInput = false;
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
        private void ClearSelectedCellAsync()
        {
            if (_selectedRow < 0 || _selectedCol < 0)
                return;

            var cell = _currentBoard.GetCell(_selectedRow, _selectedCol);
            if (cell.IsGiven || cell.Value == 0)
                return;

            cell.Value = 0;
            cell.HasError = false;
            _validator.UpdateErrorFlags(_currentBoard);
            UpdateGrid();
            
            // Mark that user has made a change (clearing is also a change)
            _viewModel.HasUserMadeEntries = true;
        }
        
        /// <summary>
        /// Checks if the board contains any user-entered values (non-given cells with values).
        /// </summary>
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
        
        /// <summary>
        /// Handles puzzle completion - stops timer, updates statistics, and shows summary popup.
        /// </summary>
        private async Task OnPuzzleSolvedAsync()
        {
            // Prevent duplicate calls if already showing summary
            if (_viewModel.IsPuzzleSolved)
                return;
            
            _viewModel.IsPuzzleSolved = true;
            _viewModel.StopTimer();
            await _gameStateService.ClearGameStateAsync();
            
            // Load statistics to check for best time
            var stats = _settingsService.LoadStatistics();
            var settings = _settingsService.LoadSettings();
            var currentDifficultyEnum = settings.LastPlayedDifficulty ?? CoreDifficulty.Medium;
            var previousBestTime = stats.GetBestTime(currentDifficultyEnum);
            
            // Update best time if this is a new record or first completion
            if (!previousBestTime.HasValue || _viewModel.ElapsedSeconds < previousBestTime.Value)
            {
                stats.SetBestTime(currentDifficultyEnum, _viewModel.ElapsedSeconds);
                await _settingsService.SaveStatisticsAsync(stats);
            }
            
            // Show summary popup - IsBusy will be cleared by the popup event handlers
            await SummaryPopup.ShowAsync(
                _viewModel.CurrentDifficulty,
                _viewModel.ElapsedSeconds,
                previousBestTime,
                _viewModel.MistakesCount,
                _viewModel.HintsUsedCount
            );
        }
        
        private void OnSummaryDoneRequested(object? sender, EventArgs e)
        {
            // Popup already hidden by the control
            _viewModel.IsProcessingInput = false;
        }
        
        private async void OnSummaryPlayAgainRequested(object? sender, EventArgs e)
        {
            // Show difficulty selection for new game
            _viewModel.IsProcessingInput = false;
            await ShowDifficultySelectionAsync();
        }

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
                    ClearSelectedCellAsync();
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

        private void OnWindowActivated(object? sender, EventArgs e)
        {
            // Resume timer if puzzle not solved and timer exists
            if (!_viewModel.IsPuzzleSolved)
            {
                _viewModel.StartTimer();
            }
        }
        
        private void OnWindowDeactivated(object? sender, EventArgs e)
        {
            // Pause timer when window loses focus
            _viewModel.StopTimer();
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            AttachWindowKeyHandler();
            AttachWindowFocusHandler();
            
            // On first appearance, check for saved game or show difficulty selection
            if (_isFirstAppearing)
            {
                var savedGame = _gameStateService.LoadGameState();
                if (savedGame != null)
                {
                    RestoreGame(savedGame);
                }
                else
                {
                    // No saved game - show difficulty selection modal (cannot be dismissed)
                    _ = ShowDifficultySelectionAsync(canDismiss: false);
                }
                _isFirstAppearing = false;
            }
            else
            {
                // Apply settings to refresh button visibility
                ApplySettings();
                // Refresh cell colors to pick up current theme
                UpdateGrid();
                // Restart timer if coming back from settings
                if (!_viewModel.IsPuzzleSolved)
                {
                    _viewModel.StartTimer();
                }
            }
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DetachWindowKeyHandler();
            DetachWindowFocusHandler();
            // Stop timer when leaving the page
            _viewModel.StopTimer();
            
            // Save game state when navigating away
            _ = SaveCurrentGameStateAsync();
        }
        
        /// <summary>
        /// Shows the difficulty selection popup.
        /// </summary>
        /// <param name="canDismiss">Whether the user can dismiss without selecting (false for first launch).</param>
        private async Task ShowDifficultySelectionAsync(bool canDismiss = true)
        {
            // Pause timer while modal is open
            _viewModel.StopTimer();
            
            var settings = _settingsService.LoadSettings();
            var statistics = _settingsService.LoadStatistics();
            
            // Determine last played difficulty from settings or saved game state
            CoreDifficulty? lastPlayed = settings.LastPlayedDifficulty;
            
            await DifficultyPopup.ShowAsync(lastPlayed, statistics, canDismiss);
        }
        
        /// <summary>
        /// Handles difficulty selection from the popup.
        /// </summary>
        private async void OnDifficultySelected(object? sender, CoreDifficulty difficulty)
        {
            await StartNewGameAsync(difficulty);
        }
        
        /// <summary>
        /// Handles difficulty popup dismissal (user tapped outside or clicked X).
        /// </summary>
        private void OnDifficultyPopupDismissed(object? sender, EventArgs e)
        {
            // User dismissed the modal without selecting - resume timer if puzzle not solved
            if (!_viewModel.IsPuzzleSolved)
            {
                _viewModel.StartTimer();
            }
        }
    }
}







