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

            List<(YouTubeService, YouTubeAnalyticsService)> services =
                Credentials.GetAllServices();
            Console.WriteLine($"We were able to retrieve {services.Count} services.");

            var jobs = new List<AbstractJob> {
                new YouTubeStudioFetcherJob(),
                new Groups_EnsureAllItemsAreInDB(services),
                new Groups_EnsureAllGroupsAreInDB(services),
                new Groups_AssociateGroupsAndItems(services),
                new Groups_InsertOrphanItems(services)
            };
            return jobs;
        }
    }
}
