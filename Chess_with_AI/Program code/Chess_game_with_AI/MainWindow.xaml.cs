using System.Windows;

namespace Chess_game_with_AI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            new Difficulty_Window().ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Results_Click(object sender, RoutedEventArgs e)
        {
            new HighScoreListWindow().ShowDialog();
        }
    }
}
