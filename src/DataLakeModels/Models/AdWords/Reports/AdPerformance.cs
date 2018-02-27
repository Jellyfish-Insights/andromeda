using System;

namespace DataLakeModels.Models.AdWords.Reports {

    public class AdPerformance : IValidityRange, IEquatable<AdPerformance> {

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        public string CampaignId { get; set; }
        public string AdGroupId { get; set; }

        // The actual name in the selector for this field is "Id"
        public string AdId { get; set; }
        public string Headline { get; set; }
        public string Date { get; set; }
        public string Impressions { get; set; }
        public string VideoViews { get; set; }
        public string Clicks { get; set; }
        public string Engagements { get; set; }
        public string AverageCpm { get; set; }
        public string AverageCpv { get; set; }
        public string AverageCpc { get; set; }
        public string AverageCpe { get; set; }
        public string Cost { get; set; }

        /**
           This function is for comparing the "values" not the "entity", so it compares all fields that are not part of the key.
         */
        public bool Equals(AdPerformance other) {
            return this.AdGroupId == other.AdGroupId &&
                   this.AverageCpc == other.AverageCpc &&
                   this.AverageCpe == other.AverageCpe &&
                   this.AverageCpm == other.AverageCpm &&
                   this.AverageCpv == other.AverageCpv &&
                   this.CampaignId == other.CampaignId &&
                   this.Clicks == other.Clicks &&
                   this.Cost == other.Cost &&
                   this.Engagements == other.Engagements &&
                   this.Headline == other.Headline &&
                   this.Impressions == other.Impressions &&
                   this.VideoViews == other.VideoViews;
        }
    }
}
