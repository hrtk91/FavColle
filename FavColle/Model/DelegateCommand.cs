using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FavColle.Model
{
	public class DelegateCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		private Action<object> ExecuteFunc;
		private Func<object, bool> CanExecuteFunc;


		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			ExecuteFunc = execute ?? ((obj) => { });
			CanExecuteFunc = canExecute ?? ((obj) => true);
		}

        public DelegateCommand(Action execute, Func<object, bool> canExecute = default(Func<object, bool>))
            : this(obj => execute(), canExecute) { }

		public bool CanExecute(object parameter)
		{
			return CanExecuteFunc(parameter);
		}


		public void Execute(object parameter)
		{
			ExecuteFunc(parameter);
		}


		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
