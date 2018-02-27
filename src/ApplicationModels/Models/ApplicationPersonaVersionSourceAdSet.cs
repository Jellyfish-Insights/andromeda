using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public abstract class BaseApplicationPersonaVersionSourceAdSet : IMutableEntity {
        public int PersonaVersionId { get; set; }
        public ApplicationPersonaVersion PersonaVersion { get; set; }
        public string AdSetId { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey => MutableEntityExtentions.AutoPK(AdSetId);
    }

    public class GeneratedApplicationPersonaVersionSourceAdSet : BaseApplicationPersonaVersionSourceAdSet {}

    public class UserApplicationPersonaVersionSourceAdSet : BaseApplicationPersonaVersionSourceAdSet {
        public bool Suppress { get; set; } = false;
    }

    public class ApplicationPersonaVersionSourceAdSet : BaseApplicationPersonaVersionSourceAdSet {}
}
