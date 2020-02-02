using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using FavColle;
using CoreTweet;
using Newtonsoft.Json;
using FavColle.Model.Interface;

namespace FavColle.Model
{
	public partial class TwitterClient
	{
        [JsonObject]
        private class ApiKey
        {
            [JsonProperty("ConsumerKey")]
            public string ConsumerKey = string.Empty;

            [JsonProperty("ConsumerSecret")]
            public string ConsumerSecret = string.Empty;
        }

        [JsonObject]
        private class AuthToken
        {
            [JsonProperty("Token")]
            public string Token = string.Empty;

            [JsonProperty("Secret")]
            public string Secret = string.Empty;
        }

        public CoreTweet.Tokens Tokens = null;
		protected string ConsumerKey;
		protected string ConsumerSecret;
        protected readonly string ApiKeyFileName = "ApiKey.json";
        protected readonly string AuthTokenFileName = "AccessToken.json";
		protected CoreTweet.OAuth.OAuthSession Session = null;

        public TwitterClient()
        {
            // TLS1.2での接続をWebClientで使用可能にする
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public async Task Initialize()
        {
            if (!File.Exists(ApiKeyFileName)) return;

            using (var sr = new StreamReader(ApiKeyFileName, Encoding.UTF8))
            {
                var lines = await sr.ReadToEndAsync();
                var apiKey = JsonConvert.DeserializeObject<ApiKey>(lines);
                ConsumerKey = apiKey.ConsumerKey;
                ConsumerSecret = apiKey.ConsumerSecret;
            }
        }

        public async Task<bool> Authorize()
		{
			Debug.WriteLine("Authorize : 認証開始");

            if (File.Exists(AuthTokenFileName))
			{
                Debug.WriteLine("Authorize : 以前ログインしたIDで認証します。");

                using (var sr = new StreamReader(AuthTokenFileName, Encoding.UTF8))
                {
                    var authToken = JsonConvert.DeserializeObject<AuthToken>(await sr.ReadToEndAsync());
                    Tokens = CoreTweet.Tokens.Create(ConsumerKey, ConsumerSecret, authToken.Token, authToken.Secret);
                }

                Debug.WriteLine("Authorize : 認証完了");

                return true;
            }

            Session = CoreTweet.OAuth.Authorize(ConsumerKey, ConsumerSecret);
            Process.Start(Session.AuthorizeUri.AbsoluteUri);

            return false;
		}


		public async void AuthorizeWithPin(string pin)
		{
            if (Session != null)
            {
                Tokens = CoreTweet.OAuth.GetTokens(Session, pin);

                var authToken = JsonConvert.SerializeObject(new AuthToken() { Token = Tokens.AccessToken, Secret = Tokens.AccessTokenSecret });
                using (var sw = new StreamWriter(AuthTokenFileName, false, Encoding.UTF8))
                {
                    await sw.WriteAsync(authToken);
                }
            }
            else
            {
                throw new Exception("Session not connected.");
            }
        }


        public async Task<(string, ImageSource)> GetProfile()
        {
            var self = await Tokens.Account.VerifyCredentialsAsync();
            var result = await Tokens.Users.ShowAsync(user_id => self.Id);

            var screenName = result.ScreenName;
            var image = await new ProfileImage(result.ProfileImageUrl).Download();

            return (screenName, image.Convert());
        }

        public async Task<ImageSource> GetProfileIcon()
		{
			var self = await Tokens.Account.VerifyCredentialsAsync();
			var result = await Tokens.Users.ShowAsync(user_id => self.Id);
			var image = await new ProfileImage(result.ProfileImageUrl).Download();
			return image.Convert();
		}


		public async Task<IEnumerable<Tweet>> FetchHomeTimeline(long? sinceid = null, long? maxid = null, int? num = 100)
		{
			var result = await Tokens.Statuses.HomeTimelineAsync(since_id => sinceid, max_id => maxid, count => num);
			var tweets = result.Select(status =>
			{
				var tweet = status.RetweetedStatus != null ? GetRetweetContent(status) : GetTweetContent(status);

				var entities = status.ExtendedEntities ?? status.Entities;
				if (entities?.Media == null) return tweet;

				var photos = entities.Media
						.Where(media => media.Type == "photo")
						.Select(media => new TweetImage(media.MediaUrl));

				//var videos = status.Entities.Media
				//		.Where(media => media.Type == "animated_gif" || media.Type == "video")
				//		.SelectMany(
				//			media => media?.VideoInfo?.Variants
				//				.Select(video => video.Url))
				//				.Where(url => System.IO.Path.GetExtension(url) == ".mp4");

				//tweet.MediaURLs = photos.Concat(videos);
				tweet.Medias = photos;

				return tweet;
			});
			return tweets;
		}

        private Tweet GetTweetContent(Status status)
        {
            return new Tweet(status);
        }

        private Tweet GetRetweetContent(Status status)
        {
            var retweet = status.RetweetedStatus;
            var tweet = new Tweet(
                retweet.User.ProfileImageUrl,
                retweet.User.Name,
                retweet.User.ScreenName,
                status.Id,
                retweet.FullText ?? retweet.Text,
                status.IsRetweeted ?? false,
                status.IsFavorited ?? false,
                retweet.RetweetCount ?? 0,
                retweet.FavoriteCount ?? 0,
                new Tweet(
                    status.User.ProfileImageUrl,
                    status.User.Name,
                    status.User.ScreenName,
                    status.Id,
                    status.FullText ?? status.Text));

            return tweet;
        }


        public async Task<IEnumerable<Tweet>> GetSelfTimeline(bool excludeReply = true, bool includeRetweet = true)
		{
			return await GetUserTimeline(Tokens.ScreenName, excludeReply, includeRetweet);
		}


		public async Task<IEnumerable<Tweet>> GetUserTimeline(string screenName, bool excludeReply = false, bool includeRetweet = false)
		{
			Debug.WriteLine("GetTimeline Start");

			int num = 100;
			var result = await Tokens.Statuses.UserTimelineAsync(screen_name => screenName, count => num, exclude_replies => excludeReply, include_rts => includeRetweet);
			var timeline = result;
			var tweets = timeline.Select(status =>
			{
				var tweet = new Tweet(
					status.User.ProfileImageUrl,
					status.User.Name,
					status.User.ScreenName,
					status.Id,
					status.FullText ?? status.Text,
					status.IsRetweeted ?? false,
                    status.IsFavorited ?? false,
					null,
					status.ExtendedEntities?.Media.Select(media => new TweetImage(media.MediaUrl)));
				return tweet;
			});

			Debug.WriteLine("GetTimeline End");

			return tweets;
		}


		public void WriteConsole(IEnumerable<Tweet> tweets)
		{
			tweets.ToList().ForEach(tweet =>
			{
				var name = tweet.ScreenName;
				var text = tweet.Text;
				var medias = tweet.Medias;
				Console.WriteLine("{0}:{1}:{2}", name, text, medias != null ? string.Concat(medias?.Select(url => "[" + url + "]")) : "");
			});
		}


		public async Task<IEnumerable<Tweet>> Search(string keyword, long? maxid = null, int num = 15)
		{
			Debug.WriteLine("searchTweet Start : Keyword={0}", keyword);

			var result = await Tokens.Search.TweetsAsync(count => num, q => keyword, max_id => maxid);
			Func<CoreTweet.Status, Tweet> tweetSelector = status =>
			{
				var tweet = new Tweet(
					status.User.ProfileImageUrl,
					status.User.Name,
					status.User.ScreenName,
					status.Id,
					status.FullText ?? status.Text,
                    status.IsRetweeted ?? false,
                    status.IsFavorited ?? false,
                    status.RetweetCount ?? 0,
                    status.FavoriteCount ?? 0);

				var entities = status.ExtendedEntities ?? status.Entities;
				if (entities?.Media == null) return tweet;

				var photos = entities.Media
						.Where(media => media.Type == "photo")
						.Select(media => media.MediaUrl);
				tweet.Medias = photos.Select(url => new TweetImage(url));

				//tweet.Medias = tweet.Medias.Concat(
				//	status.ExtendedEntities.Media
				//		.Where(media => media.Type == "animated_gif" || media.Type == "video")
				//		.SelectMany(
				//			media => media?.VideoInfo?.Variants
				//				.Select(video => video.Url))
				//				.Where(url => System.IO.Path.GetExtension(url) == ".mp4"));

				return tweet;
			};

			return result.Select(tweetSelector);
        }


		public async Task<IEnumerable<Tweet>> SearchImageTweet(string keyword)
		{
			IEnumerable<Tweet> tweetList = null;
			try
			{
				Console.WriteLine("Search Start : Keyword={0}", keyword);

				var resultList = new List<SearchResult>();
				var result = await Tokens.Search.TweetsAsync(count => 100, q => keyword);
				var mediaTweets = result.Where(status => status.Entities.Media != null);//.Select(status => Tokens.Statuses.Show(status.Id));
				Func<Status, Tweet> tweetSelector = status =>
				{
					var tweet = new Tweet(
						status.User.Name,
						status.User.ScreenName,
						status.Id,
						status.FullText ?? status.Text,
						status.ExtendedEntities.Media.Where(media => media.Type == "photo").Select(media => new TweetImage(media.MediaUrl)));

					//tweet.MediaURLs = tweet.MediaURLs.Concat(
					//	status.ExtendedEntities.Media
					//		.Where(media => media.Type == "animated_gif" || media.Type == "video")
					//		.SelectMany(
					//			media => media.VideoInfo.Variants
					//				.Select(video => video.Url))
					//				.Where(url => System.IO.Path.GetExtension(url) == ".mp4"));

					return tweet;
				};

				tweetList = mediaTweets.Select(tweetSelector);

				var hasNext = result?.SearchMetadata?.NextResults != null;
				var maxId = result?.Min(status => status?.Id);
				maxId = maxId ?? maxId - 1;
				while (hasNext == true)
				{
					var nextResult = await Tokens.Search.TweetsAsync(count => 100, q => keyword, max_id => maxId);
					mediaTweets = nextResult.Where(status => status.Entities.Media != null);
					tweetList = tweetList.Concat(mediaTweets.Select(tweetSelector));

					hasNext = nextResult.SearchMetadata.NextResults != null;
					maxId = nextResult.Min(status => status.Id) - 1;
				}

				return tweetList;
			}
			catch (CoreTweet.TwitterException e)
			{
                ILogger logger = Logger.GetLogger();
                logger.Print("APIの使用回数制限に達したので検索を終了します。", e);

				return tweetList;
			}
			catch (Exception e)
			{
                ILogger logger = Logger.GetLogger();
                logger.Print("エラーが発生したので検索結果を途中まで返却します。", e);

				return null;
			}
			finally
			{
				Console.WriteLine("Search Complete : Keyword={0}", keyword);
			}
		}


		public void OutputTweetList(IEnumerable<Tweet> tweetlist)
		{
			using (var sw = new System.IO.StreamWriter("./out.log", false, Encoding.UTF8))
			{
				tweetlist.ToList().ForEach(tweet => {
					var str = tweet.ToString();
					Console.WriteLine(str);
					sw.WriteLine(str);
				});
			}
		}


		public void DownloadImages(IEnumerable<Tweet> tweetlist)
		{
			DownloadImages("./image/", tweetlist);
		}


		public void DownloadImages(string directory, IEnumerable<Tweet> tweetlist)
		{
			var downloadlist = tweetlist.SelectMany(tweet => tweet.Medias).Distinct();

			Console.WriteLine("Image Downloading...");

			System.Net.ServicePointManager.DefaultConnectionLimit = 16;

			var imagelist = downloadlist.Select(image => image.Download(SizeOpt.Orig));
            foreach(var it in imagelist)
            {
				it.ContinueWith(task =>
				{
					var image = task.Result;
                    var url = image.Url;
					var data = image.Data;
					var filename = Path.GetFileName(url);
					var filepath = directory + filename;

					if (Directory.Exists(directory) == false)
					{
						Directory.CreateDirectory(directory);
					}
					if (data != null)
					{
						File.WriteAllBytes(filepath, data);
						Console.Write("\rDownload Status : {0} Completed.", image.Url);
					}
					else
					{
						Console.Write("\rDownloadStaus : {0} Failed.", image.Url);
					}
				});
            }
			
			Console.WriteLine();
			Console.WriteLine("Completed.");
		}
	}

