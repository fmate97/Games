using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;

namespace _2048_game
{
    public partial class AIGameWindow : Window
    {
        private Random _rnd = new Random();
        private static int _RowAndColumnNumber = 4, _ChanceFor4 = 30;
        private int[,] _playTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber], _aiPlayTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];
        private int _playerScore = 0, _playerStepCount = 0, _aIScore = 0, _aISteapCount = 0, _searchDepth = 0;
        private Timer _timer = new Timer();
        private double _timerIntervalInMS = 500;
        public enum Difficulty { NaN, Easy, Medium, Hard, VeryHard };
        private Difficulty _difficulty = Difficulty.Easy;
        private bool _playerGameEnded = false, _aiGameEnded = false;

        public AIGameWindow(Difficulty difficulty)
        {
            InitializeComponent();

            _difficulty = difficulty;

            switch (_difficulty)
            {
                case Difficulty.Easy:
                    _searchDepth = 0;
                    break;
                case Difficulty.Medium:
                    _searchDepth = 1;
                    break;
                case Difficulty.Hard:
                    _searchDepth = 3;
                    break;
                case Difficulty.VeryHard:
                    _searchDepth = 5;
                    break;
            }

            _timer.Interval = _timerIntervalInMS;
            _timer.Enabled = true;
            _timer.Elapsed += new ElapsedEventHandler(AI_movement);

            PlayTable_Init();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            if (MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                new DifficultyChoosingWindow().Show();
                this.Close();
            }
            _timer.Start();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PlayTable_Init()
        {
            _timer.Stop();
            Array.Clear(_playTableMatrix, 0, _playTableMatrix.Length);
            Array.Clear(_aiPlayTableMatrix, 0, _aiPlayTableMatrix.Length);

            _aIScore = 0;
            _aISteapCount = -1;
            AIStepCountIncAndRefresh();
            AIHighScoreRefresh();

            _playerScore = 0;
            _playerStepCount = -1;
            StepCountIncAndRefresh();
            HighScoreRefresh();

            GenerateNewNumber();
            GenerateNewNumber();

            AIGenerateNewNumber();
            AIGenerateNewNumber();

            _timer.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_playerGameEnded){
                if (e.Key == Key.W || e.Key == Key.Up)
                {
                    StepCountIncAndRefresh();
                    UpMovement();
                    HighScoreRefresh();
                }
                else if (e.Key == Key.A || e.Key == Key.Left)
                {
                    StepCountIncAndRefresh();
                    LeftMovement();
                    HighScoreRefresh();
                }
                else if (e.Key == Key.S || e.Key == Key.Down)
                {
                    StepCountIncAndRefresh();
                    DownMovement();
                    HighScoreRefresh();
                }
                else if (e.Key == Key.D || e.Key == Key.Right)
                {
                    StepCountIncAndRefresh();
                    RightMovement();
                    HighScoreRefresh();
                }
            }
        }

        private void UpMovement()
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    column.Add(_playTableMatrix[j, i]);
                }

                columns.Add(i, column);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = 0; j < columns[i].Count; j++)
                {
                    if (columns[i][j] == 0)
                    {
                        columns[i].RemoveAt(j);
                        j = -1;
                    }
                    else if (j == columns[i].Count - 1)
                    {
                        break;
                    }
                    else if (columns[i][j] == columns[i][j + 1])
                    {
                        columns[i][j] += columns[i][j];
                        columns[i].RemoveAt(j + 1);
                    }
                }
                while (columns[i].Count != _RowAndColumnNumber)
                {
                    columns[i].Add(0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _playTableMatrix[i, j] = columns[j][i];
                }
            }

            SetPlayTable(_playTableMatrix);
            GenerateNewNumber();
        }

        private void DownMovement()
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    column.Add(_playTableMatrix[j, i]);
                }

                columns.Add(i, column);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = columns[i].Count - 1; j > 0; j--)
                {
                    if (columns[i][j] == 0)
                    {
                        columns[i].RemoveAt(j);
                        j = columns[i].Count;
                    }
                    else if (j == 0)
                    {
                        break;
                    }
                    else if (columns[i][j] == columns[i][j - 1])
                    {
                        columns[i][j] += columns[i][j];
                        columns[i].RemoveAt(j - 1);
                    }
                }
                while (columns[i].Count != _RowAndColumnNumber)
                {
                    columns[i].Insert(0, 0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _playTableMatrix[i, j] = columns[j][i];
                }
            }

            SetPlayTable(_playTableMatrix);
            GenerateNewNumber();
        }

        private void LeftMovement()
        {
            Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    row.Add(_playTableMatrix[i, j]);
                }

                rows.Add(i, row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Count; j++)
                {
                    if (rows[i][j] == 0)
                    {
                        rows[i].RemoveAt(j);
                        j = -1;
                    }
                    else if (j == rows[i].Count - 1)
                    {
                        break;
                    }
                    else if (rows[i][j] == rows[i][j + 1])
                    {
                        rows[i][j] += rows[i][j];
                        rows[i].RemoveAt(j + 1);
                    }
                }
                while (rows[i].Count != _RowAndColumnNumber)
                {
                    rows[i].Add(0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _playTableMatrix[j, i] = rows[j][i];
                }
            }

            SetPlayTable(_playTableMatrix);
            GenerateNewNumber();
        }

        private void RightMovement()
        {
            Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    row.Add(_playTableMatrix[i, j]);
                }

                rows.Add(i, row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = rows[i].Count - 1; j > 0; j--)
                {
                    if (rows[i][j] == 0)
                    {
                        rows[i].RemoveAt(j);
                        j = rows[i].Count;
                    }
                    else if (j == 0)
                    {
                        break;
                    }
                    else if (rows[i][j] == rows[i][j - 1])
                    {
                        rows[i][j] += rows[i][j];
                        rows[i].RemoveAt(j - 1);
                    }
                }
                while (rows[i].Count != _RowAndColumnNumber)
                {
                    rows[i].Insert(0, 0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _playTableMatrix[j, i] = rows[j][i];
                }
            }

            SetPlayTable(_playTableMatrix);
            GenerateNewNumber();
        }

        private Brush NumberBackgroundColor(string number)
        {
            switch (number)
            {
                case "": return Brushes.WhiteSmoke;
                case "2": return Brushes.Gray;
                case "4": return Brushes.DarkGray;
                case "8": return Brushes.LightYellow;
                case "16": return Brushes.Yellow;
                case "32": return Brushes.Orange;
                case "64": return Brushes.DarkOrange;
                case "128": return Brushes.OrangeRed;
                case "256": return Brushes.Red;
                case "512": return Brushes.IndianRed;
                case "1024": return Brushes.DarkRed;
                case "2048": return Brushes.LightGreen;
                case "4096": return Brushes.GreenYellow;
                case "8192": return Brushes.Green;
                case "16384": return Brushes.DarkSeaGreen;
                case "32768": return Brushes.DarkGreen;
                case "65536": return Brushes.DarkOliveGreen;
                case "131072": return Brushes.LightSteelBlue;
                case "262144": return Brushes.Blue;
                default: return Brushes.Black;
            }
        }

        private bool TableIsFull()
        {
            foreach (int number in _playTableMatrix)
            {
                if (number == 0)
                    return false;
            }
            return true;
        }

        private int[,] GetPlayTable()
        {
            int[,] ReturnPlayTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];

            foreach (Label number in PlayTable.Children)
            {
                ReturnPlayTableMatrix[Grid.GetRow(number), Grid.GetColumn(number)] = (string)number.Content == "" ? 0 : Convert.ToInt32((string)number.Content);
            }

            return ReturnPlayTableMatrix;
        }

        private void SetPlayTable(int[,] GotPlayTableMatrix)
        {
            foreach (Label number in PlayTable.Children)
            {
                number.Content = GotPlayTableMatrix[Grid.GetRow(number), Grid.GetColumn(number)] == 0 ? "" : $"{GotPlayTableMatrix[Grid.GetRow(number), Grid.GetColumn(number)]}";
                number.Background = NumberBackgroundColor((string)number.Content);
            }
        }


        private void GenerateNewNumber()
        {
            if (TableIsFull())
            {
                playerGameEnd_Label.Visibility = Visibility.Visible;
                _playerGameEnded = true;
                if (_aiGameEnded)
                {
                    GameEnded();
                }
                else
                {
                    return;
                }
            }

            bool success = false;
            do
            {
                int number_column = _rnd.Next(0, _RowAndColumnNumber), number_row = _rnd.Next(0, _RowAndColumnNumber);
                if (_playTableMatrix[number_row, number_column] == 0)
                {
                    if (_rnd.Next(0, 101) <= _ChanceFor4)
                    {
                        _playTableMatrix[number_row, number_column] = 4;
                    }
                    else
                    {
                        _playTableMatrix[number_row, number_column] = 2;
                    }
                    success = true;
                }
            } while (!success);

            SetPlayTable(_playTableMatrix);
        }

        private void StepCountIncAndRefresh()
        {
            _playerStepCount++;
            stepcount_label.Content = _playerStepCount;
        }

        private void HighScoreRefresh()
        {
            foreach (int number in _playTableMatrix)
            {
                if (number > _playerScore)
                {
                    _playerScore = number;
                }
            }

            if (_playerScore != Convert.ToInt32(score_label.Content))
            {
                score_label.Content = _playerScore;
            }
        }

        private void AI_movement(object sender, ElapsedEventArgs e)
        {
            if (_difficulty == Difficulty.Easy)
            {
                switch (_rnd.Next(0, 4))
                {
                    case 0:
                        AIStepCountIncAndRefresh();
                        AIUpMovement();
                        AIHighScoreRefresh();
                        break;
                    case 1:
                        AIStepCountIncAndRefresh();
                        AILeftMovement();
                        AIHighScoreRefresh();
                        break;
                    case 2:
                        AIStepCountIncAndRefresh();
                        AIDownMovement();
                        AIHighScoreRefresh();
                        break;
                    case 3:
                        AIStepCountIncAndRefresh();
                        AIRightMovement();
                        AIHighScoreRefresh();
                        break;
                }
            }
            else if (_difficulty == Difficulty.Medium)
            { 
                List<AIMovementHelper> aiMovementHelper = new List<AIMovementHelper>();

                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Up, AIUpMovement_Test(_aiPlayTableMatrix)));
                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Down, AIDownMovement_Test(_aiPlayTableMatrix)));
                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Left, AILeftMovement_Test(_aiPlayTableMatrix)));
                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Right, AIRightMovement_Test(_aiPlayTableMatrix)));

                aiMovementHelper = aiMovementHelper.OrderByDescending(x => x.goodnessOfStep).ToList();

                switch (aiMovementHelper.First().direction)
                {
                    case AIMovementHelper.Direction.Up:
                        AIStepCountIncAndRefresh();
                        AIUpMovement();
                        AIHighScoreRefresh();
                        break;
                    case AIMovementHelper.Direction.Left:
                        AIStepCountIncAndRefresh();
                        AILeftMovement();
                        AIHighScoreRefresh();
                        break;
                    case AIMovementHelper.Direction.Down:
                        AIStepCountIncAndRefresh();
                        AIDownMovement();
                        AIHighScoreRefresh();
                        break;
                    case AIMovementHelper.Direction.Right:
                        AIStepCountIncAndRefresh();
                        AIRightMovement();
                        AIHighScoreRefresh();
                        break;
                }
            }
            else if (_difficulty == Difficulty.Hard || _difficulty == Difficulty.VeryHard)
            {
                List<AIMovementHelper> aiMovementHelper = new List<AIMovementHelper>();

                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Up, AIUpMovement_Test(_aiPlayTableMatrix)));
                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Down, AIDownMovement_Test(_aiPlayTableMatrix)));
                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Left, AILeftMovement_Test(_aiPlayTableMatrix)));
                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Right, AIRightMovement_Test(_aiPlayTableMatrix)));

                for(int i = 1; i <= _searchDepth; i++)
                {
                    for(int j = 0; j < _RowAndColumnNumber; j++)
                    {
                        List<AIMovementHelper> aiMovementHelperDepth = new List<AIMovementHelper>();

                        aiMovementHelperDepth.Add(new AIMovementHelper(AIMovementHelper.Direction.Up, AIUpMovement_Test(aiMovementHelper[j].playTableMatrix)));
                        aiMovementHelperDepth.Add(new AIMovementHelper(AIMovementHelper.Direction.Down, AIDownMovement_Test(aiMovementHelper[j].playTableMatrix)));
                        aiMovementHelperDepth.Add(new AIMovementHelper(AIMovementHelper.Direction.Left, AILeftMovement_Test(aiMovementHelper[j].playTableMatrix)));
                        aiMovementHelperDepth.Add(new AIMovementHelper(AIMovementHelper.Direction.Right, AIRightMovement_Test(aiMovementHelper[j].playTableMatrix)));

                        aiMovementHelperDepth.OrderByDescending(x => x.goodnessOfStep).ToList();

                        switch (aiMovementHelper[j].direction)
                        {
                            case AIMovementHelper.Direction.Up:
                                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Up, aiMovementHelperDepth.First().playTableMatrix));
                                break;
                            case AIMovementHelper.Direction.Left:
                                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Left, aiMovementHelperDepth.First().playTableMatrix));
                                break;
                            case AIMovementHelper.Direction.Down:
                                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Down, aiMovementHelperDepth.First().playTableMatrix));
                                break;
                            case AIMovementHelper.Direction.Right:
                                aiMovementHelper.Add(new AIMovementHelper(AIMovementHelper.Direction.Right, aiMovementHelperDepth.First().playTableMatrix));
                                break;
                        }
                    }
                    aiMovementHelper.RemoveRange(0, 4);
                }

                aiMovementHelper = aiMovementHelper.OrderByDescending(x => x.goodnessOfStep).ToList();

                switch (aiMovementHelper.First().direction)
                {
                    case AIMovementHelper.Direction.Up:
                        AIStepCountIncAndRefresh();
                        AIUpMovement();
                        AIHighScoreRefresh();
                        break;
                    case AIMovementHelper.Direction.Left:
                        AIStepCountIncAndRefresh();
                        AILeftMovement();
                        AIHighScoreRefresh();
                        break;
                    case AIMovementHelper.Direction.Down:
                        AIStepCountIncAndRefresh();
                        AIDownMovement();
                        AIHighScoreRefresh();
                        break;
                    case AIMovementHelper.Direction.Right:
                        AIStepCountIncAndRefresh();
                        AIRightMovement();
                        AIHighScoreRefresh();
                        break;
                }
            }
        }

        private void AIStepCountIncAndRefresh()
        {
            _aISteapCount++;
            this.Dispatcher.Invoke(() =>
            {
                ai_stepcount_label.Content = _aISteapCount;
            });
        }

        private void AIHighScoreRefresh()
        {
            foreach (int number in _aiPlayTableMatrix)
            {
                if (number > _aIScore)
                {
                    _aIScore = number;
                }
            }

            this.Dispatcher.Invoke(() =>
            {
                if (_aIScore != Convert.ToInt32(ai_score_label.Content))
                {
                    ai_score_label.Content = _aIScore;
                }
            });
        }

        private void AIUpMovement()
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    column.Add(_aiPlayTableMatrix[j, i]);
                }

                columns.Add(i, column);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = 0; j < columns[i].Count; j++)
                {
                    if (columns[i][j] == 0)
                    {
                        columns[i].RemoveAt(j);
                        j = -1;
                    }
                    else if (j == columns[i].Count - 1)
                    {
                        break;
                    }
                    else if (columns[i][j] == columns[i][j + 1])
                    {
                        columns[i][j] += columns[i][j];
                        columns[i].RemoveAt(j + 1);
                    }
                }
                while (columns[i].Count != _RowAndColumnNumber)
                {
                    columns[i].Add(0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _aiPlayTableMatrix[i, j] = columns[j][i];
                }
            }

            AISetPlayTable(_aiPlayTableMatrix);
            AIGenerateNewNumber();
        }

        private void AIDownMovement()
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    column.Add(_aiPlayTableMatrix[j, i]);
                }

                columns.Add(i, column);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = columns[i].Count - 1; j > 0; j--)
                {
                    if (columns[i][j] == 0)
                    {
                        columns[i].RemoveAt(j);
                        j = columns[i].Count;
                    }
                    else if (j == 0)
                    {
                        break;
                    }
                    else if (columns[i][j] == columns[i][j - 1])
                    {
                        columns[i][j] += columns[i][j];
                        columns[i].RemoveAt(j - 1);
                    }
                }
                while (columns[i].Count != _RowAndColumnNumber)
                {
                    columns[i].Insert(0, 0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _aiPlayTableMatrix[i, j] = columns[j][i];
                }
            }

            AISetPlayTable(_aiPlayTableMatrix);
            AIGenerateNewNumber();
        }

        private void AILeftMovement()
        {
            Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    row.Add(_aiPlayTableMatrix[i, j]);
                }

                rows.Add(i, row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Count; j++)
                {
                    if (rows[i][j] == 0)
                    {
                        rows[i].RemoveAt(j);
                        j = -1;
                    }
                    else if (j == rows[i].Count - 1)
                    {
                        break;
                    }
                    else if (rows[i][j] == rows[i][j + 1])
                    {
                        rows[i][j] += rows[i][j];
                        rows[i].RemoveAt(j + 1);
                    }
                }
                while (rows[i].Count != _RowAndColumnNumber)
                {
                    rows[i].Add(0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _aiPlayTableMatrix[j, i] = rows[j][i];
                }
            }

            AISetPlayTable(_aiPlayTableMatrix);
            AIGenerateNewNumber();
        }

        private void AIRightMovement()
        {
            Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    row.Add(_aiPlayTableMatrix[i, j]);
                }

                rows.Add(i, row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = rows[i].Count - 1; j > 0; j--)
                {
                    if (rows[i][j] == 0)
                    {
                        rows[i].RemoveAt(j);
                        j = rows[i].Count;
                    }
                    else if (j == 0)
                    {
                        break;
                    }
                    else if (rows[i][j] == rows[i][j - 1])
                    {
                        rows[i][j] += rows[i][j];
                        rows[i].RemoveAt(j - 1);
                    }
                }
                while (rows[i].Count != _RowAndColumnNumber)
                {
                    rows[i].Insert(0, 0);
                }
            }

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    _aiPlayTableMatrix[j, i] = rows[j][i];
                }
            }

            AISetPlayTable(_aiPlayTableMatrix);
            AIGenerateNewNumber();
        }

        private int[,] AIUpMovement_Test(int[,] playTableMatrix)
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    column.Add(playTableMatrix[j, i]);
                }

                columns.Add(i, column);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = 0; j < columns[i].Count; j++)
                {
                    if (columns[i][j] == 0)
                    {
                        columns[i].RemoveAt(j);
                        j = -1;
                    }
                    else if (j == columns[i].Count - 1)
                    {
                        break;
                    }
                    else if (columns[i][j] == columns[i][j + 1])
                    {
                        columns[i][j] += columns[i][j];
                        columns[i].RemoveAt(j + 1);
                    }
                }
                while (columns[i].Count != _RowAndColumnNumber)
                {
                    columns[i].Add(0);
                }
            }

            int[,] returnPlayTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    returnPlayTableMatrix[i, j] = columns[j][i];
                }
            }

            return returnPlayTableMatrix;
        }

        private int[,] AIDownMovement_Test(int[,] playTableMatrix)
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    column.Add(playTableMatrix[j, i]);
                }

                columns.Add(i, column);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                for (int j = columns[i].Count - 1; j > 0; j--)
                {
                    if (columns[i][j] == 0)
                    {
                        columns[i].RemoveAt(j);
                        j = columns[i].Count;
                    }
                    else if (j == 0)
                    {
                        break;
                    }
                    else if (columns[i][j] == columns[i][j - 1])
                    {
                        columns[i][j] += columns[i][j];
                        columns[i].RemoveAt(j - 1);
                    }
                }
                while (columns[i].Count != _RowAndColumnNumber)
                {
                    columns[i].Insert(0, 0);
                }
            }

            int[,] returnPlayTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    returnPlayTableMatrix[i, j] = columns[j][i];
                }
            }

            return returnPlayTableMatrix;
        }

        private int[,] AILeftMovement_Test(int[,] playTableMatrix)
        {
            Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    row.Add(playTableMatrix[i, j]);
                }

                rows.Add(i, row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; j < rows[i].Count; j++)
                {
                    if (rows[i][j] == 0)
                    {
                        rows[i].RemoveAt(j);
                        j = -1;
                    }
                    else if (j == rows[i].Count - 1)
                    {
                        break;
                    }
                    else if (rows[i][j] == rows[i][j + 1])
                    {
                        rows[i][j] += rows[i][j];
                        rows[i].RemoveAt(j + 1);
                    }
                }
                while (rows[i].Count != _RowAndColumnNumber)
                {
                    rows[i].Add(0);
                }
            }

            int[,] returnPlayTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    returnPlayTableMatrix[j, i] = rows[j][i];
                }
            }

            return returnPlayTableMatrix;
        }

        private int[,] AIRightMovement_Test(int[,] playTableMatrix)
        {
            Dictionary<int, List<int>> rows = new Dictionary<int, List<int>>();

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> row = new List<int>();

                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    row.Add(_aiPlayTableMatrix[i, j]);
                }

                rows.Add(i, row);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = rows[i].Count - 1; j > 0; j--)
                {
                    if (rows[i][j] == 0)
                    {
                        rows[i].RemoveAt(j);
                        j = rows[i].Count;
                    }
                    else if (j == 0)
                    {
                        break;
                    }
                    else if (rows[i][j] == rows[i][j - 1])
                    {
                        rows[i][j] += rows[i][j];
                        rows[i].RemoveAt(j - 1);
                    }
                }
                while (rows[i].Count != _RowAndColumnNumber)
                {
                    rows[i].Insert(0, 0);
                }
            }

            int[,] returnPlayTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];

            for (int i = 0; i < _RowAndColumnNumber; i++)
            {
                for (int j = 0; j < _RowAndColumnNumber; j++)
                {
                    returnPlayTableMatrix[j, i] = rows[j][i];
                }
            }

            return returnPlayTableMatrix;
        }


        private bool AITableIsFull()
        {
            foreach (int number in _aiPlayTableMatrix)
            {
                if (number == 0)
                    return false;
            }
            return true;
        }

        private void AISetPlayTable(int[,] GotPlayTableMatrix)
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (Label number in AIPlayTable.Children)
                {
                    number.Content = GotPlayTableMatrix[Grid.GetRow(number), Grid.GetColumn(number)] == 0 ? "" : $"{GotPlayTableMatrix[Grid.GetRow(number), Grid.GetColumn(number)]}";
                    number.Background = NumberBackgroundColor((string)number.Content);
                }
            });
        }

        private void AIGenerateNewNumber()
        {
            if (AITableIsFull())
            {
                this.Dispatcher.Invoke(() =>
                {
                    aiGameEnd_Label.Visibility = Visibility.Visible;
                });
                _aiGameEnded = true;
                _timer.Stop();
                if (_playerGameEnded)
                {
                    GameEnded();
                }
                else
                {
                    return;
                }
            }

            bool success = false;
            do
            {
                int number_column = _rnd.Next(0, _RowAndColumnNumber), number_row = _rnd.Next(0, _RowAndColumnNumber);
                if (_aiPlayTableMatrix[number_row, number_column] == 0)
                {
                    if (_rnd.Next(0, 101) <= _ChanceFor4)
                    {
                        _aiPlayTableMatrix[number_row, number_column] = 4;
                    }
                    else
                    {
                        _aiPlayTableMatrix[number_row, number_column] = 2;
                    }
                    success = true;
                }
            } while (!success);

            AISetPlayTable(_aiPlayTableMatrix);
        }

        private void GameEnded()
        {
            if (_aIScore > _playerScore)
                MessageBox.Show($"You lost!", "Game Over", MessageBoxButton.OK);
            else if (_playerScore > _aIScore)
                MessageBox.Show($"You win!", "Game Over", MessageBoxButton.OK);
            else if (_playerScore == _aIScore)
                MessageBox.Show($"Draw!", "Game Over", MessageBoxButton.OK);

            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_playerGameEnded && _aiGameEnded)
            {
                List<HighScoreListModel> HighScoreList = new HighScoreListFile().ReadFile().ToList();

                if (_aIScore > _playerScore)
                    HighScoreList.Add(new HighScoreListModel("AI Play", _difficulty ,"AI", DateTime.Now, _aIScore, _aISteapCount));
                else if (_playerScore > _aIScore)
                    HighScoreList.Add(new HighScoreListModel("AI Play", _difficulty, "Player", DateTime.Now, _playerScore, _playerStepCount));
                else if (_playerScore == _aIScore)
                    HighScoreList.Add(new HighScoreListModel("AI Play", _difficulty, "NaN", DateTime.Now, _playerScore, _playerStepCount));

                new HighScoreListFile().WriteFile(HighScoreList);

            }

            _timer.Stop();

            base.OnClosing(e);
        }
    }
}
