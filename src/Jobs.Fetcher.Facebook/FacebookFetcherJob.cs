using System;
using System.Collections.Generic;
using Common.Jobs;
using Common.Logging;
using DataLakeModels;
using Serilog.Core;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Jobs.Fetcher.Facebook {

    using CustomEdge = Dictionary<(string SchemaName, string TableName, string CustomEdgeName), Action<Logger, JObject>>;

    public class FacebookFetcher : AbstractJob {

        protected static readonly Dictionary<string, string> AdAccountDependencies = new Dictionary<string, string> {
            { "ads", "adsets" },
            { "adsets", "campaigns" },
            { "campaigns", "customaudiences" },
            { "customaudiences", null },
        };

        protected Schema Schema;

        /**
           An edge of the root level schema
         */
        protected string EdgeKey = null;

        /**
           On this context, a sub edge is the edge of an edge.
         */
        protected string SubEdgeKey = null;
        protected Fetcher Fetcher;
        protected CustomEdge CustomEdge;

        public FacebookFetcher(Schema schema, string edgeKey, string subEdgeKey, Fetcher fetcher, CustomEdge customEdge) {
            Schema = schema;
            EdgeKey = edgeKey;
            SubEdgeKey = subEdgeKey;
            Fetcher = fetcher;
            CustomEdge = customEdge;
        }

        public override List<string> Dependencies() {
            var deps = new List<string>() {};
            if (Schema.Name == "adaccount") {
                var dep = AdAccountDependencies[EdgeKey];
                if (dep != null) {
                    deps.Add(IdOf(Schema.Name, dep));
                }
            }
            return deps;
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override string Id() {
            return IdOf(Schema.Name, EdgeKey);
        }

        public static string IdOf(string schema, string table) {
            if (table == null) {
                return $"{typeof(FacebookFetcher).Namespace}.{schema}";
            } else {
                return $"{typeof(FacebookFetcher).Namespace}.{schema}.{table}";
            }
        }

        public override void Run() {
            if (EdgeKey == null) {
                // the root level of a schema is fetched from credential files
                var staticRow = SchemaLoader.ParseCredentials<JObject>(Schema.Name);
                FetchDetailsOfRow(Schema, staticRow);
            } else {
                // non root level are fetched from the API
                var table = Schema.Edges[EdgeKey];

                var tableRows = Fetcher.FetchAllEntitiesOnTable(table, Logger, Configuration.MaxEntities).AsParallel().WithDegreeOfParallelism(Schema.Threads);
                tableRows.ForAll(row => FetchDetailsOfRow(table, row));
            }
        }

        private void FetchDetailsOfRow(Table table, JObject row) {
            /**
               The details of a row include: metrics, insights, and nested edges.
             */
            if (Configuration.IgnoreEdges) {
                return;
            }

            GetLogger().Information($"Fetching details of ({table.Name},{row ? ["id"]})");
            try {
                if (SubEdgeKey != null) {
                    // DEPRECATED: case in which the user informed a single edge to be executed
                    if (SubEdgeKey == "insights") {
                        Fetcher.FetchInsights(table, row);
                    }
                    var edge = table.Edges.GetValueOrDefault(SubEdgeKey, null);
                    var key = (SchemaName : Schema.Name, TableName : table.Name, CustomEdgeName : SubEdgeKey);
                    if (CustomEdge.ContainsKey(key)) {
                        CustomEdge[key](Logger, row);
                    }
                    if (edge != null) {
                        Fetcher.FetchChildrenOnEdge(edge, row);
                    }
                } else {
                    if (EdgeKey != null) {
                        foreach (var subEdge in table.Edges) {
                            Fetcher.FetchChildrenOnEdge(subEdge.Value, row);
                        }
                        foreach (var edge in CustomEdge.Where(x => x.Key.SchemaName == Schema.Name && x.Key.TableName == table.Name)) {
                            edge.Value(Logger, row);
                        }
                    }
                    Fetcher.FetchInsights(table, row);
                }
            } catch (AggregateException e) {
                foreach (var ie in e.InnerExceptions) {
                    if (ie is FacebookApiUnreachable) {
                        throw ie;
                    }
                }
                Logger.Warning(e, $"Failure while fetching details of ({table.TableName}, {row.ToString()}");
            } catch (Exception e) {
                Logger.Warning(e, $"Failure while fetching details of ({table.TableName}, {row.ToString()}");
            }
        }
    }
}
