using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models.DataViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using ApplicationModels.Models.AccountViewModels.Constants;

namespace WebApp.Controllers {
    [Route("api/[controller]")]
    [Authorize(Roles = Contansts.Permissions.ReadOnly)]
    public class ContentDataController : AbstractDataController<IContentDataBackend> {

        private List<Video> VideoList = new List<Video>();

        public ContentDataController(IContentDataBackend backend) {
            Backend = backend;
        }

        [HttpGet("[action]/{metric?}/{type?}/{startDate?}/{endDate?}/{filters?}/{archive?}")]
        public JsonResult GetDemographicsChartData(string metric, string type, string startDate, string endDate, string filters, ArchiveMode archive = ArchiveMode.UnArchived) {
            var filterList = JsonConvert.DeserializeObject<Tag[]>(filters);
            var metricList = JsonConvert.DeserializeObject<string[]>(metric);
            var start = DateUtilities.ReadDate(startDate);
            var end = DateUtilities.ReadDate(endDate);
            return Json(metricList.Select(m => new {
                StartDate = startDate,
                EndDate = endDate,
                Metrics = m,
                Data = GetDemographics(m, type, start, end, filterList, archive)
            })
                        );
        }

        private TimeSeries GenerateMockedChartData(DateTime startDate, DateTime endDate, IEnumerable<string> groups) {
            IEnumerable<DateTime> dates = DateUtilities.GetDatesBetween(startDate, endDate);
            var Values = new List<TimeSeriesDataGroup>();
            Random rnd = new Random();

            foreach (var groupName in groups) {
                var values = dates.Select(x => (rnd.Next(1, 300)) * 1.0).ToArray();
                Values.Add(new TimeSeriesDataGroup { GroupName = groupName, Values = values });
            }

            return AbstractDataBackend.ReduceTimeSeries(dates, Values);
        }

        private TimeSeries MockChartData(string metric, string type, DateTime startDate, DateTime endDate, Tag[] filters) {
            if (type.Equals("persona")) {
                return GenerateMockedChartData(startDate, endDate, Backend.PersonaList());
            } else {
                List<Tag> tags;
                Backend.MetaTagsList().TryGetValue(type.ToLower(), out tags);
                return GenerateMockedChartData(startDate, endDate, tags.Select(x => x.Value));
            }
        }

        public DemographicData GetDemographics(string metric, string type, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive = ArchiveMode.UnArchived) {
            var unstructuredData = Backend.GetUnstructuredDemographicData(metric, type, startDate, endDate, filters, archive);
            return BuildDemographicData(unstructuredData);
        }

        private static DemographicData BuildDemographicData(IEnumerable<(string Group, string Age, string Gender, double Value)> unstructuredData) {
            var demographicData = new DemographicData {
                Groups = Constants.GenderGroups,
                Values = (
                    from e in unstructuredData
                    group e by e.Group into g
                    select new DemographicDataItem(){
                    GroupName = g.Key,
                    Values = (
                        from gg in g
                        group gg by gg.Age into ageGroupData
                        select new { Key = ageGroupData.Key, Values = ageGroupData.ToDictionary(j => j.Gender, j => j.Value) }
                        ).ToDictionary(z => z.Key, z => z.Values)
                }).ToList()
            };

            // Compute totals on each GroupDataItem
            demographicData.Values = demographicData.Values.Select(FillInGroupTotal).ToList();

            // Add Grand Total Group
            var grandTotalDataItem = ComputeTotalDemographicDataItem(demographicData.Values);
            demographicData.Values.Add(grandTotalDataItem);

            // Normalize all Data Group Items
            demographicData.Values = demographicData.Values.Select(NormalizeDemographicDataItem).ToList();

            return demographicData;
        }

        private static DemographicDataItem FillInGroupTotal(DemographicDataItem item) {
            return new DemographicDataItem() {
                       GroupName = item.GroupName,
                       Values = item.Values,
                       Total = (from x in item.Values from y in x.Value select y.Value).Sum()
            };
        }

        private static DemographicDataItem NormalizeDemographicDataItem(DemographicDataItem data) {
            /**
               Normalize all groups into range [0.0, 1.0]
             */
            return new DemographicDataItem {
                       GroupName = data.GroupName,
                       Values = data.Values.ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Key, y => (data.Total != 0) ? y.Value / data.Total : 0)),
                       Total = data.Total
            };
        }

        private static DemographicDataItem ComputeTotalDemographicDataItem(List<DemographicDataItem> topicDemographic) {
            var data = (from topic in topicDemographic
                        from x in topic.Values
                        from y in x.Value
                        select new {
                Group = topic.GroupName,
                AgeGroup = x.Key,
                Gender = y.Key,
                Value = y.Value
            }).ToList();

            return new DemographicDataItem() {
                       GroupName = Constants.GrandTotalName,
                       Values = (
                           from topic in data
                           group topic by new { Age = topic.AgeGroup, Gender = topic.Gender } into g
                           select new { Age = g.Key.Age, Gender = g.Key.Gender, Value = g.Sum(x => x.Value) } into demographics
                           group demographics by demographics.Age into groupAgeDemographics
                           select new { groupAgeDemographics.Key, GenderToValue = groupAgeDemographics.ToDictionary(z => z.Gender, z => z.Value) }
                           ).ToDictionary(z => z.Key, z => z.GenderToValue),
                       Total = data.Sum(x => x.Value)
            };
        }

        public override Dictionary<MetricInfo, TimeSeries> ChartData(string[] metrics, string type, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive = ArchiveMode.UnArchived) {
            var metricInfo = metrics.Select(metric => Constants.ContentMetrics.Find(x => x.Type == metric));
            return Backend.ComputeTimeSeries(metricInfo.ToArray(), type, startDate, endDate, filters, archive);
        }

        protected override List<MetricInfo> GetMetricInfoList() {
            return Constants.ContentMetrics;
        }
    }
}
