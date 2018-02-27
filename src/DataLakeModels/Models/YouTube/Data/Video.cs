using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLakeModels.Models.YouTube.Data {

    public class Video : IValidityRange, IEquatable<Video> {

        public Video() {
            PrivacyStatus = "public";
        }

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public DateTime PublishedAt { get; set; }
        public string VideoId { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Title { get; set; }
        public string Duration { get; set; }
        public string PrivacyStatus { get; set; }

        /**
           The channel Id as read from youtube's API
         */
        public string ChannelId { get; set; }

        [Column(TypeName = "text[]")]
        public string[] Tags { get; set; }

        bool IEquatable<Video>.Equals(Video other) {
            return PublishedAt == other.PublishedAt &&
                   ThumbnailUrl == other.ThumbnailUrl &&
                   Title == other.Title &&
                   Duration == other.Duration &&
                   ChannelId == other.ChannelId &&
                   PrivacyStatus == other.PrivacyStatus &&
                   ((Tags == null && other.Tags == null) || Tags.SequenceEqual(other.Tags));
        }

        public YouTubePrivacyStatus PrivacyEnum {
            get {
                YouTubePrivacyStatus o;
                var s = PrivacyStatus.First().ToString().ToUpper() + PrivacyStatus.Substring(1);

                if (!Enum.TryParse<YouTubePrivacyStatus>(s, out o)) {
                    throw new InvalidCastException($"Invalid YouTube privacy '{PrivacyStatus}'");
                }

                return o;
            }
        }
    }

    public enum YouTubePrivacyStatus {
        Private,
        Public,
        Unlisted
    }
}
