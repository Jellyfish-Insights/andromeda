using DataLakeModels;
using Andromeda.Common.Jobs;
using Serilog.Core;
using Andromeda.Common.Logging;
using System.Collections.Generic;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;

namespace Jobs.Fetcher.YouTube {

    public abstract class YoutubeFetcher : AbstractJob {
        public List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> AccountInfos;

        public YoutubeFetcher(List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos) {
            AccountInfos = accountInfos;
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            foreach (var(DataService, AnalyticsService) in AccountInfos) {
                RunBody(DataService, AnalyticsService);
            }
        }

        abstract public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService);
    }
}
