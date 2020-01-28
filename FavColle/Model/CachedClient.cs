using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FavColle.Model
{
	public class CachedWebClient
	{
        protected static HttpClient client = new HttpClient();
        public async Task SaveAsAsync(string address, string path)
        {
            using (var stream = await client.GetStreamAsync(address))
            using (var fs = new FileStream(path, FileMode.Create))
            {
                await stream.CopyToAsync(fs);
            }
        }

        public async Task<byte[]> DownloadDataAsync(string address, int retry = 5)
		{
			try
			{
                if (retry >= 0)
                {
                    //var directory = "./caches/";
                    //if (System.IO.Directory.Exists(directory) == false)
                    //{
                    // System.IO.Directory.CreateDirectory(directory);
                    //}

                    //var filename = ToCacheFileName(address);
                    //var filepath = Path.Combine(directory, filename);

                    using (var stream = await client.GetStreamAsync(address))
                    using (var ms = new MemoryStream())
                    {
                        await stream.CopyToAsync(ms);
                        ms.Position = 0;
                        //await ms.CopyToAsync(fs);
                        //ms.Position = 0;

                        return ms.ToArray();
                    }
                }
                else
                {
                    return new byte[] { byte.MinValue };
                }
            }
            catch (Exception e)
			{
				Debug.WriteLine($"Error: {e}");
                await Task.Delay(100);

                return await DownloadDataAsync(address, retry--);
            }
		}


		protected string ToCacheFileName(string address)
		{
			var extension = System.IO.Path.GetExtension(address);
			var byteArray = System.Text.Encoding.UTF8.GetBytes(address);
			var filename = System.Convert.ToBase64String(byteArray).Replace("\\/", ":");
			return filename + extension;
		}


		protected string ToAddress(string base64)
		{
			var withoutextension = System.IO.Path.GetFileNameWithoutExtension(base64);
			var filename = withoutextension.Replace(":", "/");
			var byteArray = System.Convert.FromBase64String(filename);
			return System.Text.Encoding.UTF8.GetString(byteArray);
		}


		protected async Task CreateCache(byte[] data, string filepath)
		{
			try
			{
				using (var fs = new System.IO.FileStream(filepath, System.IO.FileMode.OpenOrCreate, FileAccess.Write))
				{
					await fs.WriteAsync(data, 0, data.Length);
				}
			}
			catch (System.IO.IOException e)
			{
				Debug.WriteLine("Message={0},StackTrace={1}", e.Message, e.StackTrace);
                await Task.Delay(100);
				await CreateCache(data, filepath);
			}
		}
	}
}
