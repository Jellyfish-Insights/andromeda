using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.TikTok {

    public class AuthorStats : IEquatable<AuthorStats>, IValidityRange {

        public string AuthorId { get; set; }
        public long FollowingCount { get; set; }
        public long FollowerCount { get; set; }
        public long HeartCount { get; set; }
        public long VideoCount { get; set; }
        public long DiggCount { get; set; }
        public long Heart { get; set; }
        [Column(TypeName = "date")]
        public DateTime EventDate { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public Author Author { get; set; }
        bool IEquatable<AuthorStats>.Equals(AuthorStats other) {
            return AuthorId == other.AuthorId &&
                   FollowingCount == other.FollowingCount &&
                   FollowerCount == other.FollowerCount &&
                   HeartCount == other.HeartCount &&
                   VideoCount == other.VideoCount &&
                   DiggCount == other.DiggCount &&
                   Heart == other.Heart &&
                   EventDate == other.EventDate;
        }
    }
}
