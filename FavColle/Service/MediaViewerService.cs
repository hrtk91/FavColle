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
    class MediaViewerService
    {
        public static void Insert(Window owner, object dataContext = null)
        {
            if (owner.Content is Panel panel)
            {
                var viewer = new View.MediaViewer
                {
                    Width = panel.ActualWidth,
                    Height = panel.ActualHeight,
                    DataContext = dataContext
                };
                Panel.SetZIndex(viewer, int.MaxValue);
                panel.Children.Add(viewer);

                viewer.MouseUp += (s, e) =>
                {
                    panel.Children.Remove(viewer);
                };
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static void Insert(object dataContext)
        {
            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive) is Window activeWindow)
            {
                Insert(activeWindow, dataContext);
            }
        }
    }
}
