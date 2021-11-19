using System;
using Tweetinvi.Models.V2;

namespace DataLakeModels.Models.Twitter.Data {

    public class MediaMetrics : MediaMetricsV2, IValidityRange {
        public string MediaId { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Media Media { get; set; }
    }
}
