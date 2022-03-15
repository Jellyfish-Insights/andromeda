using Serilog.Core;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

using DataLakeModels;
using Andromeda.Common.Jobs;
using Andromeda.Common.Logging;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;

namespace Jobs.Fetcher.YouTube {

    public abstract class YoutubeFetcher : AbstractJob {
        public List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> AccountInfos;

        protected bool _runParallel;
        protected int DegreeOfParallelism = 1;

        public YoutubeFetcher(
                List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos,
                bool runParallel
                ) {
            AccountInfos = accountInfos;
            _runParallel = runParallel;

            IConfiguration configuration = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetCurrentDirectory())
                                            .AddJsonFile("appsettings.json")
                                            .Build();
            DegreeOfParallelism = int.Parse(configuration["DegreeOfParallelism"]);
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public void UseLogger(Logger logger) {
            Logger = logger;
        }

        public override void Run() {
            var nAccounts = AccountInfos.Count();
            Logger.Information($"We have {nAccounts} accounts to process");

            if (_runParallel) {
                var threads = new List<Thread>();
                for (int i = 0; i < nAccounts; i++) {
                    var (DataService, AnalyticsService) = AccountInfos[i];
                    Logger.Verbose($"Starting thread # {i + 1}");
                    var t = new Thread(() => {
                        try {
                            RunBody(DataService, AnalyticsService);
                        }
                        catch (Exception e) {
                            Logger.Error($"Oopsie... Thread # {i + 1} threw an error:\n"
                                + e.ToString());
                        }
                    });
                    threads.Add(t);
                    t.Start();
                }

                foreach(var t in threads) {
                    t.Join();
                }
            } else {
                Logger.Information("This job will NOT run in parallel");
                for (int i = 0; i < nAccounts; i++) {
                    var (DataService, AnalyticsService) = AccountInfos[i];
                    Logger.Verbose($"Processing account # {i + 1}");
                    try {
                        RunBody(DataService, AnalyticsService);
                    }
                    catch (Exception e) {
                        Logger.Error($"Oopsie... Error processing account # {i + 1}:\n"
                        + e.ToString());
                    }
                }
            }
        }

        abstract public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService);
    }
}
