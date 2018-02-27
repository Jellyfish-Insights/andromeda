using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {

    public abstract class AbstractDataBackend {

        public abstract Dictionary<MetricType, TimeSeries> ComputePrimitiveTimeSeries(MetricType[] metric, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive);

        public List<string> SourceList() {
            return Constants.Sources;
        }

        public TimeSeries ComputeAverageTimeSeries(Dictionary<MetricType, TimeSeries> metricStore, MetricType numeratorName, MetricType denominatorName, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive) {
            var topSeries = metricStore[numeratorName];
            var bottomSeries = metricStore[denominatorName];
            return Divide(topSeries, bottomSeries);
        }

        public static TimeSeries ReduceTimeSeries(IEnumerable<DateTime> dateSet, IEnumerable<TimeSeriesDataGroup> timeSeriesGroup, double[] preTotalSeries = null) {
            var totalSeries = preTotalSeries ?? (timeSeriesGroup
                                                     .Select(x => x.Values.AsEnumerable())
                                                     .DefaultIfEmpty(new List<double>().AsEnumerable())
                                                     .Aggregate((x, y) => x.Zip(y, (a, b) => a + b))
                                                     .ToArray());

            return new TimeSeries(){
                       Dates = dateSet.Select(x => x.Date.ToString(DateUtilities.DateFormat)).ToList(),
                       Values = timeSeriesGroup.ToList(),
                       TotalTimeSeries = new TimeSeriesDataGroup(){
                           GroupName = Constants.GrandTotalName,
                           Values = totalSeries
                       },
                       TotalPerGroup = timeSeriesGroup.ToDictionary(x => x.GroupName, x => x.Values.Sum()),
                       TotalOnPeriod = totalSeries.Sum(),
            };
        }

        private HashSet<MetricType> GetRequiredMetricsFromRequestedMetrics(MetricInfo[] metricsInfo) {
            var requiredMetrics = new HashSet<MetricType>();
            foreach (var metric in metricsInfo) {
                if (IsDemographicMetricType(metric.TypeId)) {
                    throw new ArgumentException($"Unexpected demographic type {metric.TypeId}");
                }
                if (metric.GetType() == typeof(AverageMetric)) {
                    var avg = (AverageMetric) metric;
                    requiredMetrics.Add(avg.Numerator);
                    requiredMetrics.Add(avg.Denominator);
                } else {
                    requiredMetrics.Add(metric.TypeId);
                }
            }
            return requiredMetrics;
        }

        public Dictionary<MetricInfo, TimeSeries> ComputeTimeSeries(MetricInfo[] metricsInfo, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive) {
            var requiredMetrics = GetRequiredMetricsFromRequestedMetrics(metricsInfo);
            var primitiveStore = ComputePrimitiveTimeSeries(requiredMetrics.ToArray(), type, start, end, filters, archive);

            var metricStore = new ConcurrentDictionary<MetricInfo, TimeSeries>();
            foreach (var metric in metricsInfo) {
                if (metric.GetType() != typeof(AverageMetric)) {
                    metricStore[metric] = primitiveStore[metric.TypeId];
                }
            }
            metricsInfo
                .Distinct()
                .AsParallel()
                .WithDegreeOfParallelism(2)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(metric => {
                if (metric.GetType() == typeof(AverageMetric)) {
                    var avg = (AverageMetric) metric;
                    var avgSeries = ComputeAverageTimeSeries(primitiveStore, avg.Numerator, avg.Denominator, type, start, end, filters, archive);
                    metricStore[metric] = avgSeries;
                }
            });
            return metricStore.ToDictionary(x => x.Key, x => x.Value);
        }

        public static double SafelyDivide(double numerator, double denominator) {
            return denominator == 0 ? 0 : numerator / denominator;
        }

        public static bool FilterArchive(ArchiveMode mode, bool archive) {
            switch (mode) {
                case ArchiveMode.Archived:
                    return archive == true;
                case ArchiveMode.UnArchived:
                    return archive == false;
                default:
                    return true;
            }
        }

        public static bool AggregatePublishedMode(IEnumerable<bool> sources, PublishedMode mode) {
            if (sources == null) {
                return false;
            }
            switch (mode) {
                case PublishedMode.AllPublished:
                    return sources.Aggregate((acc, cur) => acc && cur);
                case PublishedMode.AllUnpublished:
                    return !sources.Aggregate((acc, cur) => acc || cur);
                case PublishedMode.SomePublished:
                    return sources.Aggregate((acc, cur) => acc || cur);
                case PublishedMode.All:
                    return true;
                default:
                    throw new ArgumentException($"Invalid publish mode '{mode}'");
            }
        }

        public static TimeSeries Divide(TimeSeries numerator, TimeSeries denominator) {
            return new TimeSeries(){
                       Dates = numerator.Dates,
                       Values = (from t in numerator.Values
                                 join b in denominator.Values on t.GroupName equals b.GroupName
                                 select new TimeSeriesDataGroup(){
                    GroupName = t.GroupName,
                    Values = t.Values.Zip(b.Values, SafelyDivide).ToArray()
                }).ToList(),
                       TotalTimeSeries = new TimeSeriesDataGroup(){
                           GroupName = numerator.TotalTimeSeries.GroupName,
                           Values = numerator.TotalTimeSeries.Values.Zip(denominator.TotalTimeSeries.Values, (x, y) => SafelyDivide(x, y)).ToArray()
                       },
                       TotalOnPeriod = SafelyDivide(numerator.TotalOnPeriod, denominator.TotalOnPeriod),
                       TotalPerGroup = (from t in numerator.TotalPerGroup.AsEnumerable()
                                        join b in denominator.TotalPerGroup.AsEnumerable() on t.Key equals b.Key
                                        select KeyValuePair.Create<string, double>(t.Key, SafelyDivide(t.Value, b.Value))).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        public bool IsDemographicMetricType(MetricType metricType) {
            return Constants.DemographicTypes.Where(x => x == metricType).Any();
        }
    }
}
