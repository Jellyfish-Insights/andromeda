using DataLakeModels;
using Andromeda.Common.Jobs;
using Serilog.Core;
using Andromeda.Common.Logging;
using System.Collections.Generic;

namespace Jobs.Fetcher.YouTubeStudio {
    public abstract class YouTubeStudioFetcher : AbstractJob {
        public YouTubeStudioFetcher() {

        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            RunBody();
        }

        abstract public void RunBody();
    }
}
