using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class AuthorStats : IEquatable<AuthorStats> {

        public string FollowingCount { get; set; }
        public string FollowerCount { get; set; }
        public string HeartCount { get; set; }
        public string VideoCount { get; set; }
        public string DiggCount { get; set; }
        public string Heart { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public string AuthorId { get; set; }
        public string Author { get; set; }
        
        bool IEquatable<AuthorStats>.Equals(AuthorStats other) {
            return AuthorId == other.AuthorId &&
                   FollowingCount == other.FollowingCount &&
                   FollowerCount == other.FollowerCount &&
                   HeartCount == other.HeartCount &&
                   VideoCount == other.VideoCount &&
                   DiggCount == other.DiggCount &&
                   Heart == other.Heart;
        }
    }
}
