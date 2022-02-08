using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace _2048_game
{
    public partial class SinglePlayerWindow : Window
    {
        private Random _rnd = new Random();
        private static int _RowAndColumnNumber = 4, _ChanceFor4 = 30;
        private int[,] _playTableMatrix = new int[_RowAndColumnNumber, _RowAndColumnNumber];
        private int _highScore = 0, _stepCount = 0, _highScoreStepCount = 0;
        private DateTime _highScoreTime = new DateTime();

        public SinglePlayerWindow()
        {
            InitializeComponent();

            PlayTable_Init();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                PlayTable_Init();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PlayTable_Init()
        {
            Array.Clear(_playTableMatrix, 0, _playTableMatrix.Length);

            _stepCount = -1;
            StepCountIncAndRefresh();

            GenerateNewNumber();
            GenerateNewNumber();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.W || e.Key == Key.Up)
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

        private void UpMovement()
        {
            Dictionary<int, List<int>> columns = new Dictionary<int, List<int>>();

            for(int i = 0; i < _RowAndColumnNumber; i++)
            {
                List<int> column = new List<int>();

                for(int j = 0; j < _RowAndColumnNumber; j++)
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
                while(columns[i].Count != _RowAndColumnNumber)
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
                for(int j = 0; j < _RowAndColumnNumber; j++)
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
                MessageBox.Show("Game Over!");
                PlayTable_Init();
                return;
            }

            bool success = false;
            do
            {
                int number_column = _rnd.Next(0, _RowAndColumnNumber), number_row = _rnd.Next(0, _RowAndColumnNumber);
                if(_playTableMatrix[number_row, number_column] == 0)
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
            _stepCount++;
            stepcount_label.Content = _stepCount;
        }

        private void HighScoreRefresh()
        {
            foreach (int number in _playTableMatrix)
            {
                if (number > _highScore)
                {
                    _highScore = number;
                    _highScoreStepCount = _stepCount;
                    _highScoreTime = DateTime.Now;
                }
            }

            if (_highScore != Convert.ToInt32(highscore_label.Content))
            {
                highscore_label.Content = _highScore;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_highScore != 0)
            {
                List<HighScoreListModel> HighScoreList = new HighScoreListFile().ReadFile().ToList();
                HighScoreList.Add(new HighScoreListModel("Single Play", AIGameWindow.Difficulty.NaN, "NaN", _highScoreTime, _highScore, _highScoreStepCount));
                new HighScoreListFile().WriteFile(HighScoreList);
            }

            base.OnClosing(e);
        }
    }
}
