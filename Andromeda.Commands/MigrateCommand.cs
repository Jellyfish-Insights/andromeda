using System;
using DataLakeModels;
using Andromeda.Common;
using System.Collections.Generic;

namespace Andromeda.Commands {

    public static class MigrateCommand {

        public static void Migrate(Databases db, bool force_update = false) {
            switch (db) {
                case Databases.LakeYouTubeData:
                    DatabaseOperations.Migrate<DataLakeYouTubeDataContext>();
                    break;
                case Databases.LakeYouTubeStudio:
                    DatabaseOperations.Migrate<DataLakeYouTubeStudioContext>();
                    break;
                case Databases.LakeYouTubeAnalytics:
                    DatabaseOperations.Migrate<DataLakeYouTubeAnalyticsContext>();
                    break;
                case Databases.LakeAdWords:
                    DatabaseOperations.Migrate<DataLakeAdWordsContext>();
                    break;
                case Databases.LakeLogging:
                    DatabaseOperations.Migrate<DataLakeLoggingContext>();
                    break;
                case Databases.LakeTwitterData:
                    DatabaseOperations.Migrate<DataLakeTwitterDataContext>();
                    break;
                case Databases.LakeTwitterAds:
                    DatabaseOperations.Migrate<DataLakeTwitterAdsContext>();
                    break;
                case Databases.LakeFacebook:
                    Jobs.Fetcher.Facebook.DatabaseInitializer.Init(force_update);
                    Jobs.Fetcher.Facebook.DatabaseInitializer.Init(new List<string> { "instagram" });
                    break;
                default:
                    throw new Exception("Invalid database");
            }
        }

        public static void MigrateDataLake() {
            DatabaseOperations.Migrate<DataLakeYouTubeDataContext>();
            DatabaseOperations.Migrate<DataLakeYouTubeStudioContext>();
            DatabaseOperations.Migrate<DataLakeYouTubeAnalyticsContext>();
            DatabaseOperations.Migrate<DataLakeAdWordsContext>();
            DatabaseOperations.Migrate<DataLakeTwitterDataContext>();
            DatabaseOperations.Migrate<DataLakeTwitterAdsContext>();
            DatabaseOperations.Migrate<DataLakeLoggingContext>();
            Console.WriteLine("Successfully migrated data-lake.");
        }

        public static void MigrateFacebook(bool force_update = false) {
            Migrate(Databases.LakeFacebook, force_update);
            Console.WriteLine("Successfully migrated Facebook-lake.");
        }
    }
}
