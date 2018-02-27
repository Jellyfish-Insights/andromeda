using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {
    public class DemographicDataItem {

        // GroupName is one of the possible values of the grouping criteria.
        // Possible grouping criteria include "Topic", "Length", and "Medium".
        public string GroupName;

        // Dictionary of age group => gender => number of people
        public Dictionary<string, Dictionary<string, double>> Values { get; set; }

        // The total for this category
        // ==> the percentage of people that watched video of Topic 0, for instance
        public double Total { get; set; }
    }
}
