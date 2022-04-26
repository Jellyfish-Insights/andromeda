using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
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
                var page_count = 0;
                var error_count = 0;
                while (!iterator.Completed) {
                    try {
                        page_count++;
                        Logger.Error($"Fetching Twitter Ads Line Items for {username}, page {page_count}");
                        var lineItemsPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteLineItems(adsAccountId, lineItemsPage.Content, dbContext, Logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not fetch or write Twitter Ads Line Items for {username}");
                        Logger.Debug($"Error: {e}");
                        error_count++;
                        _globalErr++;
                        if (error_count > LOCAL_ERR_LIMIT || _globalErr > GLOBAL_ERR_LIMIT) {
                            Logger.Debug($"It was not possible to get line items. Giving up for now.");
                            break;
                        } else {
                            Thread.Sleep(SLEEP_TIME);
                        }
                    }
                }
            }

            foreach (var adsAccountId in adsAccountIds) {
                ApiDataFetcher.GetLineItems(adsAccountId, client as TwitterAdsClient, ProccessLineItemResult);
            }
        }
    }
}
