using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLakeModels.Models.YouTube.Studio {

    public class Video : IValidityRange, IEquatable<Video> {

        public Video(
            DateTime validityStart,
            DateTime validityEnd,
            DateTime eventDate,
            string videoId,
            string metric,
            double value
            ) {
            ValidityStart = validityStart;
            ValidityEnd = validityEnd;
            EventDate = eventDate;
            VideoId = videoId;
            Metric = metric;
            Value = value;
        }

        public Video() {}

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }
        public string VideoId { get; set; }
        public string Metric { get; set; }
        public double Value { get; set; }

        bool IEquatable<Video>.Equals(Video other) {
            return VideoId == other.VideoId &&
                   Metric == other.Metric &&
                   EventDate == other.EventDate &&
                   (
                ValidityStart >= other.ValidityStart &&
                ValidityStart < other.ValidityEnd
                   );
        }

        public override string ToString() {
            return $@"
                ValidityStart: {ValidityStart},
                ValidityEnd: {ValidityEnd},
                EventDate: {EventDate},
                VideoId: {VideoId},
                Metric: {Metric},
                Value: {Value}
            ";
        }
    }
}