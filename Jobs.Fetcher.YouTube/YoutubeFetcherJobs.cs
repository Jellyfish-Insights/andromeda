using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;

using Jobs.Fetcher.YouTube.Helpers;
using Andromeda.Common.Extensions;

namespace Jobs.Fetcher.YouTube {

    public class VideosQuery : YoutubeFetcher {
        public VideosQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos
            ): base(accountInfos, true) {}

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        private const int _retry = 3;

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {

            var fetcher = new ApiDataFetcher(Logger, DataService, AnalyticsService);
            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();

            var videoIds = fetcher.FetchVideoIds(uploadsListId)
                               .ToHashSet()
                               .ToList();

            for (int i = 0; i < _retry; i++) {
                var batches = ListExtensions.DivideIntoBatches(videoIds, DegreeOfParallelism);
                Logger.Information(ListExtensions.DebugBatches(batches));
                var rejected = new List<string>();
                var threads = new List<Thread>();

                foreach (var batch in batches) {
                    var t = new Thread(() => {
                        var convertedVideos = fetcher.FetchVideoProperties(batch)
                                                  .Select(x => Api2DbObjectConverter.ConvertVideo(x));
                        DbWriter.Write(convertedVideos, channelId, Logger);
                        rejected.AddRange(fetcher.RejectedGet());
                    });
                    threads.Add(t);
                    t.Start();
                }

                foreach (var t in threads) {
                    t.Join();
                }

                videoIds = rejected;

                if (videoIds.Count() > 0 && i < _retry - 1) {
                    fetcher.Diagnostics();
                    fetcher.ResetRejected();
                    Logger.Information($"Retrying, # {i + 1} time (Max. = {_retry})");
                } else {
                    Logger.Information("All videos fetched successfully!");
                    break;
                }
            }

            fetcher.PrintAPIStatistics();
        }
    }

    public class PlaylistsQuery : YoutubeFetcher {
        public PlaylistsQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos
            ): base(accountInfos, true) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideosQuery>() };
        }

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {
            var fetcher = new ApiDataFetcher(Logger, DataService, AnalyticsService);
            fetcher.UseThreads(DegreeOfParallelism);

            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();
            Logger.Information($"Fetching playlists for channel {channelId}");

            var playlists = fetcher.FetchPlaylists(channelId).ToList();
            Logger.Information($"{playlists.Count()} playlists found!");

            var batches = ListExtensions.DivideIntoBatches(playlists, DegreeOfParallelism);
            Logger.Verbose(ListExtensions.DebugBatches(batches));

            var threads = new List<Thread>();

            foreach (var batch in batches) {
                var t = new Thread(() => {
                    var videoIds = batch.Select(x => fetcher.GetVideoIdsInPlaylist(x.Id));

                    var playlistsWithVideos = batch.Zip(
                        videoIds,
                        (playlist, ids) => new { playlist, ids });

                    var convertedPlaylists = playlistsWithVideos.Select(x =>
                                                                        Api2DbObjectConverter.ConvertPlaylist(x.playlist, x.ids));

                    DbWriter.Write(convertedPlaylists, Logger);
                });
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads) {
                t.Join();
            }

            fetcher.PrintAPIStatistics();
        }
    }

    public class DailyVideoMetricsQuery : YoutubeFetcher {
        public DailyVideoMetricsQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos
            ): base(accountInfos, false) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideosQuery>() };
        }

        private const int _retry = 5;

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {
            var fetcher = new ApiDataFetcher(Logger, DataService, AnalyticsService);
            fetcher.UseThreads(DegreeOfParallelism);
            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();
            var videos = DbReader.GetVideos()
                             .Where(v => v.ChannelId == channelId);

            for (int i = 0; i < _retry; i++) {

                Logger.Information($"We will get {videos.Count()} videos, channel {channelId}.");
                videos.AsParallel()
                    .WithDegreeOfParallelism(DegreeOfParallelism)
                    .ForAll(video => DbWriter.Write(fetcher.FetchDailyMetrics(channelId, video), Logger));

                /* */
                var videoIds = fetcher.RejectedGet().ToList();

                videos = DbReader.GetVideos()
                             .Where(v =>
                                    v.ChannelId == channelId
                                    && videoIds.Contains(v.VideoId)
                                    );

                fetcher.Diagnostics();
                fetcher.ResetRejected();

                if (videos.Count() == 0) {
                    break;
                }

                if (i < _retry - 1) {
                    Logger.Information($"Sleeping before trying for # {i + 1} time (Max. = {_retry} )");
                    Thread.Sleep(10 * 1000);
                }
            }

            fetcher.PrintAPIStatistics();
        }
    }

    public class ReprocessDailyVideoMetricsQuery : YoutubeFetcher {
        bool forceFetch;
        public ReprocessDailyVideoMetricsQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos,
            bool forceFetch): base(accountInfos, false) {
            this.forceFetch = forceFetch;
        }

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<DailyVideoMetricsQuery>(), IdOf<StatisticsQuery>() };
        }

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {
            var fetcher = new ApiDataFetcher(Logger, DataService, AnalyticsService);
            fetcher.UseThreads(DegreeOfParallelism);

            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();
            var comparison = DbReader.CompareVideoLifetimeDailyTotal();
            long comparisonMinLimit = 500;
            double comparisonThreshold = 0.05;

            Logger.Information($"We are comparing {comparison.Count()} items");
            Logger.Information($"forceFetch is {forceFetch}");

            foreach (var item in comparison) {
                if (item.Lifetime > 0 && item.Lifetime > comparisonMinLimit) {
                    var ratio = Math.Abs((double) item.Lifetime - item.Total) / ((double) item.Lifetime);
                    Logger.Information("Ratio is " + string.Format("{0:0.000}", ratio));
                    if (forceFetch || ratio > comparisonThreshold) {
                        Logger.Information($"Reprocessing video {item.Id.VideoId}");
                        DbWriter.Write(fetcher.FetchDailyMetrics(channelId, item.Id, true), Logger);
                    }
                }
            }
            fetcher.Diagnostics();
            fetcher.PrintAPIStatistics();
        }
    }

    public class ViewerPercentageQuery : YoutubeFetcher {
        public ViewerPercentageQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos):
            base(accountInfos, false) {}

        public override List<string> Dependencies() {
            return new List<string>(){
                       IdOf<VideosQuery>(),
                       IdOf<DailyVideoMetricsQuery>(),
                       // in logical terms, this job doesn't depend on ReprocessDailyVideoMetrics,
                       // but, as both require a very large amount of requests, it is a good
                       // idea not to run them in parallel
                       IdOf<ReprocessDailyVideoMetricsQuery>()
            };
        }

        override public void RunBody(YouTubeService DataService,
                                     YouTubeAnalyticsService AnalyticsService) {
            RunBodyWrap(DataService, AnalyticsService, false);
        }

        public void RunBodyWrap(YouTubeService DataService,
                                YouTubeAnalyticsService AnalyticsService,
                                bool fetchAll
                                ) {
            var fetcher = new ApiDataFetcher(Logger, DataService, AnalyticsService);
            fetcher.UseThreads(DegreeOfParallelism);

            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();

            var videos = DbReader.GetVideos()
                             .Where(v => v.ChannelId == channelId)
                             .ToList();

            Logger.Information($"Channel {channelId}: we will fetch {videos.Count()} videos.");

            var batches = ListExtensions.DivideIntoBatches(videos, DegreeOfParallelism);
            Logger.Information($"Pre-fetching tasks...");
            Logger.Information(ListExtensions.DebugBatches(batches));

            var tasks = new List<ViewerPercentagesTask>();
            var threads = new List<Thread>();
            foreach (var batch in batches) {
                var t = new Thread(() => {
                    foreach (var v in batch) {
                        tasks.AddRange(fetcher.GetViewerPercentagesTasks(channelId, v, fetchAll));
                    }
                });
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads) {
                t.Join();
            }

            fetcher.Diagnostics();
            fetcher.ResetRejected();
            Logger.Information($"We have {tasks.Count()} tasks to do. Estimated time "
                               + $"{fetcher.EstimateCompletionTime(tasks.Count())}");

            threads.Clear();
            var batchedTasks = ListExtensions.DivideIntoBatches(tasks, DegreeOfParallelism);
            foreach (var batch in batchedTasks) {
                var t = new Thread(() => {
                    foreach (var task in batch) {
                        fetcher.DoViewerPercentageTask(task);
                    }
                });
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads) {
                t.Join();
            }

            fetcher.Diagnostics();
            fetcher.PrintAPIStatistics();
        }
    }

    public class ReprocessViewerPercentageQuery : YoutubeFetcher {
        public ReprocessViewerPercentageQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos
            ): base(accountInfos, false) {}

        public override List<string> Dependencies() {
            return new List<string>(){ IdOf<ViewerPercentageQuery>() };
        }

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {

            IConfiguration configuration = new ConfigurationBuilder()
                                               .SetBasePath(Directory.GetCurrentDirectory())
                                               .AddJsonFile("appsettings.json")
                                               .Build();

            double probabilityOfRunning = double.Parse(configuration["ReprocessViewerPercentageQuery_Probability"]);
            int percentage = (int) (probabilityOfRunning * 100);

            Logger.Information($"This job will only be run {percentage}% of the times "
                               + "it is called. The goal is to assure there are absolutely no holes for "
                               + "our Viewer Percentages, by force refetching them all.");

            var r = (new Random()).Next(0, 100);

            if (r < percentage) {
                Logger.Information("This is your lucky day! We are refetching all "
                                   + "Viewer Percentage data.");
                var VPQ = new ViewerPercentageQuery(AccountInfos);
                VPQ.UseLogger(Logger);
                VPQ.RunBodyWrap(DataService, AnalyticsService, true);
            } else {
                Logger.Information("We are skipping this job this time.");
            }
        }
    }

    public class StatisticsQuery : YoutubeFetcher {
        public StatisticsQuery(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos
            ): base(accountInfos, true) {}

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideosQuery>() };
        }

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {
            var fetcher = new ApiDataFetcher(Logger, DataService, AnalyticsService);
            fetcher.UseThreads(DegreeOfParallelism);

            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();
            var videoIds = DbReader.GetVideos()
                               .Where(v => v.ChannelId == channelId)
                               .Select(v => v.VideoId)
                               .ToList();

            var batches = ListExtensions.DivideIntoBatches(videoIds, DegreeOfParallelism);
            Logger.Information(ListExtensions.DebugBatches(batches));

            var threads = new List<Thread>();
            foreach (var batch in batches) {
                var t = new Thread(() => {
                    var statistics = fetcher.FetchVideoStatistics(batch)
                                         .Where(x => x.Statistics != null)
                                         .Select(x => Api2DbObjectConverter.ConvertStatistics(x));
                    DbWriter.Write(statistics, Logger);
                });
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads) {
                t.Join();
            }

            fetcher.PrintAPIStatistics();
        }
    }

    public class APIStressTest : YoutubeFetcher {
        public APIStressTest(
            List<(YouTubeService dataService, YouTubeAnalyticsService analyticsService)> accountInfos
            ): base(accountInfos, false) {}

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        string Disclaimer = "\nYouTube is not clear if their quota is defined on a per minute "
                            + "basis or on a per day basis, or even both. Please compare the information "
                            + "above to what you will find in the API documentation to achieve a coherent "
                            + "interpretation.\n";

        override public void RunBody(YouTubeService DataService, YouTubeAnalyticsService AnalyticsService) {

            Logger.Warning("\n\nThis job will test what the real quota for YouTube Data "
                           + "and YouTube Analytics is. This means it will exhaust your quota for "
                           + "that day. Only do this with a mock account / project.\n\n"
                           + "We will start the test in 30 seconds, press Ctrl+C to abort.\n\n");
            Thread.Sleep(30 * 1000);

            var fetcher = new APIStressFetcher(Logger, DataService, AnalyticsService);

            // we will need this information for YTA, but this has to be done before
            // exhausting YTD quota
            var(channelId, uploadsListId) = fetcher.FetchChannelInfo();
            var videoIds = fetcher.FetchVideoIds(uploadsListId).ToList();
            if (videoIds.Count() == 0) {
                Logger.Error("No videos found, cannot continue!");
                throw new ApplicationException("No videos found");
            }
            var videoId = videoIds[0];

            Logger.Information($"Mock channel = {channelId}, mock video = {videoId}");
            Logger.Warning($"\n\nThis is your last chance to give up. You have 30 seconds\n\n");
            Thread.Sleep(30 * 1000);

            const int threadCount = 50;
            var threads = new List<Thread>();
            Logger.Information($"We will run this test with {threadCount} threads");

            /* * * * YOUTUBE ANALYTICS * * * */

            fetcher.testStartTime = DateTime.UtcNow;
            Logger.Information($"Starting YouTube Analytics: {fetcher.testStartTime.ToString()}");
            for (int tid = 0; tid < threadCount; tid++) {
                var t = new Thread(() => fetcher.StressTestYTA(channelId, videoId, tid));
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads)
                t.Join();

            fetcher.testEndTime = DateTime.UtcNow;
            Logger.Information($"Finished YouTube Analytics: {fetcher.testEndTime.ToString()}");

            TimeSpan ts = fetcher.testEndTime - fetcher.testStartTime;
            double elapsedMilliseconds = ts.TotalMilliseconds;
            Logger.Information($"Took: {elapsedMilliseconds} ms");

            var YTARequests = fetcher.GetYTARequests();
            Logger.Information($"Total requests: {YTARequests}");
            Logger.Information(Disclaimer);

            /* * * * YOUTUBE DATA * * * */

            threads.Clear();
            fetcher.testStartTime = DateTime.UtcNow;
            Logger.Information($"Starting YouTube Data: {fetcher.testStartTime.ToString()}");
            for (int tid = 0; tid < threadCount; tid++) {
                var t = new Thread(() => fetcher.StressTestYTD(tid));
                threads.Add(t);
                t.Start();
            }

            foreach (var t in threads)
                t.Join();

            fetcher.testEndTime = DateTime.UtcNow;
            Logger.Information($"Finished YouTube Data: {fetcher.testEndTime.ToString()}");

            ts = fetcher.testEndTime - fetcher.testStartTime;
            elapsedMilliseconds = ts.TotalMilliseconds;
            Logger.Information($"Took: {elapsedMilliseconds} ms");

            var YTDRequests = fetcher.GetYTDRequests();
            Logger.Information($"Total requests: {YTDRequests}");
            Logger.Information(Disclaimer);

            fetcher.PrintAPIStatistics();
        }
    }
}
