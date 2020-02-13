using System.Collections.Generic;
using System.Linq;
using Andromeda.Common;
using Andromeda.Common.Report;

namespace Andromeda.Commands {

    public enum ContinuityProperty {
        NotEmpty,
        NoOverlapingInterval,
        UniqueCurrentValue,
    }

    public class ContinuityReportCommand {

        public class TableProperty {

            public ContinuityProperty Name;
            public bool? Value;

            public TableProperty(ContinuityProperty name, bool? value) {
                Name = name;
                Value = value;
            }
        }

        public static bool? AssertEmptyResult(YearDatabase database, string commandText) {
            using (var connection = ConnectionStringHelper.GetDbConnection(database)) {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                    SELECT
                        COUNT(*)
                    FROM
                        ({commandText}) AS t
                ";
                using (var reader = cmd.ExecuteReader()) {
                    reader.Read();
                    return reader.GetInt64(0) == 0;
                }
            }
        }

        public static string GetKeyStatement(List<string> keyColumnNames) {
            return string.Join(",", keyColumnNames.Select(x => $"\"{x}\""));
        }

        public static string GetKeyWithoutTimeColumn(List<string> keyColumnNames) {
            return string.Join(",", keyColumnNames.Where(x => x != "ValidityStart").Select(x => $"\"{x}\""));
        }

        public static TableProperty CheckNoOverlapingIntervals(YearDatabase database, string schemaName, string tableName, List<string> keyColumnNames) {
            var fullKey = GetKeyStatement(keyColumnNames);
            var keyNotTime = GetKeyWithoutTimeColumn(keyColumnNames);
            var commandText = $@"
                SELECT
                *
                FROM (
                SELECT
                    {fullKey},
                    ""ValidityStart"" AS validityStartValue,
                    LAG(""ValidityEnd"") OVER (PARTITION BY {keyNotTime} ORDER BY ""ValidityStart"" ASC) AS previousValidityEnd,
                    row_number() OVER (PARTITION BY {keyNotTime} ORDER BY ""ValidityStart"" ASC) AS valueIndex
                FROM {schemaName}.""{tableName}"" ) AS w
                WHERE w.valueIndex > 1
                AND validityStartValue <> previousValidityEnd";

            return new TableProperty(ContinuityProperty.NoOverlapingInterval, AssertEmptyResult(database, commandText));
        }

        public static TableProperty CheckUniqueCurrentValue(YearDatabase database, string schemaName, string tableName, List<string> keyColumnNames) {
            var fullKey = string.Join(",", keyColumnNames.Select(x => $"\"{x}\""));
            var keyNotTime = string.Join(",", keyColumnNames.Where(x => x != "ValidityStart").Select(x => $"\"{x}\""));
            var commandText = $@"
                SELECT
                    *
                FROM
                    (SELECT
                    {keyNotTime},
                    SUM(CASE WHEN NOW() >= ""ValidityStart"" AND NOW() < ""ValidityEnd"" THEN 1 ELSE 0 END) AS current_count
                    FROM
                        {schemaName}. ""{tableName}"" AS w
                    GROUP BY
                        {keyNotTime}) AS a
                WHERE a.current_count <> 1
            ";
            return new TableProperty(ContinuityProperty.UniqueCurrentValue, AssertEmptyResult(database, commandText));
        }

        public static TableProperty CheckNonEmpty(YearDatabase database, string schemaName, string tableName) {
            using (var connection = ConnectionStringHelper.GetDbConnection(database)) {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
                    SELECT
                        COUNT(*)
                    FROM
                        {schemaName}.""{tableName}""";
                using (var reader = cmd.ExecuteReader()) {
                    reader.Read();
                    return new TableProperty(ContinuityProperty.NotEmpty, reader.GetInt64(0) > 0);
                }
            }
        }

        public static Dictionary<ContinuityProperty, bool?> ReportOnTable(YearDatabase database, string schema, string table, List<string> keys) {
            return (new List<TableProperty>() {
                CheckNonEmpty(database, schema, table),
                CheckNoOverlapingIntervals(database, schema, table, keys),
                CheckUniqueCurrentValue(database, schema, table, keys),
            }).ToDictionary(x => x.Name, x => x.Value);
        }

        public static List<List<string>> GetDataLakeReportTable(List<(string schema, string table, List<string> keys)> tables) {
            var reports = tables.ToDictionary(
                x => $"{x.schema}.{x.table}",
                x => ContinuityReportCommand.ReportOnTable(YearDatabase.DataLakeDatabase, x.schema, x.table, x.keys));

            // find the super-set of all experiments returned for every table
            var columns = reports.Select(x => x.Value.Keys.AsEnumerable())
                              .Aggregate((acc, x) => acc.Union(x))
                              .Distinct().ToList();

            var header = columns.Select(x => x.ToString()).Prepend("Table").ToList();
            var body = reports.OrderBy(x => x.Key).Select(
                data => columns.Select(property => data.Value[property].ToString()).Prepend(data.Key).ToList()
                );
            return (body.Prepend(header).ToList());
        }

        public static void Report(List<(string schema, string table, List<string> keys)> tables) {
            var reports = GetDataLakeReportTable(tables);
            ConsoleReport.WriteTable(reports);
        }
    }
}
