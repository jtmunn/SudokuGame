namespace Sudoku.Maui.Helpers
{
    /// <summary>
    /// Manages layout calculations for the Sudoku page.
    /// Button sizes scale from window width (not grid), breaking the circular dependency.
    /// Grid sizing is handled entirely by SquareLayoutControl in the MAUI layout pass.
    /// Cell font sizes are derived from the actual rendered grid size via SizeChanged.
    /// </summary>
    public class SudokuLayoutCalculator
    {
        // Minimum square size for the SquareLayoutControl
        public const double MinGridSize = 360;

        /// <summary>
        /// Calculates button and font sizes based on window dimensions.
        /// No grid sizing — MAUI's star row + SquareLayoutControl handle that.
        /// Both dimensions are considered so buttons don't steal vertical space from the grid.
        /// </summary>
        public static LayoutMetrics Calculate(double windowWidth, double windowHeight)
        {
            var result = new LayoutMetrics();

            // Button size: 5 buttons per row with 12px margin each + padding.
            // Width: (width - margins) / 8 gives a natural size that fits 5 across comfortably.
            // Height: / 14 prevents buttons from eating grid space in landscape.
            // Min of both handles portrait (width-limited) and landscape (height-limited).
            double fromWidth = (windowWidth - 60) / 7.5;
            double fromHeight = (windowHeight - 80) / 14.0;
            result.ButtonSize = Math.Clamp(Math.Min(fromWidth, fromHeight), 44, 100);

            // Font sizes proportional to button size
            result.FontSize = Math.Round(result.ButtonSize * 0.4);
            result.CountFontSize = Math.Max(8, Math.Round(result.ButtonSize * 0.18));
            result.CountMargin = new Thickness(
                0,
                Math.Round(result.ButtonSize * 0.15),
                Math.Round(result.ButtonSize * 0.15),
                0);

            return result;
        }

        /// <summary>
        /// Calculates the cell font size from the actual rendered grid size.
        /// Call this from GridBorder.SizeChanged so it uses the real measurement, not a prediction.
        /// </summary>
        public static double CalculateCellFontSize(double gridSize)
        {
            double cellSize = gridSize / 9.0;
            return Math.Max(10, cellSize * 0.55);
        }
    }

    /// <summary>
    /// Results of layout calculations.
    /// </summary>
    public class LayoutMetrics
    {
        public double ButtonSize { get; set; }
        public double FontSize { get; set; }
        public double CountFontSize { get; set; }
        public Thickness CountMargin { get; set; }
    }
}
