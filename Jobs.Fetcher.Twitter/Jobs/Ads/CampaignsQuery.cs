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

    public class CampaignsQuery : AbstractTwitterFetcher {

        public CampaignsQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<AdsAccountsQuery>() };
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var dbContext = new DataLakeTwitterAdsContext()) {
                RunBody(kvp.Key, kvp.Value, dbContext);
            }
        }

        private void RunBody(string username, ITwitterClient client, DataLakeTwitterAdsContext dbContext) {
            var adsAccountIds = DbReader.GetAdsAccountIds(username, dbContext);

            void ProccessCampaignResult(string accountId, ITwitterRequestIterator<CampaignsResponse, string> iterator) {
                while (!iterator.Completed) {
                    var campaignsPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                    DbWriter.WriteCampaigns(accountId, campaignsPage.Content, dbContext, GetLogger());
                }
            }

            foreach (var adsAccountId in adsAccountIds) {
                ApiDataFetcher.GetCampaigns(adsAccountId, client as TwitterAdsClient, ProccessCampaignResult);
            }
        }
    }
}
