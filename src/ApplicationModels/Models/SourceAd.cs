using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceAd : IMutableEntity {
        [Key]
        public string Id { get; set; }
        public string Platform { get; set; }
        public string Title { get; set; }
        public String VideoId { get; set; }
        public SourceVideo Video { get; set; }
        public string AdSetId { get; set; }
        public SourceAdSet AdSet { get; set; }
        public string CampaignId { get; set; }
        public SourceCampaign Campaign { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(Id); }
    }
}
