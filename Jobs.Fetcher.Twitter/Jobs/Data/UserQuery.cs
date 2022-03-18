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

    public class UserQuery : AbstractTwitterFetcher {

        public UserQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        public override void RunBody(KeyValuePair<string, ITwitterClient> kvp) {
            using (var dbContext = new DataLakeTwitterDataContext()) {
                var user = ApiDataFetcher.GetUserByName(kvp.Key, kvp.Value as TwitterDataClient, GetLogger());
                DbWriter.WriteUser(user, dbContext, GetLogger());
            }
        }
    }
}
