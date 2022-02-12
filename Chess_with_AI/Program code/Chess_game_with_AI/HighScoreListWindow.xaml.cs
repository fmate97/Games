﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chess_game_with_AI
{
    public partial class HighScoreListWindow : Window
    {
        private List<HighScoreListModel> _highScoreList = new HighScoreListFile().ReadFile().ToList();

        public HighScoreListWindow()
        {
            InitializeComponent();

            ListViewRefresh();
        }

        private void ListViewRefresh()
        {
            listView.Items.Clear();

            _highScoreList = _highScoreList.OrderBy(x => x.GameEndTime).ToList();
            foreach (HighScoreListModel model in _highScoreList)
            {
                ListViewItem item = new ListViewItem();
                item.Content = model;
                switch (model.Winner)
                {
                    case "Player":
                        item.Background = Brushes.Green;
                        break;
                    case "AI":
                        item.Background = Brushes.Red;
                        break;
                    default:
                        item.Background = Brushes.WhiteSmoke;
                        break;
                }
                listView.Items.Add(item);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItem != null)
            {
                HighScoreListModel removedHighScore = (listView.SelectedItem as ListViewItem).Content as HighScoreListModel;
                _highScoreList.Remove(removedHighScore);
                new HighScoreListFile().WriteFile(_highScoreList);
                ListViewRefresh();
            }
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _highScoreList.Clear();
                new HighScoreListFile().WriteFile(_highScoreList);
                ListViewRefresh();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}