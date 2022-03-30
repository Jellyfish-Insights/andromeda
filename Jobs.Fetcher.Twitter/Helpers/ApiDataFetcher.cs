using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Tweetinvi;
using Tweetinvi.Core.Iterators;
using Tweetinvi.Iterators;
using Tweetinvi.Core.Web;
using Tweetinvi.Models.V2;
using DataLakeModels.Models.Twitter.Data;
using Tweetinvi.Parameters.V2;

using FlycatcherAds;
using FlycatcherAds.Models;
using FlycatcherAds.Parameters;
using FlycatcherAds.Client;

using FlycatcherData;
using FlycatcherData.Parameters.V2;
using FlycatcherData.Models.V2;

using Andromeda.Common;
using Andromeda.Common.Extensions;

using LocalAdsAccount = DataLakeModels.Models.Twitter.Ads.AdsAccount;
using LocalTweet = DataLakeModels.Models.Twitter.Data.Tweet;

namespace Jobs.Fetcher.Twitter.Helpers {

    public static class ApiDataFetcher {

        private static readonly DateTime startDate = new DateTime(2021, 1, 1);

        public static User GetUserByName(string username, TwitterDataClient client) {

            var userV2Response = client.UsersV2.GetUserByNameAsync(username).GetAwaiter().GetResult();

            if (userV2Response != null && userV2Response.User != null) {

                UserV2 userV2 = userV2Response.User;

                var localUser = new User() {
                    Id = userV2.Id,
                    CreatedAt = userV2.CreatedAt,
                    Location = userV2.Location,
                    Name = userV2.Name,
                    ProfileImageUrl = userV2.ProfileImageUrl,
                    IsProtected = userV2.IsProtected,
                    Url = userV2.Url,
                    Username = userV2.Username,
                    Verified = userV2.Verified
                };

                return localUser;
            }

            return null;
        }

