using System;
using DataLakeModels;
<<<<<<< HEAD
using ApplicationModels;
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
using Common;

namespace ConsoleApp.Commands {

    public static class MigrateCommand {

        public static void Migrate(Databases db) {
            switch (db) {
<<<<<<< HEAD
                case Databases.AnalyticsPlatform:
                    DatabaseOperations.Migrate<ApplicationDbContext>();
                    break;
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                case Databases.LakeYouTubeData:
                    DatabaseOperations.Migrate<DataLakeYouTubeDataContext>();
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
                case Databases.LakeFacebook:
                    Jobs.Fetcher.Facebook.DatabaseInitializer.Init();
                    break;
                default:
                    throw new Exception("Invalid database");
            }
        }

<<<<<<< HEAD
        public static void MigrateApplication() {
            DatabaseOperations.Migrate<ApplicationDbContext>();
        }

=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
        public static void MigrateDataLake() {
            DatabaseOperations.Migrate<DataLakeYouTubeDataContext>();
            DatabaseOperations.Migrate<DataLakeYouTubeAnalyticsContext>();
            DatabaseOperations.Migrate<DataLakeAdWordsContext>();
            DatabaseOperations.Migrate<DataLakeLoggingContext>();
        }

        public static void MigrateFacebook() {
            Migrate(Databases.LakeFacebook);
        }
    }
}
