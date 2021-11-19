using System;
using DataLakeModels.Models;
using FlycatcherAds.Models;

namespace DataLakeModels.Models.Twitter.Ads {

    public class PromotedTweet : FlycatcherAds.Models.PromotedTweet, IValidityRange, IEquatable<PromotedTweet> {

        public string CampaignId { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public LineItem LineItem { get; set; }

        bool IEquatable<PromotedTweet>.Equals(PromotedTweet other) {
            return Id == other.Id &&
                   LineItemId == other.LineItemId &&
                   CampaignId == other.CampaignId &&
                   TweetId == other.TweetId &&
                   EntityStatus == other.EntityStatus &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   TweetIdSt == other.TweetIdSt &&
                   ApprovalStatus == other.ApprovalStatus &&
                   Deleted == other.Deleted;
        }
    }
}
