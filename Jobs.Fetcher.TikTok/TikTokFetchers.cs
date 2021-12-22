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

            var jobs = new List<AbstractJob>();
            var tiktokUserIds = new List<string>();

            try {
                var usrDirs = Directory.GetDirectories("./credentials");

                if (usrDirs.Any(dir => dir.Contains("youtube") || dir.Contains("facebook") || dir.Contains("instagram"))) {
                    Console.WriteLine($"Detected old folder structure. Loading only the old structure credentials, where TikTok is not available. Please, consider changing to the new folder structure");
                    return NoJobs;
                } else {
                    foreach (var usrDir in usrDirs) {
                        CredentialsDir = $"{usrDir}/tiktok";

                        if (!Directory.Exists(CredentialsDir)) {
                            Console.WriteLine($"Missing or invalid Youtube credentials!");
                            Console.WriteLine($"Couldn't find any credential on folder '{CredentialsDir}'");
                            continue;
                        }

                        jobs.AddRange(GetListOfJobs(tiktokUserIds));
                    }
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                string message = String.Format("Missing or invalid YouTube credentials!\n{0}", e.Message);
                if (e is DirectoryNotFoundException) {
                    message = String.Format("{0}\nCheck if the path above exists!", message);
                }
                Console.WriteLine(message);
                return NoJobs;
            };

            return FilterByName(jobs, names);
        }
    
        private List<AbstractJob> GetListOfJobs(List<string> tiktokUserIds) {
            foreach (var directory in Directory.GetDirectories(CredentialsDir)) {
                var text = File.ReadAllText(CredentialsFilePath);
                var credential = JsonConvert.DeserializeObject<TikTokUsers>(text);

                if (credential.IsValid()) {
                    tiktokAccountIds.Add(credential.UserId);
                }
            }

            return new List<AbstractJob>() {
                new PostsQuery(tiktokAccountIds),
            };
        }
    }
}
