using Sudoku.Maui.Models;

namespace Sudoku.Maui.Controls;

public partial class DifficultySelectionPopup : ContentView
{
    public event EventHandler<DifficultyLevel>? DifficultySelected;
    public event EventHandler? Dismissed;
    
    private bool _canDismiss = true; // Track if modal can be dismissed
    
    public DifficultySelectionPopup()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// Shows the difficulty selection popup with statistics.
    /// </summary>
    /// <param name="lastPlayedDifficulty">The last played difficulty to highlight, or null.</param>
    /// <param name="statistics">Game statistics containing best times.</param>
    /// <param name="canDismiss">Whether the popup can be dismissed without selecting (false for first launch).</param>
    public async Task ShowAsync(DifficultyLevel? lastPlayedDifficulty, GameStatistics statistics, bool canDismiss = true)
    {
        _canDismiss = canDismiss;
        
        // Show/hide close button based on canDismiss
        CloseButton.IsVisible = canDismiss;
        
        // Update best times for each difficulty
        UpdateBestTime(DifficultyLevel.Easy, statistics.BestTimeEasy, EasyBestTime);
        UpdateBestTime(DifficultyLevel.Medium, statistics.BestTimeMedium, MediumBestTime);
        UpdateBestTime(DifficultyLevel.Hard, statistics.BestTimeHard, HardBestTime);
        UpdateBestTime(DifficultyLevel.Expert, statistics.BestTimeExpert, ExpertBestTime);
        UpdateBestTime(DifficultyLevel.Evil, statistics.BestTimeEvil, EvilBestTime);
        
        // Hide all "Last Played" badges
        EasyLastPlayedBadge.IsVisible = false;
        MediumLastPlayedBadge.IsVisible = false;
        HardLastPlayedBadge.IsVisible = false;
        ExpertLastPlayedBadge.IsVisible = false;
        EvilLastPlayedBadge.IsVisible = false;
        
        // Show "Last Played" badge for the last played difficulty
        if (lastPlayedDifficulty.HasValue)
        {
            switch (lastPlayedDifficulty.Value)
            {
                case DifficultyLevel.Easy:
                    EasyLastPlayedBadge.IsVisible = true;
                    break;
                case DifficultyLevel.Medium:
                    MediumLastPlayedBadge.IsVisible = true;
                    break;
                case DifficultyLevel.Hard:
                    HardLastPlayedBadge.IsVisible = true;
                    break;
                case DifficultyLevel.Expert:
                    ExpertLastPlayedBadge.IsVisible = true;
                    break;
                case DifficultyLevel.Evil:
                    EvilLastPlayedBadge.IsVisible = true;
                    break;
            }
        }
        
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
    /// Hides the difficulty selection popup with animation.
    /// </summary>
    public async Task HideAsync()
    {
        await Task.WhenAll(
            PopupCard.FadeToAsync(0, 200, Easing.CubicIn),
            PopupCard.ScaleToAsync(0.9, 200, Easing.CubicIn)
        );
        
        IsVisible = false;
    }
    
    private void UpdateBestTime(DifficultyLevel difficulty, int? bestTimeInSeconds, Label label)
    {
        if (bestTimeInSeconds.HasValue)
        {
            label.Text = $"Best: {FormatTime(bestTimeInSeconds.Value)}";
            label.IsVisible = true;
        }
        else
        {
            label.IsVisible = false;
        }
    }
    
    private static string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
    
    private async void OnEasyTapped(object? sender, TappedEventArgs e)
    {
        await HideAsync();
        DifficultySelected?.Invoke(this, DifficultyLevel.Easy);
    }
    
    private async void OnMediumTapped(object? sender, TappedEventArgs e)
    {
        await HideAsync();
        DifficultySelected?.Invoke(this, DifficultyLevel.Medium);
    }
    
    private async void OnHardTapped(object? sender, TappedEventArgs e)
    {
        await HideAsync();
        DifficultySelected?.Invoke(this, DifficultyLevel.Hard);
    }
    
    private async void OnExpertTapped(object? sender, TappedEventArgs e)
    {
        await HideAsync();
        DifficultySelected?.Invoke(this, DifficultyLevel.Expert);
    }
    
    private async void OnEvilTapped(object? sender, TappedEventArgs e)
    {
        await HideAsync();
        DifficultySelected?.Invoke(this, DifficultyLevel.Evil);
    }
    
    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        if (!_canDismiss)
            return; // Don't allow dismissal if not permitted
            
        await HideAsync();
        Dismissed?.Invoke(this, EventArgs.Empty);
    }
    
    private async void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        if (!_canDismiss)
            return; // Don't allow dismissal if not permitted
            
        // Tapping overlay dismisses the modal (returns to current game)
        await HideAsync();
        Dismissed?.Invoke(this, EventArgs.Empty);
    }
}
