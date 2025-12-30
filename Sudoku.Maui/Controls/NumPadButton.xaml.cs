namespace Sudoku.Maui.Controls;

public partial class NumPadButton : ContentView
{
    public static readonly BindableProperty NumberProperty =
        BindableProperty.Create(nameof(Number), typeof(int), typeof(NumPadButton), 0);

    public static readonly BindableProperty RemainingCountProperty =
        BindableProperty.Create(nameof(RemainingCount), typeof(int), typeof(NumPadButton), 0,
            propertyChanged: OnRemainingCountChanged);

    public static readonly BindableProperty MainFontSizeProperty =
        BindableProperty.Create(nameof(MainFontSize), typeof(double), typeof(NumPadButton), 20.0);

    public static readonly BindableProperty CountFontSizeProperty =
        BindableProperty.Create(nameof(CountFontSize), typeof(double), typeof(NumPadButton), 10.0);

    public static readonly BindableProperty CountMarginProperty =
        BindableProperty.Create(nameof(CountMargin), typeof(Thickness), typeof(NumPadButton), new Thickness(0, 12, 12, 0));

    public int Number
    {
        get => (int)GetValue(NumberProperty);
        set => SetValue(NumberProperty, value);
    }

    public int RemainingCount
    {
        get => (int)GetValue(RemainingCountProperty);
        set => SetValue(RemainingCountProperty, value);
    }

    public double MainFontSize
    {
        get => (double)GetValue(MainFontSizeProperty);
        set => SetValue(MainFontSizeProperty, value);
    }

    public double CountFontSize
    {
        get => (double)GetValue(CountFontSizeProperty);
        set => SetValue(CountFontSizeProperty, value);
    }

    public Thickness CountMargin
    {
        get => (Thickness)GetValue(CountMarginProperty);
        set => SetValue(CountMarginProperty, value);
    }

    public event EventHandler<EventArgs>? Tapped;

    public NumPadButton()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        UpdateButtonState();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsEnabled))
        {
            UpdateButtonState();
        }
    }

    private void UpdateButtonState()
    {
        if (ButtonBorder == null || NumberLabel == null || CountLabel == null)
            return;

        Color backgroundColor;
        Color textColor;

        if (IsEnabled)
        {
            backgroundColor = GetThemeColor("PrimaryButtonColor");
            textColor = GetThemeColor("ButtonTextColor");
        }
        else
        {
            backgroundColor = GetThemeColor("DisabledButtonColor");
            textColor = GetThemeColor("DisabledButtonTextColor");
        }

        ButtonBorder.BackgroundColor = backgroundColor;
        ButtonBorder.Stroke = backgroundColor;
        NumberLabel.TextColor = textColor;
        CountLabel.TextColor = textColor;
    }

    private Color GetThemeColor(string key)
    {
        if (Application.Current?.Resources.MergedDictionaries != null)
        {
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict.ContainsKey(key))
                    return (Color)dict[key];
            }
        }

        return Colors.Gray;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        Tapped?.Invoke(this, EventArgs.Empty);
    }

    private static void OnRemainingCountChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is NumPadButton button)
        {
            int count = (int)newValue;
            button.CountLabel.IsEnabled = count > 0;
            button.IsEnabled = count > 0;
        }
    }
}
