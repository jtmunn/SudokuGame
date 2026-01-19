using Sudoku.Core.Models;

namespace Sudoku.Maui.Helpers
{
    /// <summary>
    /// Manages cell highlighting logic for the Sudoku grid, calculating which cells
    /// should be highlighted and with what colors based on selection and game state.
    /// </summary>
    public class CellHighlightManager
    {
        private readonly Func<string, Color, Color> _getThemeColor;

        // Cached theme colors
        private Color DefaultCellColor => _getThemeColor("CellDefaultColor", Colors.White);
        private Color GivenCellColor => _getThemeColor("CellGivenColor", Colors.White);
        private Color SelectedCellColor => _getThemeColor("CellSelectedColor", Colors.LightBlue);
        private Color ErrorCellColor => _getThemeColor("CellErrorColor", Colors.Red);
        private Color LightHighlightCellColor => _getThemeColor("CellLightHighlightColor", Colors.LightGray);
        private Color MatchingNumberColor => _getThemeColor("CellMatchingNumberColor", Colors.Gray);
        private Color CellTextColor => _getThemeColor("CellUserTextColor", Colors.Black);
        private Color GivenTextColor => _getThemeColor("CellGivenTextColor", Colors.Black);

        public CellHighlightManager(Func<string, Color, Color> getThemeColor)
        {
            _getThemeColor = getThemeColor;
        }

        /// <summary>
        /// Calculates colors for all cells based on current selection and board state.
        /// </summary>
        public CellColorMap CalculateColors(SudokuBoard board, int selectedRow, int selectedCol)
        {
            var colorMap = new CellColorMap();
            
            if (selectedRow < 0 || selectedCol < 0)
            {
                // No selection - apply default colors
                ApplyDefaultColors(board, colorMap);
                return colorMap;
            }

            var selectedCell = board.GetCell(selectedRow, selectedCol);
            var selectedValue = selectedCell.Value;

            // Calculate which 3x3 block the selected cell is in
            int blockRow = selectedRow / 3;
            int blockCol = selectedCol / 3;

            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    bool inSameBlock = (row / 3 == blockRow) && (col / 3 == blockCol);

                    // Determine background color based on priority (ERROR takes precedence)
                    Color backgroundColor;
                    Color textColor;

                    if (cell.HasError)
                    {
                        // Error cells ALWAYS show error color (highest priority)
                        backgroundColor = ErrorCellColor;
                        textColor = Colors.White;
                    }
                    else if (row == selectedRow && col == selectedCol)
                    {
                        // Selected cell - prominent highlight
                        backgroundColor = SelectedCellColor;
                        textColor = cell.IsGiven ? GivenTextColor : CellTextColor;
                    }
                    else if (selectedValue > 0 && cell.Value == selectedValue)
                    {
                        // Matching numbers
                        backgroundColor = MatchingNumberColor;
                        textColor = cell.IsGiven ? GivenTextColor : CellTextColor;
                    }
                    else if (row == selectedRow || col == selectedCol || inSameBlock)
                    {
                        // Same row, column, or 3x3 block - light highlight
                        backgroundColor = LightHighlightCellColor;
                        textColor = cell.IsGiven ? GivenTextColor : CellTextColor;
                    }
                    else if (cell.IsGiven)
                    {
                        // Given cells - use given color
                        backgroundColor = GivenCellColor;
                        textColor = GivenTextColor;
                    }
                    else
                    {
                        // User-entered cells - use default color
                        backgroundColor = DefaultCellColor;
                        textColor = CellTextColor;
                    }

                    colorMap.SetColors(row, col, backgroundColor, textColor);
                }
            }

            return colorMap;
        }

        private void ApplyDefaultColors(SudokuBoard board, CellColorMap colorMap)
        {
            for (int row = 0; row < SudokuBoard.Size; row++)
            {
                for (int col = 0; col < SudokuBoard.Size; col++)
                {
                    var cell = board.GetCell(row, col);
                    Color backgroundColor;
                    Color textColor;

                    if (cell.HasError)
                    {
                        backgroundColor = ErrorCellColor;
                        textColor = Colors.White;
                    }
                    else if (cell.IsGiven)
                    {
                        backgroundColor = GivenCellColor;
                        textColor = GivenTextColor;
                    }
                    else
                    {
                        backgroundColor = DefaultCellColor;
                        textColor = CellTextColor;
                    }

                    colorMap.SetColors(row, col, backgroundColor, textColor);
                }
            }
        }
    }

    /// <summary>
    /// Holds background and text colors for all cells in the grid.
    /// </summary>
    public class CellColorMap
    {
        private readonly (Color background, Color text)[,] _colors = new (Color, Color)[SudokuBoard.Size, SudokuBoard.Size];

        public void SetColors(int row, int col, Color backgroundColor, Color textColor)
        {
            _colors[row, col] = (backgroundColor, textColor);
        }

        public (Color background, Color text) GetColors(int row, int col)
        {
            return _colors[row, col];
        }
    }
}
