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
		public long Id { get; set; }
		public string Name { get; set; }
		public string ScreenName { get; set; }
		public string TweetText { get; set; }
		public IEnumerable<ITwitterImage> MediaSources { get; set; }
		public ObservableCollection<ImageSource> Medias { get; set; }
        public ObservableCollection<Uri> MediaUris { get; set; }
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
        
		public DelegateCommand ImagePushedCommand { get; set; }
        public DelegateCommand RetweetCommand { get; set; }
        public DelegateCommand FavoriteCommand { get; set; }
        
		public TweetControlViewModel(Tweet tweet)
		{
			Id = (long)tweet.Id;
			IconSource = tweet.IconSource;
			Name = tweet.Name;
			ScreenName = tweet.ScreenName;
			TweetText = tweet.Text;
			MediaSources = tweet.Medias;
            IsRetweeted = tweet.IsRetweet;
            IsFavorited = tweet.IsFavorited;
			RetweetCount = tweet.RetweetCount ?? 0;
			FavoriteCount = tweet.FavoriteCount ?? 0;
            OriginUser = tweet.IsRetweet ? $"{tweet.OriginUser.ScreenName}がリツイート" : "";

			ImagePushedCommand = new DelegateCommand(ImagePushed, (obj) => true);
            RetweetCommand = new DelegateCommand(Retweet, (obj) => true);
            FavoriteCommand = new DelegateCommand(Favorite, (obj) => true);
        }

        public async Task<TweetControlViewModel> DownloadIconAndMedias()
        {
            await Task.WhenAll(new[] { DownloadIcon(), DownloadMedias() });
            return this;
        }


		public async Task<TweetControlViewModel> DownloadIcon()
		{
			if (IconSource.Data == null)
			{
				await IconSource.Download(SizeOpt.Small);
			}

			using (var ms = new MemoryStream(IconSource.Data))
			{
				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = ms;
				bitmapImage.EndInit();

				Icon = bitmapImage;
			}

			return this;
		}


		public async Task<TweetControlViewModel> DownloadMedias()
		{
			if (MediaSources == null) return this;

            MediaUris = new ObservableCollection<Uri>( MediaSources.Cast<TweetImage>().Select(media => media.ConvertUri()) );

            return this;
		}


		public void ImagePushed(object obj)
		{
			if (MessageBox.Show("画像を保存しますか？", "画像保存確認", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

			var tweet = obj as TweetControlViewModel;

			var directory = Path.Combine("./Favorites/", tweet.ScreenName, tweet.Id.ToString());
			if (Directory.Exists(directory) == false)
			{
				Directory.CreateDirectory(directory);
			}

			tweet.MediaSources.ToList().ForEach(async image =>
			{
				var filename = Path.GetFileName(image.Url);
				var filepath = directory + filename;

				if (File.Exists(filepath) == true) return;

				await image.SaveAsAsync(directory, filename);
			});
		}

        public async void Retweet(object obj)
        {
            if (!(obj is View.TweetControl content) || IsRetweeted) return;

            var client = DI.Resolve<TwitterClient>();

            if (!IsRetweeted)
            {
                IsRetweeted = true;
                await client.Retweet(Id);
            }
            else
            {
                IsRetweeted = false;
                await client.UnRetweet(Id);
            }
        }

        public async void Favorite(object obj)
        {
            if (!(obj is View.TweetControl content)) return;

            var client = DI.Resolve<TwitterClient>();

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
    }




	public class TweetContentViewModel
	{

	}
}
