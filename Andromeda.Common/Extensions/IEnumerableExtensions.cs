using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
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
    public static class ListExtensions {
        public static List<List<T>> DivideIntoBatches<T>(
                                        List<T> source,
                                        int nBatches
                                        ) {
            if (nBatches == 0) {
                throw new InvalidOperationException("Cannot divide into zero batches!");
            }

            var listOfBatches = new List<List<T>>();

            for (int i = 0; i < nBatches; i++) {
                listOfBatches.Add(new List<T>());
            }

            int nElem = source.Count();
            int quotient = nElem / nBatches;
            int remainder = nElem % nBatches;

            if (quotient == 0) {
                for (int i = 0; i < nElem; i++) {
                    listOfBatches[i].Add(source[i]);
                }
                return listOfBatches;
            }

            for (int batch = 0; batch < nBatches; batch++) {
                for (int i = batch * quotient; i < (batch + 1) * quotient; i++) {
                    listOfBatches[batch].Add(source[i]);
                }
            }
            for (int i = 0; i < remainder; i++) {
                listOfBatches[i].Add(source[i + nBatches * quotient]);
            }
            return listOfBatches;
        }

        public static string DebugBatches<T>(List<List<T>> batches){
            var sb = new StringBuilder($"We have {batches.Count()} batches\n");
            for (int i = 0; i < batches.Count(); i++) {
                var batch = batches[i];
                sb.AppendLine($"Batch #{i + 1} / {batches.Count()} contains {batch.Count()} items");
            }
            return sb.ToString();
        }
    }
}
