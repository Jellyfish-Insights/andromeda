using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Models.V2;
using Serilog.Core;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Helpers;
using DataLakeModels.Models.Twitter.Data;

using Andromeda.Common;

using Microsoft.EntityFrameworkCore;

using FlycatcherData.Models.V2;
using FlycatcherAds.Models;

using LocalTweet = DataLakeModels.Models.Twitter.Data.Tweet;
using LocalAdsAccount = DataLakeModels.Models.Twitter.Ads.AdsAccount;
using LocalCampaign = DataLakeModels.Models.Twitter.Ads.Campaign;
using LocalLineItem = DataLakeModels.Models.Twitter.Ads.LineItem;
using LocalPromotedTweet = DataLakeModels.Models.Twitter.Ads.PromotedTweet;
using LocalVideoLibrary = DataLakeModels.Models.Twitter.Ads.VideoLibrary;

using OrganicTweetDailyMetrics = DataLakeModels.Models.Twitter.Ads.OrganicTweetDailyMetrics;
using PromotedTweetDailyMetrics = DataLakeModels.Models.Twitter.Ads.PromotedTweetDailyMetrics;

using LocalCustomAudience = DataLakeModels.Models.Twitter.Ads.CustomAudience;

namespace Jobs.Fetcher.Twitter.Helpers {

    public static class DbWriter {

        public static void WriteUser(User newEntry, DataLakeTwitterDataContext dbContext, Logger logger) {

            var oldEntry = dbContext.Users.Find(newEntry.Id);
            Upsert<User, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteTimeline(TimelinesV2Response timeline, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (timeline.Tweets == null || timeline.Tweets.Length == 0) {
                return;
            }

            WriteTweets(timeline, dbContext, logger);
            WriteMedias(timeline, dbContext, logger);
            WriteTweetMedias(timeline, dbContext, logger);
        }

        public static void WriteTweets(TimelinesV2Response timeline, DataLakeTwitterDataContext dbContext, Logger logger) {

            var now = DateTime.UtcNow;

            foreach (var tweet in timeline.Tweets) {

                var oldEntry = dbContext.Tweets.Find(tweet.Id);
                var newEntry = new LocalTweet() {
                    Id = tweet.Id,
                    Text = tweet.Text,
                    UserId = tweet.AuthorId,
                    ConversationId = tweet.ConversationId,
                    CreatedAt = tweet.CreatedAt,
                    InReplyToUserId = tweet.InReplyToUserId,
                    Lang = tweet.Lang,
                    PossiblySensitive = tweet.PossiblySensitive,
                    Source = tweet.Source
                };

                Upsert<LocalTweet, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);

                WritePublicMetrics(tweet, dbContext, logger);
                WriteNonPublicMetrics(tweet, dbContext, logger);
                WriteOrganicMetrics(tweet, dbContext, logger);
                WritePromotedMetrics(tweet, dbContext, logger);
            }

            dbContext.SaveChanges();
        }

