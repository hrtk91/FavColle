using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FavColle.DIContainer;
using FavColle.Model.Interface;

namespace FavColle.Model
{
    public class ProfileImage : ITwitterImage
    {
        public string Url { get; set; }
        public byte[] Data { get; set; }

        public ProfileImage(string url)
        {
            Url = url;
        }


        public async Task<ITwitterImage> Download(SizeOpt size = SizeOpt.Small)
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

                var logger = DI.Resolve<ILogger>();
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

        public Uri ToUri(SizeOpt option = SizeOpt.Orig)
            => new Uri(Url);

        public async Task SaveAsAsync(Uri source, string filepath)
        {
            try
            {
                var client = new CachedWebClient();
                await client.SaveAsAsync(source.AbsoluteUri, filepath);
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                {
                    throw ie;
                }
            }
        }
    }
}
