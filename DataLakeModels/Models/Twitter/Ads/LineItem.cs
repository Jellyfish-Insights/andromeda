using System;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Ads {

    public class LineItem : IValidityRange, IEquatable<LineItem> {

        public string Id { get; set; }
        public string CampaignId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public long? BidAmountLocalMicro { get; set; }
        public string AdvertiserDomain { get; set; }
        public long? TargetCpaLocalMicro { get; set; }
        public string PrimaryWebEventTag { get; set; }
        public string Goal { get; set; }
        public string ProductType { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string BidStrategy { get; set; }
        public int? DurationInDays { get; set; }
        public long? TotalBudgetAmountLocalMicro { get; set; }
        public string Objective { get; set; }
        public string EntityStatus { get; set; }
        public int? FrequencyCap { get; set; }
        public string Currency { get; set; }
        public string PayBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string CreativeSource { get; set; }
        public bool Deleted { get; set; }

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        public Campaign Campaign { get; set; }
        public ICollection<PromotedTweet> PromotedTweets { get; set; }

        bool IEquatable<LineItem>.Equals(LineItem other) {

            return Id == other.Id &&
                   CampaignId == other.CampaignId &&
                   Name == other.Name &&
                   StartTime == other.StartTime &&
                   BidAmountLocalMicro == other.BidAmountLocalMicro &&
                   AdvertiserDomain == other.AdvertiserDomain &&
                   TargetCpaLocalMicro == other.TargetCpaLocalMicro &&
                   PrimaryWebEventTag == other.PrimaryWebEventTag &&
                   Goal == other.Goal &&
                   ProductType == other.ProductType &&
                   EndTime == other.EndTime &&
                   BidStrategy == other.BidStrategy &&
                   DurationInDays == other.DurationInDays &&
                   TotalBudgetAmountLocalMicro == other.TotalBudgetAmountLocalMicro &&
                   Objective == other.Objective &&
                   EntityStatus == other.EntityStatus &&
                   FrequencyCap == other.FrequencyCap &&
                   Currency == other.Currency &&
                   PayBy == other.PayBy &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   CreativeSource == other.CreativeSource &&
                   Deleted == other.Deleted;
        }
    }
}
