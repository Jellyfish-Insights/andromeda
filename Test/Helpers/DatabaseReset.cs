using Andromeda.Common;
using Andromeda.Commands;

namespace Test.Helpers {

    public class DatabaseReset {

        public static void Drop(Databases db) {
            DatabaseOperations.Drop(db);
        }

        public static void Migrate(Databases db) {
            MigrateCommand.Migrate(db);
        }

        public static void MigrateFacebook() {
            MigrateCommand.MigrateDataLake();
            MigrateCommand.MigrateFacebook();
        }
    }
}
