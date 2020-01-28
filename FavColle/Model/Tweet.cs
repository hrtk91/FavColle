using System.Collections.Generic;
using System.Linq;

namespace FavColle.Model
{
	public class Tweet
    {
		public TweetImage IconSource { get; set; } = null;
		public string Name { get; set; } = null;
		public string ScreenName { get; set; } = null;
		public long? Id { get; set; } = null;
		public string Text { get; set; } = null;
		public IEnumerable<TweetImage> Medias { get; set; } = null;
		public int? RetweetCount { get; set; } = null;
		public int? FavoriteCount { get; set; } = null;
		public bool IsRetweet { get; set; } = false;
		public Tweet OriginUser { get; set; } = null;

        public Tweet()
        {

        }

        public Tweet(string iconUrl, string screenName, long id, string text, IEnumerable<TweetImage> medias = null)
        {
            IconSource = new TweetImage(iconUrl);
            ScreenName = screenName;
            Text = text;
            Id = id;
            Medias = medias;
        }


		public Tweet(string iconUrl, string name, string screenName, long id, string text, IEnumerable<TweetImage> medias = null) : this(iconUrl, screenName, id, text, medias)
		{
			Name = name;
		}


		public Tweet(string iconUrl, string name, string screenName, long id, string text, bool isRetweet, Tweet originUser = null, IEnumerable<TweetImage> medias = null) : this(iconUrl, name, screenName, id, text, medias)
		{
			IsRetweet = isRetweet;
			OriginUser = originUser;
		}


		public Tweet(string iconUrl, string name, string screenName, long id, string text, bool isRetweet, int retweetCount, int favoriteCount, Tweet originUser = null, IEnumerable<TweetImage> medias = null)
            : this(iconUrl, name, screenName, id, text, isRetweet, originUser, medias)
		{
			RetweetCount = retweetCount;
			FavoriteCount = favoriteCount;
		}


		public override string ToString()
        {
            var urls = "";
            Medias?.ToList().ForEach(url =>
            {
                urls += "[" + url + "]";
            });

            return string.Format("{0}:{1}:{2}", ScreenName, Text, urls);
        }
    }
}
