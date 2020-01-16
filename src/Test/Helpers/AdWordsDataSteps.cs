using System;
using System.Linq;
using System.Collections.Generic;
using DataLakeModels;
using DataLakeModels.Models.AdWords;
using DataLakeModels.Models.AdWords.Reports;
using Common.Logging;

namespace Test.Helpers {

    /**
       This class is used to generate data for test scenarios. More specifically,
       this class generates data for AdWords Data Lake.
     */

    public class AdWordsDataSteps {
        public void ReportHasBeenFetched<T>(T report) where T : StructuralCriteriaPerformance {
            using (var context = new DataLakeAdWordsContext()) {
                context.Add(report);
                context.SaveChanges();
            }
        }
    }
}
