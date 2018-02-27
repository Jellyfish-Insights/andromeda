using System;
using System.Collections.Generic;

namespace Common {

    public static class DateHelper {

        public static DateTime Min(DateTime a, DateTime b) {
            return (a < b) ? a : b;
        }

        public static DateTime Max(DateTime a, DateTime b) {
            return (a > b) ? a : b;
        }

        /**
            Creates a list of dates from "from" until "to", inclusive on both ends, using a single day as step.
         */
        public static IEnumerable<DateTime> DaysInRange(DateTime from, DateTime to, bool reverse = false) {
            if (from > to) {
                yield break;
            }
            var current = reverse ? to : from;
            var end = reverse ? from : to;
            int step = reverse ? -1 : 1;

            while (current != end) {
                yield return current;
                current = current.AddDays(step);
            }

            yield return current;
        }
    }
}