    /// <summary>
    /// いいねとリツイートの処理
    /// </summary>
    public partial class TwitterClient
    {
        public async Task Retweet(long id)
        {
            await Tokens.Statuses.RetweetAsync(id: id);
        }

        public async Task UnRetweet(long id)
        {
            await Tokens.Statuses.UnretweetAsync(id: id);
        }

        public async Task Favorite(long id)
        {
            await Tokens.Favorites.CreateAsync(id: id);
        }

        public async Task UnFavorite(long id)
        {
            await Tokens.Favorites.DestroyAsync(id: id);
        }
    }


    /// <summary>
    /// お気に入り取得機能
    /// </summary>
	public partial class TwitterClient
	{
		public delegate void FavoritesRetrieve(object sender, FavoritesRetrieveEventArgs e);
		public event FavoritesRetrieve FavoritesRetriveStartEvent;
        public event FavoritesRetrieve FavoritesRetrievingEvent;
		public event FavoritesRetrieve FavoritesRetrievedEvent;

		public class FavoritesRetrieveEventArgs : EventArgs
		{
			public int Maximum;
			public int NowRetrived;
			public bool HasReachedRateLimit;
			public int RateLimitElapsedTime;
			public IEnumerable<Tweet> TweetList;

			public FavoritesRetrieveEventArgs(int maximum, int nowRetrived)
			{
				Maximum = maximum;
				NowRetrived = nowRetrived;
				HasReachedRateLimit = false;
				RateLimitElapsedTime = 0;
				TweetList = null;
			}

