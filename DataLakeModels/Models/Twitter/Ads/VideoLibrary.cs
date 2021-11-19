using System;

namespace DataLakeModels.Models.Twitter.Ads {
    public class VideoLibrary : IValidityRange, IEquatable<VideoLibrary> {

        public string Id { get; set; }
        public string PosterMediaKey { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MediaStatus { get; set; }
        public string MediaUrl { get; set; }
        public string PosterMediaUrl { get; set; }
        public string AspectRatio { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Tweeted { get; set; }
        public bool Deleted { get; set; }
        public string Username { get; set; }
        public long Duration { get; set; }
        public string FileName { get; set; }

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        bool IEquatable<VideoLibrary>.Equals(VideoLibrary other) {

            return Id == other.Id &&
                   PosterMediaKey == other.PosterMediaKey &&
                   Title == other.Title &&
                   Name == other.Name &&
                   Description == other.Description &&
                   MediaStatus == other.MediaStatus &&
                   MediaUrl == other.MediaUrl &&
                   PosterMediaUrl == other.PosterMediaUrl &&
                   AspectRatio == other.AspectRatio &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   Tweeted == other.Tweeted &&
                   Deleted == other.Deleted &&
                   Username == other.Username &&
                   Duration == other.Duration &&
                   FileName == other.FileName;
        }
    }
}
