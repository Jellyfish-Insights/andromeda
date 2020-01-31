using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.Util.Reports;
using CsvHelper;
using Serilog.Core;
using Andromeda.Common.Jobs;
using Andromeda.Common.Logging;
using DataLakeModels;
using DataLakeModels.Models;

namespace Jobs.Fetcher.AdWords {

    public abstract class AdWordsFetcher : AbstractJob  {

        protected AdWordsUser User;

        protected static string ApiVersion = DataLakeAdWordsContext.AdWordsApiVersion;

        public override void Run() {
            using (var dbContext = new DataLakeAdWordsContext()) {
                Method(dbContext);
                dbContext.SaveChanges();
            }
        }

        protected abstract void Method(DataLakeAdWordsContext dbContext);

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        protected static T SubstituteLast<T>(T record, DataLakeAdWordsContext dbContext, IQueryable<T> queryLastValid) where T : class, IValidityRange, IEquatable<T> {
            var now = DateTime.UtcNow;
            var lastValid = (from i in queryLastValid
                             where i.ValidityStart <= now && now < i.ValidityEnd
                             orderby i.ValidityEnd select i).LastOrDefault();
            if (lastValid == null) {
                record.ValidityEnd = DateTime.MaxValue;
                record.ValidityStart = now;
                return record;
            }
            if (record.Equals(lastValid)) {
                return null;
            }
            lastValid.ValidityEnd = now;
            dbContext.Update(lastValid);
            record.ValidityEnd = DateTime.MaxValue;
            record.ValidityStart = now;
            return record;
        }

        protected static List<T> ReportToRecords<T>(ReportUtilities utilities, Func<dynamic, T> convert) {
            using (var response = utilities.GetResponse()) {
                using (var fileReader = new StreamReader(response.Stream)) {
                    var csv = new CsvReader(fileReader);
                    csv.Configuration.PrepareHeaderForMatch = header => header.Replace(" ", string.Empty).Replace(".", string.Empty).Replace("/", string.Empty);
                    csv.Configuration.Delimiter = "\t";
                    var records = csv.GetRecords<dynamic>();
                    // Converting to list enforces eager evaluation
                    return records.Select(record => (T) convert(record)).ToList();
                }
            }
        }
    }
}
