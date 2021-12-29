using System;
using Andromeda.Common;
using Andromeda.Common.Logging;
using Serilog;

namespace DataLakeModels {

    public class TikTokScraperDatabaseManager {

        public const string ApiVersion = "1";

        public static string SchemaName() {
            return "general_scraper_v" + ApiVersion.Replace('.', '_');
        }

        public static string ConnectionString() {
            return String.Format("{0};SearchPath={1};", ConnectionStringHelper.GetConnStr("DataLakeDatabase"), SchemaName());
        }

        protected static ILogger Logger { get => Log.ForContext<TikTokScraperDatabaseManager>(); }
    }
}
