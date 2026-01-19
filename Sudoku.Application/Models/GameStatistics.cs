using CoreDifficulty = Sudoku.Core.Services.DifficultyLevel;

namespace Sudoku.Application.Models
{
    /// <summary>
    /// Tracks game statistics including best times per difficulty.
    /// </summary>
    public class GameStatistics
    {
        /// <summary>
        /// Best time in seconds for Easy difficulty (null if never completed).
        /// </summary>
        public int? BestTimeEasy { get; set; }
        
        /// <summary>
        /// Best time in seconds for Medium difficulty (null if never completed).
        /// </summary>
        public int? BestTimeMedium { get; set; }
        
        /// <summary>
        /// Best time in seconds for Hard difficulty (null if never completed).
        /// </summary>
        public int? BestTimeHard { get; set; }
        
        /// <summary>
        /// Best time in seconds for Expert difficulty (null if never completed).
        /// </summary>
        public int? BestTimeExpert { get; set; }
        
        /// <summary>
        /// Best time in seconds for Evil difficulty (null if never completed).
        /// </summary>
        public int? BestTimeEvil { get; set; }
        
        /// <summary>
        /// Gets the best time for a specific difficulty level.
        /// </summary>
        public int? GetBestTime(CoreDifficulty difficulty)
        {
            return difficulty switch
            {
                CoreDifficulty.Easy => BestTimeEasy,
                CoreDifficulty.Medium => BestTimeMedium,
                CoreDifficulty.Hard => BestTimeHard,
                CoreDifficulty.Expert => BestTimeExpert,
                CoreDifficulty.Evil => BestTimeEvil,
                _ => null
            };
        }
        
        /// <summary>
        /// Sets the best time for a specific difficulty level.
        /// </summary>
        public void SetBestTime(CoreDifficulty difficulty, int timeInSeconds)
        {
            switch (difficulty)
            {
                case CoreDifficulty.Easy:
                    BestTimeEasy = timeInSeconds;
                    break;
                case CoreDifficulty.Medium:
                    BestTimeMedium = timeInSeconds;
                    break;
                case CoreDifficulty.Hard:
                    BestTimeHard = timeInSeconds;
                    break;
                case CoreDifficulty.Expert:
                    BestTimeExpert = timeInSeconds;
                    break;
                case CoreDifficulty.Evil:
                    BestTimeEvil = timeInSeconds;
                    break;
            }
        }
    }
}

