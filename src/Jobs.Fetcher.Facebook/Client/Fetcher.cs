using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLakeModels.Models;
using Newtonsoft.Json.Linq;
<<<<<<< HEAD
using Common.Logging;
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
using System.Collections.Immutable;
using Serilog;
using Serilog.Core;

namespace Jobs.Fetcher.Facebook {

    public class Fetcher {

        ApiManager ApiMan;

        int PageSize;

        private ILogger Logger { get => Log.ForContext<Fetcher>(); }

        public Fetcher(ApiManager apiMan, int pageSize) {
            ApiMan = apiMan;
            PageSize = pageSize;
        }

        public void GetRoot(Schema schema, string table, JToken entityId, Logger jobLogger) {
            var videos = schema.Edges[table];
            var o = FetchRoot(videos, entityId.ToObject<string>(), null, videos.Columns.Select(x => x.Value).ToList()).Result;
            AddSystime(videos, ref o);
            switch (DatabaseManager.CheckEntityModified(videos, o)) {
                case Modified.New:
                    DatabaseManager.InsertRow(videos, o);
                    break;
                case Modified.Inconsistent:
                    jobLogger.Warning("GetRoot: Inconsistent value for entity: {TableName}, {Id}, {Value}", videos.TableName, o["id"].ToString(), o.ToString());
                    break;
                case Modified.Equal:
                    break;
                case Modified.Updated:
                    DatabaseManager.VersionEntityModified(videos, o, "id", "systime");
                    DatabaseManager.InsertRow(videos, o);
                    break;
            }
            foreach (var subEdge in videos.Edges) {
                FetchChildrenOnEdge(subEdge.Value, o);
            }
            FetchInsights(videos, o);
        }

        private async Task<JObject> FetchRoot(Table root, string rowId, string edge, List<Column> fields) {
            var url_params = new Dictionary<string, string>();
            url_params.Add("fields", fields.Select(x => x.ApiName()).Aggregate((x, y) => x + ',' + y));
            var url = ApiMan.EndPoint(rowId, edge, url_params);
            var stringTask = ApiMan.CachedRequest(root.TableName, url);
            return await stringTask;
        }

        private async Task<JObject> FetchVideoId(Table root, string node, string edge, string ordering = null) {
            var url_params = new Dictionary<string, string>();
            url_params.Add("fields", "id, created_time, updated_time");
            url_params.Add("limit", PageSize.ToString());
            var url = ApiMan.EndPoint(node, edge, url_params);
            var stringTask = ApiMan.CachedRequest(root.TableName, url);
            return await stringTask;
        }

        private async Task<JObject> FetchEndpoint(Table root, string node, string edge, List<Column> fields, string ordering = null) {
            var url_params = new Dictionary<string, string>();
            url_params.Add("fields", fields.Select(x => x.ApiName()).Aggregate((x, y) => x + ',' + y));
            url_params.Add("limit", PageSize.ToString());
            if (ordering != null && ordering != "unordered") {
                if (ordering == "asc")
                    url_params.Add("order", "chronological");
                if (ordering == "desc")
                    url_params.Add("order", "reverse_chronological");
            }
            var url = ApiMan.EndPoint(node, edge, url_params);
            var stringTask = ApiMan.CachedRequest(root.TableName, url);
            return await stringTask;
        }

        public static Int32 UnixTimeStampUTC(DateTime currentTime, bool init) {
            // It is assumed that the parameter currentTime is already UTC
            Int32 unixTimeStamp;
            // either the first or the last second of the day
            DateTime zuluTime = init ? (currentTime - currentTime.TimeOfDay) : (currentTime - currentTime.TimeOfDay + new TimeSpan(0, 23, 59, 59));
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            unixTimeStamp = (Int32) (zuluTime.Subtract(unixEpoch)).TotalSeconds;
            return unixTimeStamp;
        }

