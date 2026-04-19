using Microsoft.Maui.Layouts;

namespace Sudoku.Maui.Controls
{
    /// <summary>
    /// A Layout control that maintains a perfect 1:1 (square) aspect ratio.
    /// Sizes itself to the smaller of the available width/height and centers within parent bounds.
    /// </summary>
    public class SquareLayoutControl : Layout
    {
        public static readonly BindableProperty MinimumSquareSizeProperty =
            BindableProperty.Create(
                nameof(MinimumSquareSize),
                typeof(double),
                typeof(SquareLayoutControl),
                100.0,
                propertyChanged: (bindable, _, _) =>
                {
                    if (bindable is SquareLayoutControl control)
                    {
                        control.InvalidateMeasure();
                    }
                });

        public SquareLayoutControl()
        {
            Padding = 0;
            Margin = 0;
        }

        protected override ILayoutManager CreateLayoutManager()
        {
            return new SquareLayoutManager(this);
        }

        public double MinimumSquareSize
        {
            get => (double)GetValue(MinimumSquareSizeProperty);
            set => SetValue(MinimumSquareSizeProperty, value);
        }
    }

    internal class SquareLayoutManager : ILayoutManager
    {
        private readonly SquareLayoutControl _control;

        public SquareLayoutManager(SquareLayoutControl control)
        {
            _control = control;
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            double squareSize = Math.Min(widthConstraint, heightConstraint);

            if (double.IsInfinity(squareSize))
            {
                squareSize = 200;
            }

            double minimumSize = _control.MinimumSquareSize;
            if (squareSize < minimumSize)
            {
                squareSize = minimumSize;
            }

            foreach (IView child in _control.Children)
            {
                child.Measure(squareSize, squareSize);
            }

            return new Size(squareSize, squareSize);
        }

        public Size ArrangeChildren(Rect bounds)
        {
            double minimumSize = _control.MinimumSquareSize;
            double squareSize = Math.Max(minimumSize, Math.Min(bounds.Width, bounds.Height));

            double offsetX = (bounds.Width - squareSize) / 2;
            double offsetY = (bounds.Height - squareSize) / 2;

            var squareBounds = new Rect(bounds.X + offsetX, bounds.Y + offsetY, squareSize, squareSize);

            foreach (IView child in _control.Children)
            {
                child.Arrange(squareBounds);
            }

            return new Size(squareSize, squareSize);
        }
    }
}
