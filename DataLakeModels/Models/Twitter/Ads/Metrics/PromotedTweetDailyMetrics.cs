using System;
using System.Linq;
using System.Collections.Generic;
using DataLakeModels.Models;

namespace DataLakeModels.Models.Twitter.Ads {

    public class PromotedTweetDailyMetrics : BasicTweetDailyMetrics, IEquatable<PromotedTweetDailyMetrics> {

        /// <summary>
        /// The id of the promoted tweet
        /// </summary>
        public string PromotedTweetId { get; set; }

        /// <summary>
        /// Total number of billed engagements
        /// </summary>
        public int BilledEngagements { get; set; }

        /// <summary>
        /// Total spend in micros
        /// </summary>
        public long BilledChargeLocalMicro { get; set; }

        /// <summary>
        /// Total number of views (autoplay and click) of media across Videos,
        /// Vines, GIFs, and Images.
        /// </summary>
        public int MediaViews { get; set; }

        /// <summary>
        /// Total number of clicks of media across Videos, Vines, GIFs, and Images.
        /// </summary>
        public int MediaEngagements { get; set; }

        bool IEquatable<PromotedTweetDailyMetrics>.Equals(PromotedTweetDailyMetrics other) {
            return PromotedTweetId == other.PromotedTweetId &&
                   BilledEngagements == other.BilledEngagements &&
                   BilledChargeLocalMicro == other.BilledChargeLocalMicro &&
                   MediaViews == other.MediaViews &&
                   MediaEngagements == other.MediaEngagements &&
                   (this as BasicTweetDailyMetrics).Equals(other as BasicTweetDailyMetrics);
        }

        public static IEnumerable<string> RequiredMetrics() {

            return BasicTweetDailyMetrics.BasicMetrics().Concat(new List<string>() {
                "billed_engagements", "billed_charge_local_micro", "media_views", "media_engagements"
            });
        }
    }
}
