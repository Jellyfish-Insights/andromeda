using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ApplicationModels.Helpers;
using Common;

namespace ConsoleApp.Commands {

    public static class ApplicationResetCommand {

        private static bool IsTableResetable(string tableName) {
            return tableName.StartsWith("Source") || tableName.StartsWith("Generated");
        }

        public static List<(string TableName, IEntityType Entity)> GetResetableTables() {
            return ApplicationIntrospection.GetAllEntities()
                       .Select(x => (TableName: x.Relational().TableName, Entity: x))
                       .Where(x => IsTableResetable(x.TableName)).ToList();
        }

        public static void ResetTables(List<(string schema, string table, List<string> keys)> tables) {
            var body = tables.Select(t => new List<string>() {
                $"{t.schema}.{t.table}",
                TableOperations.DeleteFromTable(YearDatabase.BusinessDatabase, t.schema, t.table).ToString()
            });
            var header = new List<string>() { "Table", "Deleted Rows" };
            var report = body.OrderBy(x => x[0]).Prepend(header);
            Common.Report.ConsoleReport.WriteTable(report.ToList());
        }
    }
}
