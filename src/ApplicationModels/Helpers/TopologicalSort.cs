using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationModels.Helpers {
    public static class TopologicalSort {
        public static IEnumerable<T> Sort<T>(HashSet<T> nodes, HashSet<(T, T)> edges) {
            var L = new List<T>();
            var S = new HashSet<T>(nodes.Where(n => edges.All((e) => !e.Item2.Equals(n))));

            while (S.Any()) {
                var n = S.First();
                S.Remove(n);

                L.Add(n);

                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList()) {
                    var m = e.Item2;

                    edges.Remove(e);

                    if (edges.All(me => !me.Item2.Equals(m))) {
                        S.Add(m);
                    }
                }
            }

            if (edges.Any()) {
                throw new Exception("Graph has cycles");
            }

            return L;
        }
    }
}
