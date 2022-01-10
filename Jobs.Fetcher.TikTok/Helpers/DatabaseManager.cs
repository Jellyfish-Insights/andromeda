using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels.Models;
using DataLakeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

using Microsoft.EntityFrameworkCore;

namespace Jobs.Fetcher.TikTok {

    public class DatabaseManager : DataLakeModels.TikTokScraperDatabaseManager {

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
                            saved_time > @last_fetch AND
                            account_name = @username
                        ");
                    cmd.Parameters.AddWithValue("last_fetch", last_fetch);
                    cmd.Parameters.AddWithValue("username", username);
                    var payloadStrings = new List<string>();
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            /*var log = new RowLog();
                               log.AddInput("videos",
                                        MutableEntityExtentions.AutoPK(reader.Prim<long>("id")));*/
                            payloadStrings.Add(reader.GetString(0));
                        }
                    }
                    return payloadStrings;
                }
        }

        public static TikTokUsers GetTikTokUser(string account_name, NpgsqlConnection connection) {
            using (var cmd = connection.CreateCommand()) {
                connection.Open();
                cmd.CommandText = String.Format(@"
                    SELECT
                        id,
                        account_name
                    FROM
                        account_name
                    WHERE
                        account_name > @account_name
                ");
                cmd.Parameters.AddWithValue("account_name", account_name);
                TikTokUsers tiktokUser = null;
                using (var reader = cmd.ExecuteReader()) {
                    if (reader.Read()) {
                        tiktokUser = new TikTokUsers(){
                            //UserId = reader.GetString(0),
                            Name = reader.GetString(1)
                        };
                    }
                }
                return tiktokUser;
            }
        }

        public static string GetTikTokId(string username){
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"
                        SELECT
                            tiktok_id::text
                        FROM
                            video_info
                        WHERE
                            account_name = @username
                        ");
                    cmd.Parameters.AddWithValue("username", username);
                    var payloadStrings = new List<string>();
                    using (var reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            return reader.GetString(0);
                        }
                    }
                    return null;
                }
        }

        public static bool TikTokUserExists(string username){
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"
                        SELECT
                            COUNT(*)
                        FROM
                            account_name
                        WHERE
                            account_name = @username
                        ");
                    cmd.Parameters.AddWithValue("username", username);
                    using (var reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            return reader.GetDecimal(0) > 0;
                        }
                    }
                    return false;
                }
        }

        public static bool TikTokScraperTablesExist(){
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
                    Console.WriteLine("No value return");
                    return false;
                }
        }

        public static DateTime GetLastFetch(string username, DataLakeTikTokContext dbContext) {
            //var oldEntry = dbContext.Posts.Find(newEntry.Id);
            var now = DateTime.UtcNow;
            return dbContext.Posts.Where(m => m.Author.UniqueId == username && m.ValidityStart <= now && m.ValidityEnd > now)
                       .OrderByDescending(m => m.ValidityStart)
                       .Select(m => m.ValidityStart)
                       .FirstOrDefault();
        }
    }
}
