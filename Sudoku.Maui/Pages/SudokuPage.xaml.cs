using Sudoku.Core.Models;
using Sudoku.Core.Services;
using Sudoku.Maui.Services;
using Sudoku.Maui.Controls;
using Models = Sudoku.Maui.Models;

namespace Sudoku.Maui.Pages
{
    public partial class SudokuPage : ContentPage
    {
        private readonly SudokuGenerator _generator;
        private readonly SudokuValidator _validator;
        private readonly SudokuSolver _solver;
        private readonly ISettingsService _settingsService;
        private readonly IGameStateService _gameStateService;

        private SudokuBoard _currentBoard;
        private SudokuBoard? _solution;
        private Button[,] _cellButtons;
        private Button? _selectedButton;
        private int _selectedRow = -1;
        private int _selectedCol = -1;
        
        private const int MinGridSize = 360;
        private const double BaseGridSize = 450.0; // Reference size for scaling calculations
        private const double BaseButtonSize = 45.0; // Base size for number pad and action buttons
        private const double BaseFontSize = 20.0; // Base font size for buttons
        private const int GameAreaPadding = 10; // Padding around game area
        private const int ActionButtonMargin = 20; // Left margin for action buttons
        private const int HeaderHeight = 56; // Header bar height
        private const int NumberPadHeight = 120; // Approximate height for number pad area
        private const int NumberButtonMargin = 6; // Margin around each number button
        
        private double _currentGridSize = BaseGridSize;
        
        // Timer
        private System.Timers.Timer? _gameTimer;
        private int _elapsedSeconds = 0;
        private string _currentDifficulty = "Easy";
        private bool _isPuzzleSolved = false;

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
                if (Application.Current?.Resources != null)
                {
                    // Search through merged dictionaries like SudokuGridView does
                    foreach (var dict in Application.Current.Resources.MergedDictionaries)
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
                         IGameStateService gameStateService)
        {
            InitializeComponent();
            
            _generator = generator;
            _validator = validator;
            _solver = solver;
            _settingsService = settingsService;
            _gameStateService = gameStateService;
            
            _currentBoard = new SudokuBoard();

            // Get cell buttons from the custom control
            _cellButtons = SudokuGridView.GetAllCellButtons();
            
            // Subscribe to cell click events
            SudokuGridView.CellClicked += OnCellClicked;

            // Grid lines are handled by DynamicResource in SudokuGridView
            
            SizeChanged += OnPageSizeChanged;
            UpdateGridSize();
            
            // Don't start game here - wait for OnAppearing when theme is loaded
            ApplySettings();
        }

        private void ApplySettings()
        {
            var settings = _settingsService.LoadSettings();
            // Only hide buttons if settings explicitly says so, otherwise show them
            HintButton.IsVisible = settings.ShowHintButton;
            CheckButton.IsVisible = settings.ShowCheckButton;
            
            // Update button sizes to ensure they're positioned correctly
            UpdateButtonSizes();
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            UpdateGridSize();
        }