        public static void WritePublicMetrics(TweetV2 tweet, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (tweet.PublicMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;
            var oldEntry = dbContext.TweetPublicMetrics.SingleOrDefault(m => m.TweetId == tweet.Id && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new TweetPublicMetrics() {
                LikeCount = tweet.PublicMetrics.LikeCount,
                QuoteCount = tweet.PublicMetrics.QuoteCount,
                ReplyCount = tweet.PublicMetrics.ReplyCount,
                RetweetCount = tweet.PublicMetrics.RetweetCount,
                TweetId = tweet.Id,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue
            };

            Insert<TweetPublicMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteNonPublicMetrics(TweetV2 tweet, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (tweet.NonPublicMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;
            var oldEntry = dbContext.TweetNonPublicMetrics.SingleOrDefault(m => m.TweetId == tweet.Id && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new TweetNonPublicMetrics() {
                ImpressionCount = tweet.NonPublicMetrics.ImpressionCount,
                UrlLinkClicks = tweet.NonPublicMetrics.UrlLinkClicks,
                UserProfileClicks = tweet.NonPublicMetrics.UserProfileClicks,

                TweetId = tweet.Id,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue
            };

            Insert<TweetNonPublicMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteOrganicMetrics(TweetV2 tweet, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (tweet.OrganicMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;
            var oldEntry = dbContext.TweetOrganicMetrics.SingleOrDefault(m => m.TweetId == tweet.Id && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new TweetOrganicMetrics() {
                ImpressionCount = tweet.OrganicMetrics.ImpressionCount,
                LikeCount = tweet.OrganicMetrics.LikeCount,
                ReplyCount = tweet.OrganicMetrics.ReplyCount,
                RetweetCount = tweet.OrganicMetrics.RetweetCount,
                UrlLinkClicks = tweet.OrganicMetrics.UrlLinkClicks,
                UserProfileClicks = tweet.OrganicMetrics.UserProfileClicks,

                TweetId = tweet.Id,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue
            };

            Insert<TweetOrganicMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WritePromotedMetrics(TweetV2 tweet, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (tweet.PromotedMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;

            var oldEntry = dbContext.TweetPromotedMetrics.SingleOrDefault(m => m.TweetId == tweet.Id && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new TweetPromotedMetrics() {
                ImpressionCount = tweet.OrganicMetrics.ImpressionCount,
                LikeCount = tweet.OrganicMetrics.LikeCount,
                ReplyCount = tweet.OrganicMetrics.ReplyCount,
                RetweetCount = tweet.OrganicMetrics.RetweetCount,
                UrlLinkClicks = tweet.OrganicMetrics.UrlLinkClicks,
                UserProfileClicks = tweet.OrganicMetrics.UserProfileClicks,

                TweetId = tweet.Id,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue
            };

            Insert<TweetPromotedMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteMedias(TimelinesV2Response timeline, DataLakeTwitterDataContext dbContext, Logger logger) {

            var now = DateTime.UtcNow;

            foreach (var media in timeline.Includes?.Media ?? new MediaV2[0]) {
                var oldEntry = dbContext.Medias.Find(media.MediaKey);
                var newEntry = new Media() {
                    Id = media.MediaKey,
                    DurationMs = media.DurationMs,
                    Height = media.Height,
                    PreviewImageUrl = media.PreviewImageUrl,
                    Type = media.Type,
                    Url = media.Url,
                    Width = media.Width
                };

                Upsert<Media, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);

                WriteMediaPublicMetrics(media, dbContext, logger);
                WriteMediaNonPublicMetrics(media, dbContext, logger);
                WriteMediaOrganicMetrics(media, dbContext, logger);
                WriteMediaPromotedMetrics(media, dbContext, logger);
            }

            dbContext.SaveChanges();
        }

        public static void WriteMediaPublicMetrics(MediaV2 media, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (media.PublicMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;
            var oldEntry = dbContext.MediaPublicMetrics.SingleOrDefault(m => m.MediaId == media.MediaKey && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new MediaPublicMetrics() {
                MediaId = media.MediaKey,
                ViewCount = media.PublicMetrics.ViewCount,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue,
            };
            Insert<MediaPublicMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteMediaNonPublicMetrics(MediaV2 media, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (media.NonPublicMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;
            var oldEntry = dbContext.MediaNonPublicMetrics.SingleOrDefault(m => m.MediaId == media.MediaKey && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new MediaNonPublicMetrics() {
                MediaId = media.MediaKey,
                Playback0Count = media.NonPublicMetrics.Playback0Count,
                Playback25Count = media.NonPublicMetrics.Playback25Count,
                Playback50Count = media.NonPublicMetrics.Playback50Count,
                Playback75Count = media.NonPublicMetrics.Playback75Count,
                Playback100Count = media.NonPublicMetrics.Playback100Count,

                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue
            };
            Insert<MediaNonPublicMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteMediaOrganicMetrics(MediaV2 media, DataLakeTwitterDataContext dbContext, Logger logger) {

            if (media.OrganicMetrics == null) {
                return;
            }

            var now = DateTime.UtcNow;
            var oldEntry = dbContext.MediaOrganicMetrics.SingleOrDefault(m => m.MediaId == media.MediaKey && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new MediaOrganicMetrics() {
                MediaId = media.MediaKey,
                Playback0Count = media.OrganicMetrics.Playback0Count,
                Playback25Count = media.OrganicMetrics.Playback25Count,
                Playback50Count = media.OrganicMetrics.Playback50Count,
                Playback75Count = media.OrganicMetrics.Playback75Count,
                Playback100Count = media.OrganicMetrics.Playback100Count,
                ViewCount = media.OrganicMetrics.ViewCount,

                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue,
            };
            Insert<MediaOrganicMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteMediaPromotedMetrics(MediaV2 media, DataLakeTwitterDataContext dbContext, Logger logger) {
            if (media.PromotedMetrics == null) {
                return;
            }
            var now = DateTime.UtcNow;
            var oldEntry = dbContext.MediaPromotedMetrics.SingleOrDefault(m => m.MediaId == media.MediaKey && m.ValidityStart <= now && m.ValidityEnd > now);
            var newEntry = new MediaPromotedMetrics() {
                MediaId = media.MediaKey,
                Playback0Count = media.PromotedMetrics.Playback0Count,
                Playback25Count = media.PromotedMetrics.Playback25Count,
                Playback50Count = media.PromotedMetrics.Playback50Count,
                Playback75Count = media.PromotedMetrics.Playback75Count,
                Playback100Count = media.PromotedMetrics.Playback100Count,
                ViewCount = media.PromotedMetrics.ViewCount,

                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue,
            };
            Insert<MediaPromotedMetrics, DataLakeTwitterDataContext>(oldEntry, newEntry, dbContext, logger);
        }

        public static void WriteTweetMedias(TimelinesV2Response timeline, DataLakeTwitterDataContext dbContext, Logger logger) {

            foreach (var tweet in timeline.Tweets) {

                foreach (var MediaKey in tweet.Attachments?.MediaKeys ?? Enumerable.Empty<string>()) {

                    if (dbContext.TweetMedia.Any(m => m.TweetId == tweet.Id && m.MediaId == MediaKey)) {
                        continue;
                    }

                    var newEntry = new TweetMedia() {
                        MediaId = MediaKey,
                        TweetId = tweet.Id,
                    };
                    Upsert<TweetMedia, DataLakeTwitterDataContext>(null, newEntry, dbContext, logger);
                }
            }
            dbContext.SaveChanges();
        }

        public static void WriteAdsAccounts(
            string userId,
            string username,
            AdsAccountsResponse adsAccounts,
            DataLakeTwitterAdsContext dbContext,
            Logger logger) {

            foreach (var adsAccount in adsAccounts.Accounts ?? new FlycatcherAds.Models.AdsAccount[0]) {

                var oldEntry = dbContext.AdsAccounts.Find(adsAccount.Id);

                var newEntry = new LocalAdsAccount() {
                    Id = adsAccount.Id,
                    UserId = userId,
                    Username = username,
                    Name = adsAccount.Name,
                    BusinessName = adsAccount.BusinessName,
                    TimeZone = adsAccount.TimeZone,
                    TimeZoneSwitchAt = adsAccount.TimeZoneSwitchAt,
                    CreatedAt = adsAccount.CreatedAt,
                    UpdatedAt = adsAccount.UpdatedAt,
                    BusinessId = adsAccount.BusinessId,
                    ApprovalStatus = adsAccount.ApprovalStatus,
                    Deleted = adsAccount.Deleted
                };
                Upsert<LocalAdsAccount, DataLakeTwitterAdsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteCampaigns(
            string accountId,
            CampaignsResponse campaigns,
            DataLakeTwitterAdsContext dbContext,
            Logger logger) {
            var now = DateTime.UtcNow;

            foreach (var campaign in campaigns.Campaigns ?? new FlycatcherAds.Models.Campaign[0]) {

                var oldEntry =
                    dbContext.Campaigns
                        .SingleOrDefault(c => c.Id == campaign.Id && c.ValidityStart <= now && c.ValidityEnd > now);

                var newEntry = new LocalCampaign() {
                    Id = campaign.Id,
                    AdsAccountId = accountId,
                    Name = campaign.Name,
                    StartTime = campaign.StartTime,
                    EndTime = campaign.EndTime,
                    Servable = campaign.Servable,
                    PurchaseOrderNumber = campaign.PurchaseOrderNumber,
                    EffectiveStatus = campaign.EffectiveStatus,
                    DailyBudgetAmountLocalMicro = campaign.DailyBudgetAmountLocalMicro,
                    FundingInstrumentId = campaign.FundingInstrumentId,
                    DurationInDays = campaign.DurationInDays,
                    StandardDelivery = campaign.StandardDelivery,
                    TotalBudgetAmountLocalMicro = campaign.TotalBudgetAmountLocalMicro,
                    EntityStatus = campaign.EntityStatus,
                    FrequencyCap = campaign.FrequencyCap,
                    Currency = campaign.Currency,
                    CreatedAt = campaign.CreatedAt,
                    UpdatedAt = campaign.UpdatedAt,
                    Deleted = campaign.Deleted,

                    ValidityStart = now,
                    ValidityEnd = DateTime.MaxValue,
                };
                Insert<LocalCampaign, DataLakeTwitterAdsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteLineItems(
            string accountId,
            LineItemsResponse lineItems,
            DataLakeTwitterAdsContext dbContext,
            Logger logger) {
            var now = DateTime.UtcNow;

            foreach (var lineItem in lineItems.LineItems ?? new FlycatcherAds.Models.LineItem[0]) {

                var oldEntry =
                    dbContext.LineItems
                        .SingleOrDefault(l => l.Id == lineItem.Id && l.ValidityStart <= now && l.ValidityEnd > now);

                var newEntry = new LocalLineItem() {
                    Id = lineItem.Id,
                    CampaignId = lineItem.CampaignId,
                    Name = lineItem.Name,
                    StartTime = lineItem.StartTime,
                    BidAmountLocalMicro = lineItem.BidAmountLocalMicro,
                    AdvertiserDomain = lineItem.AdvertiserDomain,
                    TargetCpaLocalMicro = lineItem.TargetCpaLocalMicro,
                    PrimaryWebEventTag = lineItem.PrimaryWebEventTag,
                    Goal = lineItem.Goal.ToString(),
                    ProductType = lineItem.ProductType.ToString(),
                    EndTime = lineItem.EndTime,
                    BidStrategy = lineItem.BidStrategy.ToString(),
                    DurationInDays = lineItem.DurationInDays,
                    TotalBudgetAmountLocalMicro = lineItem.TotalBudgetAmountLocalMicro,
                    Objective = lineItem.Objective.ToString(),
                    EntityStatus = lineItem.EntityStatus.ToString(),
                    FrequencyCap = lineItem.FrequencyCap,
                    Currency = lineItem.Currency,
                    PayBy = lineItem.PayBy.ToString(),
                    CreatedAt = lineItem.CreatedAt,
                    UpdatedAt = lineItem.UpdatedAt,
                    CreativeSource = lineItem.CreativeSource,
                    Deleted = lineItem.Deleted,

                    ValidityStart = now,
                    ValidityEnd = DateTime.MaxValue
                };
                Insert<LocalLineItem, DataLakeTwitterAdsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WritePromotedTweets(
            string accountId,
            PromotedTweetsResponse promotedTweets,
            DataLakeTwitterAdsContext dbContext,
            Logger logger) {

            var now = DateTime.UtcNow;

            foreach (var promotedTweet in promotedTweets.PromotedTweets ?? new FlycatcherAds.Models.PromotedTweet[0]) {

                var lineItem =
                    dbContext.LineItems
                        .SingleOrDefault(l => l.Id == promotedTweet.LineItemId && l.ValidityStart <= now && l.ValidityEnd > now);

                if (lineItem == null) {
                    logger.Debug($"LineItem {promotedTweet.LineItemId} not found");
                    continue;
                }

                var oldEntry =
                    dbContext.PromotedTweets.
                        SingleOrDefault(p => p.Id == promotedTweet.Id && p.ValidityStart <= now && p.ValidityEnd > now);

                var newEntry = new LocalPromotedTweet() {
                    Id = promotedTweet.Id,
                    LineItemId = promotedTweet.LineItemId,
                    TweetId = promotedTweet.TweetId,
                    EntityStatus = promotedTweet.EntityStatus,
                    CreatedAt = promotedTweet.CreatedAt,
                    UpdatedAt = promotedTweet.UpdatedAt,
                    TweetIdSt = promotedTweet.TweetIdSt,
                    ApprovalStatus = promotedTweet.ApprovalStatus,
                    Deleted = promotedTweet.Deleted,
                    CampaignId = lineItem.CampaignId,

                    ValidityStart = now,
                    ValidityEnd = DateTime.MaxValue
                };
                Insert<LocalPromotedTweet, DataLakeTwitterAdsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteVideoLibraries(
            string username,
            MediaLibraryResponse videoLibraries,
            DataLakeTwitterAdsContext dbContext,
            Logger logger) {

            var now = DateTime.UtcNow;

            foreach (var media in videoLibraries.MediaLibraries ?? new FlycatcherAds.Models.BaseMediaLibrary[0]) {

                var videoLibrary = media as FlycatcherAds.Models.VideoLibrary;

                if (null == videoLibrary) {
                    continue;
                }

                var oldEntry =
                    dbContext.VideoLibraries
                        .SingleOrDefault(m => m.Id == videoLibrary.MediaKey && m.ValidityStart <= now && m.ValidityEnd >= now);

                var newEntry = new LocalVideoLibrary() {
                    Id = videoLibrary.MediaKey,
                    PosterMediaKey = videoLibrary.PosterMediaKey,
                    Title = videoLibrary.Title,
                    Name = videoLibrary.Name,
                    Description = videoLibrary.Description,
                    MediaStatus = videoLibrary.MediaStatus,
                    MediaUrl = videoLibrary.MediaUrl,
                    PosterMediaUrl = videoLibrary.PosterMediaUrl,
                    AspectRatio = videoLibrary.AspectRatio,
                    CreatedAt = videoLibrary.CreatedAt,
                    UpdatedAt = videoLibrary.UpdatedAt,
                    Tweeted = videoLibrary.Tweeted,
                    Deleted = videoLibrary.Deleted,
                    Username = username,
                    Duration = videoLibrary.Duration,
                    FileName = videoLibrary.FileName,

                    ValidityStart = now,
                    ValidityEnd = DateTime.MaxValue,
                };
                Insert<LocalVideoLibrary, DataLakeTwitterAdsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteOrganicTweetDailyMetrics(
            string adsAccountId,
            DateTime start,
            DateTime end,
            SynchronousAnalyticsResponse analytics,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            if ("stats" != analytics.DataType) {
                logger.Debug($"Invalid analytics response: {analytics.DataType}");
                return;
            }

            if ((end - start).Days != analytics.TimeSeriesLength) {
                logger.Debug($"Invalid time series length. Expected {(end - start).Days}, got {analytics.TimeSeriesLength}");
                return;
            }

            foreach (var statisticsData in analytics.StatisticsData ?? new FlycatcherAds.Models.StatisticsData[0]) {

                WriteOrganicTweetDailyMetrics(
                    adsAccountId,
                    start,
                    analytics.TimeSeriesLength,
                    statisticsData,
                    adsDbContext,
                    logger);
            }
        }

        private static int GetMetrics(Dictionary<string, List<int>> metrics, string key, int index) {
            if (metrics != null && metrics.ContainsKey(key) && metrics[key] != null && metrics[key].Count > index) {
                return metrics[key][index];
            }
            return 0;
        }

        private static bool MissingMetrics(Dictionary<string, List<int>> metrics, IEnumerable<string> keys) {

            if (metrics == null) {
                return true;
            }

            foreach (var key in keys) {
                if (metrics.ContainsKey(key) && metrics[key] != null && metrics[key].Count > 0) {
                    return false;
                }
            }

            return true;
        }

        private static void WriteOrganicTweetDailyMetrics(
            string adsAccountId,
            DateTime start,
            int timeSeriesLength,
            FlycatcherAds.Models.StatisticsData statisticsData,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            var tweetId = statisticsData.Id;

            var now = DateTime.UtcNow;

            foreach (var statistics in statisticsData.Statistics ?? new FlycatcherAds.Models.Statistics[0]) {

                if (MissingMetrics(statistics.Metrics, OrganicTweetDailyMetrics.RequiredMetrics())) {
                    continue;
                }

                for (var i = 0; i < timeSeriesLength; i++) {

                    var date = DateHelper.GetDateOnly(start.AddDays(i));

                    var oldEntry =
                        adsDbContext.OrganicTweetDailyMetrics
                            .Where(o => o.TweetId == tweetId && o.Date == date && o.ValidityStart <= now && o.ValidityEnd >= now)
                            .SingleOrDefault();

                    var newEntry = new OrganicTweetDailyMetrics() {
                        Engagements = GetMetrics(statistics.Metrics, "engagements", i),
                        Impressions = GetMetrics(statistics.Metrics, "impressions", i),
                        Retweets = GetMetrics(statistics.Metrics, "retweets", i),
                        Replies = GetMetrics(statistics.Metrics, "replies", i),
                        Likes = GetMetrics(statistics.Metrics, "likes", i),
                        Follows = GetMetrics(statistics.Metrics, "follows", i),
                        CardEngagements = GetMetrics(statistics.Metrics, "card_engagements", i),
                        Clicks = GetMetrics(statistics.Metrics, "clicks", i),
                        AppClicks = GetMetrics(statistics.Metrics, "app_clicks", i),
                        UrlClicks = GetMetrics(statistics.Metrics, "url_clicks", i),
                        QualifiedImpressions = GetMetrics(statistics.Metrics, "qualified_impressions", i),

                        VideoTotalViews = GetMetrics(statistics.Metrics, "video_total_views", i),
                        VideoViews25 = GetMetrics(statistics.Metrics, "video_views_25", i),
                        VideoViews50 = GetMetrics(statistics.Metrics, "video_views_50", i),
                        VideoViews75 = GetMetrics(statistics.Metrics, "video_views_75", i),
                        VideoViews100 = GetMetrics(statistics.Metrics, "video_views_100", i),
                        VideoCtaClicks = GetMetrics(statistics.Metrics, "video_cta_clicks", i),
                        VideoContentStarts = GetMetrics(statistics.Metrics, "video_content_starts", i),
                        Video3s100pctViews = GetMetrics(statistics.Metrics, "video_3s100pct_views", i),
                        Video6sViews = GetMetrics(statistics.Metrics, "video_6s_views", i),
                        Video15sViews = GetMetrics(statistics.Metrics, "video_15s_views", i),

                        TweetId = tweetId,
                        Date = date,
                        ValidityStart = now,
                        ValidityEnd = DateTime.MaxValue
                    };

                    Insert<OrganicTweetDailyMetrics, DataLakeTwitterAdsContext>(oldEntry, newEntry, adsDbContext, logger);
                }
            }
            adsDbContext.SaveChanges();
        }

        public static void WritePromotedTweetDailyMetrics(
            string adsAccountId,
            DateTime start,
            DateTime end,
            SynchronousAnalyticsResponse analytics,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            if ("stats" != analytics.DataType) {
                logger.Debug($"Invalid analytics response: {analytics.DataType}");
                return;
            }

            if ((end - start).Days != analytics.TimeSeriesLength) {
                logger.Debug($"Invalid time series length. Expected {(end - start).Days}, got {analytics.TimeSeriesLength}");
                return;
            }

            foreach (var statisticsData in analytics.StatisticsData ?? new FlycatcherAds.Models.StatisticsData[0]) {

                WritePromotedTweetDailyMetrics(
                    adsAccountId,
                    start,
                    analytics.TimeSeriesLength,
                    statisticsData,
                    adsDbContext,
                    logger);
            }
        }

        private static void WritePromotedTweetDailyMetrics(
            string adsAccountId,
            DateTime start,
            int timeSeriesLength,
            FlycatcherAds.Models.StatisticsData statisticsData,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            var promotedTweetId = statisticsData.Id;

            var now = DateTime.UtcNow;

            foreach (var statistics in statisticsData.Statistics ?? new FlycatcherAds.Models.Statistics[0]) {

                if (MissingMetrics(statistics.Metrics, PromotedTweetDailyMetrics.RequiredMetrics())) {
                    continue;
                }

                for (var i = 0; i < timeSeriesLength; i++) {

                    var date = DateHelper.GetDateOnly(start.AddDays(i));

                    var oldEntry =
                        adsDbContext.PromotedTweetDailyMetrics
                            .Where(o => o.PromotedTweetId == promotedTweetId && o.ValidityStart <= now && o.ValidityEnd >= now)
                            .SingleOrDefault();

                    var newEntry = new PromotedTweetDailyMetrics() {
                        Engagements = GetMetrics(statistics.Metrics, "engagements", i),
                        Impressions = GetMetrics(statistics.Metrics, "impressions", i),
                        Retweets = GetMetrics(statistics.Metrics, "retweets", i),
                        Replies = GetMetrics(statistics.Metrics, "replies", i),
                        Likes = GetMetrics(statistics.Metrics, "likes", i),
                        Follows = GetMetrics(statistics.Metrics, "follows", i),
                        CardEngagements = GetMetrics(statistics.Metrics, "card_engagements", i),
                        Clicks = GetMetrics(statistics.Metrics, "clicks", i),
                        AppClicks = GetMetrics(statistics.Metrics, "app_clicks", i),
                        UrlClicks = GetMetrics(statistics.Metrics, "url_clicks", i),
                        QualifiedImpressions = GetMetrics(statistics.Metrics, "qualified_impressions", i),

                        VideoTotalViews = GetMetrics(statistics.Metrics, "video_total_views", i),
                        VideoViews25 = GetMetrics(statistics.Metrics, "video_views_25", i),
                        VideoViews50 = GetMetrics(statistics.Metrics, "video_views_50", i),
                        VideoViews75 = GetMetrics(statistics.Metrics, "video_views_75", i),
                        VideoViews100 = GetMetrics(statistics.Metrics, "video_views_100", i),
                        VideoCtaClicks = GetMetrics(statistics.Metrics, "video_cta_clicks", i),
                        VideoContentStarts = GetMetrics(statistics.Metrics, "video_content_starts", i),
                        Video3s100pctViews = GetMetrics(statistics.Metrics, "video_3s100pct_views", i),
                        Video6sViews = GetMetrics(statistics.Metrics, "video_6s_views", i),
                        Video15sViews = GetMetrics(statistics.Metrics, "video_15s_views", i),

                        MediaViews = GetMetrics(statistics.Metrics, "media_views", i),
                        MediaEngagements = GetMetrics(statistics.Metrics, "media_engagements", i),

                        BilledEngagements = GetMetrics(statistics.Metrics, "billed_engagements", i),
                        BilledChargeLocalMicro = GetMetrics(statistics.Metrics, "billed_charge_local_micro", i),

                        PromotedTweetId = promotedTweetId,
                        Date = date,
                        ValidityStart = now,
                        ValidityEnd = DateTime.MaxValue
                    };

                    Insert<PromotedTweetDailyMetrics, DataLakeTwitterAdsContext>(oldEntry, newEntry, adsDbContext, logger);
                }
            }
            adsDbContext.SaveChanges();
        }

        public static void WriteCustomAudiences(
            CustomAudiencesResponse response,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            var now = DateTime.UtcNow;

            foreach (var customAudience in response.CustomAudiences ?? new FlycatcherAds.Models.CustomAudience[0]) {

                var oldEntry = adsDbContext.CustomAudiences.Find(customAudience.Id);

                var newEntry = new LocalCustomAudience() {
                    Id = customAudience.Id,
                    Targetable = customAudience.Targetable,
                    Name = customAudience.Name,
                    TargetableTypes = customAudience.TargetableTypes,
                    AudienceType = customAudience.AudienceType,
                    Description = customAudience.Description,
                    PermissionLevel = customAudience.PermissionLevel,
                    OwnerAccountId = customAudience.OwnerAccountId,
                    ReasonsNotTargetable = customAudience.ReasonsNotTargetable,
                    CreatedAt = customAudience.CreatedAt,
                    UpdatedAt = customAudience.UpdatedAt,
                    PartnerSource = customAudience.PartnerSource,
                    Deleted = customAudience.Deleted,
                    AudienceSize = customAudience.AudienceSize
                };

                Upsert<LocalCustomAudience, DataLakeTwitterAdsContext>(oldEntry, newEntry, adsDbContext, logger);
            }
            adsDbContext.SaveChanges();
        }

        private static void Upsert<T, Context>(
            T oldEntry,
            T newEntry,
            DbContext dbContext,
            Logger logger) where T : IEquatable<T> where Context : DbContext {
            var modified = CompareEntries.CompareOldAndNewEntry<T>(oldEntry, newEntry);
            switch (modified) {
                case Modified.New:
                    logger.Debug("Inserting new {Type}: {Id}", typeof(T).Name, newEntry);
                    (dbContext as Context).Add(newEntry);
                    break;
                case Modified.Updated:
                    logger.Debug("Found update to {Type}: {Id}", typeof(T).Name, newEntry);
                    (dbContext as Context).Entry(oldEntry).CurrentValues.SetValues(newEntry);
                    break;
                default:
                    break;
            }
        }

        private static void Insert<T, Context>(
            T oldEntry,
            T newEntry,
            DbContext dbContext,
            Logger logger) where T : IValidityRange, IEquatable<T> where Context : DbContext {
            var modified = CompareEntries.CompareOldAndNewEntry<T>(oldEntry, newEntry);
            switch (modified) {
                case Modified.New:
                    logger.Debug("Inserting new {Type}: {Id}", typeof(T).Name, newEntry);
                    break;
                case Modified.Updated:
                    logger.Debug("Found update to {Type}: {Id}", typeof(T).Name, newEntry);
                    oldEntry.ValidityEnd = newEntry.ValidityStart;
                    (dbContext as Context).Update(oldEntry);
                    break;
                default:
                    return;
            }
            (dbContext as Context).Add(newEntry);
        }
    }
}
