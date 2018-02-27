using System;
using System.Collections.Generic;
using System.Linq;

namespace Jobs.Fetcher.Facebook {

    public class Schema : Table {

        /**
            This constructor is used by the "JsonConvert.DeserializeObject" call
            in the "SchemaLoader".

            The names of its arguments should match the names of the json object keys.
         */
        public Schema(
            string name,
            List<string[]> columns,
            List<Edge> edges,
            List<Insights> insights,
            List<string> time,
            List<string> required,
            List<InstagramInsights> instagram_insights,
            int page_size,
            int threads,
            int delay
            ): base(name, columns, edges, insights, time, required, instagram_insights) {
            Threads = threads;
            PageSize = page_size;
            Delay = delay;
        }

        public int Threads { get; set; }
        public int PageSize { get; set; }
        public int Delay { get; set; }
        public string Version { get; set; }
        // Set this to true if you want the fetcher to quickly list all entities

        // A schema does not generate a database table, and thus it does not need a primary key
        public override void CreatePrimaryKey() {}

        protected override void SetupEdgeConstraints(Edge v) {
            v.SetPrimaryKey(new PrimaryKey(new Column[] { Columns["id"].Clone() }, false));
        }

        protected override void SetupInsights(Insights v) {
            PrimaryKey pk;
            if (v.Granularity == "day") {
                pk = new PrimaryKey(Constants.NoNominalColumn, true);
            } else if (v.Granularity == "lifetime") {
                pk = new PrimaryKey(Constants.NoNominalColumn, false);
            } else {
                throw new Exception($"Invalid granularity: {v.Granularity}");
            }
            v.SetPrimaryKey(pk);
        }

        protected override void SetupInstagramInsights(InstagramInsights v) {
            // instagram insights are all lifetime
            v.SetPrimaryKey(new PrimaryKey(Constants.NoNominalColumn, false));
        }
    }
}
