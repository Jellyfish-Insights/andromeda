using System;
using System.Collections.Generic;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.Common.Lib;
using Andromeda.Common.Jobs;
using System.IO;

namespace Jobs.Fetcher.AdWords {

    public class AdWordsFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.AdWords;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }

            var config = new AdWordsAppConfig();
            if (config.OAuth2Mode != OAuth2Flow.APPLICATION || string.IsNullOrEmpty(config.OAuth2RefreshToken)) {
                Console.WriteLine("Missing or invalid AdWords credentials.");
                Console.WriteLine("Could not find file \'{0}/credentials/adwords/App.config\'\n", Directory.GetCurrentDirectory());
                return NoJobs;
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
