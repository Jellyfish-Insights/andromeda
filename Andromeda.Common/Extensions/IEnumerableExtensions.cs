using System;
using System.Collections.Generic;
using System.Linq;

namespace Andromeda.Common.Extensions {
    public static class IEnumerableExtensions {
        public static IEnumerable<O> ConcatMap<T, O>(
            this IEnumerable<T> a,
            Func<T, IEnumerable<O>> map
            ) {
            return a
                       .Select(map)
                       .Aggregate(
                (new List<O>(){}).AsEnumerable(),
                (acc, cur) => acc.Concat(cur)
                );
        }

        public static IEnumerable<List<T>> SplitIntoBatches<T>(this IEnumerable<T> source, int batchSize) {
            List<T> currentBatch = new List<T>();
            int count = 0;
            foreach (T e in source) {
                currentBatch.Add(e);
                count++;
                if (count >= batchSize) {
                    yield return currentBatch;
                    currentBatch = new List<T>();
                    count = 0;
                }
            }
            yield return currentBatch;
        }
    }
}
