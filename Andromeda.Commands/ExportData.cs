using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Andromeda.Common;
using Npgsql;

namespace Andromeda.Commands {
    public static class ExportData {

        private static string GetTableHeader(NpgsqlDataReader reader) {
            string header = "";
            for (int i = 0; i < reader.FieldCount; i++)
            {
                header += reader.GetName(i) + "; ";
            }
            return header;
        }

        private static List<string> GetTableContent(NpgsqlDataReader reader) {
            var ListOfContent = new List<string>();
            string content = "";

            while (reader.Read())
            {
                for (int j = 0; j < reader.FieldCount; j++)
                {
                    content += reader[j] + "; ";
                }
                ListOfContent.Add(content);
                content = "";
            }
            return ListOfContent;
        }

        public static bool SaveOnCSV(NpgsqlDataReader reader, string schema, string table, string path) {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{table}");
                sb.AppendLine(GetTableHeader(reader));
                foreach (var line in GetTableContent(reader))
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
            catch (Exception)
            {
                Console.WriteLine($"**Failing** to export {schema}.{table} to CSV");
                return false;
            }
        }

        public static bool SaveOnJSON(NpgsqlDataReader reader, string schema, string table, string path) {
            Console.WriteLine("**Failing** to export to JSON");
            return false;
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
                if (!Directory.Exists("./metrics")){
                    Directory.CreateDirectory("./metrics");
                }
                if (!Directory.Exists($"./metrics/{path}")){
                    Directory.CreateDirectory($"./metrics/{path}");
                }
                Console.WriteLine($"Created folder '{path}'");
            } catch {
                Console.WriteLine("Failed to create directory.");
                Environment.Exit(1);
            }
        }

        public static void QueryMetrics(string fileType, string selectedPlatform, int limit) {
            var path = $"export_data_{DateTime.Now.ToString("MMddyyyyHHmmss")}";
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
            Console.WriteLine(exportStatus ? "\nExporting done!" : "\nExporting **failed**!");
        }
    }
}
