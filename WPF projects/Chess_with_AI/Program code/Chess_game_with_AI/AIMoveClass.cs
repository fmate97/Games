using System;
using System.Collections.Generic;
using static Chess_game_with_AI.Chess_Window;

namespace Chess_game_with_AI
{
    public class AIMoveClass
    {
        public FieldClass OriginalFromStep { get; private set; }
        public FieldClass OriginalToStep { get; private set; }
        public string OriginalPuppet { get; private set; }

        public int Depth { get; private set; }
        public Dictionary<FieldClass, string> PlayTable { get; private set; }
        public List<string> PlayerGraveyard { get; set; }
        public List<string> AIGraveyard { get; set; }
        public int AIGoodness { get; private set; }
        public int PlayerGoodness { get; private set; }

        public AIMoveClass(FieldClass originalFromStep, FieldClass originalToStep, string originalPuppet, Colors aIColor, Colors playerColor, int depth, Dictionary<FieldClass, string> playTable, List<string> playerGraveyard, List<string> aiGraveyard, bool giveChess)
        {
            OriginalFromStep = originalFromStep;
            OriginalToStep = originalToStep;
            Depth = depth;
            PlayTable = playTable;
            PlayerGraveyard = playerGraveyard;
            AIGraveyard = aiGraveyard;
            OriginalPuppet = originalPuppet;

            if (depth % 2 == 1)
            {
                AIGoodness = CalcGoodness(aIColor, playTable, giveChess, false);
                PlayerGoodness = CalcGoodness(playerColor, playTable, false, giveChess);
            }
            else
            {
                AIGoodness = CalcGoodness(aIColor, playTable, false, giveChess);
                PlayerGoodness = CalcGoodness(playerColor, playTable, giveChess, false);
            }
        }

        public AIMoveClass(AIMoveClass getClass)
        {
            OriginalFromStep = getClass.OriginalFromStep;
            OriginalToStep = getClass.OriginalToStep;
            OriginalPuppet = getClass.OriginalPuppet;
            Depth = getClass.Depth;
            PlayTable = getClass.PlayTable;
            AIGoodness = getClass.AIGoodness;
            PlayerGoodness = getClass.PlayerGoodness;
            PlayerGraveyard = getClass.PlayerGraveyard;
            AIGraveyard = getClass.AIGraveyard;
        }

        private int CalcGoodness(Colors color, Dictionary<FieldClass, string> playTable, bool giveChess, bool getChess)
        {
            int return_value = 0;

            foreach(string puppet in playTable.Values)
            {
                if (puppet == Puppets.NaN.ToString())
                    continue;

                switch ((Puppets)Enum.Parse(typeof(Puppets), puppet.Split('_')[1]))
                {
                    case Puppets.king:
                        continue;
                    case Puppets.pawn:
                        if (puppet.Split('_')[0] == color.ToString())
                            return_value += 1;
                        else
                            return_value -= 1;
                        break;
                    case Puppets.rook:
                        if (puppet.Split('_')[0] == color.ToString())
                            return_value += 8;
                        else
                            return_value -= 8;
                        break;
                    case Puppets.knight:
                        if (puppet.Split('_')[0] == color.ToString())
                            return_value += 5;
                        else
                            return_value -= 5;
                        break;
                    case Puppets.bishop:
                        if (puppet.Split('_')[0] == color.ToString())
                            return_value += 8;
                        else
                            return_value -= 8;
                        break;
                    case Puppets.queen:
                        if (puppet.Split('_')[0] == color.ToString())
                            return_value += 15;
                        else
                            return_value -= 15;
                        break;
                }
            }

            if (giveChess)
            {                
                switch (Math.Abs(return_value))
                {
                    case int n when (n <= 10):
                        return_value += 3;
                        break;
                    case int n when (n > 10 && n <= 20):
                        return_value += 6;
                        break;
                    case int n when (n > 20 && n <= 30):
                        return_value += 9;
                        break;
                    case int n when (n > 30 && n <= 40):
                        return_value += 12;
                        break;
                    case int n when (n > 40):
                        return_value += 15;
                        break;
                }
            }
            if(getChess)
            {
                switch (Math.Abs(return_value))
                {
                    case int n when (n <= 10):
                        return_value -= 3;
                        break;
                    case int n when (n > 10 && n <= 20):
                        return_value -= 6;
                        break;
                    case int n when (n > 20 && n <= 30):
                        return_value -= 9;
                        break;
                    case int n when (n > 30 && n <= 40):
                        return_value -= 12;
                        break;
                    case int n when (n > 40):
                        return_value -= 15;
                        break;
                }
            }

            return return_value;
        }
    }
}
