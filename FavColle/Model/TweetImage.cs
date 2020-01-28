using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FavColle.Model
{
	public class TweetImage
	{
		public string Url { get; set; }
		public byte[] Data { get; set; }

		public TweetImage(string url)
		{
			Url = url;
		}


		public async Task<TweetImage> Download()
		{
			var client = new CachedWebClient();
			try
			{
				Data = await client.DownloadDataAsync(Url);
				return this;
			}
			catch (WebException e)
			{
				Data = null;

				ILogger logger = Logger.GetLogger();
				logger.Print("Download Failed.", e);

				return this;
			}
		}


		public ImageSource Convert()
		{
			var bitmapImage = new BitmapImage();

			using (var ms = new MemoryStream(Data))
			{
				bitmapImage.BeginInit();
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.StreamSource = ms;
				bitmapImage.EndInit();
			}

			return bitmapImage;
		}

        public async Task SaveAsAsync(string directory, string filename)
        {
            var filepath = directory + filename;
            if (File.Exists(filepath) == true) return;

            try
            {
                var client = new CachedWebClient();
                await client.SaveAsAsync(Url, filepath);
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                {
                    throw ie;
                }
            }
        }

        public async Task SaveAsync(string directory, string filename)
		{
            if (Data == null) await Download();

			var filepath = directory + filename;
			if (File.Exists(filepath) == true) return;

			try
			{
				using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write))
				{
					await fs.WriteAsync(Data, 0, Data.Length);
				}
			}
			catch (IOException e)
			{
				Debug.WriteLine("Message={0}, StackTrace = {1}", e.Message, e.StackTrace);
                await Task.Delay(100);
				await SaveAsync(directory, filename);
			}
		}


		public void Save(string directory, string filename)
		{
			SaveAsync(directory, filename).Wait();
		}
	}
}
