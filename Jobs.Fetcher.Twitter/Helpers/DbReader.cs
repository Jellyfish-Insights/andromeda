using System;
using System.Collections.Generic;
using System.Linq;
using Tweetinvi.Models.V2;
using Serilog.Core;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Helpers;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.Twitter.Data;
using DataLakeModels.Models.Twitter.Ads;

using LocalAdsAccount = DataLakeModels.Models.Twitter.Ads.AdsAccount;
using LocalCampaign = DataLakeModels.Models.Twitter.Ads.Campaign;
using LocalTweet = DataLakeModels.Models.Twitter.Data.Tweet;

namespace Jobs.Fetcher.Twitter.Helpers {

    public static class DbReader {

        public static User GetUserByUsername(string username, DataLakeTwitterDataContext dbContext, Logger logger) {
            try {
                return dbContext.Users.FirstOrDefault(u => u.Username == username);
            } catch (Exception e) {
                logger.Error($"Error when looking for User {username} in the database");
                logger.Verbose($"Error: {e}");
                return null;
            }
        }

        public static IEnumerable<LocalAdsAccount> GetAdsAccounts(string username, DataLakeTwitterAdsContext dbContext, Logger logger) {
            try {
                return dbContext.AdsAccounts.Where(a => a.Username == username).ToList();
            } catch (Exception e) {
                logger.Error($"Error when looking for AdsAccounts for {username} in the database");
                logger.Verbose($"Error: {e}");
                return new List<LocalAdsAccount>();
            }
        }

        public static IEnumerable<string> GetAdsAccountIds(string username, DataLakeTwitterAdsContext dbContext, Logger logger) {
            try {
                return dbContext.AdsAccounts.Where(a => a.Username == username).Select(a => a.Id).ToList();
            } catch (Exception e) {
                logger.Error($"Error when looking for AdsAccountIds for {username} in the database");
                logger.Verbose($"Error: {e}");
                return new List<string>();
            }
        }

        public static IEnumerable<LocalCampaign> GetRunningCampaigns(DataLakeTwitterAdsContext dbContext, Logger logger) {
            try {
                return dbContext.Campaigns.Where(c => c.EndTime != null && c.EndTime > DateTime.UtcNow).ToList();
            } catch (Exception e) {
                logger.Error($"Error when looking for Running Campaigns in the database");
                logger.Verbose($"Error: {e}");
                return new List<LocalCampaign>();
            }
        }

        public static IEnumerable<string> GetRunningCampaignIds(DataLakeTwitterAdsContext adsDbContext, Logger logger) {
            try {
                return adsDbContext.Campaigns.Where(c => c.EndTime != null && c.EndTime > DateTime.UtcNow).Select(c => c.Id).ToList();
            } catch (Exception e) {
                logger.Error($"Error when looking for Running Campaigns IDs in the database");
                logger.Verbose($"Error: {e}");
                return new List<string>();
            }
        }

        public static IEnumerable<string> GetTweetIdsFromUser(string userId, DataLakeTwitterDataContext dataDbContext, Logger logger) {
            try {
                return dataDbContext.Tweets.Where(t => t.UserId == userId).Select(t => t.Id).ToList();
            } catch (Exception e) {
                logger.Error($"Error when looking for Tweet IDs for user {userId} in the database");
                logger.Verbose($"Error: {e}");
                return new List<string>();
            }
        }

        public static DateTimeOffset GetOldestTweetDate(string userId, DataLakeTwitterDataContext dataDbContext, Logger logger) {
            try {
                return dataDbContext.Tweets
                           .Where(t => t.UserId == userId)
                           .OrderBy(t => t.CreatedAt)
                           .Select(t => t.CreatedAt)
                           .FirstOrDefault();
            } catch (Exception e) {
                logger.Error($"Error when looking for Oldest Tweet Date in the database");
                logger.Verbose($"Error: {e}");
                return DateTimeOffset.MinValue;
            }
        }

        public static LocalTweet GetLatestTweetFromUser(string userId, DataLakeTwitterDataContext dataDbContext, Logger logger) {
            try {
                return dataDbContext.Tweets
                           .Where(t => t.UserId == userId)
                           .OrderBy(t => t.CreatedAt)
                           .LastOrDefault();
            } catch (Exception e) {
                logger.Error($"Error when looking for Latest Tweet from {userId} in the database");
                logger.Verbose($"Error: {e}");
                return null;
            }
        }

        public static DateTimeOffset GetOrganicTweetDailyMetricsStartingDate(
            string userId,
            DataLakeTwitterDataContext dataDbContext,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            var dailyMetricsStatingDate =
                adsDbContext.OrganicTweetDailyMetrics
                    .GroupBy(p => p.TweetId)
                    .Select(g => g.Max(p => p.Date))
                    .OrderBy(d => d)
                    .FirstOrDefault();

            var tweetStartingDate = GetOldestTweetDate(userId, dataDbContext, logger);

            return dailyMetricsStatingDate < tweetStartingDate ? tweetStartingDate : dailyMetricsStatingDate;
        }

        public static DateTimeOffset GetPromotedTweetDailyMetricsStartingDate(
            string userId,
            DataLakeTwitterDataContext dataDbContext,
            DataLakeTwitterAdsContext adsDbContext,
            Logger logger) {

            var dailyMetricsStatingDate =
                adsDbContext.PromotedTweetDailyMetrics
                    .GroupBy(p => p.PromotedTweetId)
                    .Select(g => g.Max(p => p.Date))
                    .OrderBy(d => d)
                    .FirstOrDefault();

            var tweetStartingDate = GetOldestTweetDate(userId, dataDbContext, logger);

            return dailyMetricsStatingDate < tweetStartingDate ? tweetStartingDate : dailyMetricsStatingDate;
        }

        public static IEnumerable<string> GetPromotedTweetIdsFromUser(
            string userId,
            DataLakeTwitterDataContext dataContext,
            DataLakeTwitterAdsContext adsContext,
            Logger logger) {

            var tweetIds = GetTweetIdsFromUser(userId, dataContext, logger).ToHashSet<string>();

            return adsContext.PromotedTweets.Where(t => tweetIds.Contains(t.TweetId)).Select(t => t.Id).ToList();
        }
    }
}
