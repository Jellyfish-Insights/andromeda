using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Tag : IEquatable<Tag> {

        public string AweMeId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string HashtagName { get; set; }
        public string HashtagId { get; set; }
        public int Type { get; set; }
        public string UserId { get; set; }
        public bool IsCommerce { get; set; }
        public string UserUniqueId { get; set; }
        public string SecureUId { get; set; }
        public int SubType  { get; set; }

        bool IEquatable<Tag>.Equals(Tag other) {
            return AweMeId == other.AweMeId &&
                   Start == other.Start &&
                   End == other.End &&
                   HashtagName == other.HashtagName &&
                   HashtagId == other.HashtagId &&
                   Type == other.Type &&
                   UserId == other.UserId &&
                   IsCommerce == other.IsCommerce &&
                   UserUniqueId == other.UserUniqueId &&
                   SecureUId == other.SecureUId &&
                   SubType == other.SubType;
        }
    }
}
