using System.Linq;
using System.Collections.Generic;

namespace Jobs.Fetcher.Facebook {

    public static class DatabaseInitializer {

        public static void Init(bool force_update = false) {
            foreach (var schemaName in SchemaLoader.SchemaList()) {
                var schema = SchemaLoader.LoadSchema(schemaName);
                Init(schema, null, null, force_update);
            }
        }

        public static void Init(List<string> schemaList) {
            foreach (var schemaName in schemaList) {
                var schema = SchemaLoader.LoadSchema(schemaName);
                Init(schema, null, null, false);
            }
        }

        static void Init(Schema schema, string tableKey, string edge, bool force_update) {
            DatabaseManager.CreateSchema();
            if (tableKey != null) {
                InitTable(schema, tableKey, edge, force_update);
            } else {
                foreach (var v in schema.Edges.ToList()) {
                    InitTable(schema, v.Key, null, force_update);
                }
                InitInsights<Insights>(schema.Insights, force_update);
            }
        }

        static void InitTable(Schema schema, string tableKey, string edge, bool force_update) {
            var table = schema.Edges[tableKey];
            DatabaseManager.CreateTable(table, force_update);
            if (edge != null) {
                InitEdge(table, edge, force_update);
            } else {
                foreach (var v in table.Edges.ToList()) {
                    InitEdge(table, v.Key, force_update);
                }
                InitEdge(table, "insights", force_update);
                InitEdge(table, "instagram_insights", force_update);
            }
        }

        static void InitEdge(Table table, string edge, bool force_update) {
            if (edge == "insights") {
                InitInsights<Insights>(table.Insights, force_update);
            } else if (edge == "instagram_insights") {
                InitInsights<InstagramInsights>(table.InstagramInsights, force_update);
            } else {
                DatabaseManager.CreateTable(table.Edges[edge], force_update);
            }
        }

        static void InitInsights<T>(Dictionary<string, T> insights, bool force_update) where T : Insights  {
            foreach (var i in insights.ToList()) {
                DatabaseManager.CreateTable(i.Value, force_update);
            }
        }
    }
}
