using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels;
using YTD = DataLakeModels.Models.YouTube.Data;
using YTA = DataLakeModels.Models.YouTube.Analytics;
using Google.Apis.Requests;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubeAnalytics.v2;
using Serilog.Core;
using Andromeda.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Jobs.Fetcher.YouTube.Helpers {

    public static class ApiDataFetcher {

        public const int Retries = 10;

        public static (string, string) FetchChannelInfo(YouTubeService service) {
            var request = service.Channels.List("id,contentDetails");
            request.Mine = true;
            Thread.Sleep(500);
            var result = request.ExecuteAsync().Result;

            var channelId = result.Items[0].Id;
            var uploadsListId = result.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            return (channelId, uploadsListId);
        }

        public static IEnumerable<string> FetchVideoIds(YouTubeService service, string uploadsListId, Logger logger) {

            var request = service.PlaylistItems.List("snippet");
            request.PlaylistId = uploadsListId;

            PlaylistItemListResponse response;
            do {
                response = FetchWithRetries<PlaylistItemListResponse>(request, logger);
                foreach (var videoId in response.Items.Select(i => i.Snippet.ResourceId.VideoId)) {
                    yield return videoId;
                }

                request.PageToken = response.NextPageToken;
            } while (!String.IsNullOrEmpty(response.NextPageToken));
        }

        private static TResponse FetchWithRetries<TResponse>(ClientServiceRequest<TResponse> request, Logger logger)
        {
            for (int i = 0; i < Retries; i++)
            {
                try
                {
                    return (request.ExecuteAsync() as Task<TResponse>).Result;
                }
                catch (Exception e)
                {
                    logger.Information("{Message}\nRetrying in {Time} milliseconds.", e.Message, 500 * (i + 1));
                }

                Thread.Sleep(500 * (i + 1));
            }
            throw new Exception($"Request failed. We reached the limit of retries: {Retries}");
        }

        private static IEnumerable<List<T>> SplitIntoBatches<T>(IEnumerable<T> source, int batchSize) {
            List<T> currentBatch = new List<T>();
            int count = 0;
            foreach (T e in source) {
                currentBatch.Add(e);
                count++;
                if (count >= batchSize) {
                    yield return currentBatch;
                    currentBatch = new List<T>();
                    count = 0;
                }
            }
            yield return currentBatch;
        }

        private const int BatchSize = 50;

        public static IEnumerable<Video> FetchVideoProperties(YouTubeService service, IEnumerable<string> videoIds) {
            var request = service.Videos.List("snippet,contentDetails,status");

            foreach (var batch in SplitIntoBatches<string>(videoIds, BatchSize)) {
                request.Id = String.Join(',', batch);
                Thread.Sleep(500);
                foreach (var video in request.ExecuteAsync().Result.Items) {
                    yield return video;
                }
            }
        }

        public static IEnumerable<Video> FetchVideoStatistics(YouTubeService service, IEnumerable<string> videoIds) {
            var request = service.Videos.List("statistics");

            foreach (var batch in SplitIntoBatches<string>(videoIds, BatchSize)) {
                request.Id = String.Join(',', batch);
                Thread.Sleep(500);
                foreach (var video in request.ExecuteAsync().Result.Items) {
                    yield return video;
                }
            }
        }

        public static IEnumerable<Playlist> FetchPlaylists(YouTubeService service, string channelId, Logger logger) {
            var playlistRequest = service.Playlists.List("snippet");
            playlistRequest.ChannelId = channelId;

            PlaylistListResponse response;
            do {
                response = FetchWithRetries<PlaylistListResponse>(playlistRequest, logger);

                foreach (var playList in response.Items) {
                    yield return playList;
                }

                playlistRequest.PageToken = response.NextPageToken;
                Thread.Sleep(500);
            } while (!String.IsNullOrEmpty(response.NextPageToken));
        }

        private static IEnumerable<PlaylistItem> FetchItemsInPlaylist(YouTubeService service, string playListId, Logger logger) {
            var playlistItemsRequest = service.PlaylistItems.List("snippet");
            playlistItemsRequest.PlaylistId = playListId;

            PlaylistItemListResponse result = null;
            do {
                result = FetchWithRetries<PlaylistItemListResponse>(playlistItemsRequest, logger);

                foreach (var item in result.Items) {
                    yield return item;
                }

                playlistItemsRequest.PageToken = result.NextPageToken;
                Thread.Sleep(500);
            } while (!String.IsNullOrEmpty(result.NextPageToken));
        }



        private static string[] GetVideoIdsInPlaylist(YouTubeService service, string playlistId, Logger logger) {
            return FetchItemsInPlaylist(service, playlistId, logger)
                       .Where(x => x.Snippet.ResourceId.Kind == "youtube#video")
                       .Select(x => x.Snippet.ResourceId.VideoId)
                       .Distinct()
                       .ToArray();
        }

        public static IEnumerable<string[]> FetchVideoIdsInPlaylists(YouTubeService service, IEnumerable<Playlist> playlists, Logger logger) {
            return playlists.Select(x => GetVideoIdsInPlaylist(service, x.Id, logger));
        }

        private static readonly TimeSpan TimeMargin = new TimeSpan(5, 0, 0, 0);

        private static IEnumerable<IList<object>> FetchVideoDailyMetrics(YTA.VideoDailyMetric mostRecentRecord, string channelId, YTD.Video video, DateTime now, YouTubeAnalyticsService analyticsService, Logger logger, bool reprocessMetrics = false) {
            DateTime mostRecentMetricDate;
            if (mostRecentRecord == null || reprocessMetrics) {
                TimeSpan PublishedAtOffset;
                if (reprocessMetrics) {
                    PublishedAtOffset = new TimeSpan(30, 0, 0, 0);
                } else {
                    PublishedAtOffset = TimeMargin;
                }
                mostRecentMetricDate = (video.PublishedAt != null) ? video.PublishedAt - PublishedAtOffset : now;
            } else {
                mostRecentMetricDate = mostRecentRecord.Date;
            }

            var fromDate = DateHelper.Min(now - TimeMargin, mostRecentMetricDate);
            var toDate = now;

            var reportRequest = analyticsService.Reports.Query();
            reportRequest.Ids = $"channel=={channelId}";
            reportRequest.StartDate = fromDate.ToString("yyyy-MM-dd");
            reportRequest.EndDate = toDate.ToString("yyyy-MM-dd");
            reportRequest.Metrics = "views,likes,shares,comments,averageViewDuration,dislikes,subscribersGained,subscribersLost,videosAddedToPlaylists,videosRemovedFromPlaylists";
            reportRequest.Filters = $"video=={video.VideoId}";
            reportRequest.Dimensions = "day";
            reportRequest.Sort = "day";

            Thread.Sleep(500);
            var report = reportRequest.ExecuteAsync().Result;

            if (report.Rows != null) {
                logger.Information("Found {Rows} rows", report.Rows.Count);
                foreach (var row in report.Rows) {
                    yield return row;
                }
            }
        }

        public static IEnumerable<YTA.VideoDailyMetric> FetchDailyMetrics(YouTubeAnalyticsService analyticsService, string channelId, YTD.Video video, Logger logger, bool reprocess = false) {
            using (var dbContext = new DataLakeYouTubeAnalyticsContext()) {
                var now = DateTime.UtcNow;
                var mostRecentRecord = dbContext.VideoDailyMetrics
                                           .Where(x => x.VideoId == video.VideoId && x.ValidityStart <= now && now < x.ValidityEnd)
                                           .OrderByDescending(x => x.Date)
                                           .FirstOrDefault();
                return FetchVideoDailyMetrics(mostRecentRecord, channelId, video, now, analyticsService, logger, reprocess)
                           .Select(x => Api2DbObjectConverter.ConvertDailyMetricRow(video.VideoId, x));
            }
        }

        private static IList<IList<object>> RunViewerPercentageReport(YouTubeAnalyticsService analyticsService, string channelId, DateTime fromDate, DateTime toDate, YTD.Video video) {
            var reportRequest = analyticsService.Reports.Query();
            reportRequest.Ids = $"channel=={channelId}";
            reportRequest.StartDate = fromDate.ToString("yyyy-MM-dd");
            reportRequest.EndDate = toDate.ToString("yyyy-MM-dd");
            reportRequest.Metrics = "viewerPercentage";
            reportRequest.Filters = String.Format("video=={0}", video.VideoId);
            reportRequest.Dimensions = "gender,ageGroup";
            reportRequest.Sort = "gender,ageGroup";
            Thread.Sleep(2000);
            return reportRequest.ExecuteAsync().Result.Rows;
        }

        private const int MaxDaysToReplicateInIteration = 100;

        public static void FetchViewerPercentageMetrics(YouTubeAnalyticsService analyticsService, string channelId, YTD.Video video, Logger logger) {
            using (var dbContext = new DataLakeYouTubeAnalyticsContext()) {
                var now = DateTime.UtcNow;

                var lastFetchedDate = dbContext.ViewerPercentageLastDates.SingleOrDefault(x => x.VideoId == video.VideoId);

                var initialDate = video.PublishedAt;

                if (lastFetchedDate == null) {
                    // maybe the video simply doesn't support this report, check for early termination
                    if (!RunViewerPercentageReport(analyticsService, channelId, video.PublishedAt, now, video).Any()) {
                        logger.Debug("Report not available for video {VideoId}", video.VideoId);
                        return;
                    }
                    lastFetchedDate = new YTA.ViewerPercentageLastDate() { VideoId = video.VideoId, Date = initialDate };
                    dbContext.Add(lastFetchedDate);
                } else {
                    initialDate = DateHelper.Max(video.PublishedAt, DateHelper.Min(lastFetchedDate.Date, now - TimeMargin));
                }

                int replicatedDays = 0;
                foreach (var date in DateHelper.DaysInRange(initialDate.Date, now.Date)) {
                    var viewerPercentages = RunViewerPercentageReport(analyticsService, channelId, video.PublishedAt.Date, date, video);
                    lastFetchedDate.Date = date;
                    if (!viewerPercentages.Any()) {
                        continue;
                    }

                    DbWriter.Write(
                        video.VideoId,
                        date,
                        viewerPercentages.Select(x => Api2DbObjectConverter.ConvertViewerPercentageRow(x)),
                        now
                        );

                    dbContext.SaveChanges();
                    replicatedDays++;
                    if (replicatedDays % MaxDaysToReplicateInIteration == 0) {
                        break;
                    }
                }
                logger.Information("Replicated {Days} days for video {VideoId}", replicatedDays, video.VideoId);
            }
        }
    }
}
