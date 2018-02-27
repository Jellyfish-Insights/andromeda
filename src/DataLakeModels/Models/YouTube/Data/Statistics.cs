using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.YouTube.Data {
    public class Statistics : IValidityRange, IEquatable<Statistics> {
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        public string VideoId { get; set; }
        [Column(TypeName = "date")]
        public DateTime CaptureDate { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public long DislikeCount { get; set; }
        public long FavoriteCount { get; set; }
        public long CommentCount { get; set; }

        bool IEquatable<Statistics>.Equals(Statistics other) {
            return ViewCount == other.ViewCount &&
                   LikeCount == other.LikeCount &&
                   DislikeCount == other.DislikeCount &&
                   FavoriteCount == other.FavoriteCount &&
                   CommentCount == other.CommentCount;
        }
    }
}
