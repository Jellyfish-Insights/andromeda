using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
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

    public class CustomAudiencesQuery : AbstractTwitterFetcher {

        public CustomAudiencesQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<AdsAccountsQuery>() };
        }

        private void RunBody(
            string username,
            ITwitterClient client,
            DataLakeTwitterAdsContext adsDbContext,
            DataLakeTwitterDataContext dataDbContext) {

            void ProccessCustomAudiencesResult(ITwitterRequestIterator<CustomAudiencesResponse, string> iterator) {
                var page_count = 0;
                var error_count = 0;
                while (!iterator.Completed) {
                    try {
                        page_count++;
                        Logger.Information($"Fetching Twitter Ads Custom Audiences for {username}, page {page_count}");
                        var customAudiencesPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteCustomAudiences(customAudiencesPage.Content, adsDbContext, Logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not fetch or write Twitter Ads Custom Audiences for {username}, page {page_count}");
                        Logger.Debug($"Error: {e}");
                        error_count++;
                        if (error_count > ERROR_THRESHOLD) {
                            Logger.Debug($"It was not possible to get custom audiences. Giving up for now.");
                            break;
                        } else {
                            Thread.Sleep(SLEEP_TIME);
                        }
                    }
                }
            }

            foreach (var adsAccountId in DbReader.GetAdsAccountIds(username, adsDbContext)) {
                ApiDataFetcher.GetCustomAudiences(adsAccountId, client as TwitterAdsClient, ProccessCustomAudiencesResult);
            }
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var adsDbContext = new DataLakeTwitterAdsContext()) {
                using (var dataDbContext = new DataLakeTwitterDataContext()) {
                    RunBody(kvp.Key, kvp.Value, adsDbContext, dataDbContext);
                }
            }
        }
    }
}
