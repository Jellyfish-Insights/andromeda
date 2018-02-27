using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;
namespace ApplicationModels.Models {
    public abstract class BaseApplicationVideoSourceVideo : IMutableEntity {
        public int ApplicationVideoId { get; set; }
        public ApplicationVideo ApplicationVideo { get; set; }
        public string SourceVideoId { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(ApplicationVideoId, SourceVideoId); }
    }

    public class GeneratedApplicationVideoSourceVideo : BaseApplicationVideoSourceVideo {}

    public class UserApplicationVideoSourceVideo : BaseApplicationVideoSourceVideo {
        public bool Suppress { get; set; } = false;
    }

    public class ApplicationVideoSourceVideo : BaseApplicationVideoSourceVideo {}
}
