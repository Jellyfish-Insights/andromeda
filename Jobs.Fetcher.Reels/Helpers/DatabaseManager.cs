using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels.Models;
using DataLakeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

using Microsoft.EntityFrameworkCore;

namespace Jobs.Fetcher.Reels {

    public class DatabaseManager : DataLakeModels.GeneralScraperDatabaseManager {

        private static HashSet<string> reserved = new HashSet<string> { "from" };

        public static List<string> GetPayload(string username, DateTime last_fetch) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"
                        SELECT
                            json_payload
                        FROM
                            video_info
                        WHERE
                            saved_time > @last_fetch :: timestamp without time zone AND
                            account_name = @username
                        ;");
                    cmd.Parameters.AddWithValue("last_fetch", last_fetch.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("username", username);
                    var payloadStrings = new List<string>();
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            payloadStrings.Add(reader.GetString(0));
                        }
                    }
                    return payloadStrings;
                }
        }

        public static string GetReelsId(string username, DataLakeReelsContext dbContext) {
            var now = DateTime.UtcNow;
            var valueToBeReturned = dbContext.Users.Where(m => m.Username == username)
                                        .Select(m => m.Pk)
                                        .FirstOrDefault();
            return valueToBeReturned;
        }

        public static bool GeneralScraperTablesExist() {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"
                        SELECT EXISTS (
                            SELECT FROM information_schema.tables 
                            WHERE table_name   = 'video_info'
                        );");
                    using (var reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            var returnValue = reader.GetBoolean(0);
                            return returnValue;
                        }
                    }
                    return false;
                }
        }

        public static DateTime GetLastFetch(string userId, DataLakeReelsContext dbContext) {
            var now = DateTime.UtcNow;
            return dbContext.ReelStats.Where(m => m.UserId == userId && m.ValidityStart <= now && m.ValidityEnd > now)
                       .OrderByDescending(m => m.ValidityStart)
                       .Select(m => m.ValidityStart)
                       .FirstOrDefault();
        }
    }
}
