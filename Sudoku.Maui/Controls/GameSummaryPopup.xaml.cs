namespace Sudoku.Maui.Controls;

public partial class GameSummaryPopup : ContentView
{
    public event EventHandler? DoneRequested;
    public event EventHandler? PlayAgainRequested;
    
    public GameSummaryPopup()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Shows the summary popup with game statistics.
    /// </summary>
    public async Task ShowAsync(string difficulty, int timeInSeconds, int? bestTimeInSeconds, int mistakes, int hintsUsed)
    {
        // Update labels
        DifficultyLabel.Text = difficulty;
        TimeLabel.Text = FormatTime(timeInSeconds);
        MistakesLabel.Text = mistakes.ToString();
        HintsLabel.Text = hintsUsed.ToString();
        
        // Check if this is a new record
        bool isNewRecord = false;
        if (bestTimeInSeconds.HasValue)
        {
            BestTimeLabel.Text = FormatTime(bestTimeInSeconds.Value);
            
            // Show "New Record" if current time is better than previous best
            if (timeInSeconds < bestTimeInSeconds.Value)
            {
                isNewRecord = true;
            }
        }
        else
        {
            BestTimeLabel.Text = FormatTime(timeInSeconds);
            isNewRecord = true;
        }
        
        NewRecordLabel.IsVisible = isNewRecord;
        
        // Animate popup appearance
        IsVisible = true;
        PopupCard.Opacity = 0;
        PopupCard.Scale = 0.8;
        
        await Task.WhenAll(
            PopupCard.FadeToAsync(1, 300, Easing.CubicOut),
            PopupCard.ScaleToAsync(1, 300, Easing.CubicOut)
        );
    }
    
    /// <summary>
    /// Hides the summary popup with animation.
    /// </summary>
    public async Task HideAsync()
    {
        await Task.WhenAll(
            PopupCard.FadeToAsync(0, 200, Easing.CubicIn),
            PopupCard.ScaleToAsync(0.9, 200, Easing.CubicIn)
        );
        
        IsVisible = false;
    }
    
    private static string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
    
    private async void OnDoneClicked(object? sender, EventArgs e)
    {
        await HideAsync();
        DoneRequested?.Invoke(this, EventArgs.Empty);
    }
    
    private async void OnPlayAgainClicked(object? sender, EventArgs e)
    {
        await HideAsync();
        PlayAgainRequested?.Invoke(this, EventArgs.Empty);
    }
    
    private async void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        // Allow dismissing by tapping overlay (same as Done)
        await HideAsync();
        DoneRequested?.Invoke(this, EventArgs.Empty);
    }
}
