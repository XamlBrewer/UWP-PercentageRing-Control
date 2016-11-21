using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using XamlBrewer.Uwp.Controls;

namespace XamlBrewer.Uwp.PercentageRingClient
{
    public sealed partial class SquareOfSquaresPage : Page
    {
        public SquareOfSquaresPage()
        {
            InitializeComponent();
            Loaded += SquareOfSquaresPage_Loaded;
        }

        private void SquareOfSquaresPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var random = new Random((int)DateTime.Now.Ticks);
            foreach (var square in SquareOfSquares.Squares)
            {
               square.Content = new PercentageRing()
                {
                    Height = square.ActualHeight,
                    Width = square.ActualWidth,
                    ScaleBrush = new SolidColorBrush(square.RandomColor()),
                    TrailBrush = new SolidColorBrush(square.RandomColor()),
                    ValueBrush = new SolidColorBrush(Colors.White),
                    ScaleWidth = 10 + random.Next(50),
                    IsInteractive = true,
                    Value = random.Next(100)
                };
            }
        }
    }
}
