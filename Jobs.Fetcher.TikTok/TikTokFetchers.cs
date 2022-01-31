using System;
using System.IO;
using System.Linq;
using Serilog.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Andromeda.Common.Jobs;
using System.Collections.Generic;

namespace Jobs.Fetcher.TikTok {

    public class TikTokFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.TikTok;
        public string CredentialsDir = "./credentials/tiktok";
        //private string SecretsFile = "tiktok_credentials.json";

        public override IEnumerable<AbstractJob> GetJobs(
            JobType type,
            JobScope scope,
            IEnumerable<string> names,
            JobConfiguration jobConfiguration) {

            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }

            var usernames = GetTikTokUsers();
            var jobs = GetListOfJobs(usernames);

            return FilterByName(jobs, names);
        }

        private List<string> GetTikTokUsers() {
            var tiktokUsernames = new List<string>();
            try {
                var usrDirs = Directory.GetDirectories("./credentials");

                if (usrDirs.Any(dir => dir.Contains("youtube") || dir.Contains("facebook") || dir.Contains("instagram"))) {
                    Console.WriteLine($"Detected old folder structure. Loading only the old structure credentials, where TikTok is not available. Please, consider changing to the new folder structure");
                } else {
                    foreach (var usrDir in usrDirs) {
                        CredentialsDir = $"{usrDir}/tiktok";

                        if (!Directory.Exists(CredentialsDir)) {
                            Console.WriteLine($"Missing or invalid TikTok channels!");
                            Console.WriteLine($"Couldn't find any credential on folder '{CredentialsDir}'");
                            continue;
                        }
                        foreach (var user in Directory.GetFiles(CredentialsDir)) {
                            var text = File.ReadAllText($"{user}");
                            var userJson = JObject.Parse(text);
                            var username = userJson["account_name"];
                            Console.WriteLine($"The username ({username}).");
                            tiktokUsernames.Add(username.ToString());
                        }
                    }
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                string message = String.Format("Missing or invalid TikTok credentials!\n{0}", e.Message);
                if (e is DirectoryNotFoundException) {
                    message = String.Format("{0}\nCheck if the path above exists!", message);
                }
                Console.WriteLine(message);
            };
            return tiktokUsernames;
        }

        private List<AbstractJob> GetListOfJobs(List<String> tiktokAccountNames) {
            return new List<AbstractJob>() {
                       new PostsQuery(tiktokAccountNames)
            };
        }
    }
}
