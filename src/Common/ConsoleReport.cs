using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Report {

    public static class ConsoleReport {

        public static void WriteTable(List<List<string>> rows) {
            var lengths = rows.Select(x => x.Select(y => y.Length))
                              .Aggregate((a, b) => a.Zip(b, (x, y) => Math.Max(x, y)))
                              .ToArray();

            foreach (var row in rows) {
                int i = 0;
                Console.Write("\t");
                foreach (var e in row) {
                    Console.Write(" {0} ", e.PadRight(lengths[i]));
                    i++;
                }
                Console.WriteLine();
            }
        }
    }
}
