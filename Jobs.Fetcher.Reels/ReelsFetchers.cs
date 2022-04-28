using System;
using System.IO;
using System.Linq;
using Serilog.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

namespace Jobs.Fetcher.Reels {

    public class ReelsFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.Reels;
        public string CredentialsDir = "./credentials/reels";
        //private string SecretsFile = "tiktok_credentials.json";

        public override IEnumerable<AbstractJob> GetJobs(
            JobType type,
            JobScope scope,
            IEnumerable<string> names,
            JobConfiguration jobConfiguration) {

            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }

            var usernames = GetReelsUsers();
            var jobs = GetListOfJobs(usernames);

            return FilterByName(jobs, names);
        }

        private List<string> GetReelsUsers() {
            var reelsUsernames = new List<string>();
            try {
                var usrDirs = Directory.GetDirectories("./credentials");

                if (usrDirs.Any(dir => dir.Contains("youtube") || dir.Contains("facebook") || dir.Contains("instagram"))) {
                    Console.WriteLine($"Detected old folder structure. Loading only the old structure credentials, where Reels is not available. Please, consider changing to the new folder structure");
                } else {
                    foreach (var usrDir in usrDirs) {
                        CredentialsDir = $"{usrDir}/reels";

                        if (!Directory.Exists(CredentialsDir)) {
                            Console.WriteLine($"Missing or invalid Reels channels!");
                            Console.WriteLine($"Couldn't find any credential on folder '{CredentialsDir}'");
                            continue;
                        }
                        foreach (var user in Directory.GetFiles(CredentialsDir)) {
                            var userJson = JObject.Parse(File.ReadAllText($"{user}"));
                            reelsUsernames.Add(userJson["managed_account"].ToString());
                        }
                    }
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                string message = String.Format("Missing or invalid Reels credentials!\n{0}", e.Message);
                if (e is DirectoryNotFoundException) {
                    message = String.Format("{0}\nCheck if the path above exists!", message);
                }
                Console.WriteLine(message);
            };
            return reelsUsernames;
        }

        private List<AbstractJob> GetListOfJobs(List<String> reelsAccountNames) {
            return new List<AbstractJob>() {
                       new ReelsQuery(reelsAccountNames)
            };
        }
    }
}