        private JObject FetchInsights(string node, Insights edge, Range<DateTime> range = null) {
            var fields = edge.Columns.Select(x => x.Value).ToList();
            var metrics = edge.Metrics.Select(x => x.Value).ToList();
            var url_params = new Dictionary<string, string>();
            if (fields != null && fields.Any())
                url_params.Add("fields", fields.Select(x => x.Name).Aggregate((x, y) => x + ',' + y));
            if (!edge.Transposed) {
                if (edge.Granularity == "day") {
                    url_params.Add("time_increment", "1");
                }
                if (range != null) {
                    var rjson = new JObject();
                    rjson.Add("since", FormatDate(range.Minimum));
                    rjson.Add("until", FormatDate(range.Maximum));
                    url_params.Add("time_range",
                                   rjson.ToString());
                }
            } else {
                url_params.Add("metric", metrics.Select(x => x.Name).Aggregate((x, y) => x + ',' + y));
                url_params.Add("period", edge.Granularity);

                if (range != null && edge.Granularity != "lifetime") {
                    url_params.Add("since", UnixTimeStampUTC(range.Minimum, true).ToString());
                    url_params.Add("until", UnixTimeStampUTC(range.Maximum, false).ToString());
                }
            }
            var url = ApiMan.EndPoint(node, edge.Name, url_params);
            var stringTask = ApiMan.CachedRequest(edge.TableName, url);
            stringTask.Wait();
            return stringTask.Result;
        }

        private IEnumerable<JObject> PaginateEndpoint(Table table, JObject source, int max_iters) {
            List<JObject> result = new List<JObject>();
            foreach (JObject o in source.SelectToken("data").ToList()) {
                o["fetch_time"] = source["fetch_time"];
                yield return o;
            }
            if (source["paging"] != null) {
                var paging = source["paging"];
                var next = paging ? ["next"];
                int iter = 0;
                while (next != null && iter < max_iters) {
                    JObject page;
                    try {
                        page = ApiMan.CachedRequest(table.TableName, next.ToString()).Result;
                    } catch (FacebookApiException) {
                        yield break;
                    } catch (AggregateException e) {
                        foreach (var ie in e.InnerExceptions) {
                            if (!(ie is FacebookApiException)) {
                                throw e;
                            }
                        }
                        yield break;
                    }
                    foreach (JObject o in page.SelectToken("data").ToList()) {
                        o["fetch_time"] = page["fetch_time"];
                        yield return o;
                    }
                    paging = page["paging"];
                    next = paging ? ["next"];
                    iter++;
                }
            }
        }

        private IEnumerable<String> GetEntitiesId(Table table, int maxEntities, int max_iters = Int32.MaxValue) {
            /* This function will return all entities ID.
             * A limit is set on for each call to API, so even if we have 100 entities, each time we fetch the limit
             * and paginate to next ids list.
             */JObject first_page;
            try {
                first_page = FetchVideoId(table, ApiMan.Secret.Id, table.Name).Result;
            } catch (FacebookApiException) {
                yield break;
            }

            var entitiesId = PaginateEndpoint(table, first_page, max_iters).Select(x => x["id"].ToString());

            int entitiesFetched = 0;
            foreach (var id in entitiesId) {
                if (maxEntities > 0 && maxEntities <= entitiesFetched) {
                    yield break;
                }
                yield return id;
                entitiesFetched++;
            }
        }

        private JObject FetchDataById(String id, Table table, Logger jobLogger) {
            var data = FetchRoot(table, id, null, table.Columns.Select(x => x.Value).ToList()).Result;

            if (table.Required.All(x => data[x] != null)) {
                AddSystime(table, ref data);
                switch (DatabaseManager.CheckEntityModified(table, data)) {
                    case Modified.New :
                        DatabaseManager.InsertRow(table, data);
                        break;
                    case Modified.Inconsistent:
                        jobLogger.Warning("ListAll: Inconsistent value for entity: {TableName}, {Id}, {Value}", table.TableName, data["id"].ToString(), data.ToString());
                        break;
                    case Modified.Equal:
                        break;
                    case Modified.Updated:
                        DatabaseManager.VersionEntityModified(table, data, "id", "systime");
                        DatabaseManager.InsertRow(table, data);
                        break;
                }
                Logger.Information($"Fetched ({table.TableName},{data ? ["id"]})");
                return data;
            }
            return null;
        }

