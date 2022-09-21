using System.Windows;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MineCountInput.xaml
    /// </summary>
    public partial class MineCountInput : Window
    {
        private readonly int initX;
        private readonly int initY;
        private readonly int initCount;

        public int NewCount;

        public MineCountInput(int xSize, int ySize, int mineCount)
        {
            InitializeComponent();
            initX = xSize;
            initY = ySize;
            initCount = mineCount;
            CountIn.Text = mineCount.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CountIn.Text, out NewCount))
            {
                if (NewCount >= initX * initY || NewCount < 0)
                {
                    _ = MessageBox.Show("Invalid mine count provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    NewCount = initCount;
                    return;
                }
                Close();
            }
            else
            {
                _ = MessageBox.Show("Invalid mine count provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NewCount = initCount;
            Close();
        }
    }
}
