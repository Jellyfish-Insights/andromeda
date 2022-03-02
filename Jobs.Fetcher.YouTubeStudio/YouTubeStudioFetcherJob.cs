using System;
using System.IO;
using DataLakeModels;
using Andromeda.Common.Jobs;
using Serilog.Core;
using Andromeda.Common.Logging;
using System.Collections.Generic;

using Jobs.Fetcher.YouTubeStudio.Helpers;

namespace Jobs.Fetcher.YouTubeStudio {
    public class YouTubeStudioFetcherJob : AbstractJob {
        public YouTubeStudioFetcherJob() {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            string pathToData = @"../../Data/";

            Logger.Information($"Reading files from {pathToData}");

            var videos = ImportFromFileSystem.GetDTOsFromPath(pathToData, Logger);
            if (videos == null || videos.Count == 0) {
                Logger.Information("No videos to work with. Terminating.");
                return;
            }
            Logger.Information($"Decoded {videos.Count} videos.");
            DbWriter.WriteDTOs(videos, Logger);
        }
    }
}
