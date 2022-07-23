using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _2048_game
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

            _highScoreList = _highScoreList.OrderBy(x => x.Time).ToList();
            foreach(HighScoreListModel highScore in _highScoreList)
            {
                ListViewItem item = new ListViewItem();
                item.Content = highScore;
                switch (highScore.Winner)
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
            HighScoreListModel removedHighScore = (listView.SelectedItem as ListViewItem).Content as HighScoreListModel;
            _highScoreList.Remove(removedHighScore);
            new HighScoreListFile().WriteFile(_highScoreList);
            ListViewRefresh();
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

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show("test");
        }
    }
}
