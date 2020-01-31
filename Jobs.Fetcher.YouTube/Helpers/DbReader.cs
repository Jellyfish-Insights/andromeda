using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels;
using DataLakeModels.Models.YouTube.Data;

namespace Jobs.Fetcher.YouTube.Helpers {

    public static class DbReader {

        public static List<Video> GetVideos() {
            var now = DateTime.UtcNow;
            using (var db = new DataLakeYouTubeDataContext()) {
                return db.Videos.Where(x => x.ValidityStart <= now && now < x.ValidityEnd).OrderByDescending(x => x.PublishedAt).ToList();
            }
        }

        public static List<MetricDelta<Video, long>> CompareVideoLifetimeDailyTotal() {
            var now = DateTime.UtcNow;
            using (var analytics = new DataLakeYouTubeAnalyticsContext())
                using (var db = new DataLakeYouTubeDataContext()) {
                    var dayTotal =
                        analytics.VideoDailyMetrics.Where(x => x.ValidityStart <= now && now < x.ValidityEnd)
                            .GroupBy(x => x.VideoId, y => y, (k, v) => new { VideoId = k, Views = v.Sum(y => y.Views), StartDate = v.Min(y => y.Date), EndDate = v.Max(y => y.Date) }).ToList();
                    var stats =
                        from s in db.Statistics.Where(x => x.CaptureDate == now.Date && x.ValidityStart <= now && now < x.ValidityEnd)
                        join dt in  dayTotal on s.VideoId equals dt.VideoId
                        join v in db.Videos.Where(x => x.ValidityStart <= now && now < x.ValidityEnd) on s.VideoId equals v.VideoId
                        select new MetricDelta<Video, long>(){
                        Id = v,
                        Lifetime = s.ViewCount,
                        Total = dt.Views,
                        DailyStart = dt.StartDate,
                        DailyEnd = dt.EndDate,
                        LifetimeDate = s.CaptureDate
                    };
                    return stats.ToList();
                }
        }
    }

    public class MetricDelta<T, D> {
        public T Id  { get; set; }
        public D Lifetime { get; set; }
        public D Total { get; set; }
        public DateTime DailyStart { get; set; }
        public DateTime DailyEnd { get; set; }
        public DateTime LifetimeDate { get; set; }
    }
}
