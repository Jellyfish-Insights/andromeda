using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApplicationModels.Models.DataViewModels;
using ApplicationModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using ApplicationModels.Models.AccountViewModels.Constants;

namespace WebApp.Controllers {

    [Route("api/[controller]")]
    [Authorize(Roles = Contansts.Permissions.ReadOnly)]
    public abstract class AbstractDataController<T>: Controller where T : IDataBackend  {
        protected T Backend;
        public abstract Dictionary<MetricInfo, TimeSeries> ChartData(string[] metric, string type, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive = ArchiveMode.UnArchived);
        protected abstract List<MetricInfo> GetMetricInfoList();

        [HttpGet("[action]")]
        public JsonResult GetMetricInfo() {
            return Json(new {
                MetricInfo = GetMetricInfoList()
            });
        }

        [HttpPut("[action]")]
        [Authorize(Roles = Contansts.Permissions.Editor)]
        public VideoEdits EditVideos([FromBody] VideoEdits edits) {
            return Backend.EditVideos(edits);
        }

        [HttpGet("[action]/{type}")]
        public List<SourceObject> GetUnassociatedSources(SourceObjectType type) {
            return Backend.UnAssociatedSources(type);
        }

        [HttpGet("[action]/{filters?}/{archive?}")]
        public List<Video> GetVideoList(string filters = "[]", ArchiveMode archive = ArchiveMode.UnArchived) {
            var filterList = JsonConvert.DeserializeObject<Tag[]>(filters);
            return Backend.VideoList(filterList, archive);
        }

        [HttpGet("[action]/{metrics}/{type}/{startDate}/{endDate}/{filters?}/{archive?}")]
        public IEnumerable<TimeSeriesChartData> GetChartData(string metrics, string type, string startDate, string endDate, string filters = "[]", ArchiveMode archive = ArchiveMode.UnArchived) {

            var filterList = JsonConvert.DeserializeObject<Tag[]>(filters);
            var metricList = JsonConvert.DeserializeObject<string[]>(metrics);
            var start = DateUtilities.ReadDate(startDate);
            var end = DateUtilities.ReadDate(endDate);
            var timeSeries = ChartData(metricList, type, start, end, filterList, archive);
            return timeSeries.Select(x => CreateTimeSeriesChartData(x.Value, start, end, x.Key));
        }

        [HttpGet("[action]/{startDate}/{endDate}/{filters?}/{archive?}")]
        public List<VideoMetric> GetMetricList(string startDate, string endDate, string filters = "[]", ArchiveMode archive = ArchiveMode.UnArchived) {

            var filterList = JsonConvert.DeserializeObject<Tag[]>(filters);
            var start = DateUtilities.ReadDate(startDate);
            var end = DateUtilities.ReadDate(endDate);
            return Backend.MetricList(start, end, filterList, GetMetricInfoList(), archive);
        }

        [HttpGet("[action]")]
        public JsonResult GetFilters() {
            return Json(new {
                Platforms = Constants.Sources,
                Tags = Backend.TagList(),
                Personas = Backend.PersonaList(),
                Playlists = Backend.PlaylistList(),
                Sources = Backend.SourceList()
            });
        }

        private static TimeSeriesChartData CreateTimeSeriesChartData(TimeSeries timeSeries, DateTime start, DateTime end, MetricInfo metric) {
            var totalPerGroup = timeSeries.TotalPerGroup;
            if (!totalPerGroup.ContainsKey(timeSeries.TotalTimeSeries.GroupName)) {
                totalPerGroup.Add(timeSeries.TotalTimeSeries.GroupName, timeSeries.TotalOnPeriod);
            }
            return new TimeSeriesChartData(){
                       StartDate = start,
                       EndDate = end,
                       Metric = metric.Type,
                       ChartObjectArray = ConvertTimeSeriesToChartObjectArray(timeSeries),
                       TotalPerGroup = totalPerGroup,
            };
        }

        private static List<ChartObject> ConvertTimeSeriesToChartObjectArray(TimeSeries timeSeries) {
            List<ChartObject> result = new List<ChartObject>();
            var dates = timeSeries.Dates;
            int numberOfDates = dates.Count();
            for (var i = 0; i < numberOfDates; i++) {
                var values = new Dictionary<string, double>();
                timeSeries.Values.ForEach(x => values.Add(x.GroupName, x.Values.ElementAtOrDefault(i)));
                values.Add(timeSeries.TotalTimeSeries.GroupName, timeSeries.TotalTimeSeries.Values.ElementAtOrDefault(i));
                var chartObject = new ChartObject { Date = dates[i], Values = values };
                result.Add(chartObject);
            }
            return result;
        }

        public TimeSeries ChartData(string metric, string type, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive = ArchiveMode.UnArchived) {
            return ChartData(new string[] { metric }, type, startDate, endDate, filters, archive).First().Value;
        }
    }
}
