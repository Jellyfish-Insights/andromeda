<<<<<<< HEAD
using System;
using System.Linq;
using System.Collections.Generic;
using DataLakeModels;
using DataLakeModels.Models.AdWords;
using DataLakeModels.Models.AdWords.Reports;
using Common.Logging;
=======
using DataLakeModels;
using DataLakeModels.Models.AdWords.Reports;
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021

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
