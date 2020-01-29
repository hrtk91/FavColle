using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FavColle.Model;

namespace FavColle.ViewModel
{
	public class TweetContent
	{
		private TweetImage IconSource { get; set; }
		public ImageSource Icon { get; set; }
		public long Id { get; set; }
		public string Name { get; set; }
		public string ScreenName { get; set; }
		public string TweetText { get; set; }
		public IEnumerable<TweetImage> MediaSources { get; set; }
		public ObservableCollection<ImageSource> Medias { get; set; }
		public int RetweetCount { get; set; }
		public int FavoriteCount { get; set; }
        public string OriginUser { get; set; }
        
		public DelegateCommand ImagePushedCommand { get; set; }
        
		public TweetContent(Tweet tweet)
		{
			Id = (long)tweet.Id;
			IconSource = tweet.IconSource;
			Name = tweet.Name;
			ScreenName = tweet.ScreenName;
			TweetText = tweet.Text;
			MediaSources = tweet.Medias;
			RetweetCount = tweet.RetweetCount ?? 0;
			FavoriteCount = tweet.FavoriteCount ?? 0;
            OriginUser = tweet.IsRetweet ? $"{tweet.OriginUser.ScreenName}がリツイート" : "";

			ImagePushedCommand = new DelegateCommand(ImagePushed, (obj) => true);
        }

        public async Task<TweetContent> DownloadIconAndMedias()
        {
            await Task.WhenAll(new[] { DownloadIcon(), DownloadMedias() });
            return this;
        }


		public async Task<TweetContent> DownloadIcon()
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


		public async Task<TweetContent> DownloadMedias()
		{
			if (MediaSources == null) return this;

			MediaSources = await Task.WhenAll(
				MediaSources.Select(media => media.Download(SizeOpt.Small)));

			Medias = new ObservableCollection<ImageSource>(MediaSources.Select(media =>
			{
				var bitmapImage = new BitmapImage();
				using (var ms = new MemoryStream(media.Data))
				{
					bitmapImage.BeginInit();
					bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
					bitmapImage.StreamSource = ms;
					bitmapImage.EndInit();
				}
				return bitmapImage;
			}));

			return this;
		}


		public void ImagePushed(object obj)
		{
			if (MessageBox.Show("画像を保存しますか？", "画像保存確認", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

			var tweet = obj as TweetContent;

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
	}




	public class TweetContentViewModel
	{

	}
}
