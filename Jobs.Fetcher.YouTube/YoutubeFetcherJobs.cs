using System.Collections.Generic;
using System.Linq;
using System;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;
using Jobs.Fetcher.YouTube.Helpers;

namespace Jobs.Fetcher.YouTube {

    public class VideosQuery : YoutubeFetcher {

        public YouTubeService DataService;

        public VideosQuery(YouTubeService dataService) {
            DataService = dataService;
        }

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        override public void Run() {
            var(channelId, uploadsListId) = ApiDataFetcher.FetchChannelInfo(DataService);
            var videoIds = ApiDataFetcher.FetchVideoIds(DataService, uploadsListId).ToList();
            var videoProperties = ApiDataFetcher.FetchVideoProperties(DataService, videoIds);
            DbWriter.Write(videoProperties.Select(x => Api2DbObjectConverter.ConvertVideo(x)), channelId, Logger);
        }
    }

    public class PlaylistsQuery : YoutubeFetcher {

        public YouTubeService DataService;

        public PlaylistsQuery(YouTubeService dataService) {
            DataService = dataService;
        }

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideosQuery>() };
        }

        override public void Run() {
            var(channelId, uploadsListId) = ApiDataFetcher.FetchChannelInfo(DataService);
            var playlists = ApiDataFetcher.FetchPlaylists(DataService, channelId);
            var playlistsVideoIds = ApiDataFetcher.FetchVideoIdsInPlaylists(DataService, playlists);
            var playlistsWithVideos = playlists.Zip(playlistsVideoIds, (playlist, videoIds) => new { playlist, videoIds });
            DbWriter.Write(playlistsWithVideos.Select(x => Api2DbObjectConverter.ConvertPlaylist(x.playlist, x.videoIds)), Logger);
        }
    }

    public class DailyVideoMetricsQuery : YoutubeFetcher {

        public YouTubeService DataService;
        public YouTubeAnalyticsService AnalyticsService;

        public DailyVideoMetricsQuery(YouTubeService dataService, YouTubeAnalyticsService analyticsService) {
            DataService = dataService;
            AnalyticsService = analyticsService;
        }

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideosQuery>() };
        }

        private const int DegreeOfParallelism = 1;

        override public void Run() {
            var(channelId, uploadsListId) = ApiDataFetcher.FetchChannelInfo(DataService);
            var videos = DbReader.GetVideos();
            videos.AsParallel()
                .WithDegreeOfParallelism(DegreeOfParallelism)
                .ForAll(video => DbWriter.Write(ApiDataFetcher.FetchDailyMetrics(AnalyticsService, channelId, video, Logger)));
        }
    }

    public class ReprocessDailyVideoMetricsQuery : YoutubeFetcher {
        public YouTubeService DataService;
        public YouTubeAnalyticsService AnalyticsService;
        public ReprocessDailyVideoMetricsQuery(YouTubeService dataService, YouTubeAnalyticsService analyticsService) {
            DataService = dataService;
            AnalyticsService = analyticsService;
        }

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<DailyVideoMetricsQuery>(), IdOf<StatisticsQuery>() };
        }

        private const int DegreeOfParallelism = 1;

        override public void Run() {
            var(channelId, uploadsListId) = ApiDataFetcher.FetchChannelInfo(DataService);
            var comparison = DbReader.CompareVideoLifetimeDailyTotal();
            long comparisonMinLimit = 500;
            double comparisonThreshold = 0.05;
            foreach (var item in comparison) {
                if (item.Lifetime > 0) {
                    var ratio = Math.Abs((double) item.Lifetime - item.Total) / ((double) item.Lifetime);
                    if (Math.Abs(ratio) > comparisonThreshold && item.Lifetime > comparisonMinLimit) {
                        Logger.Information("Reprocessing video {0}: daily views {1} from {4} to {5}, lifetime views {2} at {6} and ratio {3}"
                                           , item.Id.VideoId, item.Total, item.Lifetime, ratio, item.DailyStart, item.DailyEnd, item.LifetimeDate);
                        DbWriter.Write(ApiDataFetcher.FetchDailyMetrics(AnalyticsService, channelId, item.Id, Logger, true));
                    }
                }
            }
        }
    }

    public class ViewerPercentageMetricsQuery : YoutubeFetcher {

        public YouTubeService DataService;
        public YouTubeAnalyticsService AnalyticsService;

        public ViewerPercentageMetricsQuery(YouTubeService dataService, YouTubeAnalyticsService analyticsService) {
            DataService = dataService;
            AnalyticsService = analyticsService;
        }

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideosQuery>(), IdOf<DailyVideoMetricsQuery>() };
        }

        private const int DegreeOfParallelism = 1;

        override public void Run() {
            var(channelId, uploadsListId) = ApiDataFetcher.FetchChannelInfo(DataService);
            var videos = DbReader.GetVideos();
            videos.AsParallel()
                .WithDegreeOfParallelism(DegreeOfParallelism)
                .ForAll(video => ApiDataFetcher.FetchViewerPercentageMetrics(AnalyticsService, channelId, video, Logger));
        }
    }

    public class StatisticsQuery : YoutubeFetcher {

        public YouTubeService DataService;

        public StatisticsQuery(YouTubeService dataService) {
            DataService = dataService;
        }

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        override public void Run() {
            var(channelId, uploadsListId) = ApiDataFetcher.FetchChannelInfo(DataService);
            var videoIds = ApiDataFetcher.FetchVideoIds(DataService, uploadsListId).ToList();
            var videoProperties = ApiDataFetcher.FetchVideoStatistics(DataService, videoIds).Where(x => x.Statistics != null);
            DbWriter.Write(videoProperties.Select(x => Api2DbObjectConverter.ConvertStatistics(x)), Logger);
        }
    }
}
