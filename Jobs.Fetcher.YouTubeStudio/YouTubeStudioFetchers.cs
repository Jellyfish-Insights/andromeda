using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;

using DataLakeModels.Models.YouTube.Studio;
using Andromeda.Common.Jobs;
using Jobs.Fetcher.YouTubeStudio.Helpers;

namespace Jobs.Fetcher.YouTubeStudio {
    public sealed class YouTubeStudioFetchers : FetcherJobsFactory {
        public override JobScope Scope { get; } = JobScope.YouTubeStudio;
        public override IEnumerable<AbstractJob> GetJobs(
            JobType type,
            JobScope scope,
            IEnumerable<string> names,
            JobConfiguration config
        ) {
            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }

            var jobs = new List<AbstractJob> {
                new YouTubeStudioFetcherJob()
            };

            List<(YouTubeService, YouTubeAnalyticsService)> services =
                                            Credentials.GetAllServices();
            Console.WriteLine($"We were able to retrieve {services.Count} services.");
            foreach (var servicePair in services) {
                var (ytd, yta) = servicePair;
                var newJobs = new List<YTSGroupsAbstractJob> {
                    new Groups_EnsureAllItemsAreInDB(ytd, yta),
                    new Groups_EnsureAllGroupsAreInDB(ytd, yta),
                    new Groups_AssociateGroupsAndItems(ytd, yta),
                    new Groups_InsertOrphanItems(ytd, yta)
                };
                jobs.AddRange(newJobs);
            }
            return jobs;
        }
    }
}
