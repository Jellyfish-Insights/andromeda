using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {

    public static class Constants {

        public static List<string> AgeGroups = new List<string>() { "13-17", "18-24", "25-34", "35-44", "45-54", "55-64", "65+" };

        public static List<string> GenderGroups = new List<string>() { "M", "F", "U" };

        public const string YouTubeSource = "youtube";

        public const string FacebookSource = "facebook";

        public static List<string> Sources = new List<string> { YouTubeSource, FacebookSource };

        public const string GrandTotalName = "Grand Total";

        public const string DefaultThumbnail = "https://via.placeholder.com/100x100";

        public const string GenericTag = "Generic";

        public const int DefaultDashboardRange = 15;
        public static List<MetricType> DemographicTypes = new List<MetricType> {
            MetricType.DemographicsViewCount,
            MetricType.DemographicsViewTime
        };
        public static bool IsMock() {
            #if MOCK
            return true;
            #else
            return false;
            #endif
        }

        public static List<MetricInfo> DashboardMetrics = new List<MetricInfo> {
            new MetricInfo {
                TypeId = MetricType.Views,
                Type = "Views",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** The (daily) number of times your video was watched for an aggregate of at least 3 seconds, or for nearly its total length, whichever happened first. Crossposted video views are not included in this counter. Got from `post_video_views` [post insights](https://developers.facebook.com/docs/graph-api/reference/v2.12/insights).

**YouTube:** The number of times that a video was viewed. See [views](https://developers.google.com/youtube/analytics/metrics#views)."
            },
            new MetricInfo {
                TypeId = MetricType.Likes,
                Type = "Likes",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Number of ""video reactions"" where type is ""LIKE"" . See [reactions](https://developers.facebook.com/docs/graph-api/reference/video/reactions/).

**YouTube:** The number of times that users indicated that they liked a video by giving it a positive rating. See [likes](https://developers.google.com/youtube/analytics/metrics#likes).

Note that the YouTube Analytics API sometimes returns a negative value.
"
            },
            new MetricInfo {
                TypeId = MetricType.Reactions,
                Type = "Reactions",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Number of ""video reactions"" of any type. See [reactions](https://developers.facebook.com/docs/graph-api/reference/video/reactions/).

**YouTube:** The number of times that users indicated that they liked a video by giving it a positive rating. See [likes](https://developers.google.com/youtube/analytics/metrics#likes).

Note that the YouTube Analytics API sometimes returns a negative value."
            },
            new MetricInfo {
                TypeId = MetricType.Shares,
                Type = "Shares",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Count of public sharedposts that shared this video. Posts are got from end point [video/sharedposts](https://developers.facebook.com/docs/graph-api/reference/video/sharedposts/).

**YouTube:** The number of times that users shared a video through the Share button. [shares](https://developers.google.com/youtube/analytics/metrics#shares).

"
            },
            new MetricInfo {
                TypeId = MetricType.Comments,
                Type = "Comments",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Count of comments got from end point [video/comments](https://developers.facebook.com/docs/graph-api/reference/video/comments/).

**YouTube:** The number of times that users commented on a video. See [comments](https://developers.google.com/youtube/analytics/metrics#comments).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerView,
                Type = "Cost per View",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpV",
                Numerator = MetricType.ViewCost,
                Denominator = MetricType.Views,
                MarkdownSource = @"
**Facebook:** Daily average cost per view. Computed during transformation as:

        N(c) = number_of_actions_of_type(c)
        CpA(c) = cost_per_action_of_type(c)
        cost = sum(N(c) * CpA(c) for c in types_of_action)
        number_of_actions = sum(N(c) for c in types_of_action)
        cost_per_view = cost / number_of_actions

See `video_10_sec_watched_actions` and `cost_per_action_type` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** The average amount you pay each time someone views your ad. The average CPV is defined by the total cost of all ad views divided by the number of views. See [averagecpv](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpv).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerClick,
                Type = "Cost per Click",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpC",
                Numerator = MetricType.ClickCost,
                Denominator = MetricType.Clicks,
                MarkdownSource = @"
**Facebook:** Daily average cost for each click (all). See `cpc` at [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** the total cost of all clicks divided by the total number of clicks received. Values can be one of: a) a money amount in micros. See [averagecpc](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpc).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerEmailCapture,
                Type = "Cost per Email Capture",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpEmail",
                Numerator = MetricType.EmailCaptureCost,
                Denominator = MetricType.EmailCaptures,
                MarkdownSource = @"
**Facebook:** Daily average cost per email captures (event whose type contain string `lead`). Computed during transformation as:

        N(c) = number_of_actions_of_type(c)
        CpA(c) = cost_per_action_of_type(c)
        cost = sum(N(c) * CpA(c) for c in types_containg_string('lead'))
        number_of_actions = sum(N(c) for c in types_containg_string('lead'))
        cost_per_email_captures = cost / number_of_actions

See `video_10_sec_watched_actions` and `cost_per_action_type` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).
See `action_type` on [ad action stats](https://developers.facebook.com/docs/marketing-api/reference/ads-action-stats/).

**YouTube:** N/A.
"
            },
            new MetricInfo {
                TypeId = MetricType.Reach,
                Type = "Reach",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The daily number of people who saw your ads at least once.
    Reach is different from impressions, which may include multiple views of your ads by the same people.
    This metric is estimated.
See `reach` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** N/A."
            },
        };

        public static List<MetricInfo> ContentMetrics = new List<MetricInfo> {
            new MetricInfo {
                TypeId = MetricType.Views,
                Type = "Views",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** The (daily) number of times your video was watched for an aggregate of at least 3 seconds, or for nearly its total length, whichever happened first. Crossposted video views are not included in this counter. Got from `post_video_views` [post insights](https://developers.facebook.com/docs/graph-api/reference/v2.12/insights).

**YouTube:** The number of times that a video was viewed. See [views](https://developers.google.com/youtube/analytics/metrics#views)."
            },
            new MetricInfo {
                TypeId = MetricType.Likes,
                Type = "Likes",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Number of ""video reactions"" where type is ""LIKE"" . See [reactions](https://developers.facebook.com/docs/graph-api/reference/video/reactions/).

**YouTube:** The number of times that users indicated that they liked a video by giving it a positive rating. See [likes](https://developers.google.com/youtube/analytics/metrics#likes).

Note that the YouTube Analytics API sometimes returns a negative value.
"
            },
            new MetricInfo {
                TypeId = MetricType.Reactions,
                Type = "Reactions",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Number of ""video reactions"" of any type. See [reactions](https://developers.facebook.com/docs/graph-api/reference/video/reactions/).

**YouTube:** The number of times that users indicated that they liked a video by giving it a positive rating. See [likes](https://developers.google.com/youtube/analytics/metrics#likes).

 Note that the YouTube Analytics API sometimes returns a negative value."
            },
            new MetricInfo {
                TypeId = MetricType.Dislikes,
                Type = "Dislikes",
                Unit = "",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** N/A.

**YouTube:** The number of times that users indicated that they disliked a video by giving it a negative rating. See [dislikes](https://developers.google.com/youtube/analytics/metrics#dislikes).

Note that the YouTube Analytics API sometimes returns a negative value."
            },
            new MetricInfo {
                TypeId = MetricType.Shares,
                Type = "Shares",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Count of public sharedposts that shared this video. Posts are got from end point [video/sharedposts](https://developers.facebook.com/docs/graph-api/reference/video/sharedposts/).

**YouTube:** The number of times that users shared a video through the Share button. [shares](https://developers.google.com/youtube/analytics/metrics#shares).
"
            },
            new MetricInfo {
                TypeId = MetricType.Impressions,
                Type = "Impressions",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
Impressions is the number of times a post from your page is displayed,
whether the post is clicked or not. People may see multiple
impressions of the same post. Do not mistake it with `reach`. Reach is
the the number of unique people who received impressions of a page
post. Reach might be less than impressions since one person can see
multiple impressions.

**Facebook:** The number of impressions of the video (lifetime). Got from `total_video_impressions` on [video\_insights](https://developers.facebook.com/docs/graph-api/reference/v2.12/video/video_insights).

**YouTube:** N/A."
            },
            new MetricInfo {
                TypeId = MetricType.Comments,
                Type = "Comments",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** Count of comments got from end point [video/comments](https://developers.facebook.com/docs/graph-api/reference/video/comments/).

**YouTube:** The number of times that users commented on a video. See [comments](https://developers.google.com/youtube/analytics/metrics#comments).
"
            },
            new MetricInfo {
                TypeId = MetricType.ViewTime,
                Type = "View Time",
                Unit = "s",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                MarkdownSource = @"
**Facebook:** The (daily) total number of milliseconds your video was watched, including replays and views less than 3 seconds. Crossposted video view time are not included in this counter . Got from `post_video_view_time` on [post insights](https://developers.facebook.com/docs/graph-api/reference/v2.12/insights).

**YouTube:** The average length, in milliseconds, of video playbacks times the number of views. See [averageViewDuration](https://developers.google.com/youtube/analytics/metrics#averageViewDuration) and [views](https://developers.google.com/youtube/analytics/metrics#views). This value is fetched as seconds and converted to milliseconds for compatibility with FB's data.
"
            },
            new AverageMetric {
                TypeId = MetricType.AverageViewTime,
                Type = "Average View Time",
                Unit = "s",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "content",
                Abbreviation = "AVT",
                Numerator = MetricType.ViewTime,
                Denominator = MetricType.Views,
                MarkdownSource = @"This metric is computed by dividing the total View Time by the total View Count."
            },
            #if !RELEASE
            // The following metrics were temporalily disabled by request of YEAR-AP's stakeholders
            new MetricInfo {
                TypeId = MetricType.DemographicsViewCount,
                Type = "Demographics View Count",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.BAR,
                PageType = "content",
                MarkdownSource = "N/A"
            },
            new MetricInfo {
                TypeId = MetricType.DemographicsViewTime,
                Type = "Demographics View Time",
                Unit = "s",
                UnitSide = "right",
                ChartType = ChartType.BAR,
                PageType = "content",
                MarkdownSource = "N/A"
            },
            #endif
        };

        public static List<MetricInfo> MarketingMetrics = new List<MetricInfo> {
            new MetricInfo {
                TypeId = MetricType.Views,
                Type = "Views",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The daily number of times your video was watched for an aggregate of at least 10 seconds, or for nearly its total length, whichever happened first.
See `video_10_sec_watched_actions` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** the number of times your video ads were viewed. See [videoViews](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#videoviews)."
            },
            new AverageMetric {
                TypeId = MetricType.CostPerView,
                Type = "Cost per View",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpV",
                Numerator = MetricType.ViewCost,
                Denominator = MetricType.Views,
                MarkdownSource = @"
**Facebook:** Daily average cost per view. Computed during transformation as:

        N(c) = number_of_actions_of_type(c)
        CpA(c) = cost_per_action_of_type(c)
        cost = sum(N(c) * CpA(c) for c in types_of_action)
        number_of_actions = sum(N(c) for c in types_of_action)
        cost_per_view = cost / number_of_actions

See `video_10_sec_watched_actions` and `cost_per_action_type` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** The average amount you pay each time someone views your ad. The average CPV is defined by the total cost of all ad views divided by the number of views. See [averagecpv](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpv).
"
            },
            new MetricInfo {
                TypeId = MetricType.Clicks,
                Type = "Clicks",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The daily number of clicks on your ads. See `clicks` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** The number of clicks . See [clicks](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#clicks).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerClick,
                Type = "Cost per Click",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpC",
                Numerator = MetricType.ClickCost,
                Denominator = MetricType.Clicks,
                MarkdownSource = @"
**Facebook:** Daily average cost for each click (all). See `cpc` at [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** the total cost of all clicks divided by the total number of clicks received. Values can be one of: a) a money amount in micros. See [averagecpc](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpc).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerImpression,
                Type = "Cost per Impression",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpI",
                Numerator = MetricType.ImpressionCost,
                Denominator = MetricType.Impressions,
                MarkdownSource = @"
**Facebook:** Daily average cost for 1,000 impressions. See `cpm` at [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** Average Cost-per-thousand impressions (CPM). Values can be one of: a) a money amount in micros. See [averagecpm](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpm).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerEmailCapture,
                Type = "Cost per Email Capture",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpEmail",
                Numerator = MetricType.EmailCaptureCost,
                Denominator = MetricType.EmailCaptures,
                MarkdownSource = @"
**Facebook:** Daily average cost per email captures (event whose type contain string `lead`). Computed during transformation as:

        N(c) = number_of_actions_of_type(c)
        CpA(c) = cost_per_action_of_type(c)
        cost = sum(N(c) * CpA(c) for c in types_containg_string('lead'))
        number_of_actions = sum(N(c) for c in types_containg_string('lead'))
        cost_per_email_captures = cost / number_of_actions

See `video_10_sec_watched_actions` and `cost_per_action_type` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).
See `action_type` on [ad action stats](https://developers.facebook.com/docs/marketing-api/reference/ads-action-stats/).

**YouTube:** N/A.
"
            },
            new MetricInfo {
                TypeId = MetricType.Impressions,
                Type = "Impressions",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The daily number of times your ads were on screen. See `impressions` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** Count of how often your ad has appeared on a search results page or website on the Google Network. [impressions](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#impressions).
"
            },
            new MetricInfo {
                TypeId = MetricType.EmailCaptures,
                Type = "Email Captures",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The daily number of people who took an action  that was attributed to your ads.
    Only action whose types contain string ""lead"" are considered. This metric is estimated.
See `unique_actions` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).
See `action_type` on [ad action stats](https://developers.facebook.com/docs/marketing-api/reference/ads-action-stats/).

**YouTube:** N/A.
"
            },
            new MetricInfo {
                TypeId = MetricType.Reach,
                Type = "Reach",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The daily number of people who saw your ads at least once.
    Reach is different from impressions, which may include multiple views of your ads by the same people.
    This metric is estimated.
See `reach` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** N/A.
"
            },
            new MetricInfo {
                TypeId = MetricType.TotalCost,
                Type = "Total Cost",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** The estimated daily total amount of money you've spent on your ad during its schedule. This metric is estimated. See `spend` on [ad insights](https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/).

**YouTube:** The sum of your cost-per-click (CPC) and cost-per-thousand impressions (CPM) costs during this period. Values can be one of: a) a money amount in micros. See [cost](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#cost).
"
            },
            #if !RELEASE
            // The following metrics were disabled by request of YEAR-AP's stakeholders
            new MetricInfo {
                TypeId = MetricType.Engagements,
                Type = "Engagements",
                Unit = "",
                UnitSide = "right",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                MarkdownSource = @"
**Facebook:** N/A.

**YouTube:** Total number of ad engagements. See [engagements](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#engagements).
"
            },
            new AverageMetric {
                TypeId = MetricType.CostPerEngagement,
                Type = "Cost per Engagement",
                Unit = "$",
                UnitSide = "left",
                ChartType = ChartType.LINE,
                PageType = "marketing",
                Abbreviation = "CpEngag",
                Numerator = MetricType.EngagementCost,
                Denominator = MetricType.Engagements,
                MarkdownSource = @"
**Facebook:** N/A.

**YouTube:** The average amount that you've been charged for an ad engagement. This amount is the total cost of all ad engagements divided by the total number of ad engagements. See [averagecpe](https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpe).
"
            },
            #endif
        };
    }
}
