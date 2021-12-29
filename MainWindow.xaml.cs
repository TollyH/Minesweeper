using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class MineButton : Button
        {
            public bool ContainsMine { get; set; }
            public bool IsPlayerFlagged { get; set; }
            public Point FieldPosition { get; set; }
        }

        private int XSize = 9;
        private int YSize = 9;
        private int MineCount = 10;

        private readonly Random RNG = new Random();

        private readonly BitmapImage MineImage = new BitmapImage(new Uri("mine.png", UriKind.Relative));

        private readonly Timer GameTimer = new Timer(1000);
        private int TimeElapsed = 0;

        private readonly Dictionary<int, Brush> AdjacentColours = new Dictionary<int, Brush>()
        {
            { 0, Brushes.Transparent },
            { 1, Brushes.Blue },
            { 2, Brushes.DarkGreen },
            { 3, Brushes.Red },
            { 4, Brushes.DarkOrange },
            { 5, Brushes.Maroon },
            { 6, Brushes.Turquoise },
            { 7, Brushes.Black },
            { 8, Brushes.Gray }
        };

        public MainWindow()
        {
            InitializeComponent();
            GameTimer.Elapsed += GameTimer_Elapsed;
        }

        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeElapsed++;
            Dispatcher.Invoke(() =>
            {
                TimeDisplay.Content = (TimeElapsed / 60).ToString().PadLeft(2, '0') + ":" + (TimeElapsed % 60).ToString().PadLeft(2, '0');
            });
        }

        private void MineField_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateMineField();
        }

        private void PopulateMineField()
        {
            MineField.Children.Clear();
            for (int y = 0; y < YSize; y++)
            {
                for (int x = 0; x < XSize; x++)
                {
                    MineField.Children.Add(new MineButton()
                    {
                        Background = Brushes.LightGray,
                        FieldPosition = new Point(x, y)
                    });
                    ((MineButton)MineField.Children[MineField.Children.Count - 1]).Click += MineButton_Click;
                    ((MineButton)MineField.Children[MineField.Children.Count - 1]).MouseRightButtonDown += MineButton_MouseRightButtonDown;
                }
            }
            ResizeMineField();
            NewGame();
        }

        private void ResizeMineField()
        {
            double TargetHeight = (GameGrid.ActualHeight - (MineField.Margin.Top + MineField.Margin.Bottom)) / YSize;
            double TargetWidth = (GameGrid.ActualWidth - (MineField.Margin.Right + MineField.Margin.Left)) / XSize;
            if (TargetHeight < 0)
            {
                TargetHeight = 0;
            }
            if (TargetWidth < 0)
            {
                TargetWidth = 0;
            }
            foreach (MineButton buttonInField in MineField.Children)
            {
                buttonInField.Height = TargetHeight;
                buttonInField.Width = TargetWidth;
            }
        }

        private void NewGame()
        {
            GameTimer.Stop();
            TimeElapsed = 0;
            TimeDisplay.Content = "00:00";
            MineDisplay.Content = MineCount;
            MineDisplay.Foreground = Brushes.Black;
            MiddleButton.Content = new Image()
            {
                Source = MineImage
            };
            LayMines();
        }

        private void LayMines()
        {
            foreach (MineButton buttonInField in MineField.Children)
            {
                buttonInField.ContainsMine = false;
                buttonInField.IsPlayerFlagged = false;
                buttonInField.IsEnabled = true;
                buttonInField.Content = null;
            }
            for (int i = 0; i < MineCount; i++)
            {
                MineButton tempButton = (MineButton)MineField.Children[RNG.Next(MineField.Children.Count)];
                if (!tempButton.ContainsMine)
                {
                    tempButton.ContainsMine = true;
                }
                else
                {
                    i--;
                }
            }
        }

        private void MineButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((MineButton)sender).IsPlayerFlagged = !((MineButton)sender).IsPlayerFlagged;
            if (((MineButton)sender).IsPlayerFlagged)
            {
                ((MineButton)sender).Content = new Viewbox()
                {
                    Child = new TextBlock()
                    {
                        Text = "🚩",
                    }
                };
            }
            else
            {
                ((MineButton)sender).Content = null;
            }
            UpdateMineDisplay();
        }

        private void MineButton_Click(object sender, RoutedEventArgs e)
        {
            if (MineField.Children.OfType<MineButton>().All(x => x.IsEnabled))
            {
                if (((MineButton)sender).ContainsMine)
                {
                    while (true)
                    {
                        MineButton tempButton = (MineButton)MineField.Children[RNG.Next(MineField.Children.Count)];
                        if (!tempButton.ContainsMine)
                        {
                            tempButton.ContainsMine = true;
                            break;
                        }
                    }
                    ((MineButton)sender).ContainsMine = false;
                }
                GameTimer.Start();
            }
            if (((MineButton)sender).IsPlayerFlagged)
            {
                return;
            }
            ComputeButtonContent((MineButton)sender);
            if (((MineButton)sender).ContainsMine)
            {
                GameLost();
                return;
            }
            CheckForVictory();
        }

        private void GameLost()
        {
            GameTimer.Stop();
            MiddleButton.Content = "❌";
            foreach (MineButton buttonInField in MineField.Children)
            {
                ComputeButtonContent(buttonInField);
            }
        }

        private void ComputeButtonContent(MineButton buttonInField)
        {
            buttonInField.IsEnabled = false;
            if (buttonInField.ContainsMine && !buttonInField.IsPlayerFlagged)
            {
                buttonInField.Content = new Viewbox()
                {
                    Child = new Image()
                    {
                        Source = MineImage,
                    }
                };
            }
            else if (!buttonInField.ContainsMine)
            {
                if (buttonInField.IsPlayerFlagged)
                {
                    buttonInField.Content = new Viewbox()
                    {
                        Child = new TextBlock()
                        {
                            Text = "⛔",
                        }
                    };
                    return;
                }
                int TotalAdjacent = 0;
                for (int ypos = (int)buttonInField.FieldPosition.Y - 1; ypos <= buttonInField.FieldPosition.Y + 1; ypos++)
                {
                    for (int xpos = (int)buttonInField.FieldPosition.X - 1; xpos <= buttonInField.FieldPosition.X + 1; xpos++)
                    {
                        if (xpos < XSize && xpos >= 0 && ypos < YSize && ypos >= 0 && ((MineButton)MineField.Children[ypos * XSize + xpos]).ContainsMine)
                        {
                            TotalAdjacent++;
                        }
                    }
                }
                buttonInField.Content = new Viewbox()
                {
                    Child = new TextBlock()
                    {
                        Text = TotalAdjacent.ToString(),
                        FontWeight = FontWeights.Bold,
                        Foreground = AdjacentColours[TotalAdjacent],
                    }
                };
                if (TotalAdjacent == 0)
                {
                    for (int ypos = (int)buttonInField.FieldPosition.Y - 1; ypos <= buttonInField.FieldPosition.Y + 1; ypos++)
                    {
                        for (int xpos = (int)buttonInField.FieldPosition.X - 1; xpos <= buttonInField.FieldPosition.X + 1; xpos++)
                        {
                            if (xpos < XSize && xpos >= 0 && ypos < YSize && ypos >= 0 && ((MineButton)MineField.Children[ypos * XSize + xpos]).IsEnabled &&
                                !((MineButton)MineField.Children[(ypos * XSize) + xpos]).ContainsMine && !((MineButton)MineField.Children[(ypos * XSize) + xpos]).IsPlayerFlagged)
                            {
                                ComputeButtonContent((MineButton)MineField.Children[ypos * XSize + xpos]);
                            }
                        }
                    }
                }
            }
        }

        private void CheckForVictory()
        {
            if (MineField.Children.OfType<MineButton>().All(x => !x.ContainsMine ^ x.IsEnabled))
            {
                GameTimer.Stop();
                MiddleButton.Content = "🏁";
                foreach (MineButton buttonInField in MineField.Children)
                {
                    if (buttonInField.ContainsMine)
                    {
                        buttonInField.IsEnabled = false;
                        buttonInField.IsPlayerFlagged = true;
                        buttonInField.Content = new Viewbox()
                        {
                            Child = new TextBlock()
                            {
                                Text = "🚩",
                            }
                        };
                    }
                }
            }
        }

        private void UpdateMineDisplay()
        {
            MineDisplay.Content = MineCount - MineField.Children.OfType<MineButton>().Count(x => x.IsPlayerFlagged);
            if ((int)MineDisplay.Content < 0)
            {
                MineDisplay.Foreground = Brushes.Red;
            }
            else
            {
                MineDisplay.Foreground = Brushes.Black;
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void SetGridSize_Click(object sender, RoutedEventArgs e)
        {
            GridSizeInput NewGridSizeWin = new GridSizeInput(XSize, YSize);
            NewGridSizeWin.ShowDialog();
            if (NewGridSizeWin.NewX != XSize || NewGridSizeWin.NewY != YSize)
            {
                XSize = NewGridSizeWin.NewX;
                YSize = NewGridSizeWin.NewY;
                if (MineCount >= XSize * YSize)
                {
                    MineCount = (XSize * YSize) - 1;
                }
                PopulateMineField();
            }
        }

        private void SetMineCount_Click(object sender, RoutedEventArgs e)
        {
            MineCountInput NewMineCountWin = new MineCountInput(XSize, YSize, MineCount);
            NewMineCountWin.ShowDialog();
            if (NewMineCountWin.NewCount != MineCount)
            {
                MineCount = NewMineCountWin.NewCount;
                NewGame();
            }
        }

        private void SquareField_Click(object sender, RoutedEventArgs e)
        {
            if (((MineButton)MineField.Children[0]).ActualHeight == ((MineButton)MineField.Children[0]).ActualWidth)
            {
                return;
            }
            if (((MineButton)MineField.Children[0]).ActualHeight > ((MineButton)MineField.Children[0]).ActualWidth)
            {
                MSWindow.Width += (((MineButton)MineField.Children[0]).ActualHeight - ((MineButton)MineField.Children[0]).ActualWidth) * XSize;
            }
            else if (((MineButton)MineField.Children[0]).ActualWidth > ((MineButton)MineField.Children[0]).ActualHeight)
            {
                MSWindow.Height += (((MineButton)MineField.Children[0]).ActualWidth - ((MineButton)MineField.Children[0]).ActualHeight) * YSize;
            }
            ResizeMineField();
        }

        private void AutoFlagMine_Click(object sender, RoutedEventArgs e)
        {
            MineButton RandomButton;
            try
            {
                RandomButton = MineField.Children.OfType<MineButton>().Where(x => x.ContainsMine && !x.IsPlayerFlagged).ElementAt(
                RNG.Next(MineField.Children.OfType<MineButton>().Count(x => x.ContainsMine && !x.IsPlayerFlagged)));
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            MineButton_MouseRightButtonDown(RandomButton, null);
        }

        private void AutoClearSpace_Click(object sender, RoutedEventArgs e)
        {
            MineButton RandomButton;
            try
            {
                RandomButton = MineField.Children.OfType<MineButton>().Where(x => !x.ContainsMine && !x.IsPlayerFlagged && x.IsEnabled).ElementAt(
                    RNG.Next(MineField.Children.OfType<MineButton>().Count(x => !x.ContainsMine && !x.IsPlayerFlagged && x.IsEnabled)));
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            MineButton_Click(RandomButton, null);
        }

        private async void AutoSolve_Click(object sender, RoutedEventArgs e)
        {
            int Delay = 5000 / (XSize * YSize);
            if (Delay < 1)
            {
                Delay = 1;
            }
            foreach (MineButton buttonInField in MineField.Children)
            {
                buttonInField.BorderBrush = Brushes.Red;
                await Task.Delay(Delay);
                if (buttonInField.ContainsMine)
                {
                    MineButton_MouseRightButtonDown(buttonInField, null);
                }
                else
                {
                    MineButton_Click(buttonInField, null);
                }
                buttonInField.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF707070");
                if (MineField.Children.OfType<MineButton>().All(x => !x.IsEnabled))
                {
                    return;
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            new AboutBox().ShowDialog();
        }

        private void MSWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeMineField();
        }
    }
}
