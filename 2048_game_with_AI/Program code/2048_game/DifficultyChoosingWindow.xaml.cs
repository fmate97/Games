using System.Windows;

namespace _2048_game
{
    public partial class DifficultyChoosingWindow : Window
    {
        public DifficultyChoosingWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            AIGameWindow.Difficulty selectedDifficulty = AIGameWindow.Difficulty.NaN;

            if ((bool)easyRadioButton.IsChecked)
                selectedDifficulty = AIGameWindow.Difficulty.Easy;
            else if ((bool)mediumRadioButton.IsChecked)
                selectedDifficulty = AIGameWindow.Difficulty.Medium;
            else if ((bool)hardRadioButton.IsChecked)
                selectedDifficulty = AIGameWindow.Difficulty.Hard;
            else if ((bool)vearyhardRadioButton.IsChecked)
                selectedDifficulty = AIGameWindow.Difficulty.VeryHard;

            new AIGameWindow(selectedDifficulty).Show();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
