using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Serilog.AspNetCore;
using YearLogging = Andromeda.Common.Logging;

namespace DataLakeModels {

    public abstract partial class AbstractDataLakeContext : DbContext {
        protected static readonly ILoggerFactory EfLoggerFactory = new SerilogLoggerFactory(YearLogging.LoggerFactory.GetEfLogger());

        public static IConfiguration Configuration = new ConfigurationBuilder()
                                                         .SetBasePath(Directory.GetCurrentDirectory())
                                                         .AddJsonFile("appsettings.json")
                                                         .Build();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseNpgsql(Configuration.GetConnectionString("DataLakeDatabase"))
                    .UseLoggerFactory(EfLoggerFactory);
            }
        }
    }
}
