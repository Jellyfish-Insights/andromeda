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

    public class TimelineQuery : AbstractTwitterFetcher {

        public TimelineQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<UserQuery>() };
        }

        public static int _globalErrors = 0;

        private void RunBody(string username, ITwitterClient client, DataLakeTwitterDataContext dbContext) {

            var user = DbReader.GetUserByUsername(username, dbContext);
            if (user == null) {
                Logger.Error($"User {username} not found in database");
                return;
            }

            void ProccessTimeLineResult(ITwitterRequestIterator<TimelinesV2Response, string> iterator) {
                var page_count = 0;
                var error_count = 0;
                while (!iterator.Completed) {
                    page_count++;
                    Logger.Information($"Fetching Twitter Timelines for {username}, page {page_count}");
                    try {
                        var timelinePage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteTimeline(timelinePage.Content, dbContext, Logger);
                    }catch (Exception e) {
                        error_count++;
                        _globalErr++;
                        Logger.Error($"Could not fetch Twitter Timelines for {username}, page {page_count}");
                        Logger.Debug($"Error: {e}");
                        if (error_count > LOCAL_ERR_LIMIT || _globalErr > GLOBAL_ERR_LIMIT) {
                            Logger.Debug($"Too many errors occurred. Stopping this job for now.");
                            throw new TwitterTooManyErrors(
                                      $"Local errors: {error_count}, global errors: {_globalErr}",
                                      e);
                        } else {
                            Thread.Sleep(SLEEP_TIME);
                        }
                    }
                }
            }

            var latestTweet = DbReader.GetLatestTweetFromUser(user.Id, dbContext);

            ApiDataFetcher.GetUserTweetsTimeline(user.Id, latestTweet, client as TwitterDataClient, ProccessTimeLineResult, Logger);
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var dbContext = new DataLakeTwitterDataContext()) {
                RunBody(kvp.Key, kvp.Value, dbContext);
            }
        }
    }
}
