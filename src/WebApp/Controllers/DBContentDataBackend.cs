using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ApplicationModels;
using Microsoft.EntityFrameworkCore;
using ApplicationModels.Models;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {
    public class DBContentDataBackend : AbstractDBDataBackend, IContentDataBackend {

        private static double MillisecondsToSeconds(double val) {
            return val / 1000.0;
        }

        private static MetricType[] SourceVideoMetrics =
            new MetricType[] { MetricType.Views, MetricType.Likes, MetricType.Dislikes, MetricType.Shares, MetricType.Comments, MetricType.ViewTime, MetricType.Reactions };

        private static double ProjectMetric(SourceVideoMetric metricRow, MetricType metricName) {
            switch (metricName)
            {
                case MetricType.Views:
                    return (double) ((metricRow?.ViewCount) ?? 0);
                case MetricType.Likes:
                    return (double) ((metricRow?.LikeCount) ?? 0);
                case MetricType.Reactions:
                    return (double) ((metricRow?.ReactionCount) ?? 0);
                case MetricType.Dislikes:
                    return (double) ((metricRow?.DislikeCount) ?? 0);
                case MetricType.Shares:
                    return (double) ((metricRow?.ShareCount) ?? 0);
                case MetricType.Comments:
                    return (double) ((metricRow?.CommentCount) ?? 0);
                case MetricType.ViewTime:
                    return (double) MillisecondsToSeconds((metricRow?.ViewTime) ?? 0);
                default:
                    throw new ArgumentException($"Invalid metric name: {metricName}");
            }
        }

        TimeSeries ReduceSample(IEnumerable<DateTime> dateInRange, IEnumerable<Sample<string>> videoMetrics, IEnumerable<Sample<string>> totalSample = null) {

            var timeSeriesGroup = from samples in videoMetrics
                                  group samples by samples.Group into personaMetric
                                  select new TimeSeriesDataGroup(){
                GroupName = personaMetric.Key,
                Values = (from date in dateInRange
                          join m in personaMetric on date equals m.Date into dateMetrics
                          from m in dateMetrics.DefaultIfEmpty()
                          select m == null ? 0 : m.Value
                          ).ToArray()
            };
            double[] totalSeries = null;
            if (totalSample != null) {
                totalSeries = (from date in dateInRange
                               join m in totalSample on date equals m.Date
                               select m.Value).ToArray();
            }
            return ReduceTimeSeries(dateInRange, timeSeriesGroup, totalSeries);
        }

        private void TimeSeriesGroupedByMetaTag(
            MetricType[] metrics,
            DateTime start,
            DateTime end,
            string type,
            IEnumerable<DateTime> dateInRange,
            List<int> filteredApplicationVideos,
            List<string> filteredSourceVideos,
            IEnumerable<MetricType> sourceVideoMetrics,
            ApplicationDbContext context,
            ConcurrentDictionary<MetricType, TimeSeries> store) {
            var metaTagTypeId = context.ApplicationMetaTagsTypes.Single(x => x.Type.ToLower() == type.ToLower()).Id;
            if (metrics.Contains(MetricType.Impressions)) {
                var videoMetricsQ = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                    join videoMetaTag in context.ApplicationVideoApplicationMetaTags.Where(videoMetaTag => videoMetaTag.Tag.TypeId == metaTagTypeId) on v.Id equals videoMetaTag.VideoId into Tags
                                    from t in Tags.DefaultIfEmpty()
                                    join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                    join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                    join delta in context.SourceDeltaEncodedVideoMetrics.AsNoTracking().Where(m => m.StartDate >= start && m.EndDate <= end) on sv.Id equals delta.VideoId
                                    group delta by new { Date = delta.StartDate.Date, Tag = t.Tag.Tag ?? "None" } into nv
                    select new Sample<string> {
                    Date = nv.Key.Date,
                    Value = nv.Sum(x => x.ImpressionsCount ?? 0),
                    Group = nv.Key.Tag
                };
                store[MetricType.Impressions] = ReduceSample(dateInRange, videoMetricsQ.ToList());
            }

            if (sourceVideoMetrics.Any()) {
                var videoMetricsQ = (from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                     join videoMetaTag in context.ApplicationVideoApplicationMetaTags.Where(videoMetaTag => videoMetaTag.Tag.TypeId == metaTagTypeId) on v.Id equals videoMetaTag.VideoId into Tags
                                     from t in Tags.DefaultIfEmpty()
                                     join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                     join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                     join videoMetric in context.SourceVideoMetrics.AsNoTracking().Where(m => m.EventDate >= start && m.EventDate <= end) on sv.Id equals videoMetric.VideoId
                                     select new { Metric = videoMetric, Group = t.Tag.Tag ?? "None" }).ToList();

                var reducedSample =
                    (from personaMetricRow in videoMetricsQ
                     group personaMetricRow by new { Group = personaMetricRow.Group, Date = personaMetricRow.Metric.EventDate } into personaMetric
                     select sourceVideoMetrics.ToDictionary(x => x, v => new Sample<string> {
                    Date = personaMetric.Key.Date,
                    Value = personaMetric.Sum(x => ProjectMetric(x.Metric, v)),
                    Group = personaMetric.Key.Group
                })).ToList();

                sourceVideoMetrics
                    .AsParallel()
                    .WithDegreeOfParallelism(2)
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .ForAll(v => {
                    store[v] = ReduceSample(dateInRange, reducedSample.Select(x => x[v]));
                });
            }
        }

        private void TimeSeriesGroupedByGenericTag(
            MetricType[] metrics,
            DateTime start,
            DateTime end,
            IEnumerable<DateTime> dateInRange,
            List<int> filteredApplicationVideos,
            List<string> filteredSourceVideos,
            IEnumerable<MetricType> sourceVideoMetrics,
            ApplicationDbContext context,
            ConcurrentDictionary<MetricType, TimeSeries> store) {
            if (metrics.Contains(MetricType.Impressions)) {
                var videoMetricsQuery = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                        join genericTag in context.ApplicationVideoApplicationGenericTags on v.Id equals genericTag.VideoId into Gtag
                                        from g in Gtag.DefaultIfEmpty()
                                        join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                        join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                        join delta in context.SourceDeltaEncodedVideoMetrics.AsNoTracking().Where(m => m.StartDate >= start && m.EndDate <= end) on sv.Id equals delta.VideoId
                                        group delta by new { Date = delta.StartDate.Date, Tag = g.Tag.Tag ?? "None" } into nv
                    select new Sample<string> {
                    Date = nv.Key.Date,
                    Value = nv.Sum(x => x.ImpressionsCount ?? 0),
                    Group = nv.Key.Tag
                };
                // Computes total per day because one video can have multiple generic tags
                var videoMetricsTotalQuery = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                             join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                             join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                             join delta in context.SourceDeltaEncodedVideoMetrics.AsNoTracking().Where(m => m.StartDate >= start && m.EndDate <= end) on sv.Id equals delta.VideoId
                                             group delta by delta.StartDate.Date into nv
                                             select new Sample<string> {
                    Date = nv.Key,
                    Value = nv.Sum(x => x.ImpressionsCount ?? 0),
                    Group = Constants.GrandTotalName
                };
                store[MetricType.Impressions] = ReduceSample(dateInRange, videoMetricsQuery.ToList(), videoMetricsTotalQuery.ToList());
            }

            if (sourceVideoMetrics.Any()) {
                var videoMetricsQuery = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                        join genericTag in context.ApplicationVideoApplicationGenericTags on v.Id equals genericTag.VideoId into Gtag
                                        from g in Gtag.DefaultIfEmpty()
                                        join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                        join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                        join videoMetric in context.SourceVideoMetrics.AsNoTracking().Where(m => m.EventDate >= start && m.EventDate <= end) on sv.Id equals videoMetric.VideoId
                                        select new { Metric = videoMetric, Group = g.Tag.Tag ?? "None" };

                var reducedSampleQuery =
                    from personaMetricRow in videoMetricsQuery.ToList()
                    group personaMetricRow by new { Group = personaMetricRow.Group, Date = personaMetricRow.Metric.EventDate } into personaMetric
                select sourceVideoMetrics.ToDictionary(x => x, v => new Sample<string> {
                    Date = personaMetric.Key.Date,
                    Value = personaMetric.Sum(x => ProjectMetric(x.Metric, v)),
                    Group = personaMetric.Key.Group
                });

                // Computes total per day because one video can have multiple generic tags
                var videoMetricsTotalQuery = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                             join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                             join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                             join videoMetric in context.SourceVideoMetrics.AsNoTracking().Where(m => m.EventDate >= start && m.EventDate <= end) on sv.Id equals videoMetric.VideoId
                                             group videoMetric by videoMetric.EventDate into totalMetric
                                             select
                                             sourceVideoMetrics.ToDictionary(x => x, x => new Sample<string> {
                    Date = totalMetric.Key,
                    Group = Constants.GrandTotalName,
                    Value = totalMetric.Sum(v => ProjectMetric(v, x))
                });
                // Force queries to avoid duplicated work
                var videoMetricsTotal = videoMetricsTotalQuery.ToList();
                var reducedSample = reducedSampleQuery.ToList();
                sourceVideoMetrics
                    .AsParallel()
                    .WithDegreeOfParallelism(2)
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .ForAll(v => {
                    store[v] = ReduceSample(
                        dateInRange,
                        reducedSample.Select(x => x[v]),
                        videoMetricsTotal.Select(x => x[v]));
                });
            }
        }

        public override Dictionary<MetricType, TimeSeries> ComputePrimitiveTimeSeries(MetricType[] metrics, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive) {

            using (var context = new ApplicationDbContext()) {
                var dateInRange = DateUtilities.GetDatesBetween(start, end);
                //To prevent null pointer exception when selected metaTagType is "generic", set metaTagTypeId 0
                var filteredApplicationVideos = ApplyFilters(context, context.ApplicationVideos, filters, archive).Select(x => x.Id).ToList();
                var filteredSourceVideos = ApplySourceFilters(context, filters).Select(x => x.Id).ToList();
                var store = new ConcurrentDictionary<MetricType, TimeSeries>();
                var sourceVideoMetrics = metrics.Select(metric => Array.Find(SourceVideoMetrics, x => x == metric)).Distinct();

                switch (type.ToLower()) {
                    case "generic":
                        TimeSeriesGroupedByGenericTag(metrics, start, end, dateInRange, filteredApplicationVideos, filteredSourceVideos, sourceVideoMetrics, context, store);
                        break;
                    default:
                        TimeSeriesGroupedByMetaTag(metrics, start, end, type, dateInRange, filteredApplicationVideos, filteredSourceVideos, sourceVideoMetrics, context, store);
                        break;
                }
                return store.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<VideoMetric> MetricList(DateTime start, DateTime end, Tag[] filters, List<MetricInfo> metrics, ArchiveMode archive) {
            using (var context = new ApplicationDbContext())
            {
                var filteredApplicationVideos = ApplyFilters(context, context.ApplicationVideos, filters, archive).Select(x => x.Id).ToList();
                var filteredSourceVideos = ApplySourceFilters(context, filters).Select(x => x.Id).ToList();

                var sourceDemo =
                    from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                    join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                    join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                    join delta in context.SourceVideoDemographicMetrics.AsNoTracking().Where(m => m.StartDate >= start && m.StartDate <= end) on sv.Id equals delta.VideoId
                    group delta by v.Id into x
                    select new
                {
                    Id = x.Key,
                    TotalMetrics = new List<Metric>() {

                        new Metric() {
                            Type = "Demographics View Time",
                            Value = MillisecondsToSeconds(x.Sum(y => y.TotalViewTime) ?? 0.0)
                        }, new Metric() {
                            Type = "Demographics View Count",
                            Value = x.Sum(y => y.TotalViewCount) ?? 0.0
                        }
                    }
                };
                var sourceDelta = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                  join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                  join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                  join delta in context.SourceDeltaEncodedVideoMetrics.AsNoTracking().Where(m => m.StartDate >= start && m.StartDate <= end) on sv.Id equals delta.VideoId
                                  group delta by v.Id into x
                                  select new
                {
                    Id = x.Key,
                    TotalMetrics = new List<Metric>() {
                        new Metric() {
                            Type = "Impressions",
                            Value = x.Sum(v => v.ImpressionsCount) ?? 0.0
                        }
                    }
                };
                var sourceDaily = from v in context.ApplicationVideos.Where(v => filteredApplicationVideos.Contains(v.Id))
                                  join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                  join sv in context.SourceVideos.Where(v => filteredSourceVideos.Contains(v.Id)) on avsv.SourceVideoId equals sv.Id
                                  join daily in context.SourceVideoMetrics.AsNoTracking().Where(m => m.EventDate >= start && m.EventDate <= end) on sv.Id equals daily.VideoId
                                  group daily by v.Id into x
                                  let views = x.Sum(y => y.ViewCount)
                                              let viewTime = x.Sum(y => y.ViewTime)
                                                             select new
                {
                    Id = x.Key,
                    TotalMetrics =
// TODO : improve prunning of metrics just request the metrics that were selected
//.Where(m => metrics.Contains(m.Type)).ToList()
                        new List<Metric>() {
                        new Metric() {
                            Type = "Views",
                            Value = views ?? 0
                        }, new Metric() {
                            Type = "Likes",
                            Value = x.Sum(y => y.LikeCount) ?? 0
                        }, new Metric() {
                            Type = "Reactions",
                            Value = x.Sum(y => y.ReactionCount) ?? 0
                        }, new Metric() {
                            Type = "Shares",
                            Value = x.Sum(y => y.ShareCount) ?? 0
                        }, new Metric() {
                            Type = "Comments",
                            Value = x.Sum(y => y.CommentCount) ?? 0
                        }, new Metric() {
                            Type = "Dislikes",
                            Value = x.Sum(y => y.DislikeCount) ?? 0
                        }, new Metric() {
                            Type = "View Time",
                            Value = MillisecondsToSeconds(viewTime ?? 0.0)
                        }, new Metric() {
                            Type = "Average View Time",
                            Value = MillisecondsToSeconds(SafelyDivide(viewTime ?? 0.0, views ?? 0.0))
                        }
                    }
                };

                var source = sourceDelta.Union(sourceDaily);
                var hasDemo = metrics.Any(x => x.TypeId == MetricType.DemographicsViewCount || x.TypeId == MetricType.DemographicsViewTime);
                if (hasDemo) {
                    source = source.Union(sourceDemo);
                }
                var query = from u in source.ToList()
                            group u by u.Id into v
                            select new VideoMetric() { Id = v.Key.ToString(), TotalMetrics = v.Select(x => x.TotalMetrics.AsEnumerable()).Aggregate((x, y) => x.Union(y)).ToList() };

                return query.ToList();
            }
        }

        // returns map of video id -> date -> metric name -> double
        public Dictionary<int, Dictionary<string, Dictionary<string, double>>> VideoMetricByDay(IEnumerable<int> apVideoIds, DateTime start, DateTime end) {
            using (var context = new ApplicationDbContext())
            {
                var datesInRange = DateUtilities.GetDatesBetween(start, end);
                var filteredIds = apVideoIds.ToList();

                var sourceDaily = from v in context.ApplicationVideos.Where(video => filteredIds.Contains(video.Id))
                                  join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                                  join sv in context.SourceVideos on avsv.SourceVideoId equals sv.Id
                                  join daily in context.SourceVideoMetrics.AsNoTracking().Where(m => m.EventDate >= start && m.EventDate <= end) on sv.Id equals daily.VideoId
                                  group daily by new { v.Id, daily.EventDate } into x
                    select new
                {
                    Id = x.Key.Id,
                    EventDate = x.Key.EventDate.Date,
                    Views = x.Sum(m => m.ViewCount ?? 0),
                    Reactions = x.Sum(m => m.ReactionCount ?? 0),
                };

                return sourceDaily
                           .ToList()
                           .GroupBy(x => x.Id)
                           .ToDictionary(
                    x => x.Key,
                    x => datesInRange.Select(
                        d => new {
                    Date = DateUtilities.ToRestApiDateFormat(d),
                    Metric =
                        new List<Metric>(){
                        new Metric() {
                            Type = "Views",
                            Value = x.SingleOrDefault(dm => d == dm.EventDate)?.Views ?? 0
                        }, new Metric() {
                            Type = "Reactions",
                            Value = x.SingleOrDefault(dm => d == dm.EventDate)?.Reactions ?? 0
                        }
                    }
                }).ToDictionary(
                        z => z.Date,
                        z => z.Metric.ToDictionary(j => j.Type.ToLower(), j => j.Value)
                        )
                    );
            }
        }

        public IEnumerable<(string Group, string Age, string Gender, double Value)> GetUnstructuredDemographicData(string metric, string metaTagType, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive) {
            var metricType = Constants.ContentMetrics.Where(x => x.Type == metric).First().TypeId;

            if (!IsDemographicMetricType(metricType)) {
                throw new ArgumentException($"Unexpected non demographic metric type {metricType}");
            }

            Func<SourceVideoDemographicMetric, double> projectMetric = x => (metricType == MetricType.DemographicsViewCount) ? (x.TotalViewCount ?? 0) : MillisecondsToSeconds(x.TotalViewTime ?? 0);

            using (var context = new ApplicationDbContext())
            {
                //To prevent null pointer exception when selected metaTagType is "generic", set metaTagTypeId 0
                var metaTagTypeId = metaTagType == Constants.GenericTag.ToLower() ? 0 : context.ApplicationMetaTagsTypes.Single(x => x.Type.ToLower() == metaTagType.ToLower()).Id;

                var filteredApplicationVideos = ApplyFilters(context, context.ApplicationVideos, filters, archive);
                var filteredSourceVideos = ApplySourceFilters(context, filters).ToList().AsQueryable();

                //compute all (tag, age, gender, value)
                var allGroupMetrics = (
                    from applicationVideo in filteredApplicationVideos
                    join avsv in context.ApplicationVideoSourceVideos on applicationVideo.Id equals avsv.ApplicationVideoId
                    join sv in filteredSourceVideos on avsv.SourceVideoId equals sv.Id
                    join demographicMetric in context.SourceVideoDemographicMetrics.AsNoTracking().Where(x => x.StartDate <= endDate && x.EndDate > endDate) on sv.Id equals demographicMetric.VideoId
                    join videoMetaTag in context.ApplicationVideoApplicationMetaTags on applicationVideo.Id equals videoMetaTag.VideoId
                    where videoMetaTag.Tag.TypeId == metaTagTypeId
                    group new
                {
                    Value = projectMetric(demographicMetric)
                }
                    by new { videoMetaTag.Tag.Tag, demographicMetric.Gender, demographicMetric.AgeGroup }
                    ).ToList();

                var allGroups = allGroupMetrics.Select(x => x.Key.Tag).Distinct().ToList();
                var allGenders = allGroupMetrics.Select(x => x.Key.Gender).Distinct().ToList();

                var unstructuredData =
                    from groupName in allGroups
                    from age in Constants.AgeGroups
                    from gender in Constants.GenderGroups
                    join bucket in allGroupMetrics on new { Group = groupName, Age = age, Gender = gender } equals new { Group = bucket.Key.Tag, Age = bucket.Key.AgeGroup, Gender = bucket.Key.Gender } into leftJoinedBuckets
                select(
                    Group : groupName,
                    Age : age,
                    Gender : gender,
                    Value : leftJoinedBuckets.Sum(x => (double) x.Sum(y => y.Value))
                    );

                return unstructuredData;
            }
        }
    }
}
