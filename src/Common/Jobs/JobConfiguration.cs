using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace Common.Jobs {
    public class JobConfiguration {
        // set to read requests from cache only
        public bool IgnoreAPI { get; set; }
        // set to always read from cache, despite of what TTL has to say.
        public bool IgnoreTTL { get; set; }
        // set to true to quickly check that the fetcher can fetch all entities
        public bool IgnoreEdges { get; set; }
        // set to positive integer to quickly check if the fetcher can fetch all data for a reduced set of entities
        public int MaxEntities { get; set; }
        // Set to false to fetch all daily metrics in a single run
        public bool Paginate { get; set; }
        // If the DefaultNowDate is null the use DateTime.Now otherwise fallback to DefaultNowDate
        public DateTime? DefaultNowDate { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        public static void DumpConfiguration(JobConfiguration configuration, string filename) {
            FileSystemHelpers.DumpJson(filename, JObject.Parse(configuration.ToString()));
        }

        public static JobConfiguration LoadConfiguration(string filename) {
            return FileSystemHelpers.LoadJson(filename).ToObject<JobConfiguration>();
        }
    }
}
