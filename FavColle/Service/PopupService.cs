using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FavColle.Service
{
    class PopupService
    {
        public static Popup Show(UIElement target, PlacementMode mode,string message)
        {
            var popup = new Popup()
            {
                Child = CreateDefaultGrid(message),
                PlacementTarget = target,
                Placement = PlacementMode.Center,
                PopupAnimation = PopupAnimation.Fade,
                HorizontalOffset = 0,
                VerticalOffset = 0,
                IsOpen = true,
            };
            //popup.PlacementRectangle =
            //    new Rect(0, 0, target.RenderSize.Width / 2, target.RenderSize.Height / 2);
            return popup;
        }

        public static async Task Flash(UIElement target, string message, int displayTime = 3000, PlacementMode mode = PlacementMode.Bottom)
        {
            var popup = Show(target, mode, message);
            await Task.Delay(displayTime);
            Close(popup);
        }


        private static Grid CreateDefaultGrid(string message)
        {
            var textblock = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Text = message,
                Background = new SolidColorBrush() { Color = Color.FromArgb(127, 40, 40, 40) },
                Foreground = Brushes.AntiqueWhite,
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.Children.Add(textblock);

            return grid;
        }

        public static void Close(Popup popup) => popup.IsOpen = false;
    }
}
