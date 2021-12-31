using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.TikTok {

    public class PostStats : IEquatable<PostStats>, IValidityRange {

        public long DiggCount { get; set; }
        public long ShareCount { get; set; }
        public long CommentCount { get; set; }
        public long PlayCount { get; set; }
        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Post Post { get; set; }
        public string PostId { get; set; }
        
        bool IEquatable<PostStats>.Equals(PostStats other) {
            return PostId == other.PostId &&
                   DiggCount == other.DiggCount &&
                   ShareCount == other.ShareCount &&
                   CommentCount == other.CommentCount &&
                   PlayCount == other.PlayCount &&
                   EventDate == other.EventDate;
        }
    }
}