        private void UpdateGridSize()
        {
            // Grid calculates size using FULL available width - action buttons positioned separately
            // This keeps the grid centered in the window, with action buttons positioned to its right
            var settings = _settingsService.LoadSettings();
            
            // Calculate available space for grid (DO NOT reserve space for action buttons here)
            double totalPadding = GameAreaPadding * 2;
            var availableWidth = Width - totalPadding;
            
            // Calculate scaled number pad height
            double preliminarySize = Math.Max(MinGridSize, Math.Min(availableWidth, Width));
            double scale = preliminarySize / BaseGridSize;
            double scaledButtonSize = Math.Round(BaseButtonSize * scale);
            double scaledNumberPadHeight = scaledButtonSize + (NumberButtonMargin * 4) + 30;
            
            // Calculate available height
            var availableHeight = Height - HeaderHeight - scaledNumberPadHeight - (GameAreaPadding * 2);
            
            // Grid size is the smaller dimension, ALWAYS clamped to minimum
            // This ensures at minimum window size, grid stays at MinGridSize
            var size = Math.Min(availableWidth, availableHeight);
            size = Math.Max(MinGridSize, size);
            
            _currentGridSize = size;
            GridBorder.WidthRequest = size;
            GridBorder.HeightRequest = size;
            
            // Update font sizes for all cell buttons
            UpdateCellFontSizes();
            
            // Update button sizes based on FINAL grid size
            UpdateButtonSizes();
            
            // Check if there's enough width to show action buttons
            double finalScale = _currentGridSize / BaseGridSize;
            double actionButtonWidth = Math.Round(BaseButtonSize * finalScale);
            double centerX = Width / 2;
            double buttonX = centerX + (_currentGridSize / 2) + ActionButtonMargin;
            double requiredWidth = buttonX + actionButtonWidth;
            
            // Only show and position action buttons if there's enough space
            bool hasSpaceForButtons = requiredWidth <= Width && (settings.ShowHintButton || settings.ShowCheckButton);
            
            if (hasSpaceForButtons)
            {
                // Position action buttons using AbsoluteLayout to the right of centered grid
                double buttonY = (Height - HeaderHeight - scaledNumberPadHeight) / 2; // Center vertically in game area
                
                AbsoluteLayout.SetLayoutBounds(ActionButtonStack, new Rect(buttonX, buttonY, actionButtonWidth, AbsoluteLayout.AutoSize));
                ActionButtonStack.IsVisible = true;
            }
            else
            {
                // Hide action buttons when window is too narrow
                ActionButtonStack.IsVisible = false;
            }
        }
        
        private void UpdateButtonSizes()
        {
            // Calculate scale factor based on current grid size vs base size
            double scale = _currentGridSize / BaseGridSize;
            
            // Number buttons - scale from base size
            double numberSize = Math.Round(BaseButtonSize * scale);
            double numberFont = Math.Round(BaseFontSize * scale);
            double countFont = Math.Round((BaseFontSize * scale * 0.5) - 1); // Count is half the main font size, minus 1
            
            // Calculate scaled margin for count position (starts at 15% of button size from edges)
            double countMargin = Math.Round(numberSize * 0.15);
            
            foreach (var child in NumberPad.Children)
            {
                if (child is Button btn)
                {
                    btn.WidthRequest = numberSize;
                    btn.HeightRequest = numberSize;
                    btn.CornerRadius = (int)(numberSize / 2); // Keep circular
                    btn.FontSize = numberFont;
                }
                else if (child is Controls.NumPadButton numPadBtn)
                {
                    numPadBtn.WidthRequest = numberSize;
                    numPadBtn.HeightRequest = numberSize;
                    numPadBtn.MainFontSize = numberFont;
                    numPadBtn.CountFontSize = countFont;
                    numPadBtn.CountMargin = new Thickness(0, countMargin, countMargin, 0);
                }
            }
            
            // Action buttons - same scaling as number buttons
            double actionSize = Math.Round(BaseButtonSize * scale);
            double actionFont = Math.Round(BaseFontSize * scale);
            
            // Only update if buttons are visible (respects settings)
            if (HintButton.IsVisible)
            {
                HintButton.WidthRequest = actionSize;
                HintButton.HeightRequest = actionSize;
                HintButton.CornerRadius = (int)(actionSize / 2);
                HintButton.FontSize = actionFont;
            }
            
            if (CheckButton.IsVisible)
            {
                CheckButton.WidthRequest = actionSize;
                CheckButton.HeightRequest = actionSize;
                CheckButton.CornerRadius = (int)(actionSize / 2);
                CheckButton.FontSize = actionFont;
            }
        }

