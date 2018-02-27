using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {
    public class MockContentDataBackend : AbstractMockDataBackend, IContentDataBackend {
        public Dictionary<int, Dictionary<string, Dictionary<string, double>>> VideoMetricByDay(IEnumerable<int> apVideoIds, DateTime start, DateTime end) {
            throw new NotImplementedException();
        }
    }
}
