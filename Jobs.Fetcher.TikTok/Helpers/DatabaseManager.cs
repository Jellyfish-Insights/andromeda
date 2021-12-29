using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Jobs.Fetcher.TikTok {

    public class DatabaseManager : DataLakeModels.TikTokScraperDatabaseManager {

        private static HashSet<string> reserved = new HashSet<string> { "from" };


        public static List<string> GetPayload(DateTime last_fetch) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"
                        SELECT
                            json_payload
                        FROM
                            video_info
                        WHERE
                            saved_time > @last_fetch
                        ");
                    cmd.Parameters.AddWithValue("last_fetch", last_fetch);
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

        public static void Transactional(Action<NpgsqlConnection> action) {
            using (var connection = new NpgsqlConnection(ConnectionString())) {
                connection.Open();
                using (var transaction = connection.BeginTransaction()) {
                    try {
                        action(connection);
                        transaction.Commit();
                    }catch (Exception e) {
                        transaction.Rollback();
                        throw e;
                    }
                }
            }
        }
    }
}
