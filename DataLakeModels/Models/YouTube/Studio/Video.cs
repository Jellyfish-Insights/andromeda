using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLakeModels.Models.YouTube.Studio {

    public class Video : IValidityRange, IEquatable<Video> {

        public Video(
            DateTime validityStart,
            DateTime validityEnd,
            DateTime dateMeasure,
            string channelId,
            string videoId,
            string metric,
            double value
        )
        {
            ValidityStart = validityStart;
            ValidityEnd = validityEnd;
            DateMeasure = dateMeasure;
            ChannelId = channelId;
            VideoId = videoId;
            Metric = metric;
            Value = value;
        }
        public Video(){}

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        /* See "DateOnly" type introduced in .NET 6 */
        public DateTime DateMeasure { get; set; }
        public string ChannelId { get; set; }
        public string VideoId { get; set; }
        public string Metric { get; set; }
        public double Value { get; set; }

        bool IEquatable<Video>.Equals(Video other) {
            return  ChannelId == other.ChannelId &&
                    VideoId == other.VideoId &&
                    Metric  == other.Metric &&
                    DateMeasure == other.DateMeasure  &&
                    (
                        ValidityStart >= other.ValidityStart &&
                        ValidityStart < other.ValidityEnd
                    );
        }

        public override string ToString()
        {
            return $@"
                ValidityStart: {ValidityStart},
                ValidityEnd: {ValidityEnd},
                DateMeasure: {DateMeasure},
                ChannelId: {ChannelId},
                VideoId: {VideoId},
                Metric: {Metric},
                Value: {Value}
            ";
        }
    }

}
