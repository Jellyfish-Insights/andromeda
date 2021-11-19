using System;
using System.Collections.Generic;
using DataLakeModels.Models;

namespace DataLakeModels.Models.Twitter.Ads {

    public class Campaign : IValidityRange, IEquatable<Campaign> {

        public string Id { get; set; }
        public string AdsAccountId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public bool Servable { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public string EffectiveStatus { get; set; }
        public long DailyBudgetAmountLocalMicro { get; set; }
        public string FundingInstrumentId { get; set; }
        public int? DurationInDays { get; set; }
        public bool StandardDelivery { get; set; }
        public long? TotalBudgetAmountLocalMicro { get; set; }
        public string EntityStatus { get; set; }
        public int? FrequencyCap { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Deleted { get; set; }

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        public AdsAccount AdsAccount { get; set; }
        public ICollection<LineItem> LineItems { get; set; }

        bool IEquatable<Campaign>.Equals(Campaign other) {
            return Id == other.Id &&
                   AdsAccountId == other.AdsAccountId &&
                   Name == other.Name &&
                   StartTime == other.StartTime &&
                   EndTime == other.EndTime &&
                   Servable == other.Servable &&
                   PurchaseOrderNumber == other.PurchaseOrderNumber &&
                   EffectiveStatus == other.EffectiveStatus &&
                   DailyBudgetAmountLocalMicro == other.DailyBudgetAmountLocalMicro &&
                   FundingInstrumentId == other.FundingInstrumentId &&
                   DurationInDays == other.DurationInDays &&
                   StandardDelivery == other.StandardDelivery &&
                   TotalBudgetAmountLocalMicro == other.TotalBudgetAmountLocalMicro &&
                   EntityStatus == other.EntityStatus &&
                   FrequencyCap == other.FrequencyCap &&
                   Currency == other.Currency &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   Deleted == other.Deleted;
        }
    }
}
