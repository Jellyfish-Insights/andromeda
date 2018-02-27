using System;
using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {
    public class TimeSeries {
        // all the dates for this time series
        // Dates[0] is a date that will refer to element [0]
        // in the property entries and so on for the [1], [2]...
        // the format of each date should be yyyyMMdd
        // to convert a DateTime x to a string, use x.ToString("yyyyMMdd")
        public List<string> Dates { get; set; }

        // the values per topic or persona
        // work as a Dictionary<string, List<double>> Values ;
        public List<TimeSeriesDataGroup> Values { get; set; }
        public TimeSeriesDataGroup TotalTimeSeries { get; set; }
        public Dictionary<string, double> TotalPerGroup { get; set; }
        public double TotalOnPeriod { get; set; }
    }

    public class ChartObject {
        public string Date { get; set; }
        public Dictionary<string, double> Values { get; set; }
    }

    public class TimeSeriesChartData {
        /**
         * From a TimeSeries object, we get both a ChartArray and a TotalPerGroup
         */
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Metric { get; set; }
        public List<ChartObject> ChartObjectArray { get; set; }
        public Dictionary<string, double> TotalPerGroup { get; set; }
        public double TotalOnPeriod { get; set; }
    }
}
