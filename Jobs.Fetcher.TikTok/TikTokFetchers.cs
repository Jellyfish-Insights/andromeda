using System;
using System.IO;
using System.Linq;
using Serilog.Core;
using Newtonsoft.Json;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

namespace Jobs.Fetcher.TikTok {

    public class TikTokFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.TikTok;
        //private string SecretsFile = "tiktok_credentials.json";

        public override IEnumerable<AbstractJob> GetJobs(
            JobType type,
            JobScope scope,
            IEnumerable<string> names,
            JobConfiguration jobConfiguration) {

            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }

            return new List<AbstractJob>() {
                    new PostsQuery(),
            };
        }
    }
}
