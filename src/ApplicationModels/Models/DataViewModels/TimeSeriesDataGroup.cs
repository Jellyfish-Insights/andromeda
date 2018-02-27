using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {
    public class TimeSeriesDataGroup {
        // Persona A, Topic 0... depends on context
        public string GroupName { get; set; }

        // values in temporal sequence;
        // the dates are the ones in the TimeSeries element
        public double[] Values { get; set; }
    }
}
