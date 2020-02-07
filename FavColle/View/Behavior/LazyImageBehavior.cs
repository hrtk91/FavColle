using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FavColle.DIContainer;
using FavColle.Model;

namespace FavColle.View.Behavior
{
    public class LazyImageBehavior
    {

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static Uri GetLazySource(DependencyObject obj)
        {
            return (Uri)obj.GetValue(LazySourceProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static void SetLazySource(DependencyObject obj, Uri value)
        {
            obj.SetValue(LazySourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LazySourceProperty =
            DependencyProperty.RegisterAttached("LazySource", typeof(Uri), typeof(LazyImageBehavior), new PropertyMetadata(null, LazySourcePropertyChanged));

        public static async void LazySourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is Image img)) return;
            if (!(e.NewValue is Uri uri) || uri == null) return;


            img.Source = await CachedWebClient.LoadImage(uri);
        }
    }
}
