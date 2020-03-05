using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;
using Google.Apis.Http;
using Google.Apis.Services;
using Andromeda.Common.Jobs;

namespace Jobs.Fetcher.YouTube {

    public class YouTubeFetchers : FetcherJobsFactory {

        public override JobScope Scope { get; } = JobScope.YouTube;

        public string CredentialsDir = "./credentials/youtube";
        public string SecretsFile = "./credentials/youtube/client_secret.json";

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope) || !CheckNameIsScope(names)) {
                return NoJobs;
            }
            var jobs = new List<AbstractJob>();
            var youtubeServices = new List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)>();
            try {
                foreach (var directory in Directory.GetDirectories(CredentialsDir)) {
                    youtubeServices.Add(GetServicesCredential(SecretsFile, directory));
                }

                if (youtubeServices.Count == 0) {
                    var path = $"{CredentialsDir}/channel_1";
                    Directory.CreateDirectory(path);
                    youtubeServices.Add(GetServicesCredential(SecretsFile, path));
                }

                jobs = new List<AbstractJob>() {
                    new VideosQuery(youtubeServices),
                    new PlaylistsQuery(youtubeServices),
                    new DailyVideoMetricsQuery(youtubeServices),
                    new ViewerPercentageMetricsQuery(youtubeServices),
                    new StatisticsQuery(youtubeServices),
                    new ReprocessDailyVideoMetricsQuery(youtubeServices),
                };
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                string message = String.Format("Missing or invalid YouTube credentials!\n{0}", e.Message);
                if (e is DirectoryNotFoundException) {
                    message = String.Format("{0}\nCheck if the path above exists!", message);
                }
                System.Console.WriteLine(message);
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
                    GoogleClientSecrets.Load(stream).Secrets,
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
    }
}
