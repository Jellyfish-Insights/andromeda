using System;

namespace DataLakeModels.Models.AdWords.Reports {

    public class StructuralCampaignPerformance : IValidityRange, IEquatable<StructuralCampaignPerformance> {

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        public string CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CampaignStatus { get; set; }
        public string ServingStatus { get; set; }
        public string BiddingStrategyId { get; set; }
        public string BiddingStrategyName { get; set; }
        public string BiddingStrategyType { get; set; }

        /**
           This function is for comparing the "values" not the "entity", so it compares all fields that are not part of the key.
         */
        public bool Equals(StructuralCampaignPerformance other) {
            return this.BiddingStrategyId == other.BiddingStrategyId &&
                   this.StartDate == other.StartDate &&
                   this.EndDate == other.EndDate &&
                   this.CampaignStatus == other.CampaignStatus &&
                   this.ServingStatus == other.ServingStatus &&
                   this.BiddingStrategyId == other.BiddingStrategyId &&
                   this.BiddingStrategyName == other.BiddingStrategyName &&
                   this.BiddingStrategyType == other.BiddingStrategyType;
        }
    }
}
