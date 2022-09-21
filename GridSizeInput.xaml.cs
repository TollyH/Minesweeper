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
            widthIn.Text = xSize.ToString();
            heightIn.Text = ySize.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(widthIn.Text, out NewX) && int.TryParse(heightIn.Text, out NewY))
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
