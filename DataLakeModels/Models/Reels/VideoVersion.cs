using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.Reels {
    public class VideoVersion : IEquatable<VideoVersion> {
        public string Id { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public string Url { get; set; }
        public long Type { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }

        bool IEquatable<VideoVersion>.Equals(VideoVersion other) {
            return Id == other.Id &&
                   Url == other.Url &&
                   Type == other.Type &&
                   Width == other.Width &&
                   Height == other.Height;
        }
    }
}
