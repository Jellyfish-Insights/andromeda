using System;
using Tweetinvi;
using Serilog.Core;
using DataLakeModels;
using Andromeda.Common.Logging;
using Andromeda.Common.Jobs;
using System.Collections.Generic;
using System.Threading;

using FlycatcherAds;
using FlycatcherAds.Client;

namespace Jobs.Fetcher.Twitter {

    public class TwitterTooManyErrors : Exception {
        public TwitterTooManyErrors() {}

        public TwitterTooManyErrors(string msg): base(msg) {}

        public TwitterTooManyErrors(string msg, Exception inner): base(msg, inner) {}
    }

    public abstract class AbstractTwitterFetcher : AbstractJob {
        protected const int GLOBAL_ERR_LIMIT = 20;
        protected const int LOCAL_ERR_LIMIT = 5;
        protected const int SLEEP_TIME = 5 * 1000;
        protected static int _globalErr = 0;
        protected Dictionary<string, ITwitterClient> Clients { get; }
        public AbstractTwitterFetcher(Dictionary<string, ITwitterClient> clients) {
            Clients = clients;
            Logger = GetLogger();
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
