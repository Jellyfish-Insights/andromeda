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

        public static User GetUserByUsername(string username, DataLakeTwitterDataContext dbContext) {
            return dbContext.Users.FirstOrDefault(u => u.Username == username);
        }

        public static IEnumerable<LocalAdsAccount> GetAdsAccounts(string username, DataLakeTwitterAdsContext dbContext) {
            return dbContext.AdsAccounts.Where(a => a.Username == username).ToList();
        }

        public static IEnumerable<string> GetAdsAccountIds(string username, DataLakeTwitterAdsContext dbContext) {
            return dbContext.AdsAccounts.Where(a => a.Username == username).Select(a => a.Id).ToList();
        }

        public static IEnumerable<LocalCampaign> GetRunningCampaigns(DataLakeTwitterAdsContext dbContext) {
            return dbContext.Campaigns.Where(c => c.EndTime != null && c.EndTime > DateTime.UtcNow).ToList();
        }

        public static IEnumerable<string> GetRunningCampaignIds(DataLakeTwitterAdsContext adsDbContext) {
            return adsDbContext.Campaigns.Where(c => c.EndTime != null && c.EndTime > DateTime.UtcNow).Select(c => c.Id).ToList();
        }

        public static IEnumerable<string> GetTweetIdsFromUser(string userId, DataLakeTwitterDataContext dataDbContext) {
            return dataDbContext.Tweets.Where(t => t.UserId == userId).Select(t => t.Id).ToList();
        }

        public static DateTimeOffset GetOldestTweetDate(string userId, DataLakeTwitterDataContext dataDbContext) {
            return dataDbContext.Tweets
                       .Where(t => t.UserId == userId)
                       .OrderBy(t => t.CreatedAt)
                       .Select(t => t.CreatedAt)
                       .FirstOrDefault();
        }

        public static LocalTweet GetLatestTweetFromUser(string userId, DataLakeTwitterDataContext dataDbContext) {
            return dataDbContext.Tweets
                       .Where(t => t.UserId == userId)
                       .OrderBy(t => t.CreatedAt)
                       .LastOrDefault();
        }

        public static LocalTweet GetFirstTweetFromUser(string userId, DataLakeTwitterDataContext dataDbContext) {
            return dataDbContext.Tweets
                       .Where(t => t.UserId == userId)
                       .OrderBy(t => t.CreatedAt)
                       .FirstOrDefault();
        }

        public static DateTimeOffset GetOrganicTweetDailyMetricsStartingDate(
            string userId,
            DataLakeTwitterDataContext dataDbContext,
            DataLakeTwitterAdsContext adsDbContext) {

            var dailyMetricsStatingDate =
                adsDbContext.OrganicTweetDailyMetrics
                    .GroupBy(p => p.TweetId)
                    .Select(g => g.Max(p => p.Date))
                    .OrderBy(d => d)
                    .FirstOrDefault();

            var tweetStartingDate = GetOldestTweetDate(userId, dataDbContext);

            return dailyMetricsStatingDate < tweetStartingDate ? tweetStartingDate : dailyMetricsStatingDate;
        }

        public static DateTimeOffset GetPromotedTweetDailyMetricsStartingDate(
            string userId,
            DataLakeTwitterDataContext dataDbContext,
            DataLakeTwitterAdsContext adsDbContext) {

            var dailyMetricsStatingDate =
                adsDbContext.PromotedTweetDailyMetrics
                    .GroupBy(p => p.PromotedTweetId)
                    .Select(g => g.Max(p => p.Date))
                    .OrderBy(d => d)
                    .FirstOrDefault();

            var tweetStartingDate = GetOldestTweetDate(userId, dataDbContext);

            return dailyMetricsStatingDate < tweetStartingDate ? tweetStartingDate : dailyMetricsStatingDate;
        }

        public static IEnumerable<string> GetPromotedTweetIdsFromUser(
            string userId,
            DataLakeTwitterDataContext dataContext,
            DataLakeTwitterAdsContext adsContext) {

            var tweetIds = GetTweetIdsFromUser(userId, dataContext).ToHashSet<string>();

            return adsContext.PromotedTweets.Where(t => tweetIds.Contains(t.TweetId)).Select(t => t.Id).ToList();
        }
    }
}
