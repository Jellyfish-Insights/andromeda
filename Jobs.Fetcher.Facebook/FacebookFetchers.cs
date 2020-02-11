using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Andromeda.Common.Jobs;
using Serilog.Core;
using System.IO;

namespace Jobs.Fetcher.Facebook {

    using CustomEdge = Dictionary<(string SchemaName, string TableName, string CustomEdgeName), Action<Logger, JObject>>;

    public class FacebookFetchers : FetcherJobsFactory {

        public const string DefaultCacheDirectory = "./cache";

        bool IgnoreCache = false;
        string CacheDirectory = DefaultCacheDirectory;

        string TableKey = null;
        string EdgeKey = null;
        List<string> Schemas = SchemaLoader.SchemaList();

        public override JobScope Scope { get; } = JobScope.Facebook;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope)) {
                return NoJobs;
            }

            var jobs = new List<FacebookFetcher>();

            try
            {
                foreach (var schemaName in Schemas) {

                    var schema = SchemaLoader.LoadSchema(schemaName);
                    var apiMan = new ApiManager(
                        SchemaLoader.GetCredentials(schemaName),
                        IgnoreCache,
                        jobConfiguration.IgnoreAPI,
                        jobConfiguration.IgnoreTTL,
                        jobConfiguration.Paginate,
                        schema.Delay,
                        CacheDirectory,
                        jobConfiguration.DefaultNowDate
                        );
                    var fetcher = new Fetcher(apiMan, schema.PageSize);
                    var customEdge = new CustomEdge();

                    if (schemaName == "adaccount") {

                        var pageSchema = SchemaLoader.LoadSchema("page");
                        var pageCredentials = SchemaLoader.GetCredentials("page");

                        var pageApiMan = new ApiManager(
                            SchemaLoader.GetCredentials("page"),
                            IgnoreCache,
                            jobConfiguration.IgnoreAPI,
                            jobConfiguration.IgnoreTTL,
                            jobConfiguration.Paginate,
                            schema.Delay,
                            CacheDirectory,
                            jobConfiguration.DefaultNowDate
                            );
                        var pageFetcher = new Fetcher(pageApiMan, pageSchema.PageSize);

                        var key = (SchemaName : "adaccount", TableName : "ads", CustomEdgeName : "custom_videos");
                        if (!customEdge.ContainsKey(key)) {

                            Action<Logger, JObject> callback = (logger, parent) => {
                                var creative = parent["creative"];

                                var video = creative ? ["video_id"];
                                if (video != null) {
                                    pageFetcher.GetRoot(pageSchema, "videos", video, logger);
                                }

                                var sourceFileVideoId = creative.IndexPathOrDefault<string>("object_story_spec.video_data.video_id", null);

                                if (sourceFileVideoId != null) {
                                    pageFetcher.GetRoot(pageSchema, "videos", sourceFileVideoId, logger);
                                }

                                var post = creative ? ["object_story_id"];
                                if (post != null) {
                                    pageFetcher.GetRoot(pageSchema, "posts", post, logger);
                                }
                            };

                            customEdge.Add(key, callback);
                        }
                    }

                    if (Schemas.Count == 1 && TableKey != null) {
                        // DEPRECATED: case in which the user specified a single edge to be executed
                        jobs.Add(new FacebookFetcher(schema, TableKey, EdgeKey, fetcher, customEdge));
                    } else {
                        foreach (var edge in schema.Edges) {
                            jobs.Add(new FacebookFetcher(schema, edge.Key, null, fetcher, customEdge));
                        }
                        if (schemaName == "instagram") {
                            jobs.Add(new FacebookFetcher(schema, null, null, fetcher, customEdge));
                        }
                    }
                }
            }
            catch (Exception e) when (e is FileNotFoundException || e is DirectoryNotFoundException)
            {
                string message = String.Format("\nMissing or invalid Facebook credentials!\n{0}", e.Message);
                if (e is DirectoryNotFoundException) {
                    message = String.Format("{0}\nCheck if the path above exists!", message);
                }
                System.Console.WriteLine(message);
                return NoJobs;
            };

            return FilterByName(jobs, names);
        }
    }
}
