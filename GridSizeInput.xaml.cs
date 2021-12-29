using System.Windows;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for GridSizeInput.xaml
    /// </summary>
    public partial class GridSizeInput : Window
    {
        private readonly int InitX;
        private readonly int InitY;

        public int NewX;
        public int NewY;

        public GridSizeInput(int XSize, int YSize)
        {
            InitializeComponent();
            InitX = XSize;
            InitY = YSize;
            WidthIn.Text = XSize.ToString();
            HeightIn.Text = YSize.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthIn.Text, out NewX) && int.TryParse(HeightIn.Text, out NewY))
            {
                if (NewX < 1 || NewY < 1)
                {
                    MessageBox.Show("Invalid size provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Close();
            }
            else
            {
                MessageBox.Show("Invalid size provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NewX = InitX;
            NewY = InitY;
            Close();
        }
    }
}
