using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models.Metadata {
    public interface IMutableEntity {
        List<JToken> PrimaryKey { get; }
        DateTime UpdateDate { get; }
    }

    public static class MutableEntityExtentions {
        public static List<JToken> AutoPK(params object[] pk) {

            var Id = new List<JToken>();
            foreach (var t in pk) {
                Id.Add(JToken.FromObject(t));
            }
            return Id;
        }

        public static DateTime Min(params DateTime[] sources) {
            return sources.Min();
        }

        public static DateTime Max(params DateTime[] sources) {
            return sources.Max();
        }
    }
}
