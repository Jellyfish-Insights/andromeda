using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions {
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
    }
}
