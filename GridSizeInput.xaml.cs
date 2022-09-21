using System.Windows;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for GridSizeInput.xaml
    /// </summary>
    public partial class GridSizeInput : Window
    {
        private readonly int initX;
        private readonly int initY;

        public int NewX;
        public int NewY;

        public GridSizeInput(int xSize, int ySize)
        {
            InitializeComponent();
            initX = xSize;
            initY = ySize;
            WidthIn.Text = xSize.ToString();
            HeightIn.Text = ySize.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WidthIn.Text, out NewX) && int.TryParse(HeightIn.Text, out NewY))
            {
                if (NewX < 1 || NewY < 1)
                {
                    _ = MessageBox.Show("Invalid size provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Close();
            }
            else
            {
                _ = MessageBox.Show("Invalid size provided", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NewX = initX;
            NewY = initY;
            Close();
        }
    }
}
