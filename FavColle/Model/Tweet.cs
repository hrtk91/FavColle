using System.Collections.Generic;
using System.Linq;

namespace FavColle.Model
{
    public interface ITweet
    {
        long Id { get; set; }
        TweetUser User { get; set; }
        string Text { get; set; }
        IEnumerable<TweetImage> Medias { get; set; }
        int RetweetCount { get; set; }
        int FavoriteCount { get; set; }
        bool IsRetweet { get; set; }
        bool IsRetweetByUser { get; set; }
        bool IsFavorited { get; set; }
        TweetUser OriginUser { get; set; }
    }

    public class TweetUser
    {
        public long UserId { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string ScreenName { get; set; } = string.Empty;
        public ProfileImage IconSource { get; set; } = null;
    }

    public class TweetFactory
    {
        public static ITweet Create(CoreTweet.Status status)
        {
            if (status.RetweetedStatus == null)
            {
                return new Tweet(status);
            }
            else
            {
                return new Retweet(status);
            }
        }
    }

    public class Tweet : ITweet
    {
        public long Id { get; set; } = 0;
        public TweetUser User { get; set; } = null;
		public string Text { get; set; } = string.Empty;
		public IEnumerable<TweetImage> Medias { get; set; } = null;
		public int RetweetCount { get; set; } = 0;
		public int FavoriteCount { get; set; } = 0;
        public bool IsRetweet { get; set; } = false;
		public bool IsRetweetByUser { get; set; } = false;
        public bool IsFavorited { get; set; } = false;
		public TweetUser OriginUser { get; set; } = null;

        public Tweet(CoreTweet.Status status)
        {
            Id = status.Id;
            User = new TweetUser
            {
                UserId = status.User.Id ?? 0,
                Name = status.User.Name,
                ScreenName = status.User.ScreenName,
                IconSource = new ProfileImage(status.User.ProfileImageUrl),
            };
            Text = status.FullText ?? status.Text ?? "";
            Medias =
                status?.ExtendedEntities?.Media
                ?.Where(media => media.Type == "photo")
                .Select(media => new TweetImage(media.MediaUrl));
            RetweetCount = status.RetweetCount ?? 0;
            FavoriteCount = status.FavoriteCount ?? 0;
            IsRetweetByUser = status.IsRetweeted ?? false;
            IsFavorited = status.IsFavorited ?? false;
            OriginUser = null;
        }
    }

    public class Retweet : ITweet
    {
        public long Id { get; set; } = 0;
        public TweetUser User { get; set; } = null;
        public string Text { get; set; } = string.Empty;
        public IEnumerable<TweetImage> Medias { get; set; } = null;
        public int RetweetCount { get; set; } = 0;
        public int FavoriteCount { get; set; } = 0;
        public bool IsRetweet { get; set; } = true;
        public bool IsRetweetByUser { get; set; } = false;
        public bool IsFavorited { get; set; } = false;
        public TweetUser OriginUser { get; set; } = null;

        public Retweet(CoreTweet.Status status)
        {
            Id = status.Id;
            User = new TweetUser
            {
                IconSource = new ProfileImage(status.RetweetedStatus.User.ProfileImageUrl),
                ScreenName = status.RetweetedStatus.User.ScreenName,
                Name = status.RetweetedStatus.User.Name,
            };
            Text = status.RetweetedStatus.FullText ?? status.RetweetedStatus.Text ?? "";
            Medias =
                status?.RetweetedStatus?.ExtendedEntities?.Media?
                .Where(media => media.Type == "photo")
                .Select(media => new TweetImage(media.MediaUrl));
            RetweetCount = status.RetweetedStatus.RetweetCount ?? 0;
            FavoriteCount = status.RetweetedStatus.FavoriteCount ?? 0;
            IsRetweetByUser = status.IsRetweeted ?? false;
            IsFavorited = status.RetweetedStatus.IsFavorited ?? false;
            OriginUser = new TweetUser
            {
                UserId = status.User.Id ?? 0,
                IconSource = new ProfileImage(status.User.ProfileImageUrl),
                ScreenName = status.User.ScreenName,
                Name = status.User.Name
            };
        }
    }
}
