using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FavColle.ViewModel
{
    public class ReactiveProperty<T> : INotifyPropertyChanged
    {
        public ReactiveProperty(T value)
        {
            _value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void RaisePropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(T)));
        }

        private T _value = default(T);
        public T Value
        {
            get { return _value; }
            set
            {
                RaisePropertyChanged();

                _value = value;
            }
        }
    }
}
