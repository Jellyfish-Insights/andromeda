using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Data {

    public class TweetMedia : IEquatable<TweetMedia> {

        public string TweetId { get; set; }
        public virtual Tweet Tweet { get; set; }
        public string MediaId { get; set; }
        public virtual Media Media { get; set; }

        bool IEquatable<TweetMedia>.Equals(TweetMedia other) {
            return TweetId == other.TweetId &&
                   MediaId == other.MediaId;
        }
    }
}
