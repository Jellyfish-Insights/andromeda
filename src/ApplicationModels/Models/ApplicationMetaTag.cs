using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ApplicationModels.Models {
    public class ApplicationMetaTag {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TypeId { get; set; }
        public ApplicationMetaTagType Type { get; set; }
        public string Tag { get; set; }
        public DateTime UpdateDate  { get; set; }
        // MetaTags of type Tone have specific colors,
        // they should be assigned here
        // For other types, it is null
        public string Color { get; set; }
    }
}
