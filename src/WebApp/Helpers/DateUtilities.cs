using System;
using System.Collections.Generic;

namespace WebApp.Controllers {
    public class DateUtilities {
        public const string DateFormat = "yyyyMMdd";
        public const string ApiDateFormat = "yyyy-MM-dd";

        public static DateTime ReadDate(string strDate) {
            return DateTime.ParseExact(strDate, DateFormat, null);
        }

        public static string ToRestApiDateFormat(DateTime date) {
            return date.ToString(ApiDateFormat);
        }

        public static DateTime ParseApiDateString(string date) {
            return DateTime.ParseExact(date, ApiDateFormat, null);
        }

        public static string ToControllersInputFormat(DateTime date) {
            return date.ToString(DateFormat);
        }

        public static IEnumerable<DateTime> GetDatesBetween(DateTime inclusiveStar, DateTime inclusiveEnd) {
            List<DateTime> allDates = new List<DateTime>();
            for (DateTime date = inclusiveStar; date <= inclusiveEnd; date = date.AddDays(1))
                allDates.Add(date);
            return allDates;
        }
    }
}
