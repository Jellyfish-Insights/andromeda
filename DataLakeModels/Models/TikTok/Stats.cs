using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Stats : IEquatable<Stats> {

        public string DiggCount { get; set; }
        public string ShareCount { get; set; }
        public string CommentCount { get; set; }
        public string PlayCount { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Post Post { get; set; }
        public string PostId { get; set; }
        
        bool IEquatable<Stats>.Equals(Stats other) {
            return DiggCount == other.DiggCount &&
                   ShareCount == other.ShareCount &&
                   CommentCount == other.CommentCount &&
                   PlayCount == other.PlayCount;
        }
    }
}
