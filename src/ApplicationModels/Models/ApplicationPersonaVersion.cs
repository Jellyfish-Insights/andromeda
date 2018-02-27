using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class ApplicationPersonaVersion : IMutableEntity {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Version { get; set; }

        public int PersonaId { get; set; }
        public ApplicationPersona Persona { get; set; }

        public bool Archived { get; set; } = false;

        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(Id); }
    }
}