        private void UpdateCellFontSizes()
        {
            // Calculate scale factor based on grid size (same as buttons)
            double scale = _currentGridSize / BaseGridSize;
            
            // Use a reasonable base font size that scales with the grid
            double fontSize = 30.0 * scale;
            
            // Apply to all cell buttons
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
        private void StartNewGame()
        {
            // Get difficulty from settings
            var settings = _settingsService.LoadSettings();
            
            // Map MAUI DifficultyLevel to Core DifficultyLevel
            var coreDifficulty = MapDifficulty(settings.DefaultDifficulty);
            
            // Generate new puzzle based on difficulty
            _currentBoard = _generator.Generate(coreDifficulty);
            
            // Get solution
            _solution = _solver.GetSolution(_currentBoard);

            // Update UI
            UpdateGrid();
            ClearSelection();
            
            // Reset and start timer
            ResetTimer();
            StartTimer();
            
            // Reset solved state
            _isPuzzleSolved = false;
            
            // Update difficulty label
            _currentDifficulty = settings.DefaultDifficulty.ToString();
            UpdateDifficultyLabel();
            
            // Clear any saved game state since we're starting fresh
            _ = _gameStateService.ClearGameStateAsync();
        }
        
        /// <summary>
        /// Restores a game from saved state.
        /// </summary>
        private void RestoreGame(Models.GameState gameState)
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
                
                // Restore timer
                _elapsedSeconds = gameState.ElapsedSeconds;
                UpdateTimerDisplay();
                
                // Restore difficulty
                _currentDifficulty = gameState.Difficulty ?? "Medium";
                UpdateDifficultyLabel();
                
                // Restore solved state
                _isPuzzleSolved = gameState.IsSolved;
                
                // Update UI
                UpdateGrid();
                ClearSelection();
                
                // Start timer if puzzle not solved
                if (!_isPuzzleSolved)
                {
                    StartTimer();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SudokuPage: Failed to restore game state: {ex.Message}");
                // Fall back to starting a new game
                StartNewGame();
            }
        }
        
        /// <summary>
        /// Saves the current game state.
        /// </summary>
        public async Task SaveCurrentGameStateAsync()
        {
            // Don't save if puzzle is solved or board is empty
            if (_isPuzzleSolved || _currentBoard.GetAllCells().All(c => c.Value == 0))
            {
                return;
            }
            
            try
            {
                var gameState = new Models.GameState
                {
                    BoardData = _currentBoard.Serialize(),
                    SolutionData = _solution?.Serialize(),
                    ElapsedSeconds = _elapsedSeconds,
                    Difficulty = _currentDifficulty,
                    IsSolved = _isPuzzleSolved
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
        private Core.Services.DifficultyLevel MapDifficulty(Models.DifficultyLevel mauiDifficulty)
        {
            return mauiDifficulty switch
            {
                Models.DifficultyLevel.Easy => Core.Services.DifficultyLevel.Easy,
                Models.DifficultyLevel.Medium => Core.Services.DifficultyLevel.Medium,
                Models.DifficultyLevel.Hard => Core.Services.DifficultyLevel.Hard,
                Models.DifficultyLevel.Expert => Core.Services.DifficultyLevel.Expert,
                Models.DifficultyLevel.Master => Core.Services.DifficultyLevel.Master,
                Models.DifficultyLevel.GrandMaster => Core.Services.DifficultyLevel.GrandMaster,
                _ => Core.Services.DifficultyLevel.Easy
            };
        }

        private void StartTimer()
        {
            _gameTimer?.Stop();
            _gameTimer = new System.Timers.Timer(1000); // 1 second interval
            _gameTimer.Elapsed += OnTimerElapsed;
            _gameTimer.Start();
        }
        
        private void StopTimer()
        {
            _gameTimer?.Stop();
        }
        
        private void ResetTimer()
        {
            _elapsedSeconds = 0;
            UpdateTimerDisplay();
        }
        
        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _elapsedSeconds++;
            MainThread.BeginInvokeOnMainThread(() => UpdateTimerDisplay());
        }
        
        private void UpdateTimerDisplay()
        {
            int minutes = _elapsedSeconds / 60;
            int seconds = _elapsedSeconds % 60;
            TimerLabel.Text = $"Time: {minutes:D2}:{seconds:D2}";
        }
        
        private void UpdateDifficultyLabel()
        {
            DifficultyLabel.Text = $"Difficulty: {_currentDifficulty}";
        }
        
        private async void OnNewGameClicked(object? sender, EventArgs e)
        {
            // Skip prompt if puzzle is already solved
            if (!_isPuzzleSolved)
            {
                bool answer = await DisplayAlertAsync("Abandon Puzzle?", "All progress will be lost. Start a new game?", "Yes", "No");
                if (!answer)
                {
                    return;
                }
            }
            
            StartNewGame();
        }
        
        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            StopTimer();
            
            // Save game state before navigating away
            await SaveCurrentGameStateAsync();
            
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }

        private async void OnHintClicked(object? sender, EventArgs e)
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
                _isPuzzleSolved = true;
                StopTimer();
                await _gameStateService.ClearGameStateAsync();
                await DisplayAlertAsync("Puzzle Solved", "You solved the puzzle!", "OK");
            }
        }

