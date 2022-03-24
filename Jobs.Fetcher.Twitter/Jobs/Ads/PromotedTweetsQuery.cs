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

    public class PromotedTweetsQuery : AbstractTwitterFetcher {

        public PromotedTweetsQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<LineItemsQuery>() };
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            RunBody(kvp.Key, kvp.Value, new DataLakeTwitterAdsContext());
        }

        private void RunBody(string username, ITwitterClient client, DataLakeTwitterAdsContext dbContext) {
            var adsAccountIds = DbReader.GetAdsAccountIds(username, dbContext);

            void ProccessPromotedTweetResult(string adsAccountId, ITwitterRequestIterator<PromotedTweetsResponse, string> iterator) {
                var page_count = 0;
                while (!iterator.Completed) {
                    try {
                        page_count++;
                        GetLogger().Error($"Fetching Twitter Ads Promoted Tweets for {username}, page {page_count}");
                        var promotedTweetsPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WritePromotedTweets(adsAccountId, promotedTweetsPage.Content, dbContext, GetLogger());
                    }catch (Exception e) {
                        GetLogger().Error($"Could not fetch Twitter Ads Promoted Tweets for {username}, page {page_count}");
                        GetLogger().Verbose($"Error: {e}");
                    }
                }
            }

            foreach (var adsAccountId in adsAccountIds) {
                ApiDataFetcher.GetPromotedTweets(adsAccountId, client as TwitterAdsClient, ProccessPromotedTweetResult);
            }
        }
    }
}