        private static IGetTimelinesV2Parameters GetTimelineParameters(string userId, DateTime start, DateTime end) {

            var timelineParameters = new GetTimelinesV2Parameters(userId);

            timelineParameters.ClearAllFields();

            timelineParameters.Expansions.Add(TweetResponseFields.Expansions.AuthorId);
            timelineParameters.Expansions.Add(TweetResponseFields.Expansions.AttachmentsMediaKeys);

            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.AuthorId);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.ConversationId);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.CreatedAt);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.InReplyToUserId);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.Lang);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.PossiblySensitive);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.Source);

            timelineParameters.MediaFields.Add(TweetResponseFields.Media.DurationMs);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.Height);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.MediaKey);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.PreviewImageUrl);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.Type);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.Url);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.Width);

            // The default value is 10
            timelineParameters.MaxResults = 100;

            // set the start and end time
            timelineParameters.StartTime = start;
            timelineParameters.EndTime = end;

            return timelineParameters;
        }

        private static IGetTimelinesV2Parameters GetTimeLineParametersWithPublicMetrics(string userId, DateTime start, DateTime end) {

            var timelineParameters = GetTimelineParameters(userId, start, end);

            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.PublicMetrics);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.PublicMetrics);

            return timelineParameters;
        }

        private static IGetTimelinesV2Parameters GetTimelineParametersWithNonPublicMetrics(string userId, DateTime start, DateTime end) {

            var timelineParameters = GetTimelineParameters(userId, start, end);

            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.NonPublicMetrics);
            timelineParameters.TweetFields.Add(TweetResponseFields.Tweet.OrganicMetrics);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.NonPublicMetrics);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.OrganicMetrics);

            return timelineParameters;
        }

        private static IGetTimelinesV2Parameters GetTimelineParametersWithAllMetrics(string userId, DateTime start, DateTime end) {

            var timelineParameters = GetTimelineParametersWithNonPublicMetrics(userId, start, end);
            timelineParameters.MediaFields.Add(TweetResponseFields.Media.PromotedMetrics);

            return timelineParameters;
        }

        public static void GetUserTweetsTimeline(
            string userId,
            LocalTweet latestTweet,
            TwitterDataClient client,
            Action<ITwitterRequestIterator<TimelinesV2Response, string>> Callback,
            Logger logger) {

            var dateRange = DateHelper.GetSemestersRange(latestTweet?.CreatedAt.DateTime ?? startDate, DateTime.UtcNow);

            foreach (var item in dateRange) {

                var start = item.Item1;
                var end = item.Item2;

                logger.Information("Fetching with Public Metrics");
                Callback(GetUserTweetsTimelineIterator(GetTimeLineParametersWithPublicMetrics(userId, start, end), client));

                logger.Information("Fetching with Non Public Metrics");
                Callback(GetUserTweetsTimelineIterator(GetTimelineParametersWithNonPublicMetrics(userId, start, end), client));

                logger.Information("Fetching with All Metrics ");
                Callback(GetUserTweetsTimelineIterator(GetTimelineParametersWithAllMetrics(userId, start, end), client));
            }
        }

        public static ITwitterRequestIterator<TimelinesV2Response, string> GetUserTweetsTimelineIterator(IGetTimelinesV2Parameters parameters, TwitterDataClient client) {
            return client.TimelinesV2.GetUserTweetsTimelineIterator(parameters);
        }

        public static ITwitterRequestIterator<AdsAccountsResponse, string> GetAdsAccountsIterator(TwitterAdsClient client) {

            return client.AdsAccountsClient.GetAccountsIterator(new GetAdsAccountsParameters());
        }

        public static void GetAdsAccounts(
            TwitterAdsClient client,
            Action<ITwitterRequestIterator<AdsAccountsResponse, string>> Callback) {

            Callback(client.AdsAccountsClient.GetAccountsIterator(new GetAdsAccountsParameters()));
        }

        public static void GetCampaigns(
            string accountId,
            ITwitterAdsClient client,
            Action<string, ITwitterRequestIterator<CampaignsResponse, string>> Callback) {

            Callback(accountId, client.CampaignsClient.GetCampaignsIterator(new GetCampaignsParameters() { AccountId = accountId }));
        }

        public static void GetLineItems(
            string adsAccountId,
            ITwitterAdsClient client,
            Action<string, ITwitterRequestIterator<LineItemsResponse, string>> Callback) {

            Callback(adsAccountId, client.LineItemsClient.GetLineItemsIterator(new GetLineItemsParameters() { AccountId = adsAccountId }));
        }

        public static void GetPromotedTweets(
            string adsAccountId,
            ITwitterAdsClient client,
            Action<string, ITwitterRequestIterator<PromotedTweetsResponse, string>> Callback) {

            Callback(adsAccountId, client.PromotedTweetsClient.GetPromotedTweetsIterator(new GetPromotedTweetsParameters() { AccountId = adsAccountId }));
        }

        public static void GetVideoLibraries(
            string adsAccountId,
            ITwitterAdsClient client,
            Action<ITwitterRequestIterator<MediaLibraryResponse, string>> Callback) {

            Callback(client.MediaLibraryClient.GetMediaLibraryIterator(new GetMediaLibraryParameters() { AccountId = adsAccountId, MediaType = "VIDEO" }));
        }

        public static async Task GetOrganicTweetDailyMetricsReport(
            LocalAdsAccount adsAccount,
            DateTimeOffset startDate,
            IEnumerable<string> tweetIds,
            ITwitterAdsClient client,
            Action<string, DateTime, DateTime, SynchronousAnalyticsResponse> Callback) {

            var parameters = new GetSynchronousAnalyticsParameters() {
                AccountId = adsAccount.Id,
                EntityType = EntityType.ORGANIC_TWEET,
                Granularity = FlycatcherAds.Models.Granularity.DAY,
                Placement = Placement.ALL_ON_TWITTER
            };

            parameters.MetricGroups.Add(MetricGroup.ENGAGEMENT);
            parameters.MetricGroups.Add(MetricGroup.VIDEO);

            foreach (var item in DateHelper.GetWeeksRange(startDate.DateTime, DateTime.UtcNow)) {

                var start = item.Item1;
                var end = item.Item2;

                foreach (var batch in tweetIds.SplitIntoBatches(20)) {

                    parameters.EntityIds = batch.ToHashSet<string>();
                    parameters.StartTime = DateHelper.AddTimezoneOffset(start, adsAccount.TimeZone);
                    parameters.EndTime = DateHelper.AddTimezoneOffset(end, adsAccount.TimeZone);

                    var result = await client.AnalyticsClient.GetSynchronousAnalyticsAsync(parameters).ConfigureAwait(false);

                    Callback(adsAccount.Id, start, end, result);
                }
            }
        }

        public static async Task GetPromotedTweetDailyMetricsReport(
            LocalAdsAccount adsAccount,
            DateTimeOffset startDate,
            IEnumerable<string> promotedTweetIds,
            ITwitterAdsClient client,
            Action<string, DateTime, DateTime, SynchronousAnalyticsResponse> Callback) {

            var parameters = new GetSynchronousAnalyticsParameters() {
                AccountId = adsAccount.Id,
                EntityType = EntityType.PROMOTED_TWEET,
                Granularity = FlycatcherAds.Models.Granularity.DAY,
                Placement = Placement.ALL_ON_TWITTER
            };

            parameters.MetricGroups.Add(MetricGroup.ENGAGEMENT);
            parameters.MetricGroups.Add(MetricGroup.VIDEO);
            parameters.MetricGroups.Add(MetricGroup.BILLING);
            parameters.MetricGroups.Add(MetricGroup.MEDIA);

            foreach (var item in DateHelper.GetWeeksRange(startDate.DateTime, DateTime.UtcNow)) {

                var start = item.Item1;
                var end = item.Item2;

                foreach (var batch in promotedTweetIds.SplitIntoBatches(20)) {

                    parameters.EntityIds = batch.ToHashSet<string>();
                    parameters.StartTime = DateHelper.AddTimezoneOffset(start, adsAccount.TimeZone);
                    parameters.EndTime = DateHelper.AddTimezoneOffset(end, adsAccount.TimeZone);

                    var result = await client.AnalyticsClient.GetSynchronousAnalyticsAsync(parameters).ConfigureAwait(false);

                    Callback(adsAccount.Id, start, end, result);
                }
            }
        }

        public static void GetCustomAudiences(
            string adsAccountId,
            ITwitterAdsClient client,
            Action<ITwitterRequestIterator<CustomAudiencesResponse, string>> Callback) {
            Callback(client.CustomAudiencesClient.GetCustomAudiencesIterator(new GetCustomAudiencesParameters() { AccountId = adsAccountId }));
        }
    }
}
