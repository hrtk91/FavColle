using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace FavColle.ViewModel
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
        
        protected static Action<T> Dispatch<T>(Action<T> action) => (arg) => Dispatcher.Invoke(() => action(arg));

        protected static Dispatcher Dispatcher { get; private set; } = Application.Current.Dispatcher;
	}
}
