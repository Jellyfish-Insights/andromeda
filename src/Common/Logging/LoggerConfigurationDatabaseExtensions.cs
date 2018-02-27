using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Microsoft.EntityFrameworkCore;

namespace Common.Logging {
    public static class LoggerConfigurationDatabaseExtensions {
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(5);

        public static LoggerConfiguration Database<T>(
            this LoggerSinkConfiguration sinkConfiguration,
            string name,
            int batchSizeLimit = 1000,
            TimeSpan? period = null,
            int queueLimit = 10000,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information
            ) where T : DbContext, new() {

            if (sinkConfiguration == null) {
                throw new ArgumentNullException(nameof(sinkConfiguration));
            }

            period = period ?? DefaultPeriod;

            return sinkConfiguration.Sink(new DatabaseSink<T>(name, batchSizeLimit, period.Value, queueLimit), restrictedToMinimumLevel);
        }
    }
}
