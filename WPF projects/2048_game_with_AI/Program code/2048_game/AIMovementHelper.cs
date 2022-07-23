using System;

namespace _2048_game
{
    public class AIMovementHelper
    {
        public enum Direction { Up, Down, Left, Right};
        public Direction direction { get; set; }
        public int[,] playTableMatrix { get; set; }
        public int goodnessOfStep { get; set; }

        public AIMovementHelper(Direction direction, int[,] playTableMatrix)
        {
            this.direction = direction;
            this.playTableMatrix = playTableMatrix;
            this.goodnessOfStep = GoodnessOfStep(this.playTableMatrix);
        }

        private int GoodnessOfStep(int[,] playTableMatrix)
        {
            int return_value = -10;
            int max_value = 0;

            foreach(int number in playTableMatrix)
            {
                if (number == 0)
                    return_value += 15;
                else
                    return_value += (int)Math.Sqrt(number);

                if (max_value < number)
                    max_value = number;
            }

            return_value += max_value * 10;

            return return_value;
        }
    }
}
