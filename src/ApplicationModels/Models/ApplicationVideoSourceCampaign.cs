using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public abstract class BaseApplicationVideoSourceCampaign : IMutableEntity {
        public int VideoId { get; set; }
        public ApplicationVideo Video { get; set; }
        public string CampaignId { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey => MutableEntityExtentions.AutoPK(CampaignId);
    }

    public class GeneratedApplicationVideoSourceCampaign : BaseApplicationVideoSourceCampaign {}

    public class UserApplicationVideoSourceCampaign : BaseApplicationVideoSourceCampaign {
        public bool Suppress { get; set; } = false;
    }

    public class ApplicationVideoSourceCampaign : BaseApplicationVideoSourceCampaign {}
}
