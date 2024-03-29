﻿using System;
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

        private int xSize = 9;
        private int ySize = 9;
        private int mineCount = 10;

        private readonly Random rng = new();

        private readonly BitmapImage mineImage = new(new Uri("mine.png", UriKind.Relative));

        private readonly Timer gameTimer = new(1000);
        private int timeElapsed = 0;

        private readonly Dictionary<int, Brush> adjacentColours = new()
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
            gameTimer.Elapsed += GameTimer_Elapsed;
        }

        private void GameTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timeElapsed++;
            _ = Dispatcher.Invoke(() => timeDisplay.Content = (timeElapsed / 60).ToString().PadLeft(2, '0') + ":" + (timeElapsed % 60).ToString().PadLeft(2, '0'));
        }

        private void MineField_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateMineField();
        }

        private void PopulateMineField()
        {
            mineField.Children.Clear();
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    MineButton newButton = new()
                    {
                        Background = Brushes.LightGray,
                        FieldPosition = new Point(x, y)
                    };
                    newButton.Click += MineButton_Click;
                    newButton.MouseRightButtonUp += MineButton_MouseRightButtonUp;
                    _ = mineField.Children.Add(newButton);
                    Canvas.SetTop(newButton, y * (mineField.ActualHeight / ySize));
                    Canvas.SetLeft(newButton, x * (mineField.ActualWidth / xSize));
                }
            }
            ResizeMineField();
            NewGame();
        }

        private void ResizeMineField()
        {
            double targetHeight = (gameGrid.ActualHeight - (mineField.Margin.Top + mineField.Margin.Bottom)) / ySize;
            double targetWidth = (gameGrid.ActualWidth - (mineField.Margin.Right + mineField.Margin.Left)) / xSize;
            if (targetHeight < 0)
            {
                targetHeight = 0;
            }
            if (targetWidth < 0)
            {
                targetWidth = 0;
            }
            foreach (MineButton buttonInField in mineField.Children)
            {
                buttonInField.Height = targetHeight;
                buttonInField.Width = targetWidth;
                Canvas.SetTop(buttonInField, buttonInField.FieldPosition.Y * (mineField.ActualHeight / ySize));
                Canvas.SetLeft(buttonInField, buttonInField.FieldPosition.X * (mineField.ActualWidth / xSize));
            }
        }

        private void NewGame()
        {
            gameTimer.Stop();
            timeElapsed = 0;
            timeDisplay.Content = "00:00";
            mineDisplay.Content = mineCount;
            mineDisplay.Foreground = Brushes.Black;
            middleButton.Content = new Image()
            {
                Source = mineImage
            };
            LayMines();
        }

        private void LayMines()
        {
            foreach (MineButton buttonInField in mineField.Children)
            {
                buttonInField.ContainsMine = false;
                buttonInField.IsPlayerFlagged = false;
                buttonInField.IsEnabled = true;
                buttonInField.Content = null;
            }
            for (int i = 0; i < mineCount; i++)
            {
                MineButton tempButton = (MineButton)mineField.Children[rng.Next(mineField.Children.Count)];
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

        private void MineButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((MineButton)sender).IsPlayerFlagged = !((MineButton)sender).IsPlayerFlagged;
            ((MineButton)sender).Content = ((MineButton)sender).IsPlayerFlagged
                ? new Viewbox()
                {
                    Child = new TextBlock()
                    {
                        Text = "🚩",
                    }
                }
                : null;
            UpdateMineDisplay();
            if (e is not null)
            {
                e.Handled = true;
            }
        }

        private void MineButton_Click(object sender, RoutedEventArgs e)
        {
            if (mineField.Children.OfType<MineButton>().All(x => x.IsEnabled))
            {
                if (((MineButton)sender).ContainsMine)
                {
                    while (true)
                    {
                        MineButton tempButton = (MineButton)mineField.Children[rng.Next(mineField.Children.Count)];
                        if (!tempButton.ContainsMine)
                        {
                            tempButton.ContainsMine = true;
                            break;
                        }
                    }
                    ((MineButton)sender).ContainsMine = false;
                }
                gameTimer.Start();
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
            if (e is not null)
            {
                e.Handled = true;
            }
        }

        private void MineField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // If both mouse buttons were pressed together for chording
            if ((e.ChangedButton == MouseButton.Left && e.RightButton == MouseButtonState.Pressed)
                || (e.ChangedButton == MouseButton.Right && e.LeftButton == MouseButtonState.Pressed))
            {
                Point relativePos = e.GetPosition(mineField);
                int clickedGridX = (int)relativePos.X / (int)(mineField.ActualWidth / xSize);
                int clickedGridY = (int)relativePos.Y / (int)(mineField.ActualHeight / ySize);
                if (!(clickedGridX < xSize && clickedGridX >= 0 && clickedGridY < ySize && clickedGridY >= 0))
                {
                    return;
                }
                if (!mineField.Children[(clickedGridY * xSize) + clickedGridX].IsEnabled)
                {
                    int totalAdjacentFlags = 0;
                    int totalAdjacentMines = 0;
                    for (int ypos = clickedGridY - 1; ypos <= clickedGridY + 1; ypos++)
                    {
                        for (int xpos = clickedGridX - 1; xpos <= clickedGridX + 1; xpos++)
                        {
                            if (xpos < xSize && xpos >= 0 && ypos < ySize && ypos >= 0)
                            {
                                MineButton toCheck = (MineButton)mineField.Children[(ypos * xSize) + xpos];
                                if (toCheck.IsPlayerFlagged)
                                {
                                    totalAdjacentFlags++;
                                }
                                if (toCheck.ContainsMine)
                                {
                                    totalAdjacentMines++;
                                }
                            }
                        }
                    }
                    if (totalAdjacentFlags == totalAdjacentMines)
                    {
                        for (int ypos = clickedGridY - 1; ypos <= clickedGridY + 1; ypos++)
                        {
                            for (int xpos = clickedGridX - 1; xpos <= clickedGridX + 1; xpos++)
                            {
                                if (xpos < xSize && xpos >= 0 && ypos < ySize && ypos >= 0)
                                {
                                    MineButton toClick = (MineButton)mineField.Children[(ypos * xSize) + xpos];
                                    if (toClick.IsEnabled && !toClick.IsPlayerFlagged)
                                    {
                                        MineButton_Click(toClick, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GameLost()
        {
            gameTimer.Stop();
            middleButton.Content = "❌";
            foreach (MineButton buttonInField in mineField.Children)
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
                        Source = mineImage,
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
                int totalAdjacent = 0;
                for (int ypos = (int)buttonInField.FieldPosition.Y - 1; ypos <= buttonInField.FieldPosition.Y + 1; ypos++)
                {
                    for (int xpos = (int)buttonInField.FieldPosition.X - 1; xpos <= buttonInField.FieldPosition.X + 1; xpos++)
                    {
                        if (xpos < xSize && xpos >= 0 && ypos < ySize && ypos >= 0 && ((MineButton)mineField.Children[(ypos * xSize) + xpos]).ContainsMine)
                        {
                            totalAdjacent++;
                        }
                    }
                }
                buttonInField.Content = new Viewbox()
                {
                    Child = new TextBlock()
                    {
                        Text = totalAdjacent.ToString(),
                        FontWeight = FontWeights.Bold,
                        Foreground = adjacentColours[totalAdjacent],
                    }
                };
                if (totalAdjacent == 0)
                {
                    for (int ypos = (int)buttonInField.FieldPosition.Y - 1; ypos <= buttonInField.FieldPosition.Y + 1; ypos++)
                    {
                        for (int xpos = (int)buttonInField.FieldPosition.X - 1; xpos <= buttonInField.FieldPosition.X + 1; xpos++)
                        {
                            if (xpos < xSize && xpos >= 0 && ypos < ySize && ypos >= 0 && ((MineButton)mineField.Children[(ypos * xSize) + xpos]).IsEnabled &&
                                !((MineButton)mineField.Children[(ypos * xSize) + xpos]).ContainsMine && !((MineButton)mineField.Children[(ypos * xSize) + xpos]).IsPlayerFlagged)
                            {
                                ComputeButtonContent((MineButton)mineField.Children[(ypos * xSize) + xpos]);
                            }
                        }
                    }
                }
            }
        }

        private void CheckForVictory()
        {
            if (mineField.Children.OfType<MineButton>().All(x => !x.ContainsMine ^ x.IsEnabled))
            {
                gameTimer.Stop();
                middleButton.Content = "🏁";
                foreach (MineButton buttonInField in mineField.Children)
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
            mineDisplay.Content = mineCount - mineField.Children.OfType<MineButton>().Count(x => x.IsPlayerFlagged);
            mineDisplay.Foreground = (int)mineDisplay.Content < 0 ? Brushes.Red : (Brush)Brushes.Black;
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void SetGridSize_Click(object sender, RoutedEventArgs e)
        {
            GridSizeInput newGridSizeWin = new(xSize, ySize);
            _ = newGridSizeWin.ShowDialog();
            if (newGridSizeWin.NewX != xSize || newGridSizeWin.NewY != ySize)
            {
                xSize = newGridSizeWin.NewX;
                ySize = newGridSizeWin.NewY;
                if (mineCount >= xSize * ySize)
                {
                    mineCount = (xSize * ySize) - 1;
                }
                PopulateMineField();
            }
        }

        private void SetMineCount_Click(object sender, RoutedEventArgs e)
        {
            MineCountInput newMineCountWin = new(xSize, ySize, mineCount);
            _ = newMineCountWin.ShowDialog();
            if (newMineCountWin.NewCount != mineCount)
            {
                mineCount = newMineCountWin.NewCount;
                NewGame();
            }
        }

        private void SquareField_Click(object sender, RoutedEventArgs e)
        {
            if (((MineButton)mineField.Children[0]).ActualHeight == ((MineButton)mineField.Children[0]).ActualWidth)
            {
                return;
            }
            if (((MineButton)mineField.Children[0]).ActualHeight > ((MineButton)mineField.Children[0]).ActualWidth)
            {
                Width += (((MineButton)mineField.Children[0]).ActualHeight - ((MineButton)mineField.Children[0]).ActualWidth) * xSize;
            }
            else if (((MineButton)mineField.Children[0]).ActualWidth > ((MineButton)mineField.Children[0]).ActualHeight)
            {
                Height += (((MineButton)mineField.Children[0]).ActualWidth - ((MineButton)mineField.Children[0]).ActualHeight) * ySize;
            }
            ResizeMineField();
        }

        private void AutoFlagMine_Click(object sender, RoutedEventArgs e)
        {
            MineButton randomButton;
            try
            {
                randomButton = mineField.Children.OfType<MineButton>().Where(x => x.ContainsMine && !x.IsPlayerFlagged).ElementAt(
                rng.Next(mineField.Children.OfType<MineButton>().Count(x => x.ContainsMine && !x.IsPlayerFlagged)));
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            MineButton_MouseRightButtonUp(randomButton, null);
        }

        private void AutoClearSpace_Click(object sender, RoutedEventArgs e)
        {
            MineButton randomButton;
            try
            {
                randomButton = mineField.Children.OfType<MineButton>().Where(x => !x.ContainsMine && !x.IsPlayerFlagged && x.IsEnabled).ElementAt(
                    rng.Next(mineField.Children.OfType<MineButton>().Count(x => !x.ContainsMine && !x.IsPlayerFlagged && x.IsEnabled)));
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            MineButton_Click(randomButton, null);
        }

        private async void AutoSolve_Click(object sender, RoutedEventArgs e)
        {
            int delay = 5000 / (xSize * ySize);
            if (delay < 1)
            {
                delay = 1;
            }
            foreach (MineButton buttonInField in mineField.Children)
            {
                if (buttonInField.IsEnabled)
                {
                    buttonInField.BorderBrush = Brushes.Red;
                    await Task.Delay(delay);
                    if (buttonInField.ContainsMine)
                    {
                        MineButton_MouseRightButtonUp(buttonInField, null);
                    }
                    else
                    {
                        MineButton_Click(buttonInField, null);
                    }
                    buttonInField.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF707070");
                    if (mineField.Children.OfType<MineButton>().All(x => !x.IsEnabled))
                    {
                        return;
                    }
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            _ = new AboutBox().ShowDialog();
        }

        private void MSWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeMineField();
        }
    }
}
