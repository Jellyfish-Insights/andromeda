using System;
using Tweetinvi.Models.V2;

namespace DataLakeModels.Models.Twitter.Data {

    public class TweetPromotedMetrics : TweetPromotedMetricsV2, IValidityRange, IEquatable<TweetPromotedMetrics> {
        public string TweetId { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Tweet Tweet { get; set; }

        bool IEquatable<TweetPromotedMetrics>.Equals(TweetPromotedMetrics other) {
            return TweetId == other.TweetId &&
                   ImpressionCount == other.ImpressionCount &&
                   LikeCount == other.LikeCount &&
                   ReplyCount == other.ReplyCount &&
                   RetweetCount == other.RetweetCount &&
                   UrlLinkClicks == other.UrlLinkClicks &&
                   UserProfileClicks == other.UserProfileClicks;
        }
    }
}
