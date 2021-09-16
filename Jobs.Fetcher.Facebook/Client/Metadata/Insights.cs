using System;
using System.Collections.Generic;
using System.Linq;
namespace Jobs.Fetcher.Facebook {
    public class Insights : Edge {
        public Insights(
            string name,
            List<string[]> columns,
            List<string> time,
            List<string[]> metrics,
            string granularity,
            List<string[]> bounds,
            bool? summary
            ): base(name, columns, null, null, time, null, null, "unordered") {
            // treat missing parameters
            if (bounds != null && bounds.Count() >= 1) {
                Start = bounds[0];
                if (bounds.Count() >= 2) {
                    End = bounds[1];
                }
            }
            Granularity = granularity ?? "lifetime";
            Summary = summary ?? false;
            metrics = metrics ?? new List<string[]>();
            Metrics = metrics.Select(x => new Metrics(x)).ToDictionary(x => x.Name);

            AddGeneratedColumn(Constants.FetchTime.Name, Constants.FetchTime.Type);
        }

        public Dictionary<string, Metrics> Metrics { get; set; }
        public IEnumerable<Column> MetricColumns {
            get {
                if (Transposed) {
                    return Metrics.ToList().Select(x => (Column) x.Value);
                } else {
                    return Columns.ToList().Select(x => x.Value);
                }
            }
        }
        public string Granularity { get; set; }
        public bool Summary { get; set; }
        // If false: API returns data in the format: (date, metric, value), which is the format dealt by the code base.
        // If true: API returns data in the format: (metric, date, value), which must be transposed to (date, metric, value) before processing
        public bool Transposed { get => Metrics.Count() > 0; }

        public override string TableName { get => $"{Source.Name}_{Name}_{Granularity}"; }
        public override Dictionary<string, Column> ColumnDefinition => TransposeInsights();

        public Dictionary<string, Column> TransposeInsights() {
            var cols = new Dictionary<string, Column>();

            foreach (var v in base.ColumnDefinition.ToList()) {
                cols.Add(v.Key, v.Value);
            }

            if (Transposed) {
                foreach (var v in Metrics.ToList()) {
                    cols.Add(v.Key, v.Value);
                }
                cols.Remove("name");
                cols.Remove("values");
            }
            return cols;
        }

        /**
           Source object's field indicating its creation date.
           This information indicates when to start querying insight metrics for the source object.
           For nested fields, use array syntax. Example: ["campaign", "stop_time"].
         */
        public string[] Start { get; set; }
        public string[] End { get; set; }
    }

    public class InstagramInsights : Insights {
        public InstagramInsights(
            string name,
            string granularity,
            bool? summary,
            List<string[]> bounds,
            List<string[]> columns,
            List<string[]> metrics,
            List<string> time,
            string instagram_media_type
            ): base(name, columns, time, metrics, granularity, bounds, summary) {
            InstagramMediaType = instagram_media_type ?? "";
        }

        public string InstagramMediaType { get; set; }
        public override string TableName { get => $"{Source.TableName}_{Name}_{Granularity}_{InstagramMediaType}"; }
    }
}
