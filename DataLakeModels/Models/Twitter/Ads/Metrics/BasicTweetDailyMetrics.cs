using System;
using System.Collections.Generic;
using DataLakeModels.Models;

namespace DataLakeModels.Models.Twitter.Ads {

    public class BasicTweetDailyMetrics : IValidityRange, IEquatable<BasicTweetDailyMetrics> {

        /// <summary>
        /// The date of the daily metrics
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Total number of engagements
        /// </summary>
        public int Engagements { get; set; }

        /// <summary>
        /// Total number of impressions
        /// </summary>
        public int Impressions { get; set; }

        /// <summary>
        /// Total number of retweets
        ///
        public int Retweets { get; set; }

        /// <summary>
        /// Total number of replies
        /// </summary>
        public int Replies { get; set; }

        /// <summary>
        /// Total number of likes
        /// </summary>
        public int Likes { get; set; }

        /// <summary>
        /// Total number of follows
        /// </summary>
        public int Follows { get; set; }

        /// <summary>
        /// Total number of card engagements
        /// </summary>
        public int CardEngagements { get; set; }

        /// <summary>
        /// Total number of clicks, including favorites and other engagements
        /// </summary>
        public int Clicks { get; set; }

        /// <summary>
        /// Number of app install or app open attempts
        /// </summary>
        public int AppClicks { get; set; }

        /// <summary>
        /// Total clicks on the link or Website Card in an ad, including earned
        /// </summary>
        public int UrlClicks { get; set; }

        /// <summary>
        /// Total number of qualified impressions
        /// </summary>
        public int QualifiedImpressions { get; set; }

        /// <summary>
        /// Total number of video views</td>
        /// </summary>
        public int VideoTotalViews { get; set; }

        /// <summary>
        /// Total number of views where at least 25% of the video was viewed.</td>
        /// </summary>
        public int VideoViews25 { get; set; }

        /// <summary>
        /// Total number of views where at least 50% of the video was viewed.</td>
        /// </summary>
        public int VideoViews50 { get; set; }

        /// <summary>
        /// Total number of views where at least 75% of the video was viewed.</td>
        /// </summary>
        public int VideoViews75 { get; set; }

        /// <summary>
        /// Total number of views where at least 100% of the video was viewed.</td>
        /// </summary>
        public int VideoViews100 { get; set; }

        /// <summary>
        /// Total clicks on the call to action</td>
        /// </summary>
        public int VideoCtaClicks { get; set; }

        /// <summary>
        /// Total number of video playback starts</td>
        /// </summary>
        public int VideoContentStarts { get; set; }

        /// <summary>
        /// Total number of views where at least 3 seconds were played while 100% in view (legacy
        /// </summary>
        public int Video3s100pctViews { get; set; }

        /// <summary>
        /// Total number of views where at least 6 seconds of the video was viewed</td>
        /// </summary>
        public int Video6sViews  { get; set; }

        /// <summary>
        /// Total number of views where at least 15 seconds of the video or for 95% of the total duration was viewed
        /// </summary>
        public int Video15sViews  { get; set; }

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        bool IEquatable<BasicTweetDailyMetrics>.Equals(BasicTweetDailyMetrics other) {
            return Date == other.Date &&
                   Engagements == other.Engagements &&
                   Impressions == other.Impressions &&
                   Retweets == other.Retweets &&
                   Replies == other.Replies &&
                   Likes == other.Likes &&
                   Follows == other.Follows &&
                   CardEngagements == other.CardEngagements &&
                   Clicks == other.Clicks &&
                   AppClicks == other.AppClicks &&
                   UrlClicks == other.UrlClicks &&
                   QualifiedImpressions == other.QualifiedImpressions &&
                   VideoTotalViews == other.VideoTotalViews &&
                   VideoViews25 == other.VideoViews25 &&
                   VideoViews50 == other.VideoViews50 &&
                   VideoViews75 == other.VideoViews75 &&
                   VideoViews100 == other.VideoViews100 &&
                   VideoCtaClicks == other.VideoCtaClicks &&
                   VideoContentStarts == other.VideoContentStarts &&
                   Video3s100pctViews == other.Video3s100pctViews &&
                   Video6sViews == other.Video6sViews &&
                   Video15sViews == other.Video15sViews;
        }

        public static IEnumerable<string> BasicMetrics() {
            return new List<string>(){
                       "engagements",
                       "impressions",
                       "retweets",
                       "replies",
                       "likes",
                       "follows",
                       "card_engagements",
                       "clicks",
                       "app_clicks",
                       "url_clicks",
                       "qualified_impressions",
                       "video_total_views",
                       "video_views_25",
                       "video_views_50",
                       "video_views_75",
                       "video_views_100",
                       "video_cta_clicks",
                       "video_content_starts",
                       "video_3s_100pct_views",
                       "video_6s_views",
                       "video_15s_views"
            };
        }
    }
}
