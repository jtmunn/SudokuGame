using Plugin.Maui.Audio;

namespace Sudoku.Maui.Services
{
    /// <summary>
    /// Service for playing sound effects in the game.
    /// </summary>
    public class SoundService
    {
        private readonly IAudioManager _audioManager;
        private Dictionary<string, IAudioPlayer?> _soundCache = new();
        private bool _soundEnabled = true;

        public SoundService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        public bool IsSoundEnabled
        {
            get => _soundEnabled;
            set => _soundEnabled = value;
        }

        /// <summary>
        /// Plays a sound effect by name.
        /// </summary>
        public async Task PlaySoundAsync(string soundName)
        {
            if (!_soundEnabled)
                return;

            try
            {
                // Try to get from cache first
                if (!_soundCache.ContainsKey(soundName))
                {
                    var audioStream = await FileSystem.OpenAppPackageFileAsync($"Sounds/{soundName}.mp3");
                    if (audioStream != null)
                    {
                        var player = _audioManager.CreatePlayer(audioStream);
                        _soundCache[soundName] = player;
                    }
                    else
                    {
                        _soundCache[soundName] = null;
                    }
                }

                var cachedPlayer = _soundCache[soundName];
                if (cachedPlayer != null)
                {
                    // Reset to beginning if already playing
                    if (cachedPlayer.IsPlaying)
                    {
                        cachedPlayer.Stop();
                    }
                    
                    cachedPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                // Silently fail if sound file is not found
                System.Diagnostics.Debug.WriteLine($"Sound playback failed for {soundName}: {ex.Message}");
            }
        }

        public Task PlayCorrectSound() => PlaySoundAsync("correct");
        public Task PlayErrorSound() => PlaySoundAsync("error");
        public Task PlayCompleteSound() => PlaySoundAsync("complete");
        public Task PlayHintSound() => PlaySoundAsync("hint");
        public Task PlaySelectSound() => PlaySoundAsync("select");

        public void Dispose()
        {
            foreach (var player in _soundCache.Values)
            {
                player?.Dispose();
            }
            _soundCache.Clear();
        }
    }
}
