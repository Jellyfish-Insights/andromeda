using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Andromeda.Common.Jobs;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;

namespace Jobs.Fetcher.YouTube {

    public class YouTubeFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.YouTube;

        public string CredentialsDir = "./credentials/youtube";
        public string SecretsFile = "./credentials/client_secret.json";

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }
            var jobs = new List<AbstractJob>();
            var youtubeServices = new List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)>();
            try {
                var usrDirs = Directory.GetDirectories("./credentials");

                if (usrDirs.Any(dir => dir.Contains("youtube") || dir.Contains("facebook") || dir.Contains("instagram"))) {
                    Console.WriteLine($"Detected old folder structure. Loading only the old structure credentials. Please, consider changing to the new folder structure");
                    jobs.AddRange(GetListOfJobs(youtubeServices, jobConfiguration.ForceFetch));
                } else {
                    foreach (var usrDir in usrDirs) {
                        CredentialsDir = $"{usrDir}/youtube";

                        if (!Directory.Exists(CredentialsDir) || !File.Exists(SecretsFile)) {
                            Console.WriteLine($"Missing or invalid Youtube credentials!");
                            Console.WriteLine($"Couldn't find any credential on folder '{CredentialsDir}'");
                            continue;
                        }

                        jobs.AddRange(GetListOfJobs(youtubeServices, jobConfiguration.ForceFetch));
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

        public static (YouTubeService dataService, YouTubeAnalyticsService analyticsService) GetServicesCredential(string clientSecretFileName, string dataStoreFolder) {
            var credential = GetUserCredential(clientSecretFileName, dataStoreFolder);
            return ((GetDataService(credential), GetAnalyticsService(credential)));
        }

        public static UserCredential GetUserCredential(string clientSecretFileName, string dataStoreFolder) {
            var scopes = new string[] {
                YouTubeService.Scope.YoutubeReadonly,
                YouTubeAnalyticsService.Scope.YoutubeReadonly,
                YouTubeAnalyticsService.Scope.YtAnalyticsMonetaryReadonly,
            };

            using (var stream = new FileStream(clientSecretFileName, FileMode.Open, FileAccess.Read)) {
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "Credentials.json",
                    CancellationToken.None,
                    new FileDataStore(dataStoreFolder, true)
                    ).Result;
            }
        }

        public static YouTubeService GetDataService(IConfigurableHttpClientInitializer credential) {
            return new YouTubeService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = "YouTube Daemon"
            });
        }

        public static YouTubeAnalyticsService GetAnalyticsService(IConfigurableHttpClientInitializer credential) {
            return new YouTubeAnalyticsService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = "YouTube Daemon"
            });
        }

        private List<AbstractJob> GetListOfJobs(List<(YouTubeService, YouTubeAnalyticsService)> youtubeServices, bool forceFetch) {
            foreach (var directory in Directory.GetDirectories(CredentialsDir)) {
                youtubeServices.Add(GetServicesCredential(SecretsFile, directory));
            }

            if (youtubeServices.Count == 0) {
                var path = $"{CredentialsDir}/channel_1";
                Directory.CreateDirectory(path);
                youtubeServices.Add(GetServicesCredential(SecretsFile, path));
            }

            return new List<AbstractJob>() {
                       new DailyVideoMetricsQuery(youtubeServices),
                    //    new PlaylistsQuery(youtubeServices),
                       new ReprocessDailyVideoMetricsQuery(youtubeServices, forceFetch),
                    //    new ReprocessViewerPercentageQuery(youtubeServices),
                    //    new StatisticsQuery(youtubeServices),
                    //    new VideosQuery(youtubeServices),
                    //    new ViewerPercentageQuery(youtubeServices),

                       // don't turn the following job on unless you know what you're doing
                        //   new APIStressTest(youtubeServices),
            };
        }
    }
}
