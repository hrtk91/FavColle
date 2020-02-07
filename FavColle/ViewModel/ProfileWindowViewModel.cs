using FavColle.DIContainer;
using FavColle.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FavColle.ViewModel
{
	public class ProfileWindowViewModel : ViewModelBase
	{
        /// <summary>1件表示するごとに待機する時間(ms)</summary>
        private const int DELAY = 10;

        public ObservableCollection<TweetControlViewModel> Favorites { get; private set; } = new ObservableCollection<TweetControlViewModel>();
        
		public DelegateCommand GetFavoritesCommand { get; set; }
        public ReactiveCommand<ProfileWindow> InitializeCommand { get; private set; }

        private string _progressLabel = string.Empty;
        public string ProgressLabel
        {
            get => _progressLabel;
            set
            {
                _progressLabel = value;
                RaisePropertyChanged(nameof(ProgressLabel));
            }
        }

        private int _progressValue = 0;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                RaisePropertyChanged(nameof(ProgressValue));
            }
        }

        private int _progressMaximum = 1;
        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                _progressMaximum = value;
                RaisePropertyChanged(nameof(ProgressMaximum));
            }
        }

        public ProfileWindowViewModel()
        {
            InitializeCommand = new ReactiveCommand<ProfileWindow>(Initialize);
            GetFavoritesCommand = new DelegateCommand(GetFavorites, _ => true);
        }

        private void Initialize(ProfileWindow window)
        {
            UpdateFavorites(window);
        }

		public async void UpdateFavorites(ProfileWindow window)
		{
			try
			{
				var client = DI.Resolve<TwitterClient>();
				var favorites = await client.GetMyFavorites();
                var contents = favorites.Select(tweet =>
                {
                    var content = new TweetControlViewModel(tweet);
                    return content.SetProfileAndMediaSource();
                });

                foreach (var content in contents)
                {
                    Dispatch<TweetControlViewModel>(Favorites.Add)(content);
                    await Task.Delay(DELAY);
                }
			}
			catch (CoreTweet.TwitterException exception)
			{
				Debug.WriteLine("{0} = {1}", exception.Message, exception.StackTrace);
				MessageBox.Show("エラーが発生しました。\r\n時間をおいて接続しなおしてください。");
				window.Close();
			}
        }


		public async void GetFavorites(object obj)
		{
			var client = DI.Resolve<TwitterClient>();
			client.FavoritesRetrievingEvent += async (o, e) =>
			{
				if (e.HasReachedRateLimit == true)
				{
					Debug.WriteLine("{0}秒経過", e.RateLimitElapsedTime);
					return;
				}

				var tweets = e.TweetList;
				if (tweets == null) return;

                var medias = tweets
                    .Where(tweet => tweet.Medias != null)
                    .SelectMany(tweet => tweet.Medias);

                foreach (var media in medias)
                {
                    var filename = Path.GetFileName(media.Url);
                    var directory = "./Favorites/";

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    try
                    {
                        await media.SaveAsAsync(directory, filename);
                    }
                    catch (AggregateException err)
                    {
                        foreach (var ie in err.InnerExceptions)
                        {
                            throw ie;
                        }
                    }
                }

                UpdateProgress(e.NowRetrived, e.Maximum, $"お気に入り取得：{e.NowRetrived} / {e.Maximum}");
            };

			client.FavoritesRetrievedEvent += (o, e) =>
			{
                UpdateProgress(e.NowRetrived, e.Maximum, $"お気に入り取得：{e.NowRetrived} / {e.Maximum}");

                MessageBox.Show($"お気に入り画像の保存が完了しました：{e.Maximum}ツイート分", "お気に入り一括取得");
			};

            await client.GetMyFavoritesContinuously();
		}

        protected void UpdateProgress(int value, int maximum, string label = "")
        {
            ProgressLabel = label;
            ProgressValue = value;
            ProgressMaximum = maximum;
        }
	}
}
