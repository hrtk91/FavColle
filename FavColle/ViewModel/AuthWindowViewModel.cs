using FavColle.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FavColle.ViewModel
{
	public class AuthWindowViewModel : ViewModelBase
	{
		private string _pin;
		public string PIN
		{
			get { return _pin; }
			set { _pin = value; RaisePropertyChanged("PIN"); }
		}


		public DelegateCommand _authButton;
		public DelegateCommand AuthButton
		{
			get
			{
				return _authButton = _authButton ?? new DelegateCommand((obj) =>
				{
					if (string.IsNullOrEmpty(PIN) == true)
					{
						MessageBox.Show("PINコードを入力してください。", "PIN未入力");
						return;
					}
					var authWindow = (AuthWindow)obj;
					authWindow.Hide();
				});
			}
			set
			{
				AuthButton = value;
			}
		}
	}
}
