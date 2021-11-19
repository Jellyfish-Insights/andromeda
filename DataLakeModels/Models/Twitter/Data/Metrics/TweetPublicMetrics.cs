using System;
using Tweetinvi.Models.V2;

namespace DataLakeModels.Models.Twitter.Data {
    public class TweetPublicMetrics : TweetPublicMetricsV2, IValidityRange, IEquatable<TweetPublicMetrics> {
        public string TweetId { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Tweet Tweet { get; set; }

        bool IEquatable<TweetPublicMetrics>.Equals(TweetPublicMetrics other) {
            return TweetId == other.TweetId &&
                   LikeCount == other.LikeCount &&
                   QuoteCount == other.QuoteCount &&
                   ReplyCount == other.ReplyCount &&
                   RetweetCount == other.RetweetCount;
        }
    }
}
