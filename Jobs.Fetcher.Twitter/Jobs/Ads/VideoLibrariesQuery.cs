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

    public class VideoLibrariesQuery : AbstractTwitterFetcher {

        public VideoLibrariesQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<PromotedTweetsQuery>() };
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var dbContext = new DataLakeTwitterAdsContext()) {
                RunBody(kvp.Key, kvp.Value, dbContext);
            }
        }

        private void RunBody(string username, ITwitterClient client, DataLakeTwitterAdsContext dbContext) {
            var adsAccountIds = DbReader.GetAdsAccountIds(username, dbContext, GetLogger());

            void ProccessVideoLibraryResult(ITwitterRequestIterator<MediaLibraryResponse, string> iterator) {
                try {
                    while (!iterator.Completed) {
                        var mediaLibraryPage = iterator.NextPageAsync().GetAwaiter().GetResult();
                        DbWriter.WriteVideoLibraries(username, mediaLibraryPage.Content, dbContext, GetLogger());
                    }
                }catch (Exception e) {
                    GetLogger().Error($"Could not fetch Twitter Video Libraries for {username}");
                    throw e;
                }
            }

            foreach (var adsAccountId in adsAccountIds) {
                ApiDataFetcher.GetVideoLibraries(
                    adsAccountId,
                    client as TwitterAdsClient,
                    ProccessVideoLibraryResult);
            }
        }
    }
}
