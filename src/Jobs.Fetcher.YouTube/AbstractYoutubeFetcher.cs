using DataLakeModels;
using Common.Jobs;
using Serilog.Core;
using Common.Logging;

namespace Jobs.Fetcher.YouTube {

    public abstract class YoutubeFetcher : AbstractJob {
        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }
    }
}
