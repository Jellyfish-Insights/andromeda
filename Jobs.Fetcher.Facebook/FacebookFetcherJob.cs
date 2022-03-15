using System;
using System.Collections.Generic;
using Andromeda.Common.Jobs;
using Andromeda.Common.Logging;
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
        protected string PageId;

        public FacebookFetcher(Schema schema, string edgeKey, string subEdgeKey, Fetcher fetcher, CustomEdge customEdge, string pageId = "") {
            Schema = schema;
            EdgeKey = edgeKey;
            SubEdgeKey = subEdgeKey;
            Fetcher = fetcher;
            CustomEdge = customEdge;
            PageId = pageId;
        }

        public override List<string> Dependencies() {
            var deps = new List<string>() {};
            if (Schema.Name == "adaccount") {
                var dep = AdAccountDependencies[EdgeKey];
                if (dep != null) {
                    deps.Add(IdOf(Schema.Name, dep, PageId));
                }
            }
            return deps;
        }

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        public override string Id() {
            return IdOf(Schema.Name, EdgeKey, PageId);
        }

        public static string IdOf(string schema, string table, string pageId) {
            if (table == null) {
                return $"{typeof(FacebookFetcher).Namespace}.{schema}.{pageId}";
            } else {
                return $"{typeof(FacebookFetcher).Namespace}.{schema}.{table}.{pageId}";
            }
        }

        public override void Run() {
            if (EdgeKey == null) {
                // the root level of a schema is fetched from credential files
                var staticRow = new JObject();
                try {
                    staticRow = SchemaLoader.ParseCredentials<JObject>(Schema.Name);
                } catch (Exception e) {
                    Logger.Error(e, $"Couldn't parse schema {Schema.Name}");
                    throw;
                }

                FetchDetailsOfRow(Schema, staticRow);
            } else {
                // non root level are fetched from the API
                var table = Schema.Edges[EdgeKey];
                try {
                    var tableRows = Fetcher.FetchAllEntitiesOnTable(table, Logger, Configuration.MaxEntities).AsParallel().WithDegreeOfParallelism(Schema.Threads);

                    tableRows.ForAll(row => FetchDetailsOfRow(table, row));
                } catch (Exception) {
                    Logger.Error($"Couldn't fetch entities from {Schema.Name}");
                }
            }
        }

        private void FetchDetailsOfRow(Table table, JObject row) {
            /**
               The details of a row include: metrics, insights, and nested edges.
             */
            if (Configuration.IgnoreEdges) {
                return;
            }

            Logger.Information($"Fetching details of ({table.Name},{row ? ["id"]})");
            try {
                if (EdgeKey != null) {
                    foreach (var subEdge in table.Edges) {
                        Logger.Debug($"Fetching subEdge {subEdge.Value.Name}");
                        Fetcher.FetchChildrenOnEdge(subEdge.Value, row);
                    }
                }
                Fetcher.FetchInsights(table, row);
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
