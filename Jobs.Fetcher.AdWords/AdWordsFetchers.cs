using System;
using System.Collections.Generic;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.Common.Lib;
using Andromeda.Common.Jobs;

namespace Jobs.Fetcher.AdWords {

    public class AdWordsFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.AdWords;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope)) {
                return NoJobs;
            }

            var config = new AdWordsAppConfig();
            if (config.OAuth2Mode != OAuth2Flow.APPLICATION || string.IsNullOrEmpty(config.OAuth2RefreshToken)) {
                Console.WriteLine("AdWords: missing or invalid App.config.");
                Environment.Exit(1);
            }
            var user = new AdWordsUser(config);

            var jobs = new List<AbstractJob>() {
                new StructuralVideoPerformanceReport(user),
                new StructuralCriteriaPerformanceReport(user),
                new StructuralCampaignPerformanceReport(user),
                new AdPerformanceReport(user),
            };

            return FilterByName(jobs, names);
        }
    }
}
