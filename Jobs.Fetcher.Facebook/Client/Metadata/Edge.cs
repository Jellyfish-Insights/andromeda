using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
namespace Jobs.Fetcher.Facebook {
    public class Edge : Table {
        [JsonConstructor]
        public Edge(
            string name,
            List<string[]> columns,
            List<Edge> edges,
            List<Insights> insights,
            List<string> time,
            List<string> required,
            List<InstagramInsights> instagram_insights,
            string ordering
            ): base(name, columns, edges, insights, time, required, instagram_insights) {
            Ordering = ordering;
        }

        public string Ordering { get; set; }

        // the primary key of an edge is created by its parent node
        public override void CreatePrimaryKey() {}
    }
}
