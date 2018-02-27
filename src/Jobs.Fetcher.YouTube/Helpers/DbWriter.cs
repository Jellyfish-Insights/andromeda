using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Models.YouTube.Data;
using DataLakeModels.Models.YouTube.Analytics;
using Serilog.Core;

namespace Jobs.Fetcher.YouTube.Helpers {

    public static class DbWriter {

        private static Modified compareOldAndNew<T>(T storedObj, T newObj) where T : IEquatable<T> {
            if (storedObj == null)
                return Modified.New;

            if (!storedObj.Equals(newObj)) {
                return Modified.Updated;
            }
            return Modified.Equal;
        }

        public static Modified compareOldAndNew<T>(IEnumerable<T> storedObjs, IEnumerable<T> newObjs) where T : IEquatable<T> {
            if (storedObjs == null)
                return Modified.New;

            if (!storedObjs.ToHashSet().SetEquals(newObjs)) {
                return Modified.Updated;
            }
            return Modified.Equal;
        }

        public static void Write(IEnumerable<Video> videos, string channelId, Logger logger) {
            using (var dlContext = new DataLakeYouTubeDataContext()) {
                var now = DateTime.UtcNow;
                foreach (var newObj in videos) {
                    var storedObj = dlContext.Videos.SingleOrDefault(v => v.VideoId == newObj.VideoId && v.ValidityStart <= now && now < v.ValidityEnd);

                    newObj.ValidityEnd = DateTime.MaxValue;
                    newObj.ValidityStart = DateTime.UtcNow;
                    newObj.ChannelId = channelId;

                    var modified = compareOldAndNew(storedObj, newObj);
                    switch (modified) {
                        case Modified.New:
                            logger.Debug("Found new video: {VideoId}", newObj.VideoId);
                            dlContext.Add(newObj);
                            break;
                        case Modified.Updated:
                            logger.Debug("Found update to: {VideoId}", newObj.VideoId);
                            storedObj.ValidityEnd = newObj.ValidityStart;
                            dlContext.Add(newObj);
                            break;
                        default:
                            break;
                    }
                }
                dlContext.SaveChanges();
            }
        }

        public static void Write(IEnumerable<Statistics> videos, Logger logger) {
            using (var dlContext = new DataLakeYouTubeDataContext()) {
                var now = DateTime.UtcNow;
                foreach (var newObj in videos) {
                    var storedObj = dlContext.Statistics.SingleOrDefault(v => v.VideoId == newObj.VideoId && v.CaptureDate == newObj.CaptureDate && v.ValidityStart <= now && now < v.ValidityEnd);

                    newObj.ValidityEnd = DateTime.MaxValue;
                    newObj.ValidityStart = DateTime.UtcNow;

                    var modified = compareOldAndNew(storedObj, newObj);
                    switch (modified) {
                        case Modified.New:
                            logger.Debug("Found new statistics: {VideoId} {CaptureDate}", newObj.VideoId, newObj.CaptureDate);
                            dlContext.Add(newObj);
                            break;
                        case Modified.Updated:
                            logger.Debug("Found update to: {VideoId} {CaptureDate}", newObj.VideoId, newObj.CaptureDate);
                            storedObj.ValidityEnd = newObj.ValidityStart;
                            dlContext.Add(newObj);
                            break;
                        default:
                            break;
                    }
                }
                dlContext.SaveChanges();
            }
        }

        public static void Write(IEnumerable<Playlist> playlists, Logger logger) {
            using (var dlContext = new DataLakeYouTubeDataContext()) {
                var now = DateTime.UtcNow;
                foreach (var newObj in playlists) {
                    var storedObj = dlContext.Playlists.SingleOrDefault(v => v.PlaylistId == newObj.PlaylistId && v.ValidityStart <= now && now < v.ValidityEnd);

                    newObj.ValidityEnd = DateTime.MaxValue;
                    newObj.ValidityStart = now;

                    var modified = compareOldAndNew(storedObj, newObj);
                    switch (modified) {
                        case Modified.New:
                            logger.Debug("Found new playlist: {PlaylistId}", newObj.PlaylistId);
                            dlContext.Add(newObj);
                            break;
                        case Modified.Updated:
                            logger.Debug("Found update to: {PlaylistId}", newObj.PlaylistId);
                            storedObj.ValidityEnd = newObj.ValidityStart;
                            dlContext.Add(newObj);
                            break;
                        default:
                            break;
                    }
                }
                dlContext.SaveChanges();
            }
        }

        public static void Write(IEnumerable<VideoDailyMetric> dailyMetrics) {
            using (var dlContext = new DataLakeYouTubeAnalyticsContext()) {
                var now = DateTime.UtcNow;
                foreach (var newObj in dailyMetrics) {
                    var storedObj = dlContext.VideoDailyMetrics.SingleOrDefault(v => v.VideoId == newObj.VideoId && v.Date == newObj.Date && v.ValidityStart <= now && now < v.ValidityEnd);

                    newObj.ValidityEnd = DateTime.MaxValue;
                    newObj.ValidityStart = now;

                    var modified = compareOldAndNew(storedObj, newObj);
                    switch (modified) {
                        case Modified.New:
                            dlContext.Add(newObj);
                            break;
                        case Modified.Updated:
                            storedObj.ValidityEnd = newObj.ValidityStart;
                            dlContext.Add(newObj);
                            break;
                        default:
                            break;
                    }
                }
                dlContext.SaveChanges();
            }
        }

        public static void Write(string videoId, DateTime date, IEnumerable<ViewerPercentage> viewerPercentages, DateTime now) {
            using (var dlContext = new DataLakeYouTubeAnalyticsContext()) {
                List<ViewerPercentage> storedObjs = null;
                try {
                    storedObjs = dlContext.ViewerPercentageMetric
                                     .Where(x => x.VideoId == videoId && x.StartDate <= date && x.ValidityStart <= now && now < x.ValidityEnd)
                                     .GroupBy(x => x.StartDate)
                                     .OrderByDescending(x => x.Key)
                                     .FirstOrDefault().ToList();
                } catch (NullReferenceException) {
                    // This just means that there are no objects in the database yet.
                }

                var modified = compareOldAndNew(storedObjs, viewerPercentages);

                // Close date range of previous values
                if (modified == Modified.Updated) {
                    storedObjs.ForEach(x => { x.EndDate = date; });
                }

                // Invalidate all entries for 'future' dates
                if (modified == Modified.New || modified == Modified.Updated) {
                    dlContext.ViewerPercentageMetric
                        .Where(x => x.VideoId == videoId && x.StartDate >= date && x.ValidityStart <= now && now < x.ValidityEnd)
                        .ToList()
                        .ForEach(x => { x.ValidityEnd = now; });
                }

                foreach (var newObj in viewerPercentages) {
                    switch (modified) {
                        case Modified.New:
                        case Modified.Updated:
                            newObj.VideoId = videoId;
                            newObj.StartDate = date;
                            newObj.EndDate = DateTime.MaxValue;
                            newObj.ValidityEnd = DateTime.MaxValue;
                            newObj.ValidityStart = now;
                            dlContext.Add(newObj);
                            break;
                        default:
                            break;
                    }
                }
                dlContext.SaveChanges();
            }
        }
    }
}
