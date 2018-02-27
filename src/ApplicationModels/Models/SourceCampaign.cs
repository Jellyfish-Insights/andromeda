using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceCampaign : IMutableEntity {
        [Key]
        public string Id { get; set; }
        public string Platform { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Objective { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(Id); }
    }
}
