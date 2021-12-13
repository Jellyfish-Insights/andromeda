using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Tag : IEquatable<Tag> {

        public string AweMeId { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string HashtagName { get; set; }
        public string HashtagID { get; set; }
        public int Type { get; set; }
        public string UserID { get; set; }
        public bool IsCommerce { get; set; }
        public string UserUniqueID { get; set; }
        public string SecureUID { get; set; }
        public int SubType  { get; set; }

        bool IEquatable<Tag>.Equals(Tag other) {
            return AweMeId == other.AweMeId &&
                   Start == other.Start &&
                   End == other.End &&
                   HashtagName == other.HashtagName &&
                   HashtagID == other.HashtagID &&
                   Type == other.Type &&
                   UserID == other.UserID &&
                   IsCommerce == other.IsCommerce &&
                   UserUniqueID == other.UserUniqueID &&
                   SecureUID == other.SecureUID &&
                   SubType == other.SubType;
        }
    }
}
