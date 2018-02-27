using System;

namespace DataLakeModels.Models.AdWords.Reports {

    public class StructuralCriteriaPerformance : IValidityRange, IEquatable<StructuralCriteriaPerformance> {

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public string KeywordId { get; set; }

        public string CampaignId { get; set; }
        public string AdGroupId { get; set; }
        public string AdGroupName { get; set; }
        public string Criteria { get; set; }
        public string CriteriaType { get; set; }
        public string DisplayName { get; set; }
        public string IsNegative { get; set; }

        /**
           This function is for comparing the "values" not the "entity", so it compares all fields that are not part of the key.
         */
        public bool Equals(StructuralCriteriaPerformance other) {
            return this.KeywordId == other.KeywordId &&
                   this.CampaignId == other.CampaignId &&
                   this.Criteria == other.Criteria &&
                   this.CriteriaType == other.CriteriaType &&
                   this.DisplayName == other.DisplayName &&
                   this.IsNegative == other.IsNegative &&
                   this.AdGroupName == other.AdGroupName;
        }
    }
}
