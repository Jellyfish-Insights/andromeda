using System;
using System.IO;
using Tweetinvi;
using System.Linq;
using Serilog.Core;
using Newtonsoft.Json;
using Andromeda.Common.Jobs;
using Tweetinvi.Models;
using System.Collections.Generic;

using DataLakeModels;
using Andromeda.Common.Logging;

using FlycatcherAds;
using FlycatcherData;

namespace Jobs.Fetcher.Twitter {

    public class TwitterFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.Twitter;
        private string SecretsFile = "twitter_credentials.json";

        private Logger Logger;

        public override IEnumerable<AbstractJob> GetJobs(
            JobType type,
            JobScope scope,
            IEnumerable<string> names,
            JobConfiguration jobConfiguration) {

            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }

            Logger = LoggerFactory.GetLogger<DataLakeLoggingContext>("TwitterFetcher");

            Dictionary<string, TwitterCredentials> credentials = GetTwitterCredentials();

            Dictionary<string, ITwitterClient> dataClients = BuildTwitterDataClients(credentials);
            Dictionary<string, ITwitterClient> adsClients = BuildTwitterAdsClients(credentials);

            return new List<AbstractJob>() {
                       new UserQuery(dataClients),
                       new TimelineQuery(dataClients),
                       new AdsAccountsQuery(adsClients),
                       new CampaignsQuery(adsClients),
                       new LineItemsQuery(adsClients),
                       new PromotedTweetsQuery(adsClients),
                       new VideoLibrariesQuery(adsClients),
                       new OrganicTweetDailyMetricsQuery(adsClients),
                       new PromotedTweetDailyMetricsQuery(adsClients),
                       new CustomAudiencesQuery(adsClients)
            };
        }

        private Dictionary<string, TwitterCredentials> GetTwitterCredentials() {

            var Credentials = new Dictionary<string, TwitterCredentials>();

            try {

                foreach (var usrDir in Directory.GetDirectories("./credentials")) {

                    foreach (var channel in Directory.GetDirectories($"{usrDir}/twitter")) {

                        var CredentialsFilePath = $"{channel}/{SecretsFile}";

                        if (!File.Exists(CredentialsFilePath)) {
                            Console.WriteLine($"Missing or invalid Twitter credentials!");
                            Console.WriteLine($"Couldn't find any credential on folder '{usrDir}/twitter'");
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

                Logger.Warning($"Unable to fetch Twitter credentials");
                Logger.Verbose($"Error: {e}");
                return new Dictionary<string, TwitterCredentials>();
            }
        }

        private Dictionary<string, ITwitterClient> BuildTwitterAdsClients(
            Dictionary<string, TwitterCredentials> credentials) {
            try {
                return credentials.ToDictionary(
                    kvp => kvp.Key, kvp => new TwitterAdsClient(new ReadOnlyTwitterCredentials(kvp.Value as IReadOnlyTwitterCredentials)) as ITwitterClient
                    );
            }catch (Exception e) {
                Logger.Warning($"Unable to fetch Twitter Ads credentials");
                Logger.Verbose($"Error: {e}");
                return new Dictionary<string, ITwitterClient>();
            }
        }

        private Dictionary<string, ITwitterClient> BuildTwitterDataClients(
            Dictionary<string, TwitterCredentials> credentials) {

            try {
                return credentials.ToDictionary(
                    kvp => kvp.Key, kvp => new TwitterDataClient(new ReadOnlyTwitterCredentials(kvp.Value as IReadOnlyTwitterCredentials)) as ITwitterClient
                    );
            }catch (Exception e) {
                Logger.Warning($"Unable to fetch Twitter Data credentials");
                Logger.Verbose($"Error: {e}");
                return new Dictionary<string, ITwitterClient>();
            }
        }
    }
}
