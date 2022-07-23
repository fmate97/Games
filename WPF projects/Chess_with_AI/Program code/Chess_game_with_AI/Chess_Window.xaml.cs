using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Chess_game_with_AI
{
    public partial class Chess_Window : Window
    {
        public enum Difficulties { Easy, Medium, Hard, VeryHard }
        public enum Colors { black, white }
        public enum Puppets { NaN, pawn, rook, bishop, knight, queen, king }
        public enum Players { NaN, Player, AI }

        private Brush _selectedFieldColor = Brushes.LightSteelBlue;
        private Brush _aiMovementFieldColor = Brushes.OrangeRed;
        private int _delayInMS = 500;

        private Difficulties _difficulty = Difficulties.Easy;
        private Colors _playerColor = Colors.white, _aIColor = Colors.black;
        private Random _rnd = new Random();
        private Image _playerSelectedPuppet = null;
        private Dictionary<Border, Brush> _playerSelectedPuppet_ValidMovement = new Dictionary<Border, Brush>();
        private Dictionary<Border, Brush> _aiMovement_ColoredField = new Dictionary<Border, Brush>();
        private bool _resurrectionFromTheCemetery = false;
        private int _stepCount = 0;
        private string _timeElapsed = "00:00:00";
        private Stopwatch _stopWatch = new Stopwatch();
        private DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Send);
        private bool _playerWin = false;
        private Dictionary<FieldClass, Image> _playTableSave = new Dictionary<FieldClass, Image>();
        private Players _lastStepCountInc = Players.NaN;
        private int _searchDepth = 0;
        private List<Image> _puppets = new List<Image>();
        private Cursor _baseMouseCursor = Mouse.OverrideCursor;
        private BackgroundWorker _worker = new BackgroundWorker();
        private bool _aiMoveComplete = false;

        public Chess_Window(Difficulties difficulty)
        {
            InitializeComponent();

            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += AIMove;
            _worker.ProgressChanged += AIMove_ProgressChanged;

            switch (difficulty)
            {
                case Difficulties.Easy:
                    _searchDepth = 1;
                    break;
                case Difficulties.Medium:
                    _searchDepth = 2;
                    break;
                case Difficulties.Hard:
                    _searchDepth = 3;
                    break;
                case Difficulties.VeryHard:
                    _searchDepth = 5;
                    break;
            }

            _difficulty = difficulty;
            PlayTable.IsEnabled = false;

            PlayTable_Init();

            if (_playerColor == Colors.white)
                PlayTable.IsEnabled = true;
            else
            {
                _aiMoveComplete = false;
                _worker.RunWorkerAsync();
                PlayTable.IsEnabled = true;
            }
        }

        private void PlayTable_Init()
        {
            PlayTable.Children.Clear();
            letterGrid1.Children.Clear();
            letterGrid2.Children.Clear();
            numberGrid1.Children.Clear();
            numberGrid2.Children.Clear();
            _puppets.Clear();
            _resurrectionFromTheCemetery = false;
            _playerWin = false;
            _stepCount = 0;
            _timeElapsed = "00:00:00";
            _stopWatch.Reset();
            chess_label.Visibility = Visibility.Collapsed;

            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _stopWatch.Start();
            _timer.Start();

            do
            {
                if (_rnd.Next(0, 2) == 0)
                {
                    _playerColor = Colors.white;
                    _aIColor = Colors.black;
                }
                else
                {
                    _playerColor = Colors.black;
                    _aIColor = Colors.white;
                }
            } while (_playerColor == _aIColor);

            for (int i = 0; i < 64; i++)
            {
                Border field = new Border();
                field.Background = (i / 8) % 2 != 0 ? (i % 2 == 0 ? Brushes.Black : Brushes.White) : (i % 2 == 0 ? Brushes.White : Brushes.Black);
                field.MouseLeftButtonDown += Field_Click;
                field.BorderThickness = new Thickness(0);
                Grid.SetRow(field, i / 8);
                Grid.SetColumn(field, i % 8);

                PlayTable.Children.Add(field);
            }

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Label letterLabel = new Label();
                    letterLabel.Foreground = Brushes.White;
                    letterLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    letterLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    letterLabel.FontWeight = FontWeights.Bold;
                    letterLabel.FontSize = 24;
                    letterLabel.Content = _playerColor == Colors.white ? (char)('A' + i) : (char)('H' - i);
                    Grid.SetRow(letterLabel, 0);
                    Grid.SetColumn(letterLabel, i);

                    if (j == 0)
                        letterGrid1.Children.Add(letterLabel);
                    else
                        letterGrid2.Children.Add(letterLabel);

                }
            }

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Label numberLabel = new Label();
                    numberLabel.Foreground = Brushes.White;
                    numberLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    numberLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    numberLabel.FontWeight = FontWeights.Bold;
                    numberLabel.FontSize = 24;
                    numberLabel.Content = _playerColor == Colors.white ? (8 - i) : (i + 1);
                    Grid.SetRow(numberLabel, i);
                    Grid.SetColumn(numberLabel, 0);

                    if (j == 0)
                        numberGrid1.Children.Add(numberLabel);
                    else
                        numberGrid2.Children.Add(numberLabel);

                }
            }

            for (int i = 0; i < 8; i++)
            {
                Image playerPawn = FindResource($"{_playerColor}_{Puppets.pawn}_{i}") as Image;
                Image aiPawn = FindResource($"{_aIColor}_{Puppets.pawn}_{i}") as Image;
                PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 6 && Grid.GetColumn(x) == i).Child = playerPawn;
                PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 1 && Grid.GetColumn(x) == i).Child = aiPawn;
                _puppets.Add(playerPawn);
                _puppets.Add(aiPawn);
            }

            Image playerRook1 = FindResource($"{_playerColor}_{Puppets.rook}_{0}") as Image;
            Image playerRook2 = FindResource($"{_playerColor}_{Puppets.rook}_{1}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 0).Child = playerRook1;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 7).Child = playerRook2;
            _puppets.Add(playerRook1);
            _puppets.Add(playerRook2);

            Image aiRook1 = FindResource($"{_aIColor}_{Puppets.rook}_{0}") as Image;
            Image aiRook2 = FindResource($"{_aIColor}_{Puppets.rook}_{1}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 0).Child = aiRook1;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 7).Child = aiRook2;
            _puppets.Add(aiRook1);
            _puppets.Add(aiRook2);

            Image playerKnight1 = FindResource($"{_playerColor}_{Puppets.knight}_{0}") as Image;
            Image playerKnight2 = FindResource($"{_playerColor}_{Puppets.knight}_{1}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 1).Child = playerKnight1;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 6).Child = playerKnight2;
            _puppets.Add(playerKnight1);
            _puppets.Add(playerKnight2);

            Image aiKnight1 = FindResource($"{_aIColor}_{Puppets.knight}_{0}") as Image;
            Image aiKnight2 = FindResource($"{_aIColor}_{Puppets.knight}_{1}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 1).Child = aiKnight1;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 6).Child = aiKnight2;
            _puppets.Add(aiKnight1);
            _puppets.Add(aiKnight2);

            Image playerBishop1 = FindResource($"{_playerColor}_{Puppets.bishop}_{0}") as Image;
            Image playerBishop2 = FindResource($"{_playerColor}_{Puppets.bishop}_{1}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 2).Child = playerBishop1;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 5).Child = playerBishop2;
            _puppets.Add(playerBishop1);
            _puppets.Add(playerBishop2);

            Image aiBishop1 = FindResource($"{_aIColor}_{Puppets.bishop}_{0}") as Image;
            Image aiBishop2 = FindResource($"{_aIColor}_{Puppets.bishop}_{1}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 2).Child = aiBishop1;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 5).Child = aiBishop2;
            _puppets.Add(aiBishop1);
            _puppets.Add(aiBishop2);

            Image playerQueen = FindResource($"{_playerColor}_{Puppets.queen}_{0}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 3).Child = playerQueen;
            _puppets.Add(playerQueen);

            Image aiQueen = FindResource($"{_aIColor}_{Puppets.queen}_{0}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 3).Child = aiQueen;
            _puppets.Add(aiQueen);

            Image playerKing = FindResource($"{_playerColor}_{Puppets.king}_{0}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 7 && Grid.GetColumn(x) == 4).Child = playerKing;
            _puppets.Add(playerKing);

            Image aiKing = FindResource($"{_aIColor}_{Puppets.king}_{0}") as Image;
            PlayTable.Children.Cast<Border>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 4).Child = aiKing;
            _puppets.Add(aiKing);
        }

        private async void Field_Click(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.OverrideCursor != Cursors.Wait)
            {
                if (!_resurrectionFromTheCemetery)
                {
                    if (_playerSelectedPuppet == null)
                    {
                        _playerSelectedPuppet = (sender as Border).Child as Image;

                        if (_playerSelectedPuppet != null && _playerSelectedPuppet.Name.Split('_')[0] != _playerColor.ToString())
                        {
                            _playerSelectedPuppet = null;
                            MessageBox.Show($"This is not your puppet!\n(your color: {_playerColor})", "Warning", MessageBoxButton.OK);
                        }

                        if (_playerSelectedPuppet != null)
                            SetValidStep();
                    }
                    else
                    {
                        if ((sender as Border).Background == _selectedFieldColor)
                        {
                            SavePlayTable();

                            (_playerSelectedPuppet.Parent as Border).Child = null;
                            if ((sender as Border).Child != null)
                                PuppetMoveInTheGraveyard((sender as Border).Child as Image, true);
                            (sender as Border).Child = _playerSelectedPuppet;

                            if (_playerSelectedPuppet.Name.Split('_')[1] == Puppets.pawn.ToString() && Grid.GetRow(sender as Border) == 0)
                                _resurrectionFromTheCemetery = true;

                            PlayerSelectedPuppet_ValidMovement_Clear();
                            IncAndShowStepCount(Players.Player);

                            if (_resurrectionFromTheCemetery)
                            {
                                bool onlyPawnsInGraveyard = true;
                                foreach (Image puppet in Player_graveyard1.Children)
                                {
                                    if (puppet.Name.Split('_')[1] != Puppets.pawn.ToString())
                                    {
                                        onlyPawnsInGraveyard = false;
                                        break;
                                    }

                                }
                                if (onlyPawnsInGraveyard)
                                {
                                    foreach (Image puppet in Player_graveyard2.Children)
                                    {
                                        if (puppet.Name.Split('_')[1] != Puppets.pawn.ToString())
                                        {
                                            onlyPawnsInGraveyard = false;
                                            break;
                                        }

                                    }
                                }

                                if (onlyPawnsInGraveyard)
                                    _resurrectionFromTheCemetery = false;
                            }

                            do
                            {
                                await Task.Delay(_delayInMS);
                            } while (_resurrectionFromTheCemetery);

                            _playerSelectedPuppet = null;

                            if (KingInChess(PlayTable.Children.Cast<Border>().First(x => x.Child != null && (x.Child as Image).Name == $"{_playerColor}_{Puppets.king}_0"), true))
                            {
                                if (chess_label.Visibility == Visibility.Visible)
                                {
                                    MessageBox.Show("This move did not eliminate the chess!", "Chess", MessageBoxButton.OK);
                                    LoadPlayTable();
                                    return;
                                }
                                else
                                {
                                    MessageBox.Show("This move will result in chess!", "Warning", MessageBoxButton.OK);
                                    LoadPlayTable();
                                    return;
                                }
                            }
                            else if (chess_label.Visibility == Visibility.Visible)
                            {
                                chess_label.Visibility = Visibility.Collapsed;
                            }

                            PlayTable.IsEnabled = false;
                            _aiMoveComplete = false;
                            _worker.RunWorkerAsync();

                            do
                            {
                                await Task.Delay(_delayInMS);
                            } while (!_aiMoveComplete);


                            if (KingInChess(PlayTable.Children.Cast<Border>().First(x => x.Child != null && (x.Child as Image).Name == $"{_playerColor}_{Puppets.king}_0"), true))
                            {
                                SavePlayTable();
                                chess_label.Visibility = Visibility.Visible;
                            }
                            PlayTable.IsEnabled = true;
                        }
                        else if ((sender as Border).Child != null && ((sender as Border).Child as Image).Name.Split('_')[0] == _playerColor.ToString())
                        {
                            _playerSelectedPuppet = (sender as Border).Child as Image;
                            SetValidStep();
                        }
                        else MessageBox.Show("That step is not allowed!", "Warning", MessageBoxButton.OK);
                    }
                }
            }
        }

        private void SetValidStep()
        {
            if (_playerSelectedPuppet_ValidMovement.Count != 0)
                PlayerSelectedPuppet_ValidMovement_Clear();

            FieldClass sourceField = new FieldClass(Grid.GetColumn((_playerSelectedPuppet.Parent) as Border), Grid.GetRow((_playerSelectedPuppet.Parent) as Border));
            switch (_playerSelectedPuppet.Name.Split('_')[1])
            {
                case "pawn":
                    foreach (FieldClass field in PawnMove(sourceField, true))
                    {
                        Border validField = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == field.column && Grid.GetRow(x) == field.row);
                        _playerSelectedPuppet_ValidMovement.Add(validField, validField.Background);
                        validField.Background = _selectedFieldColor;
                    }
                    break;
                case "rook":
                    foreach (FieldClass field in RookMove(sourceField, true))
                    {
                        Border validField = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == field.column && Grid.GetRow(x) == field.row);
                        _playerSelectedPuppet_ValidMovement.Add(validField, validField.Background);
                        validField.Background = _selectedFieldColor;
                    }
                    break;
                case "knight":
                    foreach (FieldClass field in KnightMove(sourceField, true))
                    {
                        Border validField = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == field.column && Grid.GetRow(x) == field.row);
                        _playerSelectedPuppet_ValidMovement.Add(validField, validField.Background);
                        validField.Background = _selectedFieldColor;
                    }
                    break;
                case "bishop":
                    foreach (FieldClass field in BishopMove(sourceField, true))
                    {
                        Border validField = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == field.column && Grid.GetRow(x) == field.row);
                        _playerSelectedPuppet_ValidMovement.Add(validField, validField.Background);
                        validField.Background = _selectedFieldColor;
                    }
                    break;
                case "queen":
                    foreach (FieldClass field in QueenMove(sourceField, true))
                    {
                        Border validField = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == field.column && Grid.GetRow(x) == field.row);
                        _playerSelectedPuppet_ValidMovement.Add(validField, validField.Background);
                        validField.Background = _selectedFieldColor;
                    }
                    break;
                case "king":
                    foreach (FieldClass field in KingMove(sourceField, true))
                    {
                        Border validField = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == field.column && Grid.GetRow(x) == field.row);
                        _playerSelectedPuppet_ValidMovement.Add(validField, validField.Background);
                        validField.Background = _selectedFieldColor;
                    }
                    break;
            }
        }

        private void PlayerSelectedPuppet_ValidMovement_Clear()
        {
            foreach (KeyValuePair<Border, Brush> item in _playerSelectedPuppet_ValidMovement)
            {
                item.Key.Background = item.Value;
            }
            _playerSelectedPuppet_ValidMovement.Clear();
        }

        private List<FieldClass> PawnMove(FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            if (player && sourceField.row != 0)
            {
                Border upMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row - 1);
                if (upMovement.Child == null)
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row - 1));

                Border upMovement2 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row - 2);
                if (sourceField.row == 6 && upMovement.Child == null && upMovement2.Child == null)
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row - 2));

                Border striking1 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row - 1);
                if (striking1 != null && striking1.Child != null && (striking1.Child as Image).Name.Split('_')[0] == _aIColor.ToString())
                    allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row - 1));

                Border striking2 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row - 1);
                if (striking2 != null && striking2.Child != null && (striking2.Child as Image).Name.Split('_')[0] == _aIColor.ToString())
                    allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row - 1));
            }
            else if (!player && sourceField.row != 7)
            {
                Border upMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row + 1);
                if (upMovement.Child == null)
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row + 1));

                Border upMovement2 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row + 2);
                if (sourceField.row == 1 && upMovement.Child == null && upMovement2.Child == null)
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row + 2));

                Border striking1 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row + 1);
                if (striking1 != null && striking1.Child != null && (striking1.Child as Image).Name.Split('_')[0] == _playerColor.ToString())
                    allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row + 1));

                Border striking2 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row + 1);
                if (striking2 != null && striking2.Child != null && (striking2.Child as Image).Name.Split('_')[0] == _playerColor.ToString())
                    allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row + 1));
            }

            return allowed_fields;
        }

        private List<FieldClass> AIPawnMove(Dictionary<FieldClass, string> playTable, FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            if (player && sourceField.row != 0)
            {
                KeyValuePair<FieldClass, string> upMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row - 1);
                if (upMovement.Value == Puppets.NaN.ToString())
                    allowed_fields.Add(new FieldClass(upMovement.Key));

                KeyValuePair<FieldClass, string> upMovement2 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row - 2);
                if (sourceField.row == 6 && upMovement.Value == Puppets.NaN.ToString() && upMovement2.Value == Puppets.NaN.ToString())
                    allowed_fields.Add(new FieldClass(upMovement2.Key));

                KeyValuePair<FieldClass, string> striking1 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row - 1);
                if (!striking1.Equals(default(KeyValuePair<FieldClass, string>)) && striking1.Value != Puppets.NaN.ToString() && striking1.Value.Split('_')[0] == _aIColor.ToString())
                    allowed_fields.Add(new FieldClass(striking1.Key));

                KeyValuePair<FieldClass, string> striking2 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row - 1);
                if (!striking2.Equals(default(KeyValuePair<FieldClass, string>)) && striking2.Value != Puppets.NaN.ToString() && striking2.Value.Split('_')[0] == _aIColor.ToString())
                    allowed_fields.Add(new FieldClass(striking2.Key));
            }
            else if (!player && sourceField.row != 7)
            {
                KeyValuePair<FieldClass, string> upMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row + 1);
                if (upMovement.Value == Puppets.NaN.ToString())
                    allowed_fields.Add(new FieldClass(upMovement.Key));

                KeyValuePair<FieldClass, string> upMovement2 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row + 2);
                if (sourceField.row == 1 && upMovement.Value == Puppets.NaN.ToString() && upMovement2.Value == Puppets.NaN.ToString())
                    allowed_fields.Add(new FieldClass(upMovement2.Key));

                KeyValuePair<FieldClass, string> striking1 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row + 1);
                if (!striking1.Equals(default(KeyValuePair<FieldClass, string>)) && striking1.Value != Puppets.NaN.ToString() && striking1.Value.Split('_')[0] == _playerColor.ToString())
                    allowed_fields.Add(new FieldClass(striking1.Key));

                KeyValuePair<FieldClass, string> striking2 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row + 1);
                if (!striking2.Equals(default(KeyValuePair<FieldClass, string>)) && striking2.Value != Puppets.NaN.ToString() && striking2.Value.Split('_')[0] == _playerColor.ToString())
                    allowed_fields.Add(new FieldClass(striking2.Key));
            }

            return allowed_fields;
        }

        private List<FieldClass> KnightMove(FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            Border movement1 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 2 && Grid.GetRow(x) == sourceField.row - 1);
            if (movement1 != null && (movement1.Child == null || (movement1.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 2, sourceField.row - 1));

            Border movement2 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 2 && Grid.GetRow(x) == sourceField.row + 1);
            if (movement2 != null && (movement2.Child == null || (movement2.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 2, sourceField.row + 1));

            Border movement3 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 2 && Grid.GetRow(x) == sourceField.row - 1);
            if (movement3 != null && (movement3.Child == null || (movement3.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 2, sourceField.row - 1));

            Border movement4 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 2 && Grid.GetRow(x) == sourceField.row + 1);
            if (movement4 != null && (movement4.Child == null || (movement4.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 2, sourceField.row + 1));

            Border movement5 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row - 2);
            if (movement5 != null && (movement5.Child == null || (movement5.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row - 2));

            Border movement6 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row + 2);
            if (movement6 != null && (movement6.Child == null || (movement6.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row + 2));

            Border movement7 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row - 2);
            if (movement7 != null && (movement7.Child == null || (movement7.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row - 2));

            Border movement8 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row + 2);
            if (movement8 != null && (movement8.Child == null || (movement8.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row + 2));

            return allowed_fields;
        }

        private List<FieldClass> AIKnightMove(Dictionary<FieldClass, string> playTable, FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            KeyValuePair<FieldClass, string> movement1 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 2 && x.Key.row == sourceField.row - 1);
            if (!movement1.Equals(default(KeyValuePair<FieldClass, string>)) && (movement1.Value == Puppets.NaN.ToString() || movement1.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement1.Key));

            KeyValuePair<FieldClass, string> movement2 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 2 && x.Key.row == sourceField.row + 1);
            if (!movement2.Equals(default(KeyValuePair<FieldClass, string>)) && (movement2.Value == Puppets.NaN.ToString() || movement2.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement2.Key));

            KeyValuePair<FieldClass, string> movement3 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 2 && x.Key.row == sourceField.row - 1);
            if (!movement3.Equals(default(KeyValuePair<FieldClass, string>)) && (movement3.Value == Puppets.NaN.ToString() || movement3.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement3.Key));

            KeyValuePair<FieldClass, string> movement4 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 2 && x.Key.row == sourceField.row + 1);
            if (!movement4.Equals(default(KeyValuePair<FieldClass, string>)) && (movement4.Value == Puppets.NaN.ToString() || movement4.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement4.Key));

            KeyValuePair<FieldClass, string> movement5 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row - 2);
            if (!movement5.Equals(default(KeyValuePair<FieldClass, string>)) && (movement5.Value == Puppets.NaN.ToString() || movement5.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement5.Key));

            KeyValuePair<FieldClass, string> movement6 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row + 2);
            if (!movement6.Equals(default(KeyValuePair<FieldClass, string>)) && (movement6.Value == Puppets.NaN.ToString() || movement6.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement6.Key));

            KeyValuePair<FieldClass, string> movement7 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row - 2);
            if (!movement7.Equals(default(KeyValuePair<FieldClass, string>)) && (movement7.Value == Puppets.NaN.ToString() || movement7.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement7.Key));

            KeyValuePair<FieldClass, string> movement8 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row + 2);
            if (!movement8.Equals(default(KeyValuePair<FieldClass, string>)) && (movement8.Value == Puppets.NaN.ToString() || movement8.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement8.Key));

            return allowed_fields;
        }

        private List<FieldClass> RookMove(FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            for (int i = 1; i < 8; i++)
            {
                Border upMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row - i);
                if (upMovement != null && (upMovement.Child == null || (upMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row - i));

                    if (upMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border downMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row + i);
                if (downMovement != null && (downMovement.Child == null || (downMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row + i));

                    if (downMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border leftMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - i && Grid.GetRow(x) == sourceField.row);
                if (leftMovement != null && (leftMovement.Child == null || (leftMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column - i, sourceField.row));

                    if (leftMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border rightMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + i && Grid.GetRow(x) == sourceField.row);
                if (rightMovement != null && (rightMovement.Child == null || (rightMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column + i, sourceField.row));

                    if (rightMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            return allowed_fields;
        }

        private List<FieldClass> AIRookMove(Dictionary<FieldClass, string> playTable, FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> upMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row - i);
                if (!upMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (upMovement.Value == Puppets.NaN.ToString() || upMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(upMovement.Key));

                    if (upMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> downMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row + i);
                if (!downMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (downMovement.Value == Puppets.NaN.ToString() || downMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(downMovement.Key));

                    if (downMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> leftMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - i && x.Key.row == sourceField.row);
                if (!leftMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (leftMovement.Value == Puppets.NaN.ToString() || leftMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(leftMovement.Key));

                    if (leftMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> rightMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + i && x.Key.row == sourceField.row);
                if (!rightMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (rightMovement.Value == Puppets.NaN.ToString() || rightMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(rightMovement.Key));

                    if (rightMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            return allowed_fields;
        }

        private List<FieldClass> BishopMove(FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            for (int i = 1; i < 8; i++)
            {
                Border upleftMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - i && Grid.GetRow(x) == sourceField.row - i);
                if (upleftMovement != null && (upleftMovement.Child == null || (upleftMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column - i, sourceField.row - i));

                    if (upleftMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border uprightMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + i && Grid.GetRow(x) == sourceField.row - i);
                if (uprightMovement != null && (uprightMovement.Child == null || (uprightMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column + i, sourceField.row - i));

                    if (uprightMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border downleftMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - i && Grid.GetRow(x) == sourceField.row + i);
                if (downleftMovement != null && (downleftMovement.Child == null || (downleftMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column - i, sourceField.row + i));

                    if (downleftMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border downrightMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + i && Grid.GetRow(x) == sourceField.row + i);
                if (downrightMovement != null && (downrightMovement.Child == null || (downrightMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column + i, sourceField.row + i));

                    if (downrightMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            return allowed_fields;
        }

        private List<FieldClass> AIBishopMove(Dictionary<FieldClass, string> playTable, FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> upleftMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - i && x.Key.row == sourceField.row - i);
                if (!upleftMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (upleftMovement.Value == Puppets.NaN.ToString() || upleftMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(upleftMovement.Key));

                    if (upleftMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> uprightMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + i && x.Key.row == sourceField.row - i);
                if (!uprightMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (uprightMovement.Value == Puppets.NaN.ToString() || uprightMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(uprightMovement.Key));

                    if (uprightMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> downleftMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - i && x.Key.row == sourceField.row + i);
                if (!downleftMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (downleftMovement.Value == Puppets.NaN.ToString() || downleftMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(downleftMovement.Key));

                    if (downleftMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                KeyValuePair<FieldClass, string> downrightMovement = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + i && x.Key.row == sourceField.row + i);
                if (!downrightMovement.Equals(default(KeyValuePair<FieldClass, string>)) && (downrightMovement.Value == Puppets.NaN.ToString() || downrightMovement.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(downrightMovement.Key));

                    if (downrightMovement.Value != Puppets.NaN.ToString())
                        break;
                }
                else
                    break;
            }

            return allowed_fields;
        }

        private List<FieldClass> QueenMove(FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            for (int i = 1; i < 8; i++)
            {
                Border upMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row - i);
                if (upMovement != null && (upMovement.Child == null || (upMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row - i));

                    if (upMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border downMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row + i);
                if (downMovement != null && (downMovement.Child == null || (downMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row + i));

                    if (downMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border leftMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - i && Grid.GetRow(x) == sourceField.row);
                if (leftMovement != null && (leftMovement.Child == null || (leftMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column - i, sourceField.row));

                    if (leftMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border rightMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + i && Grid.GetRow(x) == sourceField.row);
                if (rightMovement != null && (rightMovement.Child == null || (rightMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column + i, sourceField.row));

                    if (rightMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border upleftMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - i && Grid.GetRow(x) == sourceField.row - i);
                if (upleftMovement != null && (upleftMovement.Child == null || (upleftMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column - i, sourceField.row - i));

                    if (upleftMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border uprightMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + i && Grid.GetRow(x) == sourceField.row - i);
                if (uprightMovement != null && (uprightMovement.Child == null || (uprightMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column + i, sourceField.row - i));

                    if (uprightMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border downleftMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - i && Grid.GetRow(x) == sourceField.row + i);
                if (downleftMovement != null && (downleftMovement.Child == null || (downleftMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column - i, sourceField.row + i));

                    if (downleftMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            for (int i = 1; i < 8; i++)
            {
                Border downrightMovement = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + i && Grid.GetRow(x) == sourceField.row + i);
                if (downrightMovement != null && (downrightMovement.Child == null || (downrightMovement.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                {
                    allowed_fields.Add(new FieldClass(sourceField.column + i, sourceField.row + i));

                    if (downrightMovement.Child != null)
                        break;
                }
                else
                    break;
            }

            return allowed_fields;
        }

        private List<FieldClass> AIQueenMove(Dictionary<FieldClass, string> playTable, FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            foreach (FieldClass item in AIBishopMove(playTable, sourceField, player))
                allowed_fields.Add(item);

            foreach (FieldClass item in AIRookMove(playTable, sourceField, player))
                allowed_fields.Add(item);

            return allowed_fields;
        }

        private List<FieldClass> KingMove(FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            Border movement1 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row - 1);
            if (movement1 != null && (movement1.Child == null || (movement1.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row - 1));

            Border movement2 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row);
            if (movement2 != null && (movement2.Child == null || (movement2.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row));

            Border movement3 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column - 1 && Grid.GetRow(x) == sourceField.row + 1);
            if (movement3 != null && (movement3.Child == null || (movement3.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column - 1, sourceField.row + 1));

            Border movement4 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row - 1);
            if (movement4 != null && (movement4.Child == null || (movement4.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row - 1));

            Border movement5 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row);
            if (movement5 != null && (movement5.Child == null || (movement5.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row));

            Border movement6 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column + 1 && Grid.GetRow(x) == sourceField.row + 1);
            if (movement6 != null && (movement6.Child == null || (movement6.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column + 1, sourceField.row + 1));

            Border movement7 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row - 1);
            if (movement7 != null && (movement7.Child == null || (movement7.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row - 1));

            Border movement8 = PlayTable.Children.Cast<Border>().FirstOrDefault(x => Grid.GetColumn(x) == sourceField.column && Grid.GetRow(x) == sourceField.row + 1);
            if (movement8 != null && (movement8.Child == null || (movement8.Child as Image).Name.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(sourceField.column, sourceField.row + 1));

            return allowed_fields;
        }

        private List<FieldClass> AIKingMove(Dictionary<FieldClass, string> playTable, FieldClass sourceField, bool player)
        {
            List<FieldClass> allowed_fields = new List<FieldClass>();

            KeyValuePair<FieldClass, string> movement1 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row - 1);
            if (!movement1.Equals(default(KeyValuePair<FieldClass, string>)) && (movement1.Value == Puppets.NaN.ToString() || movement1.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement1.Key));

            KeyValuePair<FieldClass, string> movement2 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row);
            if (!movement2.Equals(default(KeyValuePair<FieldClass, string>)) && (movement2.Value == Puppets.NaN.ToString() || movement2.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement2.Key));

            KeyValuePair<FieldClass, string> movement3 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column - 1 && x.Key.row == sourceField.row + 1);
            if (!movement3.Equals(default(KeyValuePair<FieldClass, string>)) && (movement3.Value == Puppets.NaN.ToString() || movement3.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement3.Key));

            KeyValuePair<FieldClass, string> movement4 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row - 1);
            if (!movement4.Equals(default(KeyValuePair<FieldClass, string>)) && (movement4.Value == Puppets.NaN.ToString() || movement4.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement4.Key));

            KeyValuePair<FieldClass, string> movement5 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row);
            if (!movement5.Equals(default(KeyValuePair<FieldClass, string>)) && (movement5.Value == Puppets.NaN.ToString() || movement5.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement5.Key));

            KeyValuePair<FieldClass, string> movement6 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column + 1 && x.Key.row == sourceField.row + 1);
            if (!movement6.Equals(default(KeyValuePair<FieldClass, string>)) && (movement6.Value == Puppets.NaN.ToString() || movement6.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement6.Key));

            KeyValuePair<FieldClass, string> movement7 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row - 1);
            if (!movement7.Equals(default(KeyValuePair<FieldClass, string>)) && (movement7.Value == Puppets.NaN.ToString() || movement7.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement7.Key));

            KeyValuePair<FieldClass, string> movement8 = playTable.FirstOrDefault(x => x.Key.column == sourceField.column && x.Key.row == sourceField.row + 1);
            if (!movement8.Equals(default(KeyValuePair<FieldClass, string>)) && (movement8.Value == Puppets.NaN.ToString() || movement8.Value.Split('_')[0] == (player ? _aIColor.ToString() : _playerColor.ToString())))
                allowed_fields.Add(new FieldClass(movement8.Key));

            return allowed_fields;
        }

        private void AIMove_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            if (e.ProgressPercentage != 100)
            {
                _aiMoveComplete = false;
                progressStackPanel.Visibility = Visibility.Visible;
            }
            else
            {
                _aiMoveComplete = true;
                progressStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AIMove(object sender, DoWorkEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                AIMovement_ColoredField_Clear();
            });

            List<AIMoveClass> aiMoves = new List<AIMoveClass>();
            List<string> PlayerGraveyard = new List<string>(), AIGraveyard = new List<string>();
            Dictionary<FieldClass, string> AIPlayTable = GetPlayTableToAI(ref PlayerGraveyard, ref AIGraveyard);

            _worker.ReportProgress(0);

            for(int i = 0; i < _searchDepth; i++)
            {
                if(i == 0)
                {
                    aiMoves = new List<AIMoveClass>(CalcAIMoves(AIPlayTable, PlayerGraveyard, AIGraveyard, null, null, null, 1));

                    if(aiMoves.Count == 0)
                    {
                        break;
                    }
                }
                else
                {
                    _worker.ReportProgress(i * (100 / _searchDepth));

                    List<AIMoveClass> aiMoves1 = new List<AIMoveClass>(aiMoves.GetRange(0 * (aiMoves.Count / 4), aiMoves.Count / 4));
                    List<AIMoveClass> aiMoves2 = new List<AIMoveClass>(aiMoves.GetRange(1 * (aiMoves.Count / 4), aiMoves.Count / 4));
                    List<AIMoveClass> aiMoves3 = new List<AIMoveClass>(aiMoves.GetRange(2 * (aiMoves.Count / 4), aiMoves.Count / 4));
                    List<AIMoveClass> aiMoves4 = new List<AIMoveClass>(aiMoves.GetRange(3 * (aiMoves.Count / 4), aiMoves.Count - (aiMoves1.Count + aiMoves2.Count + aiMoves3.Count)));

                    Task task1 = SubAIMove(ref aiMoves1, i);
                    Task task2 = SubAIMove(ref aiMoves2, i);
                    Task task3 = SubAIMove(ref aiMoves3, i);
                    Task task4 = SubAIMove(ref aiMoves4, i);

                    Task.WaitAll(task1, task2, task3, task4);

                    aiMoves.Clear();
                    aiMoves.AddRange(aiMoves1);
                    aiMoves.AddRange(aiMoves2);
                    aiMoves.AddRange(aiMoves3);
                    aiMoves.AddRange(aiMoves4);
                }
            }

            _worker.ReportProgress(100);

            if (aiMoves.Count == 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    _playerWin = true;
                    this.Close();
                });
            }
            else
            {
                PerformAIsStep(EvaluationAIMoves(aiMoves));
            }

            this.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = _baseMouseCursor;
            });
        }

        private Dictionary<FieldClass, string> GetPlayTableToAI(ref List<string> playerGraveyard, ref List<string> aiGraveyard)
        {
            Dictionary<FieldClass, string> return_value = new Dictionary<FieldClass, string>();
            List<string> PlayerGraveyard = new List<string>();
            List<string> AIGraveyard = new List<string>();

            this.Dispatcher.Invoke(() =>
            {
                foreach (Border field in PlayTable.Children)
                {
                    if (field.Child is null)
                        return_value.Add(new FieldClass(Grid.GetColumn(field), Grid.GetRow(field)), Puppets.NaN.ToString());
                    else
                        return_value.Add(new FieldClass(Grid.GetColumn(field), Grid.GetRow(field)), (field.Child as Image).Name);
                }
                
                if(Player_graveyard1.Children.Count != 0)
                {
                    foreach(Image puppet in Player_graveyard1.Children)
                    {
                        if (puppet.Name.Split('_')[1] != Puppets.pawn.ToString())
                            PlayerGraveyard.Add(puppet.Name);
                    }
                }
                if (Player_graveyard2.Children.Count != 0)
                {
                    foreach (Image puppet in Player_graveyard2.Children)
                    {
                        if (puppet.Name.Split('_')[1] != Puppets.pawn.ToString())
                            PlayerGraveyard.Add(puppet.Name);
                    }
                }
                if (AI_graveyard1.Children.Count != 0)
                {
                    foreach (Image puppet in AI_graveyard1.Children)
                    {
                        if (puppet.Name.Split('_')[1] != Puppets.pawn.ToString())
                            AIGraveyard.Add(puppet.Name);
                    }
                }
                if (AI_graveyard2.Children.Count != 0)
                {
                    foreach (Image puppet in AI_graveyard2.Children)
                    {
                        if (puppet.Name.Split('_')[1] != Puppets.pawn.ToString())
                            AIGraveyard.Add(puppet.Name);
                    }
                }
            });

            playerGraveyard = new List<string>(PlayerGraveyard);
            aiGraveyard = new List<string>(AIGraveyard);
            return return_value;
        }

        private Task SubAIMove(ref List<AIMoveClass> aiMoves, int i)
        {
            if (aiMoves.Count != 0)
            {
                do
                {
                    AIMoveClass selectedMove = aiMoves.OrderBy(x => x.Depth).First();
                    if (selectedMove.Depth != (i + 1))
                    {
                        aiMoves.Remove(selectedMove);

                        List<AIMoveClass> returned_list = CalcAIMoves(selectedMove.PlayTable, selectedMove.PlayerGraveyard, selectedMove.AIGraveyard, selectedMove.OriginalFromStep, selectedMove.OriginalToStep, selectedMove.OriginalPuppet, (i + 1));
                        if (returned_list.Count != 0)
                        {
                            if (i % 2 == 1)
                            {
                                int playerHighestGoodness = returned_list.OrderBy(x => x.PlayerGoodness).Last().PlayerGoodness;
                                aiMoves.AddRange(returned_list.Where(x => x.PlayerGoodness == playerHighestGoodness).ToList());
                            }
                            else
                            {
                                int aiHighestGoodness = returned_list.OrderBy(x => x.AIGoodness).Last().AIGoodness;
                                aiMoves.AddRange(returned_list.Where(x => x.AIGoodness == aiHighestGoodness).ToList()); ;
                            }
                        }
                    }
                } while (aiMoves.Where(x => x.Depth != (i + 1)).Count() != 0);
            }

            return Task.CompletedTask;
        }

        private List<AIMoveClass> CalcAIMoves(Dictionary<FieldClass, string> AIPlayTable, List<string> PlayerGraveyard, List<string> AIGraveyard, FieldClass OriginalFromStep, FieldClass OriginalToStep, string OriginalPuppet, int depth)
        {
            List<AIMoveClass> aiMoves = new List<AIMoveClass>();

            foreach (KeyValuePair<FieldClass, string> item in AIPlayTable)
            {
                if (item.Value == Puppets.NaN.ToString())
                    continue;
                if (item.Value.Split('_')[0] == (depth % 2 == 0 ? _aIColor.ToString() : _playerColor.ToString()))
                    continue;

                List<FieldClass> allowed_movement = new List<FieldClass>();
                switch ((Puppets)Enum.Parse(typeof(Puppets), item.Value.Split('_')[1]))
                {
                    case Puppets.pawn:
                        allowed_movement = AIPawnMove(AIPlayTable, item.Key, (depth % 2 == 0));
                        break;
                    case Puppets.rook:
                        allowed_movement = AIRookMove(AIPlayTable, item.Key, (depth % 2 == 0));
                        break;
                    case Puppets.knight:
                        allowed_movement = AIKnightMove(AIPlayTable, item.Key, (depth % 2 == 0));
                        break;
                    case Puppets.bishop:
                        allowed_movement = AIBishopMove(AIPlayTable, item.Key, (depth % 2 == 0));
                        break;
                    case Puppets.queen:
                        allowed_movement = AIQueenMove(AIPlayTable, item.Key, (depth % 2 == 0));
                        break;
                    case Puppets.king:
                        allowed_movement = AIKingMove(AIPlayTable, item.Key, (depth % 2 == 0));
                        break;
                }

                foreach (FieldClass movement in allowed_movement)
                {
                    string puppet_name = AIPlayTable[item.Key];
                    Dictionary<FieldClass, string> modifiedAIPlayTable = new Dictionary<FieldClass, string>(AIPlayTable);
                    modifiedAIPlayTable[movement] = AIPlayTable[item.Key];
                    modifiedAIPlayTable[item.Key] = Puppets.NaN.ToString();

                    List<string> playerGraveyard = new List<string>(PlayerGraveyard);
                    List<string> aiGraveyard = new List<string>(AIGraveyard);
                    if(depth % 2 == 0)
                    {
                        if (modifiedAIPlayTable[movement].Split('_')[1] == Puppets.pawn.ToString() && movement.row == 0 && playerGraveyard.Count != 0)
                        {
                            string queenPuppet = playerGraveyard.Where(x => x.Split('_')[1] == Puppets.queen.ToString()).FirstOrDefault();
                            if (queenPuppet != null)
                            {
                                playerGraveyard.Remove(queenPuppet);
                                puppet_name = queenPuppet;
                            }
                            else
                            {
                                List<string> bishopANDrookPuppets = new List<string>(playerGraveyard.Where(x => x.Split('_')[1] == Puppets.bishop.ToString() || x.Split('_')[1] == Puppets.rook.ToString()).ToList());
                                if (bishopANDrookPuppets.Count != 0)
                                {
                                    string selectedPuppet = bishopANDrookPuppets[_rnd.Next(0, bishopANDrookPuppets.Count)];
                                    playerGraveyard.Remove(selectedPuppet);
                                    puppet_name = selectedPuppet;
                                }
                                else
                                {
                                    List<string> knightPuppets = new List<string>(playerGraveyard.Where(x => x.Split('_')[1] == Puppets.knight.ToString()).ToList());

                                    playerGraveyard.Remove(knightPuppets[0]);
                                    puppet_name = knightPuppets[0];
                                }
                            }
                        }
                    }
                    else
                    {
                        if(modifiedAIPlayTable[movement].Split('_')[1] == Puppets.pawn.ToString() &&  movement.row == 7 && aiGraveyard.Count != 0)
                        {
                            string queenPuppet = aiGraveyard.Where(x => x.Split('_')[1] == Puppets.queen.ToString()).FirstOrDefault();
                            if (queenPuppet != null)
                            {
                                aiGraveyard.Remove(queenPuppet);
                                puppet_name = queenPuppet;
                            }
                            else
                            {
                                List<string> bishopANDrookPuppets = new List<string>(aiGraveyard.Where(x => x.Split('_')[1] == Puppets.bishop.ToString() || x.Split('_')[1] == Puppets.rook.ToString()).ToList());
                                if (bishopANDrookPuppets.Count != 0)
                                {
                                    string selectedPuppet = bishopANDrookPuppets[_rnd.Next(0, bishopANDrookPuppets.Count)];
                                    aiGraveyard.Remove(selectedPuppet);
                                    puppet_name = selectedPuppet;
                                }
                                else
                                {
                                    List<string> knightPuppets = new List<string>(aiGraveyard.Where(x => x.Split('_')[1] == Puppets.knight.ToString()).ToList());

                                    aiGraveyard.Remove(knightPuppets[0]);
                                    puppet_name = knightPuppets[0];
                                }
                            }
                        }
                    }

                    if (puppet_name != AIPlayTable[item.Key])
                    {
                        modifiedAIPlayTable[movement] = puppet_name;
                    }

                    if(AIPlayTable[movement] != Puppets.NaN.ToString() && AIPlayTable[movement] != Puppets.pawn.ToString())
                    {
                        string strikePuppet = AIPlayTable[movement];

                        if(strikePuppet.Split('_')[0] == _playerColor.ToString())
                        {
                            playerGraveyard.Add(strikePuppet);
                        }
                        else
                        {
                            aiGraveyard.Add(strikePuppet);
                        }
                    }

                    FieldClass aiKing = modifiedAIPlayTable.First(x => x.Value == $"{_aIColor}_{Puppets.king}_{0}").Key;
                    FieldClass playerKing = modifiedAIPlayTable.First(x => x.Value == $"{_playerColor}_{Puppets.king}_{0}").Key;

                    if (depth % 2 == 0)
                    {
                        aiMoves.Add(new AIMoveClass(OriginalFromStep, OriginalToStep, OriginalPuppet, _aIColor, _playerColor, depth, modifiedAIPlayTable, playerGraveyard, aiGraveyard, AIKingInChess(modifiedAIPlayTable, aiKing, false)));
                    }
                    else
                    {
                        if (!AIKingInChess(modifiedAIPlayTable, aiKing, false))
                        {
                            if (depth == 1)
                            {
                                aiMoves.Add(new AIMoveClass(item.Key, movement, puppet_name, _aIColor, _playerColor, depth, modifiedAIPlayTable, playerGraveyard, aiGraveyard, AIKingInChess(modifiedAIPlayTable, playerKing, true)));
                            }
                            else
                            {
                                aiMoves.Add(new AIMoveClass(OriginalFromStep, OriginalToStep, OriginalPuppet, _aIColor, _playerColor, depth, modifiedAIPlayTable, playerGraveyard, aiGraveyard, AIKingInChess(modifiedAIPlayTable, playerKing, true)));
                            }
                        }
                    }
                }
            }

            return aiMoves;
        }

        private AIMoveClass EvaluationAIMoves(List<AIMoveClass> aiMoves)
        {
            AIMoveClass bestAIMove = new AIMoveClass(aiMoves.OrderBy(x => x.PlayerGoodness).ThenByDescending(x => x.AIGoodness).First());
            List<AIMoveClass> bestAIMoves = new List<AIMoveClass>(aiMoves.Where(x => x.PlayerGoodness == bestAIMove.PlayerGoodness && x.AIGoodness == bestAIMove.AIGoodness));
            AIMoveClass selectedMove = bestAIMoves[_rnd.Next(0, bestAIMoves.Count)];

            return selectedMove;
        }

        private void PerformAIsStep(AIMoveClass selectedMove)
        {
            this.Dispatcher.Invoke(() =>
            {
                IncAndShowStepCount(Players.AI);

                Border FromBorder = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == selectedMove.OriginalFromStep.column && Grid.GetRow(x) == selectedMove.OriginalFromStep.row);
                Border ToBorder = PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == selectedMove.OriginalToStep.column && Grid.GetRow(x) == selectedMove.OriginalToStep.row);
                Image puppet = FromBorder.Child as Image;

                FromBorder.Child = null;
                if (ToBorder.Child != null)
                    PuppetMoveInTheGraveyard(ToBorder.Child as Image, false);
                if (puppet.Name == selectedMove.OriginalPuppet)
                {
                    ToBorder.Child = puppet;
                }
                else if (puppet.Name.Split('_')[1] == Puppets.pawn.ToString())
                {

                    Image revivePuppet = FindResource(selectedMove.OriginalPuppet) as Image;
                    StackPanel graveyard = revivePuppet.Parent as StackPanel;
                    graveyard.Children.Remove(revivePuppet);
                    graveyard.Children.Add(puppet);
                    ToBorder.Child = revivePuppet;
                }

                AIMovement_ColoredField_Add(FromBorder);
                AIMovement_ColoredField_Add(ToBorder);
            });
        }

        private void AIMovement_ColoredField_Clear()
        {
            foreach (KeyValuePair<Border, Brush> item in _aiMovement_ColoredField)
                item.Key.Background = item.Value;

            _aiMovement_ColoredField.Clear();
        }

        private void AIMovement_ColoredField_Add(Border border)
        {
            _aiMovement_ColoredField.Add(border, border.Background);
            border.Background = _aiMovementFieldColor;
        }

        private void PuppetMoveInTheGraveyard(Image image, bool player)
        {
            if (image.Parent != null)
                (image.Parent as Border).Child = null;

            if (player)
            {
                if (AI_graveyard1.Children.Count == 8)
                {
                    AI_graveyard2.Children.Add(image);
                }
                else
                {
                    AI_graveyard1.Children.Add(image);
                }
            }
            else
            {
                if (Player_graveyard1.Children.Count == 8)
                {
                    Player_graveyard2.Children.Add(image);
                }
                else
                {
                    Player_graveyard1.Children.Add(image);
                }
            }
        }

        private bool PuppetInTheGraveyard(Image image)
        {
            StackPanel parent = image.Parent as StackPanel;
            if (parent is null)
                return false;

            switch (parent.Name)
            {
                case "AI_graveyard1":
                case "AI_graveyard2":
                case "Player_graveyard1":
                case "Player_graveyard2":
                    return true;
            }

            return false;
        }

        private void ResurrectionFromTheCemetery(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Image).Name.Split('_')[0] != _playerColor.ToString())
                return;
            if (!PuppetInTheGraveyard(sender as Image))
                return;
            if (!_resurrectionFromTheCemetery)
                return;

            Border toBorder = _playerSelectedPuppet.Parent as Border;
            StackPanel fromStackPanel = (sender as Image).Parent as StackPanel;

            if (Grid.GetRow(toBorder) != 0)
                return;

            toBorder.Child = null;
            fromStackPanel.Children.Remove(sender as Image);

            toBorder.Child = sender as Image;
            fromStackPanel.Children.Add(_playerSelectedPuppet);

            _resurrectionFromTheCemetery = false;
        }

        private bool KingInChess(Border king, bool player)
        {
            if (player)
            {
                FieldClass kingposition = new FieldClass(Grid.GetColumn(king), Grid.GetRow(king));

                foreach (Border field in PlayTable.Children)
                {
                    if (field.Child == null)
                        continue;
                    if ((field.Child as Image).Name.Split('_')[0] == _playerColor.ToString())
                        continue;

                    FieldClass puppetsPosition = new FieldClass(Grid.GetColumn(field), Grid.GetRow(field));
                    switch ((Puppets)Enum.Parse(typeof(Puppets), (field.Child as Image).Name.Split('_')[1]))
                    {
                        case Puppets.pawn:
                            foreach (FieldClass position in PawnMove(puppetsPosition, false))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.rook:
                            foreach (FieldClass position in RookMove(puppetsPosition, false))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.bishop:
                            foreach (FieldClass position in BishopMove(puppetsPosition, false))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.king:
                            foreach (FieldClass position in KingMove(puppetsPosition, false))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.queen:
                            foreach (FieldClass position in QueenMove(puppetsPosition, false))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.knight:
                            foreach (FieldClass position in KnightMove(puppetsPosition, false))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                    }
                }
            }
            else
            {
                FieldClass kingposition = new FieldClass(Grid.GetColumn(king), Grid.GetRow(king));

                foreach (Border field in PlayTable.Children)
                {
                    if (field.Child == null)
                        continue;
                    if ((field.Child as Image).Name.Split('_')[0] == _aIColor.ToString())
                        continue;

                    FieldClass puppetsPosition = new FieldClass(Grid.GetColumn(field), Grid.GetRow(field));
                    switch ((Puppets)Enum.Parse(typeof(Puppets), (field.Child as Image).Name.Split('_')[1]))
                    {
                        case Puppets.pawn:
                            foreach (FieldClass position in PawnMove(puppetsPosition, true))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.rook:
                            foreach (FieldClass position in RookMove(puppetsPosition, true))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.bishop:
                            foreach (FieldClass position in BishopMove(puppetsPosition, true))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.king:
                            foreach (FieldClass position in KingMove(puppetsPosition, true))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.queen:
                            foreach (FieldClass position in QueenMove(puppetsPosition, true))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                        case Puppets.knight:
                            foreach (FieldClass position in KnightMove(puppetsPosition, true))
                            {
                                if (position.Equals(kingposition))
                                    return true;
                            }
                            break;
                    }
                }
            }

            return false;
        }

        private bool AIKingInChess(Dictionary<FieldClass, string> playTable, FieldClass king, bool player)
        {
            foreach (KeyValuePair<FieldClass, string> field in playTable)
            {
                if (field.Value == Puppets.NaN.ToString())
                    continue;
                if (field.Value.Split('_')[0] == (player ? _playerColor.ToString() : _aIColor.ToString()))
                    continue;

                switch ((Puppets)Enum.Parse(typeof(Puppets), field.Value.Split('_')[1]))
                {
                    case Puppets.pawn:
                        foreach (FieldClass position in AIPawnMove(playTable, field.Key, !player))
                        {
                            if (position.Equals(king))
                                return true;
                        }
                        break;
                    case Puppets.rook:
                        foreach (FieldClass position in AIRookMove(playTable, field.Key, !player))
                        {
                            if (position.Equals(king))
                                return true;
                        }
                        break;
                    case Puppets.bishop:
                        foreach (FieldClass position in AIBishopMove(playTable, field.Key, !player))
                        {
                            if (position.Equals(king))
                                return true;
                        }
                        break;
                    case Puppets.king:
                        foreach (FieldClass position in AIKingMove(playTable, field.Key, !player))
                        {
                            if (position.Equals(king))
                                return true;
                        }
                        break;
                    case Puppets.queen:
                        foreach (FieldClass position in AIQueenMove(playTable, field.Key, !player))
                        {
                            if (position.Equals(king))
                                return true;
                        }
                        break;
                    case Puppets.knight:
                        foreach (FieldClass position in AIKnightMove(playTable, field.Key, !player))
                        {
                            if (position.Equals(king))
                                return true;
                        }
                        break;
                }
            }

            return false;
        }

        private void SavePlayTable()
        {
            _playTableSave.Clear();

            foreach (Border field in PlayTable.Children)
            {
                if (field.Child != null)
                    _playTableSave.Add(new FieldClass(Grid.GetColumn(field), Grid.GetRow(field)), field.Child as Image);
            }
        }

        private void LoadPlayTable()
        {
            foreach (Border field in PlayTable.Children)
            {
                field.Child = null;
            }

            foreach (KeyValuePair<FieldClass, Image> item in _playTableSave)
            {
                if ((item.Value.Parent as StackPanel) != null)
                    (item.Value.Parent as StackPanel).Children.Remove(item.Value);

                PlayTable.Children.Cast<Border>().First(x => Grid.GetColumn(x) == item.Key.column && Grid.GetRow(x) == item.Key.row).Child = item.Value;
            }
        }

        private void IncAndShowStepCount(Players inc)
        {
            if (_lastStepCountInc != inc)
            {
                _stepCount++;
                stepcounter_label.Content = _stepCount;
                _lastStepCountInc = inc;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan time = TimeSpan.FromSeconds(_stopWatch.Elapsed.TotalSeconds);
            _timeElapsed = time.ToString(@"hh\:mm\:ss");
            time_label.Content = _timeElapsed;
        }

        private void GiveUp_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_stepCount != 0)
            {
                List<HighScoreListModel> list = new HighScoreListFile().ReadFile().ToList();
                list.Add(new HighScoreListModel(_playerWin ? "Player" : "AI", _difficulty, _timeElapsed, DateTime.Now, _stepCount));
                new HighScoreListFile().WriteFile(list);

                if (_playerWin)
                {
                    MessageBox.Show($"You win in {_stepCount} steps! (Time: {_timeElapsed})", "Win", MessageBoxButton.OK);
                }
                else
                {
                    MessageBox.Show($"AI win in {_stepCount} steps! (Time: {_timeElapsed})", "Game Over", MessageBoxButton.OK);
                }
            }

            Mouse.OverrideCursor = _baseMouseCursor;
            _worker.CancelAsync();

            base.OnClosing(e);
        }
    }
}
