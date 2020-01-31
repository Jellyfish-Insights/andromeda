using System.Linq;
using System.Collections.Generic;

namespace Jobs.Fetcher.Facebook {

    public static class DatabaseInitializer {

        public static void Init() {
            foreach (var schemaName in SchemaLoader.SchemaList()) {
                var schema = SchemaLoader.LoadSchema(schemaName);

                Init(schema, null, null);
            }
        }

        static void Init(Schema schema, string tableKey, string edge) {
            DatabaseManager.CreateSchema();
            if (tableKey != null) {
                InitTable(schema, tableKey, edge);
            } else {
                foreach (var v in schema.Edges.ToList()) {
                    InitTable(schema, v.Key, null);
                }
                InitInsights<Insights>(schema.Insights);
            }
        }

        static void InitTable(Schema schema, string tableKey, string edge) {
            var table = schema.Edges[tableKey];
            DatabaseManager.CreateTable(table);
            if (edge != null) {
                InitEdge(table, edge);
            } else {
                foreach (var v in table.Edges.ToList()) {
                    InitEdge(table, v.Key);
                }
                InitEdge(table, "insights");
                InitEdge(table, "instagram_insights");
            }
        }

        static void InitEdge(Table table, string edge) {
            if (edge == "insights") {
                InitInsights<Insights>(table.Insights);
            } else if (edge == "instagram_insights") {
                InitInsights<InstagramInsights>(table.InstagramInsights);
            } else {
                DatabaseManager.CreateTable(table.Edges[edge]);
            }
        }

        static void InitInsights<T>(Dictionary<string, T> insights) where T : Insights  {
            foreach (var i in insights.ToList()) {
                DatabaseManager.CreateTable(i.Value);
            }
        }
    }
}
