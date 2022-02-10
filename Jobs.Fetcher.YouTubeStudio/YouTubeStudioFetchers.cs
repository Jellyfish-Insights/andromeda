using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DataLakeModels.Models.YouTube.Studio;
using Andromeda.Common.Jobs;

namespace Jobs.Fetcher.YouTubeStudio {
    public sealed class YouTubeStudioFetchers : FetcherJobsFactory {
        public override JobScope Scope { get; } = JobScope.YouTubeStudio;
        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration config) {
            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }
            return new List<YouTubeStudioFetcherJob> { new YouTubeStudioFetcherJob() };
        }

        public static void Main(string[] args) {
            var ytsFetcherJob = new YouTubeStudioFetcherJob();
            ytsFetcherJob.RunningAsStandAlone = true;
            ytsFetcherJob.Run();
        }
    }
}
