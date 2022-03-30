using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tweetinvi;
using DataLakeModels;
using DataLakeModels.Models.Twitter.Data;
using Tweetinvi.Models.V2;
using Tweetinvi.Parameters.V2;
using Tweetinvi.WebLogic;
using Jobs.Fetcher.Twitter.Helpers;
using Tweetinvi.Iterators;

using FlycatcherData;
using FlycatcherData.Models.V2;

using FlycatcherAds;
using FlycatcherAds.Models;
using FlycatcherAds.Client;

namespace Jobs.Fetcher.Twitter {

    public class OrganicTweetDailyMetricsQuery : AbstractTwitterFetcher {

        public OrganicTweetDailyMetricsQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideoLibrariesQuery>() };
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var adsDbContext = new DataLakeTwitterAdsContext()) {
                using (var dataDbContext = new DataLakeTwitterDataContext()) {
                    RunBody(kvp.Key, kvp.Value, adsDbContext, dataDbContext).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        private async Task RunBody(
            string username,
            ITwitterClient client,
            DataLakeTwitterAdsContext adsDbContext,
            DataLakeTwitterDataContext dataDbContext) {

            User user;

            try {
                user = DbReader.GetUserByUsername(username, dataDbContext);
            }catch (Exception e) {
                Logger.Error($"User {username} not found in database");
                Logger.Debug($"Error: {e}");
                return;
            }

            if (user == null) {
                Logger.Error($"User {username} not found in database");
                return;
            }

            void ProccessOrganicTweetDailyMetricsResult(
                string adsAccountId,
                DateTime start,
                DateTime end,
                SynchronousAnalyticsResponse synchronousAnalyticsResponse) {

                DbWriter.WriteOrganicTweetDailyMetrics(
                    adsAccountId,
                    start,
                    end,
                    synchronousAnalyticsResponse,
                    adsDbContext,
                    Logger);
            }

            IEnumerable<string> tweetIds;

            try {
                tweetIds = DbReader.GetTweetIdsFromUser(user.Id, dataDbContext);
            }catch (Exception e) {
                Logger.Error($"User {username} has no Ads tweets");
                Logger.Debug($"Error: {e}");
                return;
            }
            if (!tweetIds.Any()) {
                Logger.Error($"User {username} has no Ads tweets");
                return;
            }

            var startDate = DbReader.GetOrganicTweetDailyMetricsStartingDate(
                user.Id,
                dataDbContext,
                adsDbContext);

            foreach (var adsAccount in DbReader.GetAdsAccounts(username, adsDbContext)) {
                try {
                    await ApiDataFetcher.GetOrganicTweetDailyMetricsReport(
                        adsAccount,
                        startDate,
                        tweetIds,
                        client as TwitterAdsClient,
                        ProccessOrganicTweetDailyMetricsResult);
                }catch (Exception e) {
                    Logger.Error($"Could not get Organic Daily Metrics from {adsAccount}");
                    Logger.Debug($"Error: {e}");
                }
            }
        }
    }
}
