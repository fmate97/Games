using System;

namespace Chess_game_with_AI
{
    public class HighScoreListModel
    {
        public string Winner { get; set; }
        public Chess_Window.Difficulties Difficulty { get; set; }
        public string GameTime { get; set; }
        public DateTime GameEndTime { get; set; }
        public int StepCount { get; set; }

        public HighScoreListModel(string winner, Chess_Window.Difficulties difficulty, string gameTime, DateTime gameEndTime, int stepCount)
        {
            this.Difficulty = difficulty;
            this.Winner = winner;
            this.GameTime = gameTime;
            this.GameEndTime = gameEndTime;
            this.StepCount = stepCount;
        }
    }
}