        private async void OnCheckClicked(object? sender, EventArgs e)
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
                _isPuzzleSolved = true;
                StopTimer();
                await _gameStateService.ClearGameStateAsync();
                await DisplayAlertAsync("Puzzle Solved", "Great job! You solved the puzzle.", "OK");
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
            var selectedCell = _currentBoard.GetCell(row, col);
            var selectedValue = selectedCell.Value;
            
            // Calculate which 3x3 block the selected cell is in
            int blockRow = row / 3;
            int blockCol = col / 3;

            // Reset all cells and apply highlights
            for (int r = 0; r < SudokuBoard.Size; r++)
            {
                for (int c = 0; c < SudokuBoard.Size; c++)
                {
                    var cell = _currentBoard.GetCell(r, c);
                    var button = _cellButtons[r, c];
                    
                    // Check if cell is in same 3x3 block
                    bool inSameBlock = (r / 3 == blockRow) && (c / 3 == blockCol);
					
                    // Determine background color based on priority (ERROR takes precedence)
					if (cell.HasError)
					{
						// Error cells ALWAYS show error color (highest priority)
						button.BackgroundColor = ErrorCellColor;
					}
					else if (r == row && c == col)
					{
						// Selected cell - prominent highlight
						button.BackgroundColor = SelectedCellColor;
					}
					else if (selectedValue > 0 && cell.Value == selectedValue)
					{
						// Matching numbers
						button.BackgroundColor = MatchingNumberColor;
					}
					else if (r == row || c == col || inSameBlock)
					{
						// Same row, column, or 3x3 block - light highlight
						button.BackgroundColor = LightHighlightCellColor;
					}
					else if (cell.IsGiven)
					{
						// Given cells - use given color
						button.BackgroundColor = GivenCellColor;
					}
					else
					{
						// User-entered cells - use default color
						button.BackgroundColor = DefaultCellColor;
					}
					
					// Keep text color consistent based on cell type
					if (cell.HasError)
					{
						button.TextColor = Colors.White;
					}
					else if (cell.IsGiven)
					{
						button.TextColor = GivenTextColor;
					}
					else
					{
						button.TextColor = CellTextColor;
					}
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

            // Check if the placed number is actually correct against the solution
            if (_solution != null)
            {
                var solutionCell = _solution.GetCell(_selectedRow, _selectedCol);
                cell.HasError = (cell.Value != solutionCell.Value);
            }

            // Update grid display
            UpdateGrid();

            // Check if solved
            if (_validator.IsSolved(_currentBoard))
            {
                _isPuzzleSolved = true;
                StopTimer();
                await _gameStateService.ClearGameStateAsync();
                await DisplayAlertAsync("Congratulations!", "You solved the puzzle!", "OK");
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
        }

        private void AttachWindowKeyHandler()
        {
#if WINDOWS
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow &&
                nativeWindow.Content is Microsoft.UI.Xaml.UIElement rootElement)
            {
                rootElement.KeyUp -= OnNativeWindowKeyUp;
                rootElement.KeyUp += OnNativeWindowKeyUp;
            }
#endif
        }

        private void DetachWindowKeyHandler()
        {
#if WINDOWS
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow &&
                nativeWindow.Content is Microsoft.UI.Xaml.UIElement rootElement)
            {
                rootElement.KeyUp -= OnNativeWindowKeyUp;
            }
#endif
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

        private bool _isFirstAppearing = true;
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            AttachWindowKeyHandler();
            
            // On first appearance, check for saved game or start new
            if (_isFirstAppearing)
            {
                var savedGame = _gameStateService.LoadGameState();
                if (savedGame != null)
                {
                    RestoreGame(savedGame);
                }
                else
                {
                    StartNewGame();
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
                if (_gameTimer != null && !_gameTimer.Enabled && !_isPuzzleSolved)
                {
                    StartTimer();
                }
            }
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DetachWindowKeyHandler();
            // Stop timer when leaving the page
            StopTimer();
            
            // Save game state when navigating away
            _ = SaveCurrentGameStateAsync();
        }
    }
}
