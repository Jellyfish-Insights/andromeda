using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Andromeda.Common {

    public enum Databases {
        LakeYouTubeData,
        LakeYouTubeStudio,
        LakeYouTubeAnalytics,
        LakeAdWords,
        LakeLogging,
        LakeFacebook,
        LakeTwitterData,
        LakeTwitterAds,
        LakeTikTok
    }

    public enum YearDatabase {
        DataLakeDatabase
    }

    public static class DatabaseOperations {

        public static void Drop(Databases db) {
            string settingsField;
            switch (db) {
                case Databases.LakeYouTubeData:
                case Databases.LakeYouTubeAnalytics:
                case Databases.LakeAdWords:
                case Databases.LakeLogging:
                case Databases.LakeTwitterData:
                    settingsField = "DataLakeDatabase";
                    break;
                case Databases.LakeFacebook:
                    settingsField = "DataLakeDatabase";
                    break;
                default:
                    throw new Exception("Invalid database");
            }

            var(conn, databaseName) = ConnectionStringHelper.GetNoDbConnection(settingsField);

            conn.Open();
            using (var cmd = conn.CreateCommand()) {
                cmd.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\"";
                cmd.ExecuteNonQuery();
            }
            conn.Close();
        }

        public static void Migrate<T>() where T : DbContext, new() {
            using (var context = new T()) {
                context.Database.Migrate();
            }
        }
    }

    public static class ConnectionStringHelper {

        static string AppsettingsPath = "appsettings.json";

        public static string GetConnStr(string database) {
            using (StreamReader r = new StreamReader(AppsettingsPath)) {
                var json = JObject.Parse(r.ReadToEnd());
                return (string) json["ConnectionStrings"][database];
            }
        }

        public static NpgsqlConnection GetDbConnection(YearDatabase database) {
            return GetDbConnection(database.ToString());
        }

        public static NpgsqlConnection GetDbConnection(string database) {
            var connectionString = GetConnStr(database);
            return new NpgsqlConnection(connectionString);
        }

        public static (NpgsqlConnection, string) GetNoDbConnection(string database) {
            var connectionString = GetConnStr(database);
            var dbNameRegex = new Regex(@"Database=(?<dbName>\w+)");

            return (
                new NpgsqlConnection(dbNameRegex.Replace(connectionString, "Database=postgres")),
                dbNameRegex.Match(connectionString).Groups["dbName"].Value
                );
        }
    }

    public static class ContextIntrospection {

        public static (string schema, string table, List<string> keys) GetDatabaseInfo(IEntityType entity) {
            return (
                schema :  entity.Relational().Schema,
                table : entity.Relational().TableName,
                keys : entity.FindPrimaryKey().Properties.Select(x => x.Name).ToList()
                );
        }

        public static (string schema, string table, List<string> keys) GetDatabaseInfo(DbContext context, Type modelClass) {
            var entity = context.Model.FindEntityType(modelClass);
            return GetDatabaseInfo(entity);
        }
    }

    public static class TableOperations {

        public static int DeleteFromTable(YearDatabase database, string schemaName, string tableName) {
            using (var connection = ConnectionStringHelper.GetDbConnection(database)) {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"DELETE FROM {schemaName}.""{tableName}""";
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
