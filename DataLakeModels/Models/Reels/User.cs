using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.Reels {

    public class User : IEquatable<User> {
        public string Pk { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsUnpublished { get; set; }
        public string ProfilePicId { get; set; }
        public string ProfilePicURL { get; set; }
        public bool HasHighlightReels { get; set; }
        public string FollowFrictionType { get; set; }
        public bool HasAnonymousProfilePicture  { get; set; }

        public ICollection<Reel> Reels { get; set; }
        public ICollection<OriginalSound> Sounds { get; set; }

        bool IEquatable<User>.Equals(User other) {
            return Pk == other.Pk &&
                   Username == other.Username &&
                   FullName == other.FullName &&
                   IsPrivate == other.IsPrivate &&
                   IsVerified == other.IsVerified &&
                   IsUnpublished == other.IsUnpublished &&
                   ProfilePicId == other.ProfilePicId &&
                   ProfilePicURL == other.ProfilePicURL &&
                   HasHighlightReels == other.HasHighlightReels &&
                   FollowFrictionType == other.FollowFrictionType &&
                   HasAnonymousProfilePicture == other.HasAnonymousProfilePicture;
        }
    }
}
