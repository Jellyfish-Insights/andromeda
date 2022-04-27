using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;

/*
 * This is 95% adapted from Jobs.Fetcher.YouTube/YouTubeFetchers.cs
 *
 * The original file did not allow "services" objects to be retrieved, only to
 * be piped to the jobs list. That is why we had to adapt it.
 *
 * In the future, it may be beneficial to reorganize this code in such a manner
 * that both fetchers can access it, and then place it in Andromeda.Common
 */

namespace Jobs.Fetcher.YouTubeStudio.Helpers {
    public static class Credentials {
        private const string CredentialsDir = "./credentials";
        private static readonly string SecretsFile =
            $"{CredentialsDir}/client_secret.json";
        private static readonly List<(YouTubeService, YouTubeAnalyticsService)>
        NoServices = new List<(YouTubeService, YouTubeAnalyticsService)>();

        public static List<(YouTubeService, YouTubeAnalyticsService)>
        GetAllServices() {
            var ytServices = new List<(YouTubeService, YouTubeAnalyticsService)>();
            try
            {
                var usrDirs = Directory.GetDirectories(CredentialsDir);
                if (usrDirs.Any(dir =>
                                dir.Contains("youtube")
                                || dir.Contains("facebook")
                                || dir.Contains("instagram")
                                )) {
                    Console.WriteLine("Detected old folder structure. Loading "
                                      + "only the old structure credentials. Please, consider "
                                      + "changing to the new folder structure"
                                      );
                    return ServicesFromDirectory($"{CredentialsDir}/youtube/");
                } else {
                    foreach (var usrDir in usrDirs) {
                        var dir = $"{usrDir}/youtube/";
                        if (!Directory.Exists(dir) || !File.Exists(SecretsFile)) {
                            Console.WriteLine($"Missing or invalid Youtube credentials!");
                            Console.WriteLine($"Couldn't find any credential on folder '{CredentialsDir}'");
                            continue;
                        }
                        ytServices.AddRange(ServicesFromDirectory(dir));
                    }
                }
                return ytServices;
            }
            catch (Exception e)
                when(e is FileNotFoundException || e is DirectoryNotFoundException) {
                    Console.WriteLine($"Missing or invalid YouTube credentials!\n{e.Message}");
                    if (e is DirectoryNotFoundException) {
                        Console.WriteLine("Check if the path above exists!");
                    }
                    return NoServices;
                };
        }

        private static List<(YouTubeService, YouTubeAnalyticsService)>
        ServicesFromDirectory(string directory) {
            Console.WriteLine($"Getting services from directory {directory}");
            var ytServices = new List<(YouTubeService, YouTubeAnalyticsService)>();
            var dirs = Directory.GetDirectories(directory).ToList();
            if (dirs.Count == 0) {
                Console.WriteLine($"Directory {directory} is empty.");
            }
            foreach (var dir in dirs) {
                Console.WriteLine($"Now in {dir}");
                ytServices.Add(GetServicesFromCredential(SecretsFile, dir));
            }
            return ytServices;
        }

        private static (YouTubeService, YouTubeAnalyticsService)
        GetServicesFromCredential
        (
            string clientSecretFileName,
            string dataStoreFolder
        ) {
            const string ApplicationName = "Andromeda";
            var credential = GetUserCredential(clientSecretFileName, dataStoreFolder);
            var dataService = new YouTubeService(
                new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            var analyticsService = new YouTubeAnalyticsService(
                new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            return (dataService, analyticsService);
        }

        private static UserCredential GetUserCredential
        (
            string clientSecretFileName,
            string dataStoreFolder
        ) {
            var scopes = new string[] {
                YouTubeService.Scope.YoutubeReadonly,
                YouTubeAnalyticsService.Scope.YoutubeReadonly,
                YouTubeAnalyticsService.Scope.YtAnalyticsMonetaryReadonly,
            };

            Console.WriteLine($"clientSecretFileName is {clientSecretFileName}");
            using (var stream = new FileStream(
                       clientSecretFileName,
                       FileMode.Open,
                       FileAccess.Read)
                   ) {
                Console.WriteLine($"dataStoreFolder is {dataStoreFolder}");
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "Credentials.json",
                    CancellationToken.None,
                    new FileDataStore(dataStoreFolder, true)
                    ).Result;
            }
        }
    }
}
