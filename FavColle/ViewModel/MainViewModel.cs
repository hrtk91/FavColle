using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using FavColle.Model;
using FavColle.Service;

namespace FavColle.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		public bool IsGettingTimeline { get; private set; } = false;

        public TwitterClient Client { get; private set; } = new TwitterClient();
        public string InputBox { get; set; } = string.Empty;
        public ObservableCollection<TweetContent> TweetList { get; private set; } = new ObservableCollection<TweetContent>();

        public DelegateCommand InitializeCommand { get; private set; }
        public DelegateCommand HomeTimelineCommand { get; private set; }
        public DelegateCommand NextHomeTimelineCommand { get; private set; }
        public DelegateCommand SearchCommand { get; private set; }
        public DelegateCommand TimelineInitializedCommand { get; private set; }

        protected Page CurrentPage { get; set; }

        public MainViewModel()
        {
            InitializeCommand = new DelegateCommand(Initialize);
            HomeTimelineCommand = new DelegateCommand(FetchHomeTimeline, (obj) => IsGettingTimeline == false);
            NextHomeTimelineCommand = new DelegateCommand(FetchNextHomeTimeLine, (obj) => TweetList != null && IsGettingTimeline == false);
            SearchCommand = new DelegateCommand(FindTweet);
            TimelineInitializedCommand = new DelegateCommand(Regist);
        }

        private async void Initialize(object obj)
        {

            if (!(obj is MainWindow window))
            {
                throw new InvalidOperationException("Invalid Process Exception");
            }

            await Client.Initialize();
            await Authorize(window);
			
            CurrentPage = new View.Pages.TimelinePage();
            CurrentPage.DataContext = window.DataContext;

            window.Frame.Navigate(CurrentPage);
        }

        private async void FetchHomeTimeline(object obj)
        {
			if (HomeTimelineCommand.CanExecute(obj) == false) return;

            try
            {
                IsGettingTimeline = true;

                await PopupService.Flash(Application.Current.MainWindow, "タイムライン取得中");

                var tweets = (await Client.FetchHomeTimeline()).Select(tweet => new TweetContent(tweet));

                if (tweets.Max(t => t?.Id) == TweetList.Max(t => t?.Id)) return;

                var excepts = Enumerable.Except(tweets.Select(t => t.Id), TweetList.Select(t => t.Id));
                var orderedExcepts = tweets.Where(t => excepts.Contains(t.Id)).OrderBy(t => t.Id);

                var downloaded =
                    await Task.WhenAll(orderedExcepts.Select(tweet => tweet.DownloadIconAndMedias()));

                foreach (var content in downloaded)
                {
                    Dispatcher.Invoke(() => TweetList.Insert(0, content));
                }
            }
            catch (AggregateException err)
            {
                foreach (var ie in err.InnerExceptions)
                {
                    throw ie;
                }
            }
            catch (CoreTweet.TwitterException err)
            {
                Debug.WriteLine(err);
                MessageBox.Show("Twitter API 取得制限に達しました。\r\n15分間の利用が制限されています。");
            }
            finally
            {
                IsGettingTimeline = false;
                await PopupService.Flash(Application.Current.MainWindow, "タイムライン取得完了");
            }
        }

        private async void FetchNextHomeTimeLine(object obj)
        {
			if (!NextHomeTimelineCommand.CanExecute(obj)) return;

            try
            {
                IsGettingTimeline = true;

                await PopupService.Flash(Application.Current.MainWindow, "タイムライン取得中");

                var maxid = TweetList.Min(tweet => tweet.Id) - 1;
                var tweets = (await Client.FetchHomeTimeline(maxid: maxid)).Select(tweet => new TweetContent(tweet));

                var downloaded =
                    await Task.WhenAll(tweets.Select(tweet => tweet.DownloadIconAndMedias()));

                foreach (var content in downloaded)
                {
                    Dispatch<TweetContent>(TweetList.Add)(content);
                }
            }
            catch (AggregateException err)
            {
                foreach (var ie in err.InnerExceptions)
                {
                    throw ie;
                }
            }
            catch (CoreTweet.TwitterException err)
            {
                Debug.WriteLine(err);
                MessageBox.Show("Twitter API 取得制限に達しました。\r\n15分間の利用が制限されています。");
            }
            finally
            {
                IsGettingTimeline = false;
                await PopupService.Flash(Application.Current.MainWindow, "タイムライン取得完了");
            }
		}

        private void FindTweet(object obj)
        {
			var keyword = InputBox.Trim();
            if (string.IsNullOrEmpty(keyword) == true)
            {
                MessageBox.Show("検索ワードを入力してください。", "検索エラー");
                return;
            }

			var searchWindow = new View.SearchWindow();
			var viewmodel = searchWindow.DataContext as SearchWindowViewModel;
			viewmodel.SearchWord = keyword;
			searchWindow.Show();
        }
        
        private string _screenName;
        public string ScreenName
        {
            get => _screenName;
            set
            {
                _screenName = value;
                RaisePropertyChanged();
            }
        }

		private ImageSource _profileIcon;
		public ImageSource ProfileIcon
		{
            get => _profileIcon;
			set
            {
                _profileIcon = value;
                RaisePropertyChanged(nameof(ProfileIcon));
            }
		}
        
		private DelegateCommand _profileIconButton;
		public DelegateCommand ProfileIconButton
		{
			get
			{
				return _profileIconButton = _profileIconButton ?? new DelegateCommand((obj) =>
				{
					var profileWindow = new ProfileWindow();
					profileWindow.Show();
				});
			}
			set => _profileIconButton = value;
		}
        
		private async Task Authorize(MainWindow window)
        {
            var authed = await Client.Authorize();
            if (authed == true)
            {
                (ScreenName, ProfileIcon) = await Client.GetProfile();
                HomeTimelineCommand.Execute(null);
                return;
            }

            await AuthorizeLoop(window);
        }

        private async Task AuthorizeLoop(MainWindow window)
        {
            AuthWindow authWindow = new AuthWindow();
            bool authed = false;

            while (authed == false)
            {
                authWindow.Owner = window;
                authWindow?.ShowDialog();

                var authWindowVM = authWindow.DataContext as AuthWindowViewModel;
                var pin = authWindowVM.PIN;

                try
                {
                    Client.AuthorizeWithPin(pin);
                    authed = true;
                    authWindow.Close();
                    MessageBox.Show("認証に成功しました。", "認証完了");

                    (ScreenName, ProfileIcon) = await Client.GetProfile();
                    HomeTimelineCommand.Execute(window);

                    return;
                }
                catch (Exception exception)
                {
                    ILogger logger = Logger.GetLogger();
                    logger.Print("認証失敗", exception);

                    authed = false;
                    var redo = MessageBox.Show("認証に失敗しました。\r\nやり直しますか？", "認証失敗", MessageBoxButton.YesNo);
                    if (redo == MessageBoxResult.No)
                    {
                        authWindow.Close();
                        window.Close();
                        return;
                    }
                }
                finally
                {
                    authWindow = null;
                }
            }
        }

        private void Regist(object page)
        {
            if (page is View.Pages.TimelinePage current)
            {
                var scrollViewer = (VisualTreeHelper.GetChild(current.TweetListBox, 0) as Border)?.Child as ScrollViewer;
                scrollViewer.ScrollChanged += ScrollChanged;
            }
        }


		private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
			if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0 || !(sender is ScrollViewer sv)) return;

			var pos = e.VerticalOffset;
            var maxHeight = sv.ScrollableHeight;
            var rate = pos / maxHeight;
            var bottomLine = 0.85;
            if (rate > bottomLine)
            {
                Debug.WriteLine("pos={0}, height={1}, rate={2}", pos, maxHeight, rate);
				FetchNextHomeTimeLine(sender);
			}
		}
	}
}
