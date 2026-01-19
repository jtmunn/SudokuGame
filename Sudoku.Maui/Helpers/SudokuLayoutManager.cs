namespace Sudoku.Maui.Helpers
{
    /// <summary>
    /// Manages layout calculations for the Sudoku page, including grid sizing,
    /// button positioning, and responsive scaling.
    /// </summary>
    public class SudokuLayoutManager
    {
        // Constants for layout calculations
        public const int MinGridSize = 360;
        public const double BaseGridSize = 450.0;
        public const double BaseButtonSize = 45.0;
        public const double BaseFontSize = 20.0;
        public const int GameAreaPadding = 10;
        public const int ActionButtonMargin = 20;
        public const int HeaderHeight = 56;
        public const int NumberButtonMargin = 6;
        
        /// <summary>
        /// Calculates the optimal grid size based on available space.
        /// </summary>
        public static LayoutCalculations Calculate(double windowWidth, double windowHeight, bool showActionButtons)
        {
            var result = new LayoutCalculations();
            
            // Calculate available space for grid (DO NOT reserve space for action buttons here)
            double totalPadding = GameAreaPadding * 2;
            var availableWidth = windowWidth - totalPadding;
            
            // Calculate scaled number pad height
            double preliminarySize = Math.Max(MinGridSize, Math.Min(availableWidth, windowWidth));
            double scale = preliminarySize / BaseGridSize;
            double scaledButtonSize = Math.Round(BaseButtonSize * scale);
            double scaledNumberPadHeight = scaledButtonSize + (NumberButtonMargin * 4) + 30;
            
            // Calculate available height
            var availableHeight = windowHeight - HeaderHeight - scaledNumberPadHeight - (GameAreaPadding * 2);
            
            // Grid size is the smaller dimension, ALWAYS clamped to minimum
            var size = Math.Min(availableWidth, availableHeight);
            size = Math.Max(MinGridSize, size);
            
            result.GridSize = size;
            result.Scale = size / BaseGridSize;
            result.ScaledNumberPadHeight = scaledNumberPadHeight;
            
            // Calculate button sizes
            result.ButtonSize = Math.Round(BaseButtonSize * result.Scale);
            result.FontSize = Math.Round(BaseFontSize * result.Scale);
            result.CountFontSize = Math.Round((BaseFontSize * result.Scale * 0.5) - 1);
            result.CountMargin = new Thickness(0, Math.Round(result.ButtonSize * 0.15), Math.Round(result.ButtonSize * 0.15), 0);
            
            // Calculate action button positioning
            if (showActionButtons)
            {
                double centerX = windowWidth / 2;
                double buttonX = centerX + (result.GridSize / 2) + ActionButtonMargin;
                double requiredWidth = buttonX + result.ButtonSize;
                
                result.HasSpaceForActionButtons = requiredWidth <= windowWidth;
                
                if (result.HasSpaceForActionButtons)
                {
                    double buttonY = (windowHeight - HeaderHeight - scaledNumberPadHeight) / 2;
                    result.ActionButtonBounds = new Rect(buttonX, buttonY, result.ButtonSize, -1); // -1 = AutoSize
                }
            }
            
            // Calculate cell font size
            result.CellFontSize = 30.0 * result.Scale;
            
            return result;
        }
    }
    
    /// <summary>
    /// Results of layout calculations.
    /// </summary>
    public class LayoutCalculations
    {
        public double GridSize { get; set; }
        public double Scale { get; set; }
        public double ScaledNumberPadHeight { get; set; }
        public double ButtonSize { get; set; }
        public double FontSize { get; set; }
        public double CountFontSize { get; set; }
        public Thickness CountMargin { get; set; }
        public double CellFontSize { get; set; }
        public bool HasSpaceForActionButtons { get; set; }
        public Rect ActionButtonBounds { get; set; }
    }
}
