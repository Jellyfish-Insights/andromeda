using System;
using Tweetinvi.Models.V2;

namespace DataLakeModels.Models.Twitter.Data {

    public class TweetNonPublicMetrics : TweetNonPublicMetricsV2, IValidityRange, IEquatable<TweetNonPublicMetrics> {

        public string TweetId { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Tweet Tweet { get; set; }

        bool IEquatable<TweetNonPublicMetrics>.Equals(TweetNonPublicMetrics other) {
            return TweetId == other.TweetId &&
                   ImpressionCount == other.ImpressionCount &&
                   UrlLinkClicks == other.UrlLinkClicks &&
                   UserProfileClicks == other.UserProfileClicks;
        }
    }
}
