using System;
using Common;
using Common.Logging;
using Serilog;

namespace DataLakeModels {

    public class FacebookDatabaseManager {

        public const string ApiVersion = "3.3";

        public static string SchemaName() {
            return "facebook_v" + ApiVersion.Replace('.', '_');
        }

        public static string ConnectionString() {
            return String.Format("{0};SearchPath={1};", ConnectionStringHelper.GetConnStr("DataLakeDatabase"), SchemaName());
        }

        protected static ILogger Logger { get => Log.ForContext<FacebookDatabaseManager>(); }
    }
}
