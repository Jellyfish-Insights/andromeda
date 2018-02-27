using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationModels.Models {

    public class ApplicationGenericTag {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Tag { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
