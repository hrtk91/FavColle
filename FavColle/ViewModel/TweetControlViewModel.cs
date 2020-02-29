using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FavColle.DIContainer;
using FavColle.Model;
using FavColle.Model.Interface;

namespace FavColle.ViewModel
{
	public class TweetControlViewModel : ViewModelBase
	{
		private ProfileImage IconSource { get; set; }
		public ImageSource Icon { get; set; }
        public Uri IconUri { get; set; }
		public long Id { get; set; }
		public string Name { get; set; }
		public string ScreenName { get; set; }
		public string TweetText { get; set; }
		public IEnumerable<ITwitterImage> MediaSources { get; set; }
        public ObservableCollection<Uri> MediaUris { get; set; }
        public Uri Showing { get; set; }
        public int RetweetCount { get; set; }
		public int FavoriteCount { get; set; }
        public string OriginUser { get; set; }

        private bool _isRetweeted = false;
        public bool IsRetweeted
        {
            get => _isRetweeted;
            set { _isRetweeted = value; RaisePropertyChanged(); }
        }

        private bool _isFavorited = false;
        public bool IsFavorited
        { 
            get => _isFavorited;
            set { _isFavorited = value; RaisePropertyChanged(); }
        }
        
		public DelegateCommand MediaPushedCommand { get; set; }
        public DelegateCommand RetweetCommand { get; set; }
        public DelegateCommand FavoriteCommand { get; set; }
        public DelegateCommand ScrollCommand { get; set; }

        private ITweet OriginTweet { get; set; }
        
		public TweetControlViewModel(ITweet tweet)
		{
            OriginTweet = tweet;
			Id = tweet.Id;
			IconSource = tweet.User.IconSource;
			Name = tweet.User.Name;
			ScreenName = tweet.User.ScreenName;
			TweetText = tweet.Text;
			MediaSources = tweet.Medias;
            IsRetweeted = tweet.IsRetweetByUser;
            IsFavorited = tweet.IsFavorited;
			RetweetCount = tweet.RetweetCount;
			FavoriteCount = tweet.FavoriteCount;
            OriginUser = tweet.OriginUser != null ? $"{tweet.OriginUser.ScreenName}がリツイート" : "";

			MediaPushedCommand = new DelegateCommand(MediaPushed, (obj) => true);
            RetweetCommand = new DelegateCommand(Retweet, (obj) => true);
            FavoriteCommand = new DelegateCommand(Favorite, (obj) => true);
            ScrollCommand = new DelegateCommand(obj => { });
        }

        public TweetControlViewModel SetProfileAndMediaSource()
        {
            SetProfileSource();
            SetMediaSource();

            return this;
        }

        public TweetControlViewModel SetProfileSource()
        {
            if (IconSource == null) throw new InvalidOperationException("IconSource doesn't initialized");

            IconUri = new Uri(IconSource.Url);

            return this;
        }

        public TweetControlViewModel SetMediaSource()
		{
            // 画像は必ずしもあるわけじゃないので、ない場合は何もしない
            if (MediaSources == null) return this;

            MediaUris = new ObservableCollection<Uri>( MediaSources.Cast<TweetImage>().Select(media => media.ToUri()) );

            return this;
		}


		public void MediaPushed(object obj)
		{
            if (!(obj is Uri content)) return;

            var tweetImage = MediaSources.First(media => media.ToUri().AbsoluteUri == content.AbsoluteUri);
            Service.MediaViewerService.Insert(new MediaOverlayViewModel(tweetImage, MediaSources, OriginTweet));
		}

        public async void Retweet(object obj)
        {
            if (!(obj is View.TweetControl content) || IsRetweeted) return;

            var client = DI.Resolve<TwitterClient>();

            try
            {
                if (!IsRetweeted)
                {
                    IsRetweeted = true;
                    await client.Retweet(Id);
                    RetweetCount++;
                }
                else
                {
                    IsRetweeted = false;
                    await client.UnRetweet(Id);
                    RetweetCount--;
                }
            }
            catch (AggregateException aggregate)
            {
                foreach (var e in aggregate.InnerExceptions)
                {
                    if (e is CoreTweet.TwitterException)
                    {
                        // 以下のケースはどうしようもないから無視
                        // ・リツイート済をリツイート
                        // ・リツイート未をリツイート外す
                        // ・リツイート失敗
                        continue;
                    }
                    throw e;
                }
            }
        }

        public async void Favorite(object obj)
        {
            if (!(obj is View.TweetControl content)) return;

            var client = DI.Resolve<TwitterClient>();

            try
            {
                if (!IsFavorited)
                {
                    IsFavorited = true;
                    FavoriteCount++;
                    await client.Favorite(Id);
                }
                else
                {
                    IsFavorited = false;
                    FavoriteCount--;
                    await client.UnFavorite(Id);
                }
            }
            catch (AggregateException aggregate)
            {
                foreach (var e in aggregate.InnerExceptions)
                {
                    if (e is CoreTweet.TwitterException)
                    {
                        // 以下のケースはどうしようもないから無視
                        // ・ふぁぼ済をふぁぼった
                        // ・ふぁぼったをふぁぼらないにした
                        // ・ふぁぼ失敗
                    }
                }
            }
        }
    }
}
