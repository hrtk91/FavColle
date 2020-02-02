using System.Collections.Generic;
using System.Linq;

namespace FavColle.Model
{
	public class Tweet
    {
		public ProfileImage IconSource { get; set; } = null;
		public string Name { get; set; } = null;
		public string ScreenName { get; set; } = null;
		public long? Id { get; set; } = null;
		public string Text { get; set; } = null;
		public IEnumerable<TweetImage> Medias { get; set; } = null;
		public int? RetweetCount { get; set; } = null;
		public int? FavoriteCount { get; set; } = null;
		public bool IsRetweet { get; set; } = false;
        public bool IsFavorited { get; set; } = false;
		public Tweet OriginUser { get; set; } = null;
        
        public Tweet(string iconUrl, string screenName, long id, string text, IEnumerable<TweetImage> medias = null)
        {
            IconSource = new ProfileImage(iconUrl);
            ScreenName = screenName;
            Text = text;
            Id = id;
            Medias = medias;
        }


		public Tweet(string iconUrl, string name, string screenName, long id, string text, IEnumerable<TweetImage> medias = null) : this(iconUrl, screenName, id, text, medias)
		{
			Name = name;
		}


		public Tweet(string iconUrl, string name, string screenName, long id, string text, bool isRetweet, bool isFavorited, Tweet originUser = null, IEnumerable<TweetImage> medias = null) : this(iconUrl, name, screenName, id, text, medias)
		{
			IsRetweet = isRetweet;
            IsFavorited = isFavorited;
			OriginUser = originUser;
		}


		public Tweet(string iconUrl, string name, string screenName, long id, string text, bool isRetweet, bool isFavorited, int retweetCount, int favoriteCount, Tweet originUser = null, IEnumerable<TweetImage> medias = null)
            : this(iconUrl, name, screenName, id, text, isRetweet, isFavorited, originUser, medias)
		{
			RetweetCount = retweetCount;
			FavoriteCount = favoriteCount;
		}

        public Tweet(CoreTweet.Status status)
        {
            IsRetweet = status.IsRetweeted ?? false;
            if (!IsRetweet)
            {
                IsFavorited = status.IsFavorited ?? false;
                IconSource = new ProfileImage(status.User.ProfileImageUrl);
                ScreenName = status.User.ScreenName;
                Text = status.FullText ?? status.Text ?? "";
                Id = status.Id;
                Medias =
                    status?.ExtendedEntities?.Media
                    ?.Where(media => media.Type == "photo")
                    .Select(media => new TweetImage(media.MediaUrl));
                Name = status.User.Name;
                RetweetCount = status.RetweetCount ?? 0;
                FavoriteCount = status.FavoriteCount ?? 0;
                OriginUser = null;
            }
            else
            {
                IsFavorited = status.RetweetedStatus.IsFavorited ?? false;
                IconSource = new ProfileImage(status.RetweetedStatus.User.ProfileImageUrl);
                ScreenName = status.RetweetedStatus.User.ScreenName;
                Text = status.RetweetedStatus.FullText ?? status.RetweetedStatus.Text ?? "";
                Id = status.Id;
                Medias =
                    status.RetweetedStatus.ExtendedEntities.Media
                    .Where(media => media.Type == "photo")
                    .Select(media => new TweetImage(media.MediaUrl));
                Name = status.RetweetedStatus.User.Name;
                RetweetCount = status.RetweetedStatus.RetweetCount ?? 0;
                FavoriteCount = status.RetweetedStatus.FavoriteCount ?? 0;
                OriginUser = new Tweet(status.User.ProfileImageUrl, status.User.ScreenName, status.User.Id ?? 0, "");
            }
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
