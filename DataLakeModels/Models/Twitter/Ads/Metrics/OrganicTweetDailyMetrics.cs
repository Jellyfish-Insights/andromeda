using System;
using System.Collections.Generic;
using DataLakeModels.Models;

namespace DataLakeModels.Models.Twitter.Ads {

    public class OrganicTweetDailyMetrics : BasicTweetDailyMetrics, IEquatable<OrganicTweetDailyMetrics> {

        /// <summary>
        /// The id of the tweet
        /// </summary>
        public string TweetId { get; set; }

        bool IEquatable<OrganicTweetDailyMetrics>.Equals(OrganicTweetDailyMetrics other) {
            return TweetId == other.TweetId &&
                   (this as BasicTweetDailyMetrics).Equals(other as BasicTweetDailyMetrics);
        }

        public static IEnumerable<string> RequiredMetrics() {
            return BasicTweetDailyMetrics.BasicMetrics();
        }
    }
}
