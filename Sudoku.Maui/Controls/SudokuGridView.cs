using Sudoku.Core.Models;

namespace Sudoku.Maui.Controls
{
    /// <summary>
    /// Simple Sudoku grid using nested Grids and Borders - no complicated drawing.
    /// </summary>
    public class SudokuGridView : Border
    {
        private readonly Button[,] _cellButtons;
        private readonly Border _outerBorder;
        private readonly Grid _mainGrid;
        private readonly List<Border> _cellBorders = new();
        private readonly List<Border> _blockBorders = new();
        
        // Events
        public event EventHandler<CellClickedEventArgs>? CellClicked;

        public SudokuGridView()
        {
            _cellButtons = new Button[SudokuBoard.Size, SudokuBoard.Size];
            _outerBorder = this;
            
            // Outer thick border - will be set when Loaded event fires
            this.StrokeThickness = 4;
            this.Padding = 0;
            this.Margin = 0;
            
            // Main 3x3 grid (for the 3x3 sub-grids)
            _mainGrid = new Grid
            {
                RowSpacing = 0,
                ColumnSpacing = 0,
                Padding = 0,
                Margin = 0
            };
            
            // Create 3x3 layout
            for (int i = 0; i < 3; i++)
            {
                _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                _mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            
            // Create 9 sub-grids (3x3 each)
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    var subGrid = new Grid
                    {
                        RowSpacing = 0,
                        ColumnSpacing = 0,
                        Padding = 0,
                        Margin = 0
                    };
                    
                    // Create 3x3 cells in this sub-grid
                    for (int i = 0; i < 3; i++)
                    {
                        subGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        subGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    }
                    
                    // Add 9 cells to this sub-grid
                    for (int cellRow = 0; cellRow < 3; cellRow++)
                    {
                        for (int cellCol = 0; cellCol < 3; cellCol++)
                        {
                            var actualRow = blockRow * 3 + cellRow;
                            var actualCol = blockCol * 3 + cellCol;
                            
                            // Use Button as container for consistent API
                            var button = new Button
                            {
                                Text = "",
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 32, // Set explicit initial font size
                                CornerRadius = 0,
                                Padding = new Thickness(2), // Small padding
                                BorderWidth = 0,
                                Margin = 0,
                                Style = null, // Prevent global Button style from applying
                                HorizontalOptions = LayoutOptions.Fill,
                                VerticalOptions = LayoutOptions.Fill,
                                MinimumHeightRequest = -1,
                                MinimumWidthRequest = -1
                            };
                            
                            var cellBorder = new Border
                            {
                                StrokeThickness = 1,
                                Padding = 0,
                                Margin = 0,
                                Content = button
                            };
                            _cellBorders.Add(cellBorder);
                            
                            int r = actualRow, c = actualCol;
                            button.Clicked += (s, e) => CellClicked?.Invoke(this, new CellClickedEventArgs(r, c));
                            
                            Microsoft.Maui.Controls.Grid.SetRow(cellBorder, cellRow);
                            Microsoft.Maui.Controls.Grid.SetColumn(cellBorder, cellCol);
                            subGrid.Children.Add(cellBorder);
                            
                            _cellButtons[actualRow, actualCol] = button;
                        }
                    }
                    
                    // Wrap subgrid in a thick border for 3x3 block
                    var blockBorder = new Border
                    {
                        StrokeThickness = 2,
                        Padding = 0,
                        Margin = 0,
                        Content = subGrid
                    };
                    _blockBorders.Add(blockBorder);
                    
                    Microsoft.Maui.Controls.Grid.SetRow(blockBorder, blockRow);
                    Microsoft.Maui.Controls.Grid.SetColumn(blockBorder, blockCol);
                    _mainGrid.Children.Add(blockBorder);
                }
            }
            
            this.Content = _mainGrid;
            
            // Apply theme colors when the control is loaded
            this.Loaded += OnLoaded;
        }
        
        private void OnLoaded(object? sender, EventArgs e)
        {
            // Now that the control is loaded and theme is available, set up theme colors
            ApplyThemeColors();
            
            // Manually get and apply cell colors from theme
            Color cellDefaultColor = Colors.White; // fallback
            Color cellTextColor = Colors.Black; // fallback
            
            if (Application.Current?.Resources != null)
            {
                // Search through merged dictionaries
                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    if (dict.ContainsKey("CellDefaultColor"))
                        cellDefaultColor = (Color)dict["CellDefaultColor"];
                    if (dict.ContainsKey("CellUserTextColor"))
                        cellTextColor = (Color)dict["CellUserTextColor"];
                }
                
                // Apply to all cell buttons
                for (int row = 0; row < SudokuBoard.Size; row++)
                {
                    for (int col = 0; col < SudokuBoard.Size; col++)
                    {
                        var button = _cellButtons[row, col];
                        button.BackgroundColor = cellDefaultColor;
                        button.TextColor = cellTextColor;
                    }
                }
            }
        }
        
        private void ApplyThemeColors()
        {
            try
            {
                if (Application.Current?.Resources != null)
                {
                    // Get theme colors from merged dictionaries
                    Color gridBorderColor = Colors.Gray; // fallback
                    Color gridThickBorderColor = Colors.DarkBlue; // fallback
                    
                    // Search through merged dictionaries
                    foreach (var dict in Application.Current.Resources.MergedDictionaries)
                    {
                        if (dict.ContainsKey("GridBorderColor"))
                            gridBorderColor = (Color)dict["GridBorderColor"];
                        if (dict.ContainsKey("GridThickBorderColor"))
                            gridThickBorderColor = (Color)dict["GridThickBorderColor"];
                    }
                    
                    // Apply to all borders
                    UpdateGridLines(gridBorderColor, gridThickBorderColor);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme colors: {ex.Message}");
            }
        }

        public void UpdateGridLines(Color thinBorderColor, Color thickBorderColor)
        {
            // Update all cell borders
            foreach (var cellBorder in _cellBorders)
            {
                cellBorder.Stroke = thinBorderColor;
                cellBorder.BackgroundColor = thinBorderColor;
            }
            
            // Update all block borders
            foreach (var blockBorder in _blockBorders)
            {
                blockBorder.Stroke = thickBorderColor;
            }
            
            // Update outer border and main grid
            this.Stroke = thickBorderColor;
            this.BackgroundColor = thickBorderColor;
            _mainGrid.BackgroundColor = thickBorderColor;
        }

        public Button GetCellButton(int row, int col)
        {
            return _cellButtons[row, col];
        }

        public Button[,] GetAllCellButtons()
        {
            return _cellButtons;
        }
    }

    public class CellClickedEventArgs : EventArgs
    {
        public int Row { get; }
        public int Col { get; }

        public CellClickedEventArgs(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}
