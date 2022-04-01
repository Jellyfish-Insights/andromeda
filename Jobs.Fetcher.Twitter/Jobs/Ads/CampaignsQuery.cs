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

            void ProcessCampaignResult(string accountId, ITwitterRequestIterator<CampaignsResponse, string> iterator) {
                var page_count = 0;
                var error_count = 0;
                while (!iterator.Completed) {
                    try {
                        page_count++;
                        Logger.Debug($"Fetching Twitter Ads Campaigns for {username}, page {page_count}");
                        var campaignsPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        Logger.Debug($"Caught content {campaignsPage.Content}, will write");
                        DbWriter.WriteCampaigns(accountId, campaignsPage.Content, dbContext, Logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not fetch or write Twitter Ads Campaigns for {username}, page {page_count}");
                        Logger.Debug($"Error: {e}");
                        error_count++;
                        if (error_count > ERROR_THRESHOLD) {
                            Logger.Debug($"It was not possible to get campaigns. Giving up for now.");
                            break;
                        }
                    }
                }
            }

            foreach (var adsAccountId in adsAccountIds) {
                ApiDataFetcher.GetCampaigns(adsAccountId, client as TwitterAdsClient, ProcessCampaignResult);
            }
        }
    }
}
