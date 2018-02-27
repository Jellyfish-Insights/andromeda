using System;
using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {
    public class PersonaVersion {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public bool Archived { get; set; }
        public List<SourceObject> AdSets;
        public DateTime UpdateDate { get; set; }
    }
}
