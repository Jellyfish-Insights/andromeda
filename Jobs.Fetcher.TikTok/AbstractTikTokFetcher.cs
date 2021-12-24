using DataLakeModels;
using Serilog.Core;
using Andromeda.Common.Logging;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

namespace Jobs.Fetcher.TikTok {
    public abstract class AbstractTikTokFetcher : AbstractJob {
        protected List<string> UserIds { get; }
        public AbstractTikTokFetcher(List<string> userIds) {
            UserIds = userIds;
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            foreach (var userId in UserIds) {
                RunBody(userId);
            }
        }

        abstract public void RunBody(string userId);
    }
}
