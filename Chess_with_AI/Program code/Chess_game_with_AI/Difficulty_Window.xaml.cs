using System.Windows;

namespace Chess_game_with_AI
{
    public partial class Difficulty_Window : Window
    {
        public Difficulty_Window()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Chess_Window.Difficulties difficulty = Chess_Window.Difficulties.Easy;

            if ((bool)medium_radiobutton.IsChecked)
                difficulty = Chess_Window.Difficulties.Medium;
            else if ((bool)hard_radiobutton.IsChecked)
                difficulty = Chess_Window.Difficulties.Hard;
            else if ((bool)veryhard_radiobutton.IsChecked)
                difficulty = Chess_Window.Difficulties.VeryHard;

            new Chess_Window(difficulty).ShowDialog();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
