using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Andromeda.Common;
using Npgsql;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Andromeda.Commands {
    public static class ExportData {

        private static string GetTableHeaderCSV(NpgsqlDataReader reader) {
            string header = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                header += reader.GetName(i) + ";";
            }
            return header;
        }

        private static List<string> GetTableContentCSV(NpgsqlDataReader reader) {
            var ListOfContent = new List<string>();
            string content = "";

            while (reader.Read())
            {
                for (int j = 0; j < reader.FieldCount; j++)
                {
                    content += reader[j] + ";";
                }
                ListOfContent.Add(content);
                content = "";
            }
            return ListOfContent;
        }

        public static bool SaveOnCSV(NpgsqlDataReader reader, string schema, string table, string path) {
            try {
                var sb = new StringBuilder();
                sb.AppendLine($"{table}");
                sb.AppendLine(GetTableHeaderCSV(reader));
                foreach (var line in GetTableContentCSV(reader))
                {
                    sb.AppendLine(line);
                }
                sb.AppendLine("\n\n");
                TextWriter sw = new StreamWriter($"./metrics/{path}/{schema}.csv", true);
                sw.Write(sb.ToString());
                sw.Close();
                Console.WriteLine($"Success to export {schema}.{table} to CSV");
                return true;
            }
            catch (Exception) {
                Console.WriteLine($"**Failing** to export {schema}.{table} to CSV");
                return false;
            }
        }

        public static bool SaveOnJSON(NpgsqlDataReader reader, string schema, string tableName, string path) {
            try {
                JObject table = new JObject();
                List<JObject> metrics = new List<JObject>();

                while (reader.Read()) {
                    JObject metric = new JObject();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        metric[$"{reader.GetName(i)}"] = reader[i].ToString();
                    }
                    metrics.Add(metric);
                }

                table[tableName] = new JArray() { metrics };
                using (StreamWriter file = File.CreateText($"./metrics/{path}/{schema}_{tableName}.json")) {
                    using (JsonTextWriter writer = new JsonTextWriter(file)) {
                        table.WriteTo(writer);
                    }
                }

                Console.WriteLine($"Success to export {schema}.{tableName} to JSON");
                return true;
            } catch (Exception) {
                Console.WriteLine($"**Failing** to export {schema}.{tableName} to JSON");
                return false;
            }
        }

        public static List<Tuple<string, string>> GetTablesNameAndSchemas(string selectedPlatform) {
            List<Tuple<string, string>> schemaAndTable = new List<Tuple<string, string>>();
            using (var connection = ConnectionStringHelper.GetDbConnection("DataLakeDatabase"))
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                    SELECT
                        table_schema,
	                    table_name
                    FROM
                        information_schema.columns
                    WHERE
                        table_schema not in ('information_schema', 'pg_catalog', 'logging', 'public')
                        and table_schema like '{selectedPlatform}%'
                    GROUP BY
                        table_schema,
                        table_name
                    ORDER BY
                        table_schema,
                        table_name
                    ";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        schemaAndTable.Add(Tuple.Create(reader[0].ToString(), reader[1].ToString()));
                    }
                }
            }
            return schemaAndTable;
        }

        private static void CreateDirectory(string path) {
            try {
                if (!Directory.Exists("./metrics")) {
                    Directory.CreateDirectory("./metrics");
                }
                if (!Directory.Exists($"./metrics/{path}")) {
                    Directory.CreateDirectory($"./metrics/{path}");
                }
                Console.WriteLine($"Created folder '{path}'");
            } catch {
                Console.WriteLine("Failed to create directory.");
                Environment.Exit(1);
            }
        }

        public static void QueryMetrics(string fileType, string selectedPlatform, int limit) {
            var path = $"export_data_{fileType}_{limit}_{DateTime.Now.ToString("MMddyyyyHHmmss")}";
            CreateDirectory(path);
            Console.WriteLine($"Exporting into {fileType} limited by {limit}\n");
            var exportStatus = true;

            foreach (var(schema, table) in GetTablesNameAndSchemas(selectedPlatform)) {
                using (var connection = ConnectionStringHelper.GetDbConnection("DataLakeDatabase")) {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = $@"
                    SELECT
                        *
                    FROM
                        ""{schema}"".""{table}""
                    LIMIT
                        {limit}
                    ";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (fileType == "csv")
                            exportStatus &= SaveOnCSV(reader, schema, table, path);
                        else
                            exportStatus &= SaveOnJSON(reader, schema, table, path);
                    }
                }
            }
            if (!exportStatus) {
                if (Directory.Exists($"./metrics/{path}")) {
                    Directory.Delete($"./metrics/{path}", true);
                    Console.WriteLine($"\nDeleted folder '{path}'");
                }
                Console.WriteLine("Exporting **failed**!");
                Environment.Exit(1);
            }
            Console.WriteLine("\nExporting done!");
        }
    }
}
