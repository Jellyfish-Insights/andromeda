using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceAudience : IMutableEntity {
        [Key]
        public string Id { get; set; }
        public string Platform { get; set; }
        public string Title { get; set; }

        [Column(TypeName = "jsonb")]
        public string Definition { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(Id); }
    }
}
