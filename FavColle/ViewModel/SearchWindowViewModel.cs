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
		private DelegateCommand _initializeCommand;
		public DelegateCommand InitializeCommand
		{
			get { return _initializeCommand; }
			set { _initializeCommand = value; }
		}


		private DelegateCommand _getPicturesCommand;
		public DelegateCommand GetPicturesCommand
		{
			get { return _getPicturesCommand; }
			set { _getPicturesCommand = value; }
		}


		public ObservableCollection<TweetControlViewModel> TweetList { get; private set; }


		public string SearchWord { get; set; }
		private bool Searching = false;


		public SearchWindowViewModel()
		{
			InitializeCommand = new DelegateCommand(Initialize, (obj) => true);
			GetPicturesCommand = new DelegateCommand(GetPictures, (obj) => true);
			TweetList = new ObservableCollection<TweetControlViewModel>();
		}


		public async void Initialize(object obj)
		{
			var view = obj as View.SearchWindow;

			if (null == view)
			{
				throw new InvalidOperationException("Invalid Process Exception");
			}

			var scrollViewer = (VisualTreeHelper.GetChild(view.TweetList, 0) as Border)?.Child as ScrollViewer;
			scrollViewer.ScrollChanged += ScrollChanged;


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


		private void ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
				return;

			var viewer = sender as ScrollViewer;
			var pos = e.VerticalOffset;
			var height = e.ExtentHeight;
			var rate = pos / height;
			var bottomLine = 0.85;
			if (rate > bottomLine)
			{
				Debug.WriteLine("pos={0}, height={1}, rate={2}", pos, height, rate);
				if (Searching == false) NextSearchResult(sender);
			}
		}
	}
}
