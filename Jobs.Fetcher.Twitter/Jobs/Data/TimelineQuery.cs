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

    public class TimelineQuery : AbstractTwitterFetcher {

        public TimelineQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<UserQuery>() };
        }

        private void RunBody(string username, ITwitterClient client, DataLakeTwitterDataContext dbContext) {

            var user = DbReader.GetUserByUsername(username, dbContext, GetLogger());
            if (user == null) {
                GetLogger().Error($"User {username} not found in database");
                return;
            }

            void ProccessTimeLineResult(ITwitterRequestIterator<TimelinesV2Response, string> iterator) {

                while (!iterator.Completed) {
                    var timelinePage = iterator.NextPageAsync().GetAwaiter().GetResult();
                    DbWriter.WriteTimeline(timelinePage.Content, dbContext, GetLogger());
                }
            }

            var latestTweet = DbReader.GetLatestTweetFromUser(user.Id, dbContext, GetLogger());

            ApiDataFetcher.GetUserTweetsTimeline(user.Id, latestTweet, client as TwitterDataClient, ProccessTimeLineResult);
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var dbContext = new DataLakeTwitterDataContext()) {
                RunBody(kvp.Key, kvp.Value, dbContext);
            }
        }
    }
}
