using System.Windows;

namespace _2048_game
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SinglePlay_Click(object sender, RoutedEventArgs e)
        {
            new SinglePlayerWindow().ShowDialog();
        }

        private void AIPlay_Click(object sender, RoutedEventArgs e)
        {
            new DifficultyChoosingWindow().ShowDialog();
        }

        private void HighScoreList_Click(object sender, RoutedEventArgs e)
        {
            new HighScoreListWindow().ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
