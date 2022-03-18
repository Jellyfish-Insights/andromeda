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
                user = DbReader.GetUserByUsername(username, dataDbContext, GetLogger());
            }catch (Exception e) {
                Logger.Error($"User {username} not found in database");
                Logger.Verbose($"Error: {e}");
                return;
            }

            if (user == null) {
                GetLogger().Error($"User {username} not found in database");
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
                    GetLogger());
            }

            
            IEnumerable<string> tweetIds;

            try {
                tweetIds = DbReader.GetTweetIdsFromUser(user.Id, dataDbContext, GetLogger());
            }catch (Exception e) {
                Logger.Error($"User {username} has no tweets");
                Logger.Verbose($"Error: {e}");
                return;
            }
            if (!tweetIds.Any()) {
                GetLogger().Error($"User {username} has no tweets");
                return;
            }

            var startDate = DbReader.GetOrganicTweetDailyMetricsStartingDate(
                user.Id,
                dataDbContext,
                adsDbContext,
                GetLogger());

            foreach (var adsAccount in DbReader.GetAdsAccounts(username, adsDbContext, GetLogger())) {

                await ApiDataFetcher.GetOrganicTweetDailyMetricsReport(
                    adsAccount,
                    startDate,
                    tweetIds,
                    client as TwitterAdsClient,
                    ProccessOrganicTweetDailyMetricsResult);
            }
        }
    }
}
