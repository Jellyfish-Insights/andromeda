using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {
    public class MockMarketingDataBackend : AbstractMockDataBackend, IMarketingDataBackend {}
}
