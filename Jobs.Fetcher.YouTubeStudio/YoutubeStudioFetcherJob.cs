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

        public bool RunningAsStandAlone {get;set;} = false;
        public YouTubeStudioFetcherJob() {}

        public override List<string> Dependencies()
        {
            return new List<string>();
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override void Run() {
            string pathToData;
            if (RunningAsStandAlone) {
                pathToData = @"../../../Data/";
            }
            else {
                pathToData = @"../../Data/";
            }

            Logger.Information($"Reading files from {pathToData}");

            var videos = ImportFromFileSystem.GetVideosFromPath(pathToData);
            if (videos == null || videos.Count == 0) {
                Console.WriteLine("No videos to work with. Terminating.");
                return;
            }
            Console.WriteLine($"Decoded {videos.Count} videos.");
        }
    }
}
