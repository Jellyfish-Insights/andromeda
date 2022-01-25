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

    public class LineItemsQuery : AbstractTwitterFetcher {

        public LineItemsQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<CampaignsQuery>() };
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            RunBody(kvp.Key, kvp.Value, new DataLakeTwitterAdsContext());
        }

        private void RunBody(string username, ITwitterClient client, DataLakeTwitterAdsContext dbContext) {
            var adsAccountIds = DbReader.GetAdsAccountIds(username, dbContext);

            void ProccessLineItemResult(string adsAccountId, ITwitterRequestIterator<LineItemsResponse, string> iterator) {
                try {
                    while (!iterator.Completed) {
                        var lineItemsPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteLineItems(adsAccountId, lineItemsPage.Content, dbContext, GetLogger());
                    }
                }catch (Exception e) {
                    GetLogger().Error($"Could not fetch Twitter Line Items for {username}");
                    throw e;
                }
            }

            foreach (var adsAccountId in adsAccountIds) {
                ApiDataFetcher.GetLineItems(adsAccountId, client as TwitterAdsClient, ProccessLineItemResult);
            }
        }
    }
}
