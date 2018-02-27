using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NpgsqlTypes;
using DataLakeModels;
using DataLakeModels.Models.YouTube.Analytics;
using DataLakeModels.Models.YouTube.Data;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using Jobs.Transformation.Google;

namespace Jobs.Transformation.YouTube {

    public class VideoSync : GoogleTransformationJob<DataLakeYouTubeDataContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Jobs.Fetcher.YouTube.VideosQuery>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceVideo);
        }

        public override void ExecuteJob(DataLakeYouTubeDataContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListVideos(dlContext, trace)) {
                var storedObject = apDbContext.SourceVideos.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        private const string YouTubeWatchUrl = "https://www.youtube.com/watch?v=";

        private static IEnumerable<EntityUpdateParams<SourceVideo>> ListVideos(DataLakeYouTubeDataContext dbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var video in dbContext.Videos.Where(v => v.ValidityStart <= now && now < v.ValidityEnd)) {
                var log = new RowLog();
                log.AddInput(typeof(Video).Name, MutableEntityExtentions.AutoPK(video.VideoId, video.ValidityStart));
                yield return new EntityUpdateParams<SourceVideo>() {
                           UpdateFunction = delegate(SourceVideo v) {
                               v.Id = video.VideoId;
                               v.Platform = PLATFORM_YOUTUBE;
                               v.Title = video.Title;
                               v.VideoLength = XmlConvert.ToTimeSpan(video.Duration).TotalSeconds;
                               v.ThumbnailUrl = video.ThumbnailUrl;
                               v.SourceUrl = YouTubeWatchUrl + video.VideoId;
                               v.UpdateDate = video.ValidityStart;
                               v.PublishedAt = video.PublishedAt;
                               v.PublishedStatus = video.PrivacyEnum == YouTubePrivacyStatus.Public;
                               return v;
                           },
                           MatchFunction = v => v.Id == video.VideoId && v.Platform == PLATFORM_YOUTUBE,
                           ObjectValidity = new NpgsqlRange<DateTime>(video.ValidityStart, video.ValidityEnd),
                           Trace = log
                };
            }
        }
    }

    public class StatisticsSync : GoogleTransformationJob<DataLakeYouTubeDataContext> {
        /**
         * Produces SourceVideoMetrics out of entries from the YouTube Data API.
         *
         * Videos as recent as 4 days have their metrics drawn from this API,
         * which are stored in DataLake.Statistics.
         */

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Jobs.Fetcher.YouTube.StatisticsQuery>(), IdOf<VideoSync>(), IdOf<VideoMetricSync>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceVideoMetric);
        }

        public override void ExecuteJob(DataLakeYouTubeDataContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListStatistics(dlContext, apDbContext, trace)) {
                var storedObject = apDbContext.SourceVideoMetrics.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        private const string YouTubeWatchUrl = "https://www.youtube.com/watch?v=";

        private static IEnumerable<EntityUpdateParams<SourceVideoMetric>> ListStatistics(DataLakeYouTubeDataContext dlDbContext, ApplicationDbContext apDbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            DateTime daysAgo = DateTime.Now.AddDays(-4);
            var videos = apDbContext.SourceVideos
                             .Where(v =>
                                    v.Platform == PLATFORM_YOUTUBE &&
                                    v.PublishedAt >= daysAgo)
                             .OrderBy(x => x.Id)
                             .Select(x => x.Id)
                             .ToList();

            foreach (var videoId in videos) {
                var statistics = dlDbContext.Statistics
                                     .Where(s =>
                                            s.VideoId == videoId &&
                                            s.CaptureDate > daysAgo &&
                                            s.ValidityStart <= now &&
                                            now < s.ValidityEnd)
                                     .OrderBy(s => s.CaptureDate);
                long olderViewCount = 0;
                long olderLikeCount = 0;
                long olderDislikeCount = 0;
                long olderCommentCount = 0;

                foreach (var stat in statistics) {
                    var log = new RowLog();
                    log.AddInput(typeof(Statistics).Name, MutableEntityExtentions.AutoPK(stat.VideoId, stat.CaptureDate, stat.ValidityStart));
                    var viewCount = stat.ViewCount - (long) olderViewCount;
                    var likeCount = stat.LikeCount - (long) olderLikeCount;
                    var dislikeCount = stat.DislikeCount - (long) olderDislikeCount;
                    var commentCount = stat.CommentCount - (long) olderCommentCount;

                    olderViewCount = (long) stat.ViewCount;
                    olderLikeCount = (long) stat.LikeCount;
                    olderDislikeCount = (long) stat.DislikeCount;
                    olderCommentCount = (long) stat.CommentCount;

                    yield return new EntityUpdateParams<SourceVideoMetric>() {
                               UpdateFunction = delegate(SourceVideoMetric v) {
                                   v.VideoId = stat.VideoId;
                                   v.EventDate = stat.CaptureDate;
                                   v.ViewCount = Math.Max(viewCount, 0);
                                   v.LikeCount = likeCount;
                                   v.DislikeCount = dislikeCount;
                                   v.ReactionCount = likeCount;
                                   v.CommentCount = commentCount;
                                   return v;
                               },
                               MatchFunction = v => v.VideoId == stat.VideoId && v.EventDate == stat.CaptureDate,
                               ObjectValidity = new NpgsqlRange<DateTime>(stat.ValidityStart, stat.ValidityEnd),
                               Trace = log
                    };
                }
            }
        }
    }

    public class PlaylistSync : GoogleTransformationJob<DataLakeYouTubeDataContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Jobs.Fetcher.YouTube.PlaylistsQuery>() };
        }

        protected override Type TargetTable {
            get => typeof(SourcePlaylist);
        }

        public override void ExecuteJob(DataLakeYouTubeDataContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListPlaylists(dlContext, trace)) {
                var storedObject = apDbContext.SourcePlaylists.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        public static IEnumerable<EntityUpdateParams<SourcePlaylist>> ListPlaylists(DataLakeYouTubeDataContext dbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var playlist in dbContext.Playlists.Where(pl => pl.ValidityStart <= now && now < pl.ValidityEnd)) {
                var log = new RowLog();
                log.AddInput(typeof(Playlist).Name, MutableEntityExtentions.AutoPK(playlist.PlaylistId, playlist.ValidityStart));
                yield return new EntityUpdateParams<SourcePlaylist>() {
                           UpdateFunction = delegate(SourcePlaylist v) {
                               v.Id = playlist.PlaylistId;
                               v.Platform = PLATFORM_YOUTUBE;
                               v.Name = playlist.Title;
                               v.Description = playlist.Description;
                               v.UpdateDate = playlist.ValidityStart;
                               v.ThumbnailUrl = playlist.ThumbnailUrl;
                               return v;
                           },
                           MatchFunction = pl => pl.Id == playlist.PlaylistId && pl.Platform == PLATFORM_YOUTUBE,
                           ObjectValidity = new NpgsqlRange<DateTime>(playlist.ValidityStart, playlist.ValidityEnd),
                           Trace = log
                };
            }
        }
    }

    public class PlaylistVideoSync : GoogleTransformationJob<DataLakeYouTubeDataContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<PlaylistSync>(), IdOf<VideoSync>() };
        }

        protected override Type TargetTable {
            get => typeof(SourcePlaylistSourceVideo);
        }

        public override void ExecuteJob(DataLakeYouTubeDataContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListPlaylistVideos(dlContext, apDbContext, trace)) {
                var existing = apDbContext.SourcePlaylistSourceVideos.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, existing, updateParams);
            }
            RemoveDeletedTuples(dlContext, apDbContext, trace);
        }

        public static IEnumerable<EntityUpdateParams<SourcePlaylistSourceVideo>> ListPlaylistVideos(DataLakeYouTubeDataContext dbContext, ApplicationDbContext apDbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var playlist in dbContext.Playlists.Where(pl => pl.ValidityStart <= now && now < pl.ValidityEnd)) {
                var log = new RowLog();
                log.AddInput(typeof(Playlist).Name, MutableEntityExtentions.AutoPK(playlist.PlaylistId, playlist.ValidityStart));

                var filteredVideos = playlist.VideoIds
                                         .Where(x => apDbContext.SourceVideos.Any(y => y.Id == x && y.Platform == PLATFORM_YOUTUBE));

                foreach (var videoId in filteredVideos) {
                    yield return new EntityUpdateParams<SourcePlaylistSourceVideo>() {
                               UpdateFunction = delegate(SourcePlaylistSourceVideo v) {
                                   v.VideoId = videoId;
                                   v.PlaylistId = playlist.PlaylistId;
                                   v.UpdateDate = playlist.ValidityStart;
                                   return v;
                               },
                               MatchFunction = v => v.PlaylistId == playlist.PlaylistId && v.VideoId == videoId,
                               ObjectValidity = new NpgsqlRange<DateTime>(playlist.ValidityStart, playlist.ValidityEnd),
                               Trace = log
                    };
                }
            }
        }

        private static void RemoveDeletedTuples(DataLakeYouTubeDataContext dbContext, ApplicationDbContext apDbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var playlist in dbContext.Playlists.Where(pl => pl.ValidityStart <= now && now < pl.ValidityEnd)) {

                var deletedTuples = apDbContext.SourcePlaylistSourceVideos
                                        .Where(x => x.PlaylistId == playlist.PlaylistId && !playlist.VideoIds.Any(videoId => videoId == x.VideoId));

                foreach (var tuple in deletedTuples) {
                    var log = new RowLog() {
                        Id = tuple.PrimaryKey,
                        OldVersion = tuple.UpdateDate,
                    };
                    log.AddInput(typeof(Playlist).Name, MutableEntityExtentions.AutoPK(playlist.PlaylistId, playlist.ValidityStart));
                    trace.Add(log);
                    apDbContext.SourcePlaylistSourceVideos.Remove(tuple);
                }
            }
        }
    }

    public class VideoMetricSync :
        BatchedGoogleTransformationJob<DataLakeYouTubeAnalyticsContext,
                                       SourceVideo> {
        /**
         * Produces SourceVideoMetrics out of entries from the YouTube
         * Analytics API.
         *
         * Videos older than 4 days have their metrics drawn from this API,
         * which are stored in DataLakeYouTubeAnalyics.VideoDailyMetric.
         *
         * Note: As soon as a video becomes older than 4 days, its metrics,
         * which were previously provided by YouTube Data API, will be
         * overwriten by the values provided by YouTube Analytics API.
         */
        public override List<string> Dependencies() {
            return new List<string>(){
                       IdOf<VideoSync>(),
                       IdOf<Jobs.Fetcher.YouTube.DailyVideoMetricsQuery>()
            };
        }

        protected override Type TargetTable {
            get => typeof(SourcePlaylist);
        }

        public override SourceVideo ExecuteJob(DataLakeYouTubeAnalyticsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace, SourceVideo previous) {
            DateTime daysAgo = DateTime.Now.AddDays(-4);

            IEnumerable<SourceVideo> videos;
            if (previous != null)
                videos = apDbContext.SourceVideos
                             .Where(v =>
                                    v.Platform == PLATFORM_YOUTUBE &&
                                    v.Id.CompareTo(previous.Id) > 0 &&
                                    v.PublishedAt < daysAgo)
                             .OrderBy(x => x.Id)
                             .Take(BatchSize)
                             .ToList();
            else
                videos = apDbContext.SourceVideos
                             .Where(v => v.Platform == PLATFORM_YOUTUBE && v.PublishedAt < daysAgo)
                             .OrderBy(x => x.Id)
                             .Take(BatchSize)
                             .ToList();

            foreach (var video in videos) {
                foreach (var updateParams in ListVideoMetrics(dlContext, apDbContext, trace, video)) {
                    var existing = apDbContext.SourceVideoMetrics.Where(updateParams.MatchFunction);
                    SaveMutableEntity(apDbContext, trace, existing, updateParams);
                }
            }
            return videos.LastOrDefault();
        }

        private static long SecondsToMilliseconds(long val) {
            return val * 1000;
        }

        public IEnumerable<EntityUpdateParams<SourceVideoMetric>> ListVideoMetrics(DataLakeYouTubeAnalyticsContext dbContext, ApplicationDbContext apContext, JobTrace trace, SourceVideo video) {
            var now = DateTime.UtcNow;
            DateTime daysAgo = DateTime.Now.AddDays(-4);

            var latest = apContext.SourceVideoMetrics
                             .Where(x => x.VideoId == video.Id)
                             .Select(v => v.UpdateDate)
                             .DefaultIfEmpty(DateTime.MinValue)
                             .Max();
            Logger.Debug("Processing metrics for video {VideoId}, latest date is: {LatestDate}", video.Id, latest);

            var dailyMetrics = dbContext.VideoDailyMetrics
                                   .Where(a =>
                                          a.ValidityStart > latest &&
                                          a.ValidityStart <= now &&
                                          now < a.ValidityEnd &&
                                          a.VideoId == video.Id &&
                                          a.Date < daysAgo)
                                   .ToList();
            foreach (var m in dailyMetrics) {
                var log = new RowLog();
                log.AddInput(typeof(VideoDailyMetric).Name, MutableEntityExtentions.AutoPK(m.VideoId, m.Date, m.ValidityStart));
                yield return new EntityUpdateParams<SourceVideoMetric>() {
                           UpdateFunction = delegate(SourceVideoMetric v) {
                               v.VideoId = m.VideoId;
                               v.EventDate = m.Date;
                               v.ViewCount = m.Views;
                               v.ViewTime = SecondsToMilliseconds(m.AverageViewDuration * m.Views);
                               v.CommentCount = m.Comments;
                               v.LikeCount = m.Likes;
                               v.ReactionCount = m.Likes;
                               v.ShareCount = m.Shares;
                               v.DislikeCount = m.Dislikes;
                               v.UpdateDate = m.ValidityStart;
                               return v;
                           },
                           MatchFunction = v => v.VideoId == m.VideoId && v.EventDate == m.Date,
                           ObjectValidity = new NpgsqlRange<DateTime>(m.ValidityStart, m.ValidityEnd),
                           Trace = log
                };
            }
        }
    }

    public class VideoDemographicMetricSync : BatchedGoogleTransformationJob<DataLakeYouTubeAnalyticsContext, SourceVideo> {
        public override List<string> Dependencies() {
            return new List<string>() {
                       IdOf<VideoSync>(),
                       IdOf<VideoMetricSync>(),
                       IdOf<Jobs.Fetcher.YouTube.ViewerPercentageMetricsQuery>()
            };
        }

        protected override Type TargetTable {
            get => typeof(SourceVideoDemographicMetric);
        }

        public override SourceVideo ExecuteJob(DataLakeYouTubeAnalyticsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace, SourceVideo previous) {
            DateTime daysAgo = DateTime.Now.AddDays(-4);
            IEnumerable<SourceVideo> videos;
            if (previous != null)
                videos = apDbContext.SourceVideos
                             .Where(v =>
                                    v.Platform == PLATFORM_YOUTUBE &&
                                    v.Id.CompareTo(previous.Id) > 0 &&
                                    v.PublishedAt < daysAgo)
                             .OrderBy(x => x.Id)
                             .Take(BatchSize)
                             .ToList();
            else
                videos = apDbContext.SourceVideos
                             .Where(v =>
                                    v.Platform == PLATFORM_YOUTUBE &&
                                    v.PublishedAt < daysAgo)
                             .OrderBy(x => x.Id)
                             .Take(BatchSize)
                             .ToList();

            foreach (var video in videos) {
                var latest = apDbContext.SourceVideoDemographicMetrics
                                 .Where(x => x.VideoId == video.Id)
                                 .Select(x => x.UpdateDate)
                                 .DefaultIfEmpty(DateTime.MinValue)
                                 .Max();
                Logger.Debug("Processing demographic metrics for video {VideoId}, latest date is: {LatestDate}", video.Id, latest);

                var result = new List<SourceVideoDemographicMetric>();
                foreach (var updateParams in ListVideoViewerPercentageMetrics(dlContext, apDbContext, trace, video, latest)) {
                    var existing = apDbContext.SourceVideoDemographicMetrics.Where(updateParams.MatchFunction);
                    result.Add(SaveMutableEntity(apDbContext, trace, existing, updateParams));
                }
                RemoveDeletedTuples(dlContext, apDbContext, trace, video, latest, result.Where(x => x != null));
            }
            return videos.LastOrDefault();
        }

        private static IEnumerable<EntityUpdateParams<SourceVideoDemographicMetric>> ListVideoViewerPercentageMetrics(DataLakeYouTubeAnalyticsContext dbContext, ApplicationDbContext apContext, JobTrace trace, SourceVideo video, DateTime latest) {
            var now = DateTime.UtcNow;
            var videoDateGroups = dbContext.ViewerPercentageMetric
                                      .Where(a =>
                                             a.ValidityStart > latest &&
                                             a.ValidityStart <= now &&
                                             now < a.ValidityEnd &&
                                             a.VideoId == video.Id)
                                      .GroupBy(x =>
                                               new { VideoId = x.VideoId, StartDate = x.StartDate });
            foreach (var group in videoDateGroups) {
                var totalViewCount = apContext.SourceVideoMetrics
                                         .Where(x =>
                                                x.VideoId == group.Key.VideoId &&
                                                x.EventDate <= group.Key.StartDate)
                                         .Select(x => x.ViewCount)
                                         .Sum();
                foreach (var m in group) {
                    var log = new RowLog();
                    var ageGroup = ParseAgeGroup(m.AgeGroup);
                    var gender = ParseGender(m.Gender);
                    log.AddInput(typeof(ViewerPercentage).Name, MutableEntityExtentions.AutoPK(m.VideoId, m.StartDate, m.ValidityStart));
                    yield return new EntityUpdateParams<SourceVideoDemographicMetric>() {
                               UpdateFunction = delegate(SourceVideoDemographicMetric v) {
                                   v.VideoId = m.VideoId;
                                   v.StartDate = m.StartDate;
                                   v.EndDate = m.EndDate;
                                   v.AgeGroup = ageGroup;
                                   v.Gender = gender;
                                   v.ViewerPercentage = m.Value;
                                   v.TotalViewCount = m.Value * totalViewCount / 100;
                                   v.UpdateDate = m.ValidityStart;
                                   return v;
                               },
                               MatchFunction = v => v.VideoId == m.VideoId && v.AgeGroup == ageGroup && v.Gender == gender && v.StartDate == m.StartDate,
                               ObjectValidity = new NpgsqlRange<DateTime>(m.ValidityStart, m.ValidityEnd),
                               Trace = log
                    };
                }
            }
        }

        /**
            Deletes all rows that exist in the application table but not in the data lake table as a valid entry.
         */
        private static void RemoveDeletedTuples(DataLakeYouTubeAnalyticsContext dbContext, ApplicationDbContext apDbContext, JobTrace trace, SourceVideo video, DateTime max, IEnumerable<SourceVideoDemographicMetric> metrics) {
            var now = DateTime.UtcNow;

            if (metrics.Any()) {
                var minDate = metrics.Select(v => v.StartDate).Min();
                var rows = from m in apDbContext.SourceVideoDemographicMetrics
                           join v in apDbContext.SourceVideos on m.VideoId equals v.Id
                           where v.Platform == PLATFORM_YOUTUBE && v.Id == video.Id && m.StartDate >= minDate && m.UpdateDate <= max
                           select m;

                foreach (var row in rows) {
                    var hasMetric = dbContext.ViewerPercentageMetric
                                        .Where(x =>
                                               x.ValidityStart <= now && now < x.ValidityEnd &&
                                               x.VideoId == row.VideoId &&
                                               x.StartDate == row.StartDate &&
                                               x.EndDate == row.EndDate)
                                        .Where(x =>
                                               ParseAgeGroup(x.AgeGroup) == row.AgeGroup &&
                                               ParseGender(x.Gender) == row.Gender)
                                        .Any();

                    if (!hasMetric) {

                        var log = new RowLog() {
                            Id = MutableEntityExtentions.AutoPK(row.VideoId, row.AgeGroup, row.Gender, row.StartDate, row.EndDate),
                            OldVersion = row.StartDate,
                        };
                        log.AddInput(typeof(ViewerPercentage).Name, MutableEntityExtentions.AutoPK(now));
                        trace.Add(log);

                        apDbContext.SourceVideoDemographicMetrics.Remove(row);
                    }
                }
            }
        }

        private static string ParseGender(string apiGenderValue) {
            return apiGenderValue
                       .Replace("female", "F")
                       .Replace("male", "M");
        }

        private static string ParseAgeGroup(string apiAgeGroupValue) {
            return apiAgeGroupValue
                       .Replace("age", "")
                       .Replace("65-", "65+");
        }
    }
}
