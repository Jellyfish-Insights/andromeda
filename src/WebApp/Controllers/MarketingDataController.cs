using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models.DataViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ApplicationModels.Models.AccountViewModels.Constants;

namespace WebApp.Controllers {
    [Authorize(Roles = Contansts.Permissions.ReadOnly)]
    [Route("api/[controller]")]
    public class MarketingDataController : AbstractDataController<IMarketingDataBackend> {

        private List<Video> VideoList = new List<Video>() {};

        public MarketingDataController(IMarketingDataBackend backend) {
            Backend = backend;
        }

        public override Dictionary<MetricInfo, TimeSeries> ChartData(string[] metric, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive = ArchiveMode.UnArchived) {
            var metricInfo = Constants.MarketingMetrics.Where(x => metric.Contains(x.Type));
            return Backend.ComputeTimeSeries(metricInfo.ToArray(), null, start, end, filters, archive);
        }

        protected override List<MetricInfo> GetMetricInfoList() {
            return Constants.MarketingMetrics;
        }
    }
}
