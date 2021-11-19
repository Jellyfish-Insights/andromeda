using System;

namespace DataLakeModels.Models.Twitter.Data {

    public class MediaPublicMetrics : IValidityRange, IEquatable<MediaPublicMetrics> {

        public string MediaId { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Media Media { get; set; }
        public int ViewCount { get; set; }

        bool IEquatable<MediaPublicMetrics>.Equals(MediaPublicMetrics other) {
            return MediaId == other.MediaId &&
                   ViewCount == other.ViewCount;
        }
    }
}
