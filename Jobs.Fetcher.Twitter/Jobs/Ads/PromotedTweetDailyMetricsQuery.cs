using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tweetinvi;
using DataLakeModels;
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

    public class PromotedTweetDailyMetricsQuery : AbstractTwitterFetcher {

        public PromotedTweetDailyMetricsQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

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

        public async Task RunBody(
            string username,
            ITwitterClient client,
            DataLakeTwitterAdsContext adsDbContext,
            DataLakeTwitterDataContext dataDbContext) {

            var user = DbReader.GetUserByUsername(username, dataDbContext);

            if (user == null) {
                Logger.Error($"User {username} not found in database");
                return;
            }

            void ProccessPromotedTweetDailyMetricsResult(
                string adsAccountId,
                DateTime start,
                DateTime end,
                SynchronousAnalyticsResponse synchronousAnalyticsResponse) {

                DbWriter.WritePromotedTweetDailyMetrics(
                    adsAccountId,
                    start,
                    end,
                    synchronousAnalyticsResponse,
                    adsDbContext,
                    Logger);
            }

            var promotedTweetIds = DbReader.GetPromotedTweetIdsFromUser(user.Id, dataDbContext, adsDbContext);

            if (!promotedTweetIds.Any()) {
                Logger.Error($"User {username} has no Ads promoted tweets");
                return;
            }

            var startDate = DbReader.GetPromotedTweetDailyMetricsStartingDate(user.Id, dataDbContext, adsDbContext);
            foreach (var adsAccount in DbReader.GetAdsAccounts(username, adsDbContext)) {
                Logger.Information($"Fetching Promoted Daily Metrics for {username} from {startDate.Date}");
                try {
                    await ApiDataFetcher.GetPromotedTweetDailyMetricsReport(
                        adsAccount,
                        startDate,
                        promotedTweetIds,
                        client as TwitterAdsClient,
                        ProccessPromotedTweetDailyMetricsResult,
                        Logger);
                }catch (Exception e) {
                    Logger.Error($"Could not get Promoted Daily Metrics from {adsAccount}");
                    Logger.Debug($"Error: {e}");
                }
            }
        }
    }
}
