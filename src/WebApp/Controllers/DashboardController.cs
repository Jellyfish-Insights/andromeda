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
    public class DashboardController {

        [HttpGet("[action]")]
        public List<MetricInfo> GetMetricInfo() {
            return Constants.DashboardMetrics;
        }

        [HttpGet("[action]")]
        public int GetDefaultOffset() {
            return Constants.DefaultDashboardRange;
        }
    }
}
