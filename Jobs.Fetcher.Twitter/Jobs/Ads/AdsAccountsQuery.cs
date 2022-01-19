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

    public class AdsAccountsQuery : AbstractTwitterFetcher {

        public AdsAccountsQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<UserQuery>(), IdOf<TimelineQuery>() };
        }

        private void RunBody(
            string username,
            ITwitterClient client,
            DataLakeTwitterAdsContext adsContext,
            DataLakeTwitterDataContext dataContext) {

            var user = DbReader.GetUserByUsername(username, dataContext);
            if (user == null) {
                GetLogger().Error($"User {username} not found in database");
                return;
            }

            void ProccessAdsAccountResult(ITwitterRequestIterator<AdsAccountsResponse, string> iterator) {
                try {
                    while (!iterator.Completed) {
                        var adsAccountPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteAdsAccounts(user.Id, username, adsAccountPage.Content, adsContext, GetLogger());
                    }
                }catch (Exception e) {
                    GetLogger().Error($"Could not fetch Twitter Video Libraries for {username}");
                    throw e;
                }
            }

            ApiDataFetcher.GetAdsAccounts(client as TwitterAdsClient, ProccessAdsAccountResult);
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var adsContext = new DataLakeTwitterAdsContext()) {
                using (var dataContext = new DataLakeTwitterDataContext()) {
                    RunBody(kvp.Key, kvp.Value, adsContext, dataContext);
                }
            }
        }
    }
}