        public IEnumerable<JObject> FetchAllEntitiesOnTable(Table table, Logger jobLogger, int maxEntities, int max_iters = Int32.MaxValue) {
<<<<<<< HEAD
            /**
               On this context, an entity is a social media artifact such as page, post, image, video, etc...
             */
            Logger.Information($"Fetching table {table.TableName}");

            if (table.TableName == "videos") {
                // This is a improvement to Facebook Fetch.
                // In this new feature first we fetch the ID for each videos and later we Fetch the information for these videos
                var entititesId = GetEntitiesId(table, maxEntities, max_iters).ToList();

                int entitiesFetched = 0;
                foreach (var id in entititesId) {
                    if (maxEntities > 0 && maxEntities <= entitiesFetched) {
                        yield break;
                    }
                    // Fetch Data for each ID and insert on Database
                    var o = FetchDataById(id, table, jobLogger);

                    if (o != null) {
                        yield return o;
                        entitiesFetched++;
                    }
                }
            } else {
                // In this way we're Fetching everything at once. The problem here is that if a endpoint has a lot data it will lock the process.
                // TODO: Implement FetchDataById for others tables such as page, post, image, etc...
                JObject tip;
                try {
                    tip = FetchEndpoint(table, ApiMan.Secret.Id, table.Name, table.Columns.Select(x => x.Value).ToList()).Result;
                } catch (FacebookApiException) {
                    yield break;
                }
                var result = PaginateEndpoint(table, tip, max_iters);
                int entitiesFetched = 0;
                foreach (JObject row in result) {
                    if (maxEntities > 0 && maxEntities <= entitiesFetched) {
                        yield break;
                    }
                    var o = row;
                    if (table.Required.All(x => o[x] != null)) {
                        AddSystime(table, ref o);
                        switch (DatabaseManager.CheckEntityModified(table, o)) {
                            case Modified.New:
                                DatabaseManager.InsertRow(table, o);
                                break;
                            case Modified.Inconsistent:
                                jobLogger.Warning("ListAll: Inconsistent value for entity: {TableName}, {Id}, {Value}", table.TableName, o["id"].ToString(), o.ToString());
                                break;
                            case Modified.Equal:
                                break;
                            case Modified.Updated:
                                DatabaseManager.VersionEntityModified(table, o, "id", "systime");
                                DatabaseManager.InsertRow(table, o);
                                break;
                        }
                        Logger.Information($"Fetched ({table.TableName},{o ? ["id"]})");
                        yield return o;
                        entitiesFetched++;
                    }
=======
            JObject entities;
            int entitiesFetched = 0;
            Logger.Information($"Fetching table {table.TableName}");

            foreach (var id in GetEntitiesId(table, maxEntities, max_iters).ToList()) {
                if (maxEntities > 0 && maxEntities <= entitiesFetched) {
                    yield break;
                }

                try {
                    entities = FetchDataById(id, table, jobLogger);
                } catch (Exception) {
                    Logger.Warning($"Fetching {table.Name} {id} failed.");
                    continue;
                }

                if (entities != null) {
                    yield return entities;
                    entitiesFetched++;
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                }
            }
        }

        private static string FormatDate(DateTime date) {
            return date.ToString("yyyy-MM-dd");
        }

        private static string FormatTime(DateTime date) {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static void AddSystime(Table table, ref JObject obj) {
            if (table.Syscreate != null) {
                JToken value;
                if (table.Sysupdate != null) {
                    value = obj[table.Sysupdate];
                } else {
                    value = obj[table.Syscreate];
                }
                DateTime time;
                if (value.Type == JTokenType.Integer) {
                    time = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double) value);
                } else {
                    time = DateTime.Parse(value.ToString());
                }

                obj.Add("systime", "['" + FormatTime(time) + "',)");
            } else {
                obj.Add("systime", "[" + FormatTime(obj["fetch_time"].ToObject<DateTime>()) + ",)");
            }
        }

        public void FetchChildrenOnEdge(Edge edge, JObject node, int max_iters = Int32.MaxValue) {
            /**
               Fetch children on the `edge` api api path of the social media artifact `node`.
             */
            JObject tip;
            try {
                tip = FetchEndpoint(edge, node["id"].ToString(), edge.Name, edge.Columns.Select(x => x.Value).ToList(), edge.Ordering).Result;
            } catch (FacebookApiException) {
                return;
            }
            var result = PaginateEndpoint(edge, tip, max_iters);
            var result2 = new List<JObject>();
            DatabaseManager.Transactional(connection => {
                foreach (JObject obj in result) {
                    var o = obj;
                    obj.Add(edge.Source.Name + "_id", node["id"]);
                    AddSystime(edge, ref o);
                    if (!DatabaseManager.CheckEdgeMatch(connection, edge, o)) {
                        DatabaseManager.InsertRow(connection, edge, o);
                    } else {
                        if (edge.Ordering == "desc") {
                            break;
                        }
                    }
                    result2.Add(o);
                }
                if (result.Count() > 0) {
                    if (edge.Ordering == "unordered") {
                        DatabaseManager.DeleteEdgeNotMatch(connection, edge, result2, "systime", "fetch_time");
                    }
                }
            });
        }

        public static bool EmptyMetric(JToken v) {
            if (v.Type == JTokenType.Integer) {
                return (long) v["value"] != 0;
            } else if (v.Type == JTokenType.Array) {
                return v["value"].ToList().Count() > 0;
            } else {
                return v["value"] != null || v["value"] != new JObject();
            }
        }

        public (Modified, JObject) ? ListLifetimeInsights(Insights edge, JObject node, int max_iters = Int32.MaxValue) {
<<<<<<< HEAD
            Logger.Debug("Fetching lifetime insights for ({Id}, {Edge})", node["id"], edge.Name);
=======
            Logger.Debug($"Fetching lifetime insights for ({node["id"]}, {edge.Name})");
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
            var now = this.ApiMan.GetUtcTime().Date + new TimeSpan(3, 0, 0, 0);
            var startN = node.IndexPathOrDefault(edge.Start, this.ApiMan.GetUtcTime());
            var last = DatabaseManager.LastLifetimeDate(edge, node);
            var upperLimit = node.IndexPathOrDefault(edge.End, now);
            if (last > upperLimit) {
                return null;
            }
            JObject row = FetchInsights(node["id"].ToString(), edge, new Range<DateTime>(startN - new TimeSpan(3, 0, 0, 0), upperLimit));
            JObject nobj;
            if (edge.Transposed) {
                nobj = new JObject();
                List<JObject> result = new List<JObject>();
                result.AddRange(((JArray) row.SelectToken("data")).ToObject<List<JObject>>());
                var filtered = result.Where(x => x["values"].Where(v => EmptyMetric(v)).Count() > 0);
                foreach (JObject p in filtered) {
                    nobj[(string) p["name"]] = p["values"][0]["value"];
                }
                nobj["fetch_time"] = row["fetch_time"];
            } else if (row.SelectToken("data").Count() > 0) {
                nobj = row.SelectToken("data").ToObject<List<JObject>>()[0];
                nobj["fetch_time"] = row["fetch_time"];
            } else {
                return null;
            }

            if (!edge.Source.IsRoot) {
                nobj.Add(edge.Source.Name + "_id", node["id"]);
            }
            nobj.Add("systime", "[" + nobj["fetch_time"].ToObject<DateTime>().ToString("yyyy-MM-dd HH:mm:ss") + ",)");
            Modified mod;
            if (edge.Source.IsRoot) {
                mod = DatabaseManager.CheckLifetimeInsightMatch(edge, nobj, new string[] {});
            } else {
                var source_id = $"{edge.Source.Name}_id";
                mod = DatabaseManager.CheckLifetimeInsightMatch(edge, nobj, new string[] { source_id });
            }
            switch (mod) {
                case Modified.New:
                    DatabaseManager.InsertInsights(edge, nobj);
                    break;
                case Modified.Equal:
                    if (edge.Source.IsRoot) {
                        DatabaseManager.UpdateLastFetch(edge, nobj, new string[] {}, "systime", "fetch_time");
                    } else {
                        DatabaseManager.UpdateLastFetch(edge, nobj, new string[] { edge.Source.Name + "_id" }, "systime", "fetch_time");
                    }
                    break;
                case Modified.Updated:
                    if (edge.Source.IsRoot) {
                        DatabaseManager.VersionEntityModified(edge, nobj, new string[] {}, "systime");
                    } else {
                        DatabaseManager.VersionEntityModified(edge, nobj, edge.Source.Name + "_id", "systime");
                    }
                    DatabaseManager.InsertInsights(edge, nobj);
                    break;
            }
            return (mod, nobj);
        }

        public void FetchInsights(Table table, JObject row) {
            // Fetch insights for social media artifact represented by `row`
            //(which is assumed to be an element of `table`).
            var lifetime = table.Insights.GetValueOrDefault("lifetime", null);
            var daily = table.Insights.GetValueOrDefault("day", null);

            if (table.InstagramInsights.Any()) {
                var mediaTypeOfRow = row["media_type"].ToString().ToLower();
                var mediaSchema = table.InstagramInsights.GetValueOrDefault(mediaTypeOfRow, null);
                if (mediaSchema != null) {
                    ListLifetimeInsights(mediaSchema, row);
                } else {
<<<<<<< HEAD
                    Logger.Warning($"Missing chema for media type {mediaTypeOfRow}");
=======
                    Logger.Warning($"Missing schema for media type {mediaTypeOfRow}");
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                }
            }

            if (lifetime != null) {
                var lrow = ListLifetimeInsights(lifetime, row);

                if (lrow.HasValue && daily != null) {
                    ListDailyInsights(lifetime, daily, row);
                }
            }
        }

        public DateTime Partition(int days, DateTime start, DateTime current) {
            /**
               Returns the beggining of the time partition which contains `current`.
               The partitioning is defined by `start` and `days` after the example below.
               Say that start=0, and that days=5.
               The partitions are: (0,...,4), (5,...,9), ...
               Partition(5, 0, 0) returns 0.
               Partition(5, 0, 1) returns 0.
               Partition(5, 0, 2) returns 0.
               Partition(5, 0, 3) returns 0.
               Partition(5, 0, 4) returns 0.
               Partition(5, 0, 5) returns 5.
             */if (current < start)
                return start;
            var span = current - start;
            var partition = span.Days / days;
            return start.Add(new TimeSpan(partition * days, 0, 0, 0));
        }

        public void ListDailyInsights(Insights lifetime, Insights edge, JObject node, int max_iters = Int32.MaxValue) {
<<<<<<< HEAD
            Logger.Debug("Fetching daily insights for ({Id}, {Edge})", node["id"], edge.Name);
=======
            Logger.Debug($"Fetching daily insights for ({node["id"]}, {edge.Name})");
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
            // Size of days in each request
            const int DayPageSize = 10;
            // limits the number of time partitions replicated per call to this function
            const int ITER_LIMIT = 5;
            // Offset to query in the past and in the future
            var window = new TimeSpan(3, 0, 0, 0);
            var maxTimespan = new TimeSpan(DayPageSize, 0, 0, 0);
            var minTimespan = new TimeSpan(1, 0, 0, 0);
            var now = this.ApiMan.GetUtcTime().Date + window;
            // the earliest point in time which the asset has any metric information
            var origin = node.IndexPathOrDefault(edge.Start, now).Date - maxTimespan;
            // the date time from which should start fetching data
            var start = (DatabaseManager.LastDailyDate(edge, node) - window ?? origin).Date;
            if (start == now) {
                return;
            }
            // stops querying if we've passed over this point
            var upperLimit = node.IndexPathOrDefault(edge.End, now);
            var delta = now.Subtract(start);
            // don't fetch if there is only one day of data, because we are already looking some days forward
            if (delta <= minTimespan)
                return;
            // the actual date from which we will start fetching data
            var partitionStart = Partition(DayPageSize, origin, start);
            var range = new Range<DateTime>(partitionStart, partitionStart + maxTimespan);

            var i = 0;
            // attemps to replicate at least one date
            var hasRow = false;
            while (
                (!hasRow || i < ITER_LIMIT || !ApiMan.ShouldPaginate)
                && range.Minimum < upperLimit
                && !DatabaseManager.DailyInsightsAreComplete(lifetime, edge, node)
                ) {
                Logger.Debug("Fetching date range {From} {To}", range.Minimum.Date, range.Maximum.Date);
                i++;
                var row = FetchInsights(node["id"].ToString(), edge, range);
                var result = new List<JObject>();
                result.AddRange(((JArray) row.SelectToken("data")).ToObject<List<JObject>>());

                if (edge.Transposed) {
                    var names = result.Select(x => x["name"]).Where(y => y != null);
                    result = result.Select(x =>
                                           x["values"].ToObject<List<JObject>>().Select(o => new JObject(){ { (string) x["name"], o["value"] }, { "date_start", o["end_time"] }, { "date_stop", o["end_time"] } }))
                                 .Where(x => x.Count() > 0).DefaultIfEmpty(new List<JObject>()).Aggregate((x, y) => x.Zip(y, (a, b) => {
                        a.Merge(b);
                        return a;
                    })).ToList();
                    result = result.Where(x => names.Any(n => (int) x[(string) n] != 0)).ToList();
                }

                foreach (var nobj in result) {
                    nobj.Add("fetch_time", row["fetch_time"]);
                    nobj.Add("systime", "[" + row["fetch_time"].ToObject<DateTime>().ToString("yyyy-MM-dd HH:mm:ss") + ",)");
                    if (!edge.Source.IsRoot) {
                        nobj.Add(edge.Source.Name + "_id", node["id"]);
                    }

                    string[] pk;
                    if (edge.Source.IsRoot) {
                        pk = new string[] { "date_start", "date_end" };
                    } else {
                        pk = new string[] { $"{edge.Source.Name}_id", "date_start", "date_end" };
                    }
                    var modified = DatabaseManager.CheckInsightDailyMatch(edge, nobj);
                    switch (modified) {
                        case Modified.New:
                            hasRow = true;
                            DatabaseManager.InsertInsights(edge, nobj);
                            break;
                        case Modified.Equal:
                            DatabaseManager.UpdateLastFetch(edge, nobj, pk, "systime", "fetch_time");
                            break;
                        case Modified.Updated:
                            hasRow = true;
                            DatabaseManager.VersionEntityModified(edge, nobj, pk, "systime");
                            DatabaseManager.InsertInsights(edge, nobj);
                            break;
                    }
                }

                delta = now.Subtract(range.Maximum);

                // Don't fetch if there is only one day of data, because we are already looking some days forward
                if (delta <= minTimespan)
                    return;
                range.Minimum = range.Maximum;
                range.Maximum = range.Minimum.Add(maxTimespan);
            }
        }
    }
}
