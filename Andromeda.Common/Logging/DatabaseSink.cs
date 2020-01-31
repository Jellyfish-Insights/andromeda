using System;
using System.Collections.Generic;
using System.Linq;
using Andromeda.Common.Logging.Models;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Andromeda.Common.Logging {
    public class DatabaseSink<T>: PeriodicBatchingSink where T : DbContext, new() {

        private string Name;

        public DatabaseSink(string name, int batchSizeLimit, TimeSpan period, int queueLimit): base(batchSizeLimit, period, queueLimit) {
            Name = name;
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events) {
            using (var dbContext = new T()) {
                dbContext.AddRange(
                    events.Select(x => new RuntimeLog(Name, x)).ToArray()
                    );
                dbContext.SaveChanges();
            }
        }
    }
}
