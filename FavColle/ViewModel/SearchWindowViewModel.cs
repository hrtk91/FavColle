using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using FavColle.Model;
using FavColle.DIContainer;

namespace FavColle.ViewModel
{
	public class SearchWindowViewModel : ViewModelBase
	{
		public DelegateCommand InitializeCommand { get; set; }


		public DelegateCommand GetPicturesCommand { get; set; }

		public DelegateCommand ScrollCommand { get; set; }


		public ObservableCollection<TweetControlViewModel> TweetList { get; private set; }


		public string SearchWord { get; set; }
		private bool Searching = false;


		public SearchWindowViewModel()
		{
			InitializeCommand = new DelegateCommand(Initialize, (obj) => true);
			GetPicturesCommand = new DelegateCommand(GetPictures, (obj) => true);
			ScrollCommand = new DelegateCommand(ScrollChanged);
			TweetList = new ObservableCollection<TweetControlViewModel>();
		}


		public async void Initialize(object obj)
		{
			if (!(obj is View.SearchWindow view)) throw new InvalidOperationException("Invalid Process Exception");

			var client = DI.Resolve<TwitterClient>();
			var tweets = await client.Search(SearchWord);
			var contents = tweets.Select(tweet => new TweetControlViewModel(tweet));
			
			foreach (var content in contents)
			{
				content.SetProfileAndMediaSource();
				Dispatch<TweetControlViewModel>(TweetList.Add)(content);
			}
		}


		public void GetPictures(object obj)
		{

		}


		public async void NextSearchResult(object sender)
		{
			if (Searching) return;
			
			try
			{
				Searching = true;
				var client = DI.Resolve<TwitterClient>();

				var maxid = TweetList?.Min(tweet => tweet.Id) - 1;

				var tweets = await client.Search(SearchWord, maxid);
				var contents = tweets.Select(tweet => new TweetControlViewModel(tweet));

				foreach (var content in contents)
				{
					content.SetProfileAndMediaSource();
					Dispatch<TweetControlViewModel>(TweetList.Add)(content);
				}
			}
			finally
			{
				Searching = false;
			}
        }


		private void ScrollChanged(object obj)
		{
			(object sender, ScrollChangedEventArgs e) = (ValueTuple<object, ScrollChangedEventArgs>)obj;
			if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0 || !(sender is ScrollViewer sv)) return;

			var pos = e.VerticalOffset;
			var maxHeight = sv.ScrollableHeight;
			var rate = pos / maxHeight;
			var bottomLine = 0.85;
			if (rate > bottomLine)
			{
				Debug.WriteLine("pos={0}, height={1}, rate={2}", pos, maxHeight, rate);
				NextSearchResult(sender);
			}
		}
	}
}
