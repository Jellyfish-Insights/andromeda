using DataLakeModels;
using Serilog.Core;
using Andromeda.Common.Logging;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

namespace Jobs.Fetcher.Reels {
    public abstract class AbstractReelsFetcher : AbstractJob {
        protected List<string> Usernames { get; }
        public AbstractReelsFetcher(List<string> usernames) {
            Usernames = usernames;
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            foreach (var username in Usernames) {
                RunBody(username);
            }
        }

        abstract public void RunBody(string username);
    }
}
