using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FavColle.DIContainer;
using FavColle.Model;

namespace FavColle.View.Behavior
{
    public class ScrollBehavior
    {
        [AttachedPropertyBrowsableForType(typeof(ListBox))]
        public static DelegateCommand GetScrollCommand(ListBox listBox)
        {
            return (DelegateCommand)listBox.GetValue(ScrollCommandProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(ListBox))]
        public static void SetScrollCommand(ListBox listBox, DelegateCommand value)
        {
            // コマンド登録済みならハンドラから登録値を削除
            if (GetScrollCommand(listBox) != null)
            {
                RemoveCommand(listBox);
            }

            listBox.SetValue(ScrollCommandProperty, value);
        }

        private static void RemoveCommand(ListBox listBox)
        {
            if (attachedCommand == null) return;
            if (!(VisualTreeHelper.GetChild(listBox, 0) is Border border)) return;
            if (!(border.Child is ScrollViewer scrollViewer)) return;
            scrollViewer.ScrollChanged -= attachedCommand;
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollCommandProperty =
            DependencyProperty.RegisterAttached("ScrollCommand", typeof(DelegateCommand), typeof(ScrollBehavior), new PropertyMetadata(null, ScrollCommandPropertyChanged));

        private static ScrollChangedEventHandler attachedCommand { get; set; } = null;
        public static void ScrollCommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;
            if (!listBox.IsLoaded)
            {
                listBox.Loaded += (_s, _ev) =>
                {
                    if (!(VisualTreeHelper.GetChildrenCount(listBox) != 0 && VisualTreeHelper.GetChild(listBox, 0) is Border border)) return;
                    if (!(border.Child is ScrollViewer scrollViewer)) return;
                    if (!(e.NewValue is DelegateCommand command)) return;

                    attachedCommand = (object s, ScrollChangedEventArgs ev) =>
                    {
                        if (command.CanExecute((s, ev))) command.Execute((s, ev));
                    };

                    scrollViewer.ScrollChanged += attachedCommand;
                };
            }
        }
    }
}
