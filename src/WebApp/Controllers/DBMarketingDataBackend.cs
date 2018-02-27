using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {
    public class DBMarketingDataBackend : AbstractDBDataBackend, IMarketingDataBackend {

        private static double ProjectMetric(SourceAdMetric adMetrics, MetricType metricName) {
            switch (metricName) {
                case MetricType.Reach:
                    return (adMetrics?.Reach) ?? 0;
                case MetricType.Views:
                    return (adMetrics?.Views) ?? 0;
                case MetricType.Engagements:
                    return (adMetrics?.Engagements) ?? 0;
                case MetricType.Impressions:
                    return (adMetrics?.Impressions) ?? 0;
                case MetricType.Clicks:
                    return (adMetrics?.Clicks) ?? 0;
                case MetricType.EmailCaptures:
                    return (adMetrics?.EmailCapture) ?? 0;
                case MetricType.TotalCost:
                    return (adMetrics?.Cost) ?? 0;
                case MetricType.EngagementCost:
                    return (adMetrics?.CostPerEngagement * adMetrics?.Engagements) ?? 0;
                case MetricType.ClickCost:
                    return (adMetrics?.CostPerClick * adMetrics?.Clicks) ?? 0;
                case MetricType.ViewCost:
                    return (adMetrics?.CostPerView * adMetrics?.Views) ?? 0;
                case MetricType.ImpressionCost:
                    return (adMetrics?.CostPerImpression * adMetrics?.Impressions) ?? 0;
                case MetricType.EmailCaptureCost:
                    return (adMetrics?.CostPerEmailCapture * adMetrics?.EmailCapture) ?? 0;
                default:
                    throw new Exception("Metric not available");
            }
        }

        private class VideoMetricPersona {
            public int VideoId { get; set; }
            public SourceAdMetric Metric { get; set; }
            public string Persona { get; set; }
            public string Platform { get; set; }
        }

        private IQueryable<VideoMetricPersona> MetricsByPersona(DateTime start, DateTime end, ApplicationDbContext context, Tag[] filters, ArchiveMode archive) {
            var videoList = context.ApplicationVideos.AsQueryable();

            return from v in ApplyFilters(context, videoList, filters, archive)
                   join vc in context.ApplicationVideoSourceCampaigns on v.Id equals vc.VideoId
                   join a in ApplySourcAdFilters(context, filters).ToList().AsQueryable() on vc.CampaignId equals a.CampaignId
                   join am in context.SourceAdMetrics.Where(am => (am.EventDate >= start && am.EventDate <= end)) on a.Id equals am.AdId
                   join aau in context.SourceAdSets on a.AdSetId equals aau.Id
                   join ap in context.ApplicationPersonaVersionSourceAdSets on aau.Id equals ap.AdSetId into AdSetPersonaVersions
                   from ap in AdSetPersonaVersions.DefaultIfEmpty()
                   join pv in context.ApplicationPersonaVersions on ap.PersonaVersionId equals pv.Id into PersonaVersions
                   from pv in PersonaVersions.DefaultIfEmpty()
                   join p in context.ApplicationPersonas on pv.PersonaId equals p.Id into Personas
                   from p in Personas.DefaultIfEmpty()
                   select new VideoMetricPersona() { VideoId = v.Id, Metric = am, Persona = p.Name ?? "None", Platform = a.Platform };
        }

        public override Dictionary<MetricType, TimeSeries> ComputePrimitiveTimeSeries(MetricType[] metricNames, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive) {
            using (var context = new ApplicationDbContext()) {
                var videoMetricPersonas = MetricsByPersona(start, end, context, filters, archive);

                var allSamples = (from vmp in videoMetricPersonas
                                  select vmp).ToList();
                // metricNames.Select(metricName => )).ToList();

                var dateSet = DateUtilities.GetDatesBetween(start, end);

                return metricNames.Select((v, ix) => {
                    var timeSeriesGroup = from personaMetricRow in allSamples.Select(vmp => new Sample<string>(){ Date = vmp.Metric.EventDate, Value = ProjectMetric(vmp.Metric, v), Group = vmp.Persona })
                                          group personaMetricRow by personaMetricRow.Group into personaMetric
                                          select new TimeSeriesDataGroup() {
                        GroupName = personaMetric.Key,
                        Values = (from date in dateSet
                                  join m in personaMetric on date equals m.Date into dateMetrics
                                  select dateMetrics.Select(x => x.Value).DefaultIfEmpty(0).Sum()).ToArray()
                    };
                    return (v, ReduceTimeSeries(dateSet, timeSeriesGroup.AsEnumerable()));
                }).ToDictionary(x => x.Item1, x => x.Item2);
            }
        }

        public Metric ComputeMetric(IEnumerable<SourceAdMetric> source, MetricInfo metricInfo) {
            if (metricInfo.GetType() == typeof(AverageMetric)) {
                var avg = (AverageMetric) metricInfo;
                var x = source.Sum(a => ProjectMetric(a, avg.Numerator));
                var y = source.Sum(a => ProjectMetric(a, avg.Denominator));
                return new Metric() {
                           Type = metricInfo.Type,
                           Value = SafelyDivide(x, y)
                };
            } else {
                return new Metric() {
                           Type = metricInfo.Type,
                           Value = source.Sum(a => ProjectMetric(a, metricInfo.TypeId))
                };
            }
        }

        public List<VideoMetric> MetricList(DateTime start, DateTime end, Tag[] filters, List<MetricInfo> metrics, ArchiveMode archive) {
            using (var context = new ApplicationDbContext()) {
                var videoMetricPersonas = MetricsByPersona(start, end, context, filters, archive);

                return (from vmp in videoMetricPersonas
                        group new { Metric = vmp.Metric, Persona = vmp.Persona ?? "None" } by vmp.VideoId into nv
                        select new VideoMetric() {
                    Id = nv.Key.ToString(),
                    MetricsPerPersona = nv.GroupBy(x => x.Persona).Select(x => new PersonaMetric() {
                        Persona = x.Key,
                        Metrics = metrics.Select(v => ComputeMetric(x.Select(b => b.Metric), v)).ToList()
                    }).ToList(),
                    TotalMetrics = metrics.Select(v => ComputeMetric(nv.Select(b => b.Metric), v)).ToList()
                }).ToList();
            }
        }
    }
}