            public FavoritesRetrieveEventArgs(int count, int nowRetrived, IEnumerable<Tweet> tweetList) : this(count, nowRetrived)
            {
                TweetList = tweetList;
            }

            public FavoritesRetrieveEventArgs(int count, int nowRetrived, int elapsedTime) : this(count, nowRetrived)
            {
                HasReachedRateLimit = true;
                RateLimitElapsedTime = elapsedTime;
            }
		}


		public async Task<IEnumerable<Tweet>> GetMyFavorites(int num = 20, long? maxid = null)
		{
			var result = await Tokens.Favorites.ListAsync(count => num, max_id => maxid);
			var favorites = result.Select(status =>
			{
				var tweet = new Tweet(status.User.ProfileImageUrl, status.User.Name, status.User.ScreenName, status.Id, status.FullText ?? status.Text);
				var entities = status.ExtendedEntities ?? status.Entities;
				if (entities?.Media == null) return tweet;

				var photos = entities.Media
						.Where(media => media.Type == "photo")
						.Select(media => media.MediaUrl);
				tweet.Medias = photos.Select(url => new TweetImage(url));
				return tweet;
			});

			return favorites;
		}


		public async Task<IEnumerable<Tweet>> GetMyFavoritesContinuously()
		{
			var user = await Tokens.Account.VerifyCredentialsAsync();
			var maxnum = 3200;
			var favCount = user.FavouritesCount < maxnum ? user.FavouritesCount : maxnum;

            // お気に入り取得開始イベント発行
			FavoritesRetriveStartEvent?.Invoke(this, new FavoritesRetrieveEventArgs(favCount, 0));

            var maxFetchNum = 100;
            var num = (favCount < maxFetchNum) ? favCount : maxFetchNum;
            var fetchedFavorites = await GetMyFavorites(num);

            while (favCount > fetchedFavorites.Count())
			{
                var fetchedCount = fetchedFavorites.Count();
                Debug.WriteLine("GetMyFavoritesContinuously : {0}/{1}", fetchedCount, favCount);
				try
				{
					var fetchNum = (favCount - fetchedCount) > maxFetchNum ? maxFetchNum : (favCount - fetchedCount);
                    var maxid = fetchedFavorites.Min(tweet => tweet.Id) - 1;
                    var fetched = await GetMyFavorites(fetchNum, maxid);

					if (fetched.Any() == false) break;

                    // お気に入り部分取得イベント発行
					FavoritesRetrievingEvent?.Invoke(
                        this, new FavoritesRetrieveEventArgs(favCount, fetchedCount, fetched));

					fetchedFavorites = Enumerable.Concat(fetchedFavorites, fetched);
				}
				catch (CoreTweet.TwitterException e)
				{
					var reset = e.RateLimit.Reset;
					Debug.WriteLine("{0} = {1}", e.Message, e.StackTrace);
					// 15分間停止
					var i = 1;
					while (reset > new DateTimeOffset(DateTime.UtcNow))
					{
                        await Task.Delay(1000);
						FavoritesRetrievingEvent?.Invoke(
                            this, new FavoritesRetrieveEventArgs(favCount, fetchedCount, i++));
					}
				}
                finally
                {
                    await Task.Delay(5000);
                }
			}

            // お気に入り取得終了イベント発行
			FavoritesRetrievedEvent?.Invoke(
                this, new FavoritesRetrieveEventArgs(fetchedFavorites.Count(), fetchedFavorites.Count()));

			Debug.WriteLine("GetMyFavoritesContinuously : Complete.");

			return fetchedFavorites;
		}
	}
}
