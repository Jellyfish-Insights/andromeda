using System.Collections.Generic;
using TypeScriptBuilder;

namespace ApplicationModels.Models.DataViewModels {
    public class VideoMetric {
        public string Id { get; set; }
        public List<Metric> TotalMetrics { get; set; }
        [TSOptional]
        public List<PersonaMetric> MetricsPerPersona { get; set; }
    }
}
