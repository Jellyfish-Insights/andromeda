using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLakeModels;
using YTD = DataLakeModels.Models.YouTube.Data;
using YTA = DataLakeModels.Models.YouTube.Analytics;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubeAnalytics.v2;
using Serilog.Core;
using Andromeda.Common;
using System.Threading;

namespace Jobs.Fetcher.YouTube.Helpers {

    public static class ApiDataFetcher {

        public static int ViewerPercentageCallCount { get; set; } = 0;

        public static (string, string) FetchChannelInfo(YouTubeService service) {
            var request = service.Channels.List("id,contentDetails");
            request.Mine = true;
            Thread.Sleep(500);
            var result = request.ExecuteAsync().Result;

            var channelId = result.Items[0].Id;
            var uploadsListId = result.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            return (channelId, uploadsListId);
        }

        public static IEnumerable<string> FetchVideoIds(YouTubeService service, string uploadsListId) {

            var request = service.PlaylistItems.List("snippet");
            request.PlaylistId = uploadsListId;

            PlaylistItemListResponse response;
            do {
                response = request.ExecuteAsync().Result;

                foreach (var videoId in response.Items.Select(i => i.Snippet.ResourceId.VideoId)) {
                    yield return videoId;
                }

                request.PageToken = response.NextPageToken;
            } while (!String.IsNullOrEmpty(response.NextPageToken));
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

        public static IEnumerable<Playlist> FetchPlaylists(YouTubeService service, string channelId) {
            var playlistRequest = service.Playlists.List("snippet");
            playlistRequest.ChannelId = channelId;

            PlaylistListResponse response;
            do {
                response = playlistRequest.ExecuteAsync().Result;

                foreach (var playList in response.Items) {
                    yield return playList;
                }

                playlistRequest.PageToken = response.NextPageToken;
                Thread.Sleep(500);
            } while (!String.IsNullOrEmpty(response.NextPageToken));
        }

        private static IEnumerable<PlaylistItem> FetchItemsInPlaylist(YouTubeService service, string playListId) {
            var playlistItemsRequest = service.PlaylistItems.List("snippet");
            playlistItemsRequest.PlaylistId = playListId;

            PlaylistItemListResponse result;
            do {
                result = playlistItemsRequest.ExecuteAsync().Result;

                foreach (var item in result.Items) {
                    yield return item;
                }

                playlistItemsRequest.PageToken = result.NextPageToken;
                Thread.Sleep(500);
            } while (!String.IsNullOrEmpty(result.NextPageToken));
        }

        private static string[] GetVideoIdsInPlaylist(YouTubeService service, string playlistId) {
            return FetchItemsInPlaylist(service, playlistId)
                       .Where(x => x.Snippet.ResourceId.Kind == "youtube#video")
                       .Select(x => x.Snippet.ResourceId.VideoId)
                       .Distinct()
                       .ToArray();
        }

        public static IEnumerable<string[]> FetchVideoIdsInPlaylists(YouTubeService service, IEnumerable<Playlist> playlists) {
            return playlists.Select(x => GetVideoIdsInPlaylist(service, x.Id));
        }

        private static readonly TimeSpan TimeMargin = new TimeSpan(5, 0, 0, 0);

        private static IEnumerable<IList<object>> FetchVideoDailyMetrics(YTA.VideoDailyMetric mostRecentRecord, string channelId, YTD.Video video, DateTime now, YouTubeAnalyticsService analyticsService, Logger logger, bool reprocessMetrics = false) {
            DateTime mostRecentMetricDate;
            try {
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
            } catch (Exception e) {
                logger.Error($"Error fetching most recent Metric for video {video.VideoId}");
                throw e;
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

            Thread.Sleep(100);
            var report = reportRequest.ExecuteAsync().Result;

            if (report.Rows != null) {
                logger.Debug("Found {Rows} rows", report.Rows.Count);
                foreach (var row in report.Rows) {
                    yield return row;
                }
            }
        }

        public static List<(DateTime date, long subscriberViews)> FetchSubscriberViews(YTA.VideoDailyMetric mostRecentRecord, string channelId, YTD.Video video, DateTime now, YouTubeAnalyticsService analyticsService, Logger logger, bool reprocessMetrics = false) {
            DateTime mostRecentMetricDate;
            try {
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
            } catch (Exception e) {
                logger.Error($"Error fetching most recent Metric for video {video.VideoId}");
                throw e;
            }

            var fromDate = DateHelper.Min(now - TimeMargin, mostRecentMetricDate);

            var toDate = now;
            var reportRequest = analyticsService.Reports.Query();
            reportRequest.Ids = $"channel=={channelId}";
            reportRequest.StartDate = fromDate.ToString("yyyy-MM-dd");
            reportRequest.EndDate = toDate.ToString("yyyy-MM-dd");
            reportRequest.Metrics = "views";
            reportRequest.Filters = $"video=={video.VideoId};subscribedStatus==SUBSCRIBED";
            reportRequest.Dimensions = "subscribedStatus,day";

            var subscriberViewsList = new List<(DateTime date, long subscriberViews)>();

            Thread.Sleep(100);
            try {
                var report = reportRequest.ExecuteAsync().Result;
                if (report.Rows != null && report.Rows.Count > 0) {
                    foreach (var row in report.Rows) {
                        subscriberViewsList.Add((Convert.ToDateTime(row[1]).Date, (long) row[2]));
                    }
                } else {
                    logger.Warning($"Empty rows of Subscriber Views for video {video.VideoId}");
                }
            }
            catch (Exception exc) {
                logger.Error($"Could not get Subscriber Views for video {video.VideoId}");
                if (exc.InnerException is Google.GoogleApiException) {
                    logger.Error("Google API raised an error!");
                } else {
                    logger.Error("An unknown exception was raised!");
                    logger.Error(exc.ToString());
                }
            }
            return subscriberViewsList;
        }

        public static IEnumerable<YTA.VideoDailyMetric> FetchDailyMetrics(YouTubeAnalyticsService analyticsService, string channelId, YTD.Video video, Logger logger, bool reprocess = false) {
            using (var dbContext = new DataLakeYouTubeAnalyticsContext()) {
                var now = DateTime.UtcNow;
                var mostRecentRecord = dbContext.VideoDailyMetrics
                                           .Where(x => x.VideoId == video.VideoId && x.ValidityStart <= now && x.ValidityEnd >= now)
                                           .OrderByDescending(x => x.Date)
                                           .FirstOrDefault();

                var videoDailyMetrics = FetchVideoDailyMetrics(mostRecentRecord, channelId, video, now, analyticsService, logger, reprocess);
                var subscriberViews = FetchSubscriberViews(mostRecentRecord, channelId, video, now, analyticsService, logger, reprocess);
                return videoDailyMetrics.Select(x => Api2DbObjectConverter.ConvertDailyMetricRow(video.VideoId, x, subscriberViews));
            }
        }

        private static IList<IList<object>> RunViewerPercentageReport(YouTubeAnalyticsService analyticsService, string channelId, DateTime fromDate, DateTime toDate, YTD.Video video, Logger logger) {
            var reportRequest = analyticsService.Reports.Query();

            var parameters = new Dictionary<string, string> {
                { "StartDate", fromDate.ToString("yyyy-MM-dd") },
                { "EndDate", toDate.ToString("yyyy-MM-dd") },
                { "Ids", $"channel=={channelId}" },
                { "Metrics", "viewerPercentage" },
                { "Filters", String.Format("video=={0}", video.VideoId) },
                { "Dimensions", "gender,ageGroup" },
                { "Sort", "gender,ageGroup" }
            };

            reportRequest.StartDate = parameters["StartDate"];
            reportRequest.EndDate = parameters["EndDate"];
            reportRequest.Ids = parameters["Ids"];
            reportRequest.Metrics = parameters["Metrics"];
            reportRequest.Filters = parameters["Filters"];
            reportRequest.Dimensions = parameters["Dimensions"];
            reportRequest.Sort = parameters["Sort"];

            StringBuilder sb = new StringBuilder("We will send this request:\n");
            foreach (var item in parameters) {
                sb.Append(String.Format("\t{0,15} : {1}\n", item.Key, item.Value));
            }
            logger.Debug(sb.ToString());

            ViewerPercentageCallCount++;
            Thread.Sleep(1000);
            try {
                return reportRequest.ExecuteAsync().Result.Rows;
            }
            catch {
                logger.Error($"Could not get Viewer Percentage Report for video "
                             + $"{video.VideoId} from channel {channelId} from "
                             + $"{parameters["StartDate"]} to {parameters["EndDate"]}.");
                return (IList<IList<object>>)Enumerable.Empty<IList<object>>();
            }
        }

        private const int MaxDaysToReplicateInIteration = 100;

        public static void FetchViewerPercentageMetrics(YouTubeAnalyticsService analyticsService, string channelId, YTD.Video video, Logger logger) {
            using (var dbContext = new DataLakeYouTubeAnalyticsContext()) {
                var now = DateTime.UtcNow;

                var lastFetchedDate = dbContext.ViewerPercentageLastDates.SingleOrDefault(x => x.VideoId == video.VideoId);

                var initialDate = video.PublishedAt;

                if (lastFetchedDate == null) {
                    // maybe the video simply doesn't support this report, check for early termination
                    if (!RunViewerPercentageReport(analyticsService, channelId, video.PublishedAt, now, video, logger).Any()) {
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
                    var viewerPercentages = RunViewerPercentageReport(analyticsService, channelId, video.PublishedAt.Date, date, video, logger);
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
                logger.Debug("Replicated {Days} days for video {VideoId}", replicatedDays, video.VideoId);
            }
        }
    }
}
