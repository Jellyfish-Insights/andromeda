using TypeScriptBuilder;
namespace ApplicationModels.Models.DataViewModels {
    public class Metric {
        // Cost Per View, Cost per click...
        public string Type { get; set; }

        // content or marketing
        // needs to be set in case array of Metric is mixed with content/marketing
        // otherwise, will be set by the dashboard client component when merging content
        // and marketing arrays
        [TSOptional]
        public string ControllerType { get; set; }
        // numeric value
        public double Value { get; set; }
    }
}
