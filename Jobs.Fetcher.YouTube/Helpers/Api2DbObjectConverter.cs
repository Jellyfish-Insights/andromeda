using System;
using System.Collections.Generic;
using System.Linq;
using YTD = DataLakeModels.Models.YouTube.Data;
using YTA = DataLakeModels.Models.YouTube.Analytics;
using Google.Apis.YouTube.v3.Data;

namespace Jobs.Fetcher.YouTube.Helpers {

    public static class Api2DbObjectConverter {

        public static YTD.Playlist ConvertPlaylist(Playlist pl, string[] videoIds) {
            return new YTD.Playlist() {
                       PlaylistId = pl.Id,
                       Title = pl.Snippet.Title,
                       ThumbnailUrl = GetThumbnail(pl),
                       Description = pl.Snippet.Description,
                       VideoIds = videoIds
            };
        }

        public static YTD.Video ConvertVideo(Video v) {
            return new YTD.Video() {
                       VideoId = v.Id,
                       ThumbnailUrl = GetThumbnail(v),
                       Title = v.Snippet.Title,
                       Tags = (v.Snippet.Tags != null) ? v.Snippet.Tags.OrderBy(x => x).ToArray() : new string[] {},
                       PublishedAt = (DateTime) v.Snippet.PublishedAt,
                       Duration = v.ContentDetails.Duration,
                       PrivacyStatus = v.Status.PrivacyStatus
            };
        }

        public static YTD.Statistics ConvertStatistics(Video v) {
            return new YTD.Statistics() {
                       VideoId = v.Id,
                       CaptureDate = DateTime.Today,
                       ViewCount = (long) (v.Statistics.ViewCount ?? 0),
                       LikeCount = (long) (v.Statistics.LikeCount ?? 0),
                       DislikeCount = (long) (v.Statistics.DislikeCount ?? 0),
                       FavoriteCount = (long) (v.Statistics.FavoriteCount ?? 0),
                       CommentCount = (long) (v.Statistics.CommentCount ?? 0),
            };
        }

        public static YTA.VideoDailyMetric ConvertDailyMetricRow(string videoId, IList<object> row, List<(DateTime date, long subscriberViews)> subscriberViews) {
            var date = Convert.ToDateTime(row[0]).Date;
            return new YTA.VideoDailyMetric() {
                       VideoId = videoId,
                       Date = date,
                       Views = (long) row[1],
                       Likes = (long) row[2],
                       Shares = (long) row[3],
                       Comments = (long) row[4],
                       AverageViewDuration = (long) row[5],
                       Dislikes = (long) row[6],
                       SubscriberViews = subscriberViews.Where(x => x.date == date).Select(y => y.subscriberViews).FirstOrDefault(),
                       SubscribersGained = (long) row[7],
                       SubscribersLost = (long) row[8],
                       VideosAddedToPlaylists = (long) row[9],
                       VideosRemovedFromPlaylists = (long) row[10],
            };
        }

        public static YTA.ViewerPercentage ConvertViewerPercentageRow(IList<object> row) {
            return new YTA.ViewerPercentage() {
                       Gender = (string) row[0],
                       AgeGroup = (string) row[1],
                       Value = Convert.ToDouble(row[2])
            };
        }

        private static string GetThumbnail(Video v, string defaultValue = null) {
            if (v.Snippet.Thumbnails.Standard != null)
                return v.Snippet.Thumbnails.Standard.Url;
            if (v.Snippet.Thumbnails.Default__ != null)
                return v.Snippet.Thumbnails.Default__.Url;
            if (v.Snippet.Thumbnails.High != null)
                return v.Snippet.Thumbnails.High.Url;

            return defaultValue;
        }

        private static string GetThumbnail(Playlist pl, string defaultValue = null) {
            if (pl.Snippet.Thumbnails.Standard != null)
                return pl.Snippet.Thumbnails.Standard.Url;

            return defaultValue;
        }
    }
}
