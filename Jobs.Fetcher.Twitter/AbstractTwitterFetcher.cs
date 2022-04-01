using Tweetinvi;
using Serilog.Core;
using DataLakeModels;
using Andromeda.Common.Logging;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

using FlycatcherAds;
using FlycatcherAds.Client;

namespace Jobs.Fetcher.Twitter {
    public abstract class AbstractTwitterFetcher : AbstractJob {
        protected const int ERROR_THRESHOLD = 10;
        protected Dictionary<string, ITwitterClient> Clients { get; }
        public AbstractTwitterFetcher(Dictionary<string, ITwitterClient> clients) {
            Clients = clients;
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            foreach (var client in Clients) {
                RunBody(client);
            }
        }

        abstract public void RunBody(KeyValuePair<string, ITwitterClient> client);
    }
}
