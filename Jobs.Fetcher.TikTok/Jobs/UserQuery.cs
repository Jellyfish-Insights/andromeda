using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.Twitter.Helpers;

namespace Jobs.Fetcher.TikTok {

    public class UserQuery : AbstractTikTokFetcher {

        //public UserQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        public override void RunBody() {
            using (var dbContext = new DataLakeTikTokDataContext()) {
                //var user = ApiDataFetcher.GetUserByName(kvp.Key, kvp.Value as TwitterDataClient);
                //DbWriter.WriteUser(user, dbContext, GetLogger());
            }
        }
    }
}
