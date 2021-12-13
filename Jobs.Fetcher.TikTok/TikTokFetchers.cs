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

            //Dictionary<string, TikTokCredentials> credentials = GetTikTokCredentials();

            //Dictionary<string, ITikTokClient> dataClients = BuildTikTokDataClients(credentials);

            return new List<AbstractJob>() {
                       //new UserQuery(dataClients),
            };
        }
/*
        private Dictionary<string, TwitterCredentials> GetTwitterCredentials() {

            var Credentials = new Dictionary<string, TwitterCredentials>();

            try {

                foreach (var usrDir in Directory.GetDirectories("./credentials")) {

                    foreach (var channel in Directory.GetDirectories($"{usrDir}/tiktok")) {

                        var CredentialsFilePath = $"{channel}/{SecretsFile}";

                        if (!File.Exists(CredentialsFilePath)) {
                            Console.WriteLine($"Missing or invalid Twitter credentials!");
                            Console.WriteLine($"Couldn't find any credential on folder '{usrDir}/tiktok'");
                            continue;
                        }

                        var text = File.ReadAllText(CredentialsFilePath);

                        var credential = JsonConvert.DeserializeObject<TwitterCredentials>(text);

                        if (credential.IsValid()) {
                            Credentials.Add(credential.Username, credential);
                        }
                    }
                }

                return Credentials;

            } catch (Exception e) {

                // TODO log the error
                System.Console.WriteLine(e.ToString());
                return new Dictionary<string, TwitterCredentials>();
            }
        }

        private Dictionary<string, ITwitterClient> BuildTwitterAdsClients(
            Dictionary<string, TwitterCredentials> credentials) {

            return credentials.ToDictionary(
                kvp => kvp.Key, kvp => new TwitterAdsClient(new ReadOnlyTwitterCredentials(kvp.Value as IReadOnlyTwitterCredentials)) as ITwitterClient
                );
        }

        private Dictionary<string, ITwitterClient> BuildTwitterDataClients(
            Dictionary<string, TwitterCredentials> credentials) {

            return credentials.ToDictionary(
                kvp => kvp.Key, kvp => new TwitterDataClient(new ReadOnlyTwitterCredentials(kvp.Value as IReadOnlyTwitterCredentials)) as ITwitterClient
                );
        }*/
    }
}
