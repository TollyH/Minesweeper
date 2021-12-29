using System.Windows;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MineCountInput.xaml
    /// </summary>
    public partial class MineCountInput : Window
    {
        private readonly int InitX;
        private readonly int InitY;
        private readonly int InitCount;

        public int NewCount;

        public MineCountInput(int XSize, int YSize, int MineCount)
        {
            InitializeComponent();
            InitX = XSize;
            InitY = YSize;
            InitCount = MineCount;
            CountIn.Text = MineCount.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CountIn.Text, out NewCount))
            {
                if (NewCount >= InitX * InitY || NewCount < 0)
                {
                    MessageBox.Show("Invalid mine count provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    NewCount = InitCount;
                    return;
                }
                Close();
            }
            else
            {
                MessageBox.Show("Invalid mine count provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NewCount = InitCount;
            Close();
        }
    }
}
