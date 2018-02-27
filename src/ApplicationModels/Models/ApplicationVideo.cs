using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class ApplicationVideo : IMutableEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }

        public bool Archived { get; set; } = false;

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey => MutableEntityExtentions.AutoPK(Id);
    }
}
