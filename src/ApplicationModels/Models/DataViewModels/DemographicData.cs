using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {
    public class DemographicData {

        // the values per meta tag
        public List<DemographicDataItem> Values { get; set; }
        public List<string> Groups { get; set; }
    }
}
