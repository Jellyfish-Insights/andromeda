using DataLakeModels;
using Serilog.Core;
using Andromeda.Common.Logging;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

namespace Jobs.Fetcher.TikTok {
    public abstract class AbstractTikTokFetcher : AbstractJob {
        //protected Dictionary<string, ITwitterClient> Clients { get; }
        /*public AbstractTwitterFetcher(Dictionary<string, ITwitterClient> clients) {
            Clients = clients;
        }*/

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            RunBody();
            /*foreach (var client in Clients) {
                RunBody(client);
            }*/
        }

        abstract public void RunBody();
    }
}
