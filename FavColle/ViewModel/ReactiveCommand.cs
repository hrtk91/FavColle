using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FavColle.ViewModel
{
    public class ReactiveCommand : ICommand
    {
        private Func<object, bool> _canExecute = null;
        private Action _execute = null;

        public ReactiveCommand(Action execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute();
    }

    public class ReactiveCommand<T> : ICommand
    {
        private Func<object, bool> _canExecute = null;
        private Action<T> _execute = null;

        public ReactiveCommand(Action<T> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute((T)parameter);
    }
}
