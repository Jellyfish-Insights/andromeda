using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models.Metadata {

    using CompositeKey = List<Newtonsoft.Json.Linq.JToken>;

    public class RowLog {
        // Input keys read from source tables in transformation
        // Stored as a dictionary of tables names as keys and values a set of
        // list of composite keys
        public Dictionary<string, HashSet<CompositeKey>> Source { get; set; }
        // Id of the table that was modified
        public CompositeKey Id { get; set; }
        public DateTime? OldVersion { get; set; }
        public DateTime? NewVersion { get; set; }

        public RowLog() {
            Source = new Dictionary<string, HashSet<CompositeKey>>();
        }

        public void AddId(params object[] p) {
            Id = new List<JToken>();
            foreach (var t in p) {
                Id.Add(JToken.FromObject(t));
            }
        }

        public void AddInput(Type table, CompositeKey p) {
            AddInput(table.Name, p);
        }

        public void AddInput(string table, CompositeKey p) {
            Source.Add(table, new HashSet<CompositeKey>() { p });
        }

        public void AddInput(string table, params object[] p) {
            var l = new List<JToken>();
            foreach (var t in p) {
                l.Add(JToken.FromObject(t));
            }
            Source.Add(table, new HashSet<CompositeKey>() { l });
        }
    }
}
