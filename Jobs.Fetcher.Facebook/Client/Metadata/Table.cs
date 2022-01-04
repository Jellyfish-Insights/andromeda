using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
namespace Jobs.Fetcher.Facebook {
    public class Table : PostgresTable {
        public Table(
            string name,
            List<string[]> columns,
            List<Edge> edges,
            List<Insights> insights,
            List<string> time,
            List<string> required,
            List<InstagramInsights> instagramInsights) {
            if (time != null && time.Count > 0) {
                Syscreate = time[0];
                if (time.Count > 1) {
                    Sysupdate = time[1];
                }
            }

            // treat missing parameters
            Required = required ?? new List<string>();
            CreatePrimaryKey();
            columns = columns ?? new List<string[]>();
            edges = edges ?? new List<Edge>();
            insights = insights ?? new List<Insights>();
            instagramInsights = instagramInsights ?? new List<InstagramInsights>();

            Name = name;
            Constraints = new List<string>();
            GenColumns = new Dictionary<string, Column>();
            Columns = columns.Select(x => new Column(x)).ToDictionary(x => x.Name);

            Edges = edges.ToDictionary(x => x.Name);
            Insights = insights.ToDictionary(x => x.Granularity);
            InstagramInsights = instagramInsights.ToDictionary(x => x.InstagramMediaType);
        }

        public void FinishSetup() {
            SetupEdges();
            SetupInsights();
            SetupInstagramInsights();
        }

        protected virtual void SetupEdgeConstraints(Edge v) {
            var nominalKeys = new Column[] {
                new Column($"{Name}_id", Columns["id"].Type, "id"),
                Columns["id"].Clone()
            };
            v.SetPrimaryKey(new PrimaryKey(nominalKeys, false));
        }

        private void SetupEdges() {
            foreach (Edge v in Edges.Values) {
                v.Source = this;
                SetupEdgeConstraints(v);
                v.FinishSetup();
            }
        }

        protected virtual void SetupInsights(Insights v) {
            PrimaryKey pk;
            var nominalColumn = new Column[] { Columns["id"].Clone(Name) };
            if (v.Granularity == "day") {
                pk = new PrimaryKey(nominalColumn, true);
            } else if (v.Granularity == "lifetime" || v.Granularity == "maximum") {
                pk = new PrimaryKey(nominalColumn, false);
            } else {
                throw new Exception($"Invalid granularity: {v.Granularity}");
            }
            v.SetPrimaryKey(pk);
        }

        private void SetupInsights() {
            foreach (Insights v in Insights.Values) {
                v.Source = this;
                SetupInsights(v);
                v.FinishSetup();
            }
        }

        protected virtual void SetupInstagramInsights(InstagramInsights v) {
            var pk = new PrimaryKey(new Column[] { Columns["id"].Clone(Name) }, false);
            v.SetPrimaryKey(pk);
        }

        /// Adds constraints and columns to instagram insight children
        private void SetupInstagramInsights() {
            foreach (InstagramInsights v in InstagramInsights.Values) {
                v.Source = this;
                SetupInstagramInsights(v);
                v.FinishSetup();
            }
        }

        public string Name { get; set; }
        public Dictionary<string, Column> Columns { get; set; }
        public Dictionary<string, Edge> Edges { get; set; }
        public List<string> Constraints { get; set; }
        // If not empty, lists fields that must be present in order that the entity is considered
        public List<string> Required { get; set; }
        public virtual string TableName {
            get => (IsRoot || Source.IsRoot) ? Name : $"{Source.Name}_{Name}";
        }
        public Table Source { get; set; }
        public bool IsRoot { get => Source == null; }
        public virtual Dictionary<string, Column> ColumnDefinition {
            get {
                var col = new Dictionary<string, Column>();
                foreach (var kv in Columns.ToList()) {
                    col.Add(kv.Key, kv.Value);
                }
                foreach (var kv in GenColumns.ToList()) {
                    col.Add(kv.Key, kv.Value);
                }
                return col;
            }
        }

        public Dictionary<string, Insights> Insights { get; set; }
        public Dictionary<string, InstagramInsights> InstagramInsights { get; set; }
        public Dictionary<string, Column> GenColumns { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        // Name of API response field from which to read creation of object. Defaults to "fetch_time"
        public string Syscreate { get; set; }
        // Name of API response field from which to read time on which object was updated. If not defined, uses same field as Syscreate
        public string Sysupdate { get; set; }

        public virtual void CreatePrimaryKey() {
            var idType = Columns.ContainsKey("id") ? Columns["id"].Type : "text";
            var nominalColumns = new Column[] {
                new Column("id", idType)
            };
            SetPrimaryKey(new PrimaryKey(nominalColumns, false));
        }

        public void SetPrimaryKey(PrimaryKey key) {
            // enforces that all columns defined on key are defined
            foreach (var c in key.Columns) {
                if (!GenColumns.ContainsKey(c.Name) && !Columns.ContainsKey(c.Name)) {
                    AddGeneratedColumn(c.Name, c.Type);
                }
            }
            Constraints.Add($"CONSTRAINT pk_{TableName} PRIMARY KEY({string.Join(',', key.ColumnNames)})");
            PrimaryKey = key;
        }

        public void AddGeneratedColumn(string name, string type) {
            string[] col = { name, type };
            GenColumns.Add(name, new Column(col));
        }
    }
}
