using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Data {

    public class Tweet : IEquatable<Tweet> {

        public string Id { get; set; }
        public string Text { get; set; }
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string InReplyToUserId { get; set; }
        public string Lang { get; set; }
        public bool PossiblySensitive { get; set; }
        public string Source { get; set; }

        public User User { get; set; }
        public ICollection<TweetPublicMetrics> PublicMetrics { get; set; }
        public ICollection<TweetNonPublicMetrics> NonPublicMetrics { get; set; }
        public ICollection<TweetOrganicMetrics> OrganicMetrics { get; set; }
        public ICollection<TweetPromotedMetrics> PromotedMetrics { get; set; }
        public ICollection<TweetMedia> TweetMedia { get; set; }

        bool IEquatable<Tweet>.Equals(Tweet other) {
            return Id == other.Id &&
                   Text == other.Text &&
                   UserId == other.UserId &&
                   ConversationId == other.ConversationId &&
                   CreatedAt == other.CreatedAt &&
                   InReplyToUserId == other.InReplyToUserId &&
                   Lang == other.Lang &&
                   PossiblySensitive == other.PossiblySensitive &&
                   Source == other.Source;
        }
    }
}
