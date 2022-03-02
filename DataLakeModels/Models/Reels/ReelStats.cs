using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.Reels {
    public class ReelStats : IEquatable<ReelStats>, IValidityRange {
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public string UserId { get; set; }
        public long LikeCount { get; set; }
        public long PlayCount { get; set; }
        public long ViewCount { get; set; }
        public long CommentCount { get; set; }

        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        bool IEquatable<ReelStats>.Equals(ReelStats other) {
            return ReelId == other.ReelId &&
                   LikeCount == other.LikeCount &&
                   PlayCount == other.PlayCount &&
                   ViewCount == other.ViewCount &&
                   CommentCount == other.CommentCount;
        }
    }
}