using Microsoft.Maui.Graphics;

namespace Sudoku.Maui.Controls
{
    /// <summary>
    /// Draws the Sudoku grid lines (thin borders between cells, thick borders between 3x3 blocks).
    /// </summary>
    public class SudokuGridDrawable : IDrawable
    {
        private readonly Color _thinBorderColor;
        private readonly Color _thickBorderColor;
        
        public SudokuGridDrawable(Color thinBorderColor, Color thickBorderColor)
        {
            _thinBorderColor = thinBorderColor;
            _thickBorderColor = thickBorderColor;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var width = dirtyRect.Width;
            var height = dirtyRect.Height;
            var cellSize = width / 9f;

            // First draw a white background
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(0, 0, width, height);

            // Draw ALL thin borders for cells (1px, light gray)
            canvas.StrokeColor = _thinBorderColor;
            canvas.StrokeSize = 1;
            canvas.Antialias = false;

            // Vertical thin lines for all 9 columns
            for (int i = 1; i < 9; i++)
            {
                var x = i * cellSize;
                canvas.DrawLine(x, 0, x, height);
            }

            // Horizontal thin lines for all 9 rows
            for (int i = 1; i < 9; i++)
            {
                var y = i * cellSize;
                canvas.DrawLine(0, y, width, y);
            }

            // Draw thick borders as filled rectangles (6px, dark)
            canvas.FillColor = _thickBorderColor;

            var thickness = 6f;

            // Outer border - draw as 4 rectangles (top, right, bottom, left)
            canvas.FillRectangle(0, 0, width, thickness); // Top
            canvas.FillRectangle(0, height - thickness, width, thickness); // Bottom
            canvas.FillRectangle(0, 0, thickness, height); // Left
            canvas.FillRectangle(width - thickness, 0, thickness, height); // Right

            // Vertical thick lines (at columns 3 and 6)
            for (int i = 3; i < 9; i += 3)
            {
                var x = i * cellSize;
                canvas.FillRectangle(x - thickness / 2, 0, thickness, height);
            }

            // Horizontal thick lines (at rows 3 and 6)
            for (int i = 3; i < 9; i += 3)
            {
                var y = i * cellSize;
                canvas.FillRectangle(0, y - thickness / 2, width, thickness);
            }
        }
    }
}
