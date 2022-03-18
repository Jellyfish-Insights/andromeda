using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Jobs.Fetcher.Facebook {

    public class DatabaseManager : DataLakeModels.FacebookDatabaseManager {

        private static HashSet<string> reserved = new HashSet<string> { "from" };

        public static string QuoteReserved(string field) {
            if (reserved.Contains(field)) {
                return "\"" + field + "\"";
            } else {
                return field;
            }
        }

        public static void CreateSchema() {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format("CREATE SCHEMA {0}", SchemaName());
                    try {
                        cmd.ExecuteNonQuery();
                    } catch (Npgsql.PostgresException) {
                        Logger.Warning($"Schema already exists: {SchemaName()}");
                    }
                }
        }

        public static void CreateTable(Table table, bool force_update) {
            CreateTable(table.TableName, table.ColumnDefinition, table.Constraints, force_update);
        }

        public static void CreateTable(string name, Dictionary<string, Column> columns, List<string> constraints, bool force_update) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var fields = columns
                                     .Select(x => QuoteReserved(x.Value.Name) + " " + x.Value.Type + " " + x.Value.Constraint);
                    cmd.CommandText = String.Format("CREATE TABLE {0} (\n{1}\n)", name, fields.Concat(constraints).Aggregate((x, y) => x + ",\n" + y));
                    try {
                        cmd.ExecuteNonQuery();
                        Logger.Information($"Table created: {name}");
                    } catch (Npgsql.PostgresException e) {
                        if (e.Message.Contains("already exists")) {
                            Logger.Warning($"Table already exists: {name}");
                            if (force_update) {
                                foreach (var column in columns.Values) {
                                    CheckAndCreateColumns(name, column);
                                }
                            }
                        } else
                            Logger.Error($"Table creation operation failed: {e.Message}");
                    }
                }
        }

        public static void CheckAndCreateColumns(string name, Column column) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"SELECT EXISTS (SELECT 1 FROM information_schema.columns 
                                                        WHERE  table_name='{0}' AND column_name='{1}')",
                                                    name,
                                                    column.Name
                                                    );
                    try {
                        using (var r = cmd.ExecuteReader()) {
                            r.Read();
                            if (r.GetBoolean(0)) {
                                Logger.Warning($"Column already exists: {column.Name}");
                            } else {
                                CreateColumn(name, column);
                                Logger.Information($"Column created: {column.Name}");
                            }
                        }
                    } catch (Npgsql.PostgresException e) {
                        Logger.Error($"Column check and creation failed: {e.Message}");
                    }
                }
        }

        public static void CreateColumn(string name, Column column) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format("ALTER TABLE {0} ADD COLUMN {1} {2} {3}", name, column.Name, column.Type, column.Constraint);
                    try {
                        cmd.ExecuteNonQuery();
                        Logger.Information($"Column created: {name}");
                    } catch (Npgsql.PostgresException e) {
                        Logger.Error($"Column creation failed: {e.Message}");
                    }
                }
        }

        public static void UpdateLastFetch(Edge table, JObject row, string[] ids, string version_field, string last_fetch_name) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var parameters = table.ColumnDefinition
                                         .Where(x => row[x.Key] != null && ids.Contains(x.Key));
                    var systime = table.ColumnDefinition[version_field];
                    var last_fetch = table.ColumnDefinition[last_fetch_name];
                    var fields = parameters
                                     .Select(x => QuoteReserved(x.Value.Name) + " = @" + x.Value.Hash() + " :: " + x.Value.Type)
                                     .Aggregate("True", (x, y) => x + " AND " + y);

                    cmd.CommandText = String.Format(@"
                        UPDATE
                            {0}
                        SET
                            {1} = @{2}
                        WHERE
                            {3}
                            AND {4} && @{5}:: TSRANGE
                        ",
                                                    table.TableName,
                                                    last_fetch.Name,
                                                    last_fetch.Hash(),
                                                    fields,
                                                    systime.Name,
                                                    systime.Hash()
                                                    );

                    foreach (KeyValuePair<string, Column> x in parameters) {
                        AddParameter(cmd, x.Value, row);
                    }

                    AddParameter(cmd, systime, row);
                    AddParameter(cmd, last_fetch, row);
                    var rows = cmd.ExecuteNonQuery();
                }
        }

        public static void VersionEntityModified(Table table, JObject row, string nominalColumnName, string version_field) {
            VersionEntityModified(table, row, new string[] { nominalColumnName }, version_field);
        }

        public static void VersionEntityModified(Table table, JObject row, string[] nominalColumnNames, string version_field) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var parameters = table.ColumnDefinition
                                         .Where(x => row[x.Key] != null && nominalColumnNames.Contains(x.Key));
                    var systime = table.ColumnDefinition[version_field];
                    var fields = parameters
                                     .Select(x => ColumnSqlEquals(x.Value))
                                     .Aggregate((x, y) => x + " AND " + y);

                    cmd.CommandText = String.Format(@"
                        UPDATE
                            {0}
                        SET
                            {1} = {2} - @{3}:: TSRANGE
                        WHERE
                            {4}
                            AND {5} && @{6}:: TSRANGE
                        ",
                                                    table.TableName,
                                                    version_field,
                                                    version_field,
                                                    systime.Hash(),
                                                    fields,
                                                    version_field,
                                                    systime.Hash()
                                                    );

                    foreach (KeyValuePair<string, Column> x in parameters) {
                        AddParameter(cmd, x.Value, row);
                    }
                    AddParameter(cmd, systime, row);
                    cmd.ExecuteNonQuery();
                }
        }

        public static Modified CheckEntityModified(Table table, JObject row) {
            return CheckEntityModified(table, row, new[] { "id" });
        }

        public static Modified CheckEntityModified(Table table, JObject row, string[] ids) {
            Modified res;
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var parameters = table.ColumnDefinition
                                         .Where(x => row[x.Key] != null && ids.Contains(x.Key));
                    var systime = table.ColumnDefinition["systime"];
                    var fields = parameters
                                     .Select(x => ColumnSqlEquals(x.Value))
                                     .Aggregate((x, y) => x + " AND " + y);

                    cmd.CommandText = String.Format(@"
                        SELECT
                            isempty(systime - @{0}:: TSRANGE),systime = @{0} :: TSRANGE
                        FROM
                            {1}
                        WHERE
                            {2}
                            AND systime && @{3}:: TSRANGE
                        ",
                                                    systime.Hash(),
                                                    table.TableName,
                                                    fields,
                                                    systime.Hash());

                    foreach (KeyValuePair<string, Column> x in parameters) {
                        AddParameter(cmd, x.Value, row);
                    }
                    AddParameter(cmd, systime, row);
                    res = ReadResultModifiedOrInconsitent(cmd);
                }
            return res;
        }

        public static Modified ReadResultModifiedOrInconsitent(NpgsqlCommand cmd) {
            using (var r = cmd.ExecuteReader()) {
                if (r.HasRows) {
                    r.Read();
                    return r.GetBoolean(0) ?
                           (r.GetBoolean(1) ? Modified.Equal : Modified.Inconsistent) :
                           Modified.Updated;
                } else {
                    return Modified.New;
                }
            }
        }

        public static Modified ReadResultModified(NpgsqlCommand cmd) {
            using (var r = cmd.ExecuteReader()) {
                if (r.HasRows) {
                    r.Read();
                    return r.GetBoolean(0) ?
                           Modified.Equal :
                           Modified.Updated;
                } else {
                    return Modified.New;
                }
            }
        }

        public static Modified CheckInsightDailyMatch(Insights table, JObject row) {
            return CheckLifetimeInsightMatch(table, row, new string[] { table.Source.Name.ToString() + "_id", "date_start", "date_end" });
        }

        public static void DeleteEdgeNotMatch(NpgsqlConnection connection, Edge table, IEnumerable<JObject> row, string systime, string fetch_time) {
            using (var cmd = connection.CreateCommand()) {
                var source = table.ColumnDefinition[table.Source.Name + "_id"];
                var id = table.ColumnDefinition["id"];
                var systime_f = table.ColumnDefinition[systime];

                cmd.CommandText = String.Format(@"
                        UPDATE
                            {0}
                        SET
                            {1} = {2} - @{3}:: TSRANGE
                        WHERE
                            {4} @> NOW()::TIMESTAMP without TIME zone
                            AND NOT (id = ANY (@{5}))
                            AND {6} = @{7}
                        ",
                                                table.TableName,
                                                systime_f.Name,
                                                systime_f.Name,
                                                systime_f.Hash(),
                                                systime_f.Name,
                                                id.Hash(),
                                                source.Name,
                                                source.Hash()
                                                );

                var ids = JValue.FromObject(row.Select(x => x["id"]));
                AddParameter(cmd, id.Hash().ToString(), id.Type + "[]", ids);
                AddParameter(cmd, source, row.First());
                AddParameter(cmd, systime_f, row.First());

                var updated = cmd.ExecuteNonQuery();
            }
        }

        public static bool CheckEdgeMatch(NpgsqlConnection connection, Edge table, JObject row) {
            using (var cmd = connection.CreateCommand()) {
                var source_id = table.Source.Name + "_id";
                var parameters = table.ColumnDefinition
                                     .Where(x => row[x.Key] != null && (x.Key == source_id || x.Key == "id"));
                var fields = parameters
                                 .Select(x => ColumnSqlEquals(x.Value))
                                 .Aggregate((x, y) => x + " AND " + y);

                cmd.CommandText = String.Format("SELECT {0} FROM {1} WHERE {2}", source_id, table.TableName, fields);

                foreach (KeyValuePair<string, Column> x in parameters) {
                    AddParameter(cmd, x.Value, row);
                }
                using (var r = cmd.ExecuteReader()) {
                    return r.HasRows;
                }
            }
        }

        public static string ColumnSqlEquals(Column x) {
            return String.Format(@"
                (
                    CASE WHEN {0} IS NOT NULL
                        OR @{1} IS NOT NULL THEN
                        COALESCE({2} = @{3}::{4}, FALSE)
                    ELSE
                        TRUE
                    END)
                ",
                                 x.Name,
                                 x.Hash().ToString(),
                                 QuoteReserved(x.Name),
                                 x.Hash(),
                                 x.Type
                                 );
        }

        public static Modified CheckLifetimeInsightMatch(Insights lifetime, JObject row, string[] keyColumns) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var parameters = lifetime.ColumnDefinition
                                         .Where(x => row[x.Key] != null && x.Key != "fetch_time" && x.Key != "systime");
                    var fields = parameters
                                     .Select(x => ColumnSqlEquals(x.Value))
                                     .Aggregate((x, y) => x + " AND " + y);

                    var idsPred = parameters
                                      .Where(x => row[x.Key] != null && keyColumns.Contains(x.Key))
                                      .Select(x => ColumnSqlEquals(x.Value))
                                      .Aggregate("True", (x, y) => x + " AND " + y);

                    cmd.CommandText = String.Format(@"
                        SELECT
                            ({0})
                        FROM
                            {1}
                        WHERE
                            systime @> NOW()::TIMESTAMP
                            AND {2}
                        ",
                                                    fields,
                                                    lifetime.TableName,
                                                    idsPred
                                                    );

                    foreach (KeyValuePair<string, Column> x in parameters) {
                        AddParameter(cmd, x.Value, row);
                    }
                    AddParameter(cmd, "new_fetch_time", "timestamp without time zone", row["fetch_time"]);
                    return ReadResultModified(cmd);
                }
        }

        public static DateTime? LastLifetimeDate(Insights table, JObject row) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var nominalKeyColumns = table.PrimaryKey.NominalColumns;
                    var whereClause = nominalKeyColumns.Aggregate("True", (acc, cur) => $"{acc} AND {cur.Name}=@{cur.Hash()}");
                    cmd.CommandText = String.Format(@"
                        SELECT
                            MAX(UPPER(systime))
                        FROM
                            {0}
                        WHERE
                            {1}
                        ",
                                                    table.TableName,
                                                    whereClause
                                                    );
                    foreach (var k in nominalKeyColumns) {
                        AddParameter(cmd, k, row);
                    }
                    using (var s = cmd.ExecuteReader()) {
                        if (s.Read()) {
                            if (s.IsDBNull(0)) {
                                return null;
                            }
                            return s.GetDateTime(0);
                        } else {
                            return null;
                        }
                    }
                }
        }

        public static DateTime? LastDailyDate(Insights daily, JObject row) {
            using (var connection = new NpgsqlConnection(ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    var nominalColumns = daily.PrimaryKey.NominalColumns;
                    var where_clause = nominalColumns.Aggregate("True", (acc, cur) => $"{acc} AND {cur.Name}=@{cur.Hash()}");
                    cmd.CommandText = String.Format(@"
                        SELECT
                            MAX(date_start)
                        FROM
                            {0}
                        WHERE
                            {1}
                        ",
                                                    daily.TableName,
                                                    where_clause);
                    foreach (var c in nominalColumns) {
                        AddParameter(cmd, c, row);
                    }
                    using (var s = cmd.ExecuteReader()) {
                        if (s.Read()) {
                            if (s.IsDBNull(0)) {
                                return null;
                            }
                            return s.GetDateTime(0);
                        } else {
                            return null;
                        }
                    }
                }
        }

        private static string[] INTEGER_METRICS = { "bigint", "integer" };

        public static bool DailyInsightsAreComplete(Insights lifetime, Insights daily, JObject row) {
            /**
               States whether daily feed has caught up to lifetime feed.
               Note: check can only be performed on metrics that are present both on lifetime and on daily feed.
             */// Current source id
            var id = lifetime.Source.ColumnDefinition["id"];
            var hid = id.Hash();

            var source_id = lifetime.Source.Name + "_id";
            var integerMetrics =
                daily.MetricColumns
                    .Where(dailyMetric => INTEGER_METRICS.Any(m => m == dailyMetric.Type))
                    .Where(dailyMetric => lifetime.MetricColumns.Any(lifetimeMetric => lifetimeMetric.Name == dailyMetric.Name));

            foreach (var metric in integerMetrics) {
                bool threshold;
                using (var connection = new NpgsqlConnection(ConnectionString()))
                    using (var cmd = connection.CreateCommand()) {
                        connection.Open();

                        cmd.CommandText = String.Format(@"
                        SELECT
                            COALESCE(lifetimeTotal.value * 0.9999, 0) <= COALESCE(dailyTotal.value, 0)
                        FROM (
                            SELECT
                                {0} AS value
                            FROM
                                {1}
                            WHERE
                                systime @> NOW() :: timestamp
                                and
                                {3} = @{4}) lifetimeTotal, (
                                SELECT
                                    SUM(value) AS value
                                FROM (
                                    SELECT
                                        {0} AS value
                                    FROM
                                        {2}
                                    WHERE
                                        systime @> NOW() :: timestamp
                                            and
                                        {3} = @{4}) d) dailyTotal
                        ",
                                                        metric.Name,
                                                        lifetime.TableName,
                                                        daily.TableName,
                                                        source_id,
                                                        hid
                                                        );
                        AddParameter(cmd, id, row);
                        threshold = (bool) cmd.ExecuteScalar();
                    }
                if (!threshold)
                    return false;
            }
            // If no metric was checked, we cannot assert the completness of daily insights
            return integerMetrics.Any();
        }

        public static void InsertInsights(Insights table, JObject oobj) {
            InsertRow(table.TableName, table.ColumnDefinition, oobj);
        }

        public static void InsertRow(NpgsqlConnection conn, Table table, JObject row) {
            InsertRow(conn, table.TableName, table.ColumnDefinition, row);
        }

        public static void InsertRow(Table table, JObject row) {
            InsertRow(table.TableName, table.ColumnDefinition, row);
        }

        public static void AddParameter(NpgsqlCommand cmd, Column field, JObject row) {
            if (!row.ContainsKey(field.ApiResponseName)) {
                Logger.Warning("Row {Row} does not cointain field {FieldName}", row.ToString(), field.ApiResponseName);
            }
            AddParameter(cmd, field.Hash().ToString(), field.Type, row[field.ApiResponseName]);
        }

        public static void AddParameter(NpgsqlCommand cmd, string hash, string type, JToken value) {
            switch (type) {
                case "bigint":
                    cmd.Parameters.AddWithValue(hash, (long) value);
                    break;
                case "bigint[]":
                    cmd.Parameters.AddWithValue(hash, value.ToArray().Select(x => (long) x).ToList());
                    break;
                case "text[]":
                    cmd.Parameters.AddWithValue(hash, value.ToArray().Select(x => (string) x).ToList());
                    break;
                case "integer":
                    cmd.Parameters.AddWithValue(hash, (int) value);
                    break;
                case "timestamp without time zone":
                    DateTime time;
                    if (value.Type == JTokenType.Integer) {
                        time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double) value);
                    } else {
                        time = DateTime.Parse(value.ToString());
                    }
                    cmd.Parameters.AddWithValue(hash, time);
                    break;
                case "jsonb":
                case "json":
                    cmd.Parameters.AddWithValue(hash, JsonConvert.SerializeObject(value));
                    break;
                case "bool":
                    cmd.Parameters.AddWithValue(hash, (bool) value);
                    break;
                case "text":
                    cmd.Parameters.AddWithValue(hash, (string) value);
                    break;
                case "double precision":
                    cmd.Parameters.AddWithValue(hash, (double) value);
                    break;
                case "daterange":
                    cmd.Parameters.AddWithValue(hash, (string) value);
                    break;
                case "tsrange":
                    cmd.Parameters.AddWithValue(hash, (string) value);
                    break;
                default:
                    throw new Exception("Undefined field type :" + type);
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

        public static void InsertRow(string name, Dictionary<string, Column> columns, JObject row) {
            using (var connection = new NpgsqlConnection(ConnectionString())) {
                connection.Open();
                InsertRow(connection, name, columns, row);
            }
        }

        public static void InsertRow(NpgsqlConnection connection, string name, Dictionary<string, Column> columns, JObject row) {
            using (var cmd = connection.CreateCommand()) {
                var fields = columns
                                 .Where(x => row[x.Key] != null).Select(x => QuoteReserved(x.Value.Name))
                                 .Aggregate((x, y) => x + "," + y);
                var value_ref = columns
                                    .Where(x => row[x.Key] != null).Select(x => "@" + x.Value.Hash() + " :: " + x.Value.Type)
                                    .Aggregate((x, y) => x + "," + y);

                cmd.CommandText = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", name, fields, value_ref);
                foreach (KeyValuePair<string, Column> x in columns.Where(x => row[x.Key] != null)) {
                    AddParameter(cmd, x.Value, row);
                }

                cmd.ExecuteNonQuery();
            }
        }
    }
}
