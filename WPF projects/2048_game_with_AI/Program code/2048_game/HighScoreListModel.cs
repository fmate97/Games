using System;

namespace _2048_game
{
    public class HighScoreListModel
    {
        public string Mode { get; set; }
        public AIGameWindow.Difficulty Difficulty { get; set; }
        public string Winner { get; set; }
        public DateTime Time { get; set; }
        public int HighScore { get; set; }
        public int HighScoreStepCount { get; set; }

        public HighScoreListModel(string mode, AIGameWindow.Difficulty difficulty, string winner, DateTime time, int highScore, int highScoreStepCount)
        {
            this.Mode = mode;
            this.Difficulty = difficulty;
            this.Winner = winner;
            this.Time = time;
            this.HighScore = highScore;
            this.HighScoreStepCount = highScoreStepCount;
        }
    }
}
