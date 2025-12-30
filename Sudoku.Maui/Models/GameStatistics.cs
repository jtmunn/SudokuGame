namespace Sudoku.Maui.Models
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
        /// Best time in seconds for Master difficulty (null if never completed).
        /// </summary>
        public int? BestTimeMaster { get; set; }
        
        /// <summary>
        /// Best time in seconds for GrandMaster difficulty (null if never completed).
        /// </summary>
        public int? BestTimeGrandMaster { get; set; }
        
        /// <summary>
        /// Gets the best time for a specific difficulty level.
        /// </summary>
        public int? GetBestTime(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => BestTimeEasy,
                DifficultyLevel.Medium => BestTimeMedium,
                DifficultyLevel.Hard => BestTimeHard,
                DifficultyLevel.Expert => BestTimeExpert,
                DifficultyLevel.Master => BestTimeMaster,
                DifficultyLevel.GrandMaster => BestTimeGrandMaster,
                _ => null
            };
        }
        
        /// <summary>
        /// Sets the best time for a specific difficulty level.
        /// </summary>
        public void SetBestTime(DifficultyLevel difficulty, int timeInSeconds)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    BestTimeEasy = timeInSeconds;
                    break;
                case DifficultyLevel.Medium:
                    BestTimeMedium = timeInSeconds;
                    break;
                case DifficultyLevel.Hard:
                    BestTimeHard = timeInSeconds;
                    break;
                case DifficultyLevel.Expert:
                    BestTimeExpert = timeInSeconds;
                    break;
                case DifficultyLevel.Master:
                    BestTimeMaster = timeInSeconds;
                    break;
                case DifficultyLevel.GrandMaster:
                    BestTimeGrandMaster = timeInSeconds;
                    break;
            }
        }
    }
}
