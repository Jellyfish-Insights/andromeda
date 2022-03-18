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
                try {
                    while (!iterator.Completed) {
                        var customAudiencesPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteCustomAudiences(customAudiencesPage.Content, adsDbContext, GetLogger());
                    }
                }catch (Exception e) {
                    GetLogger().Error($"Could not fetch or write Twitter Video Libraries for {username}");
                    GetLogger().Verbose($"Error: {e}");
                    throw;
                }
            }

            foreach (var adsAccountId in DbReader.GetAdsAccountIds(username, adsDbContext, GetLogger())) {
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
