using System;
using System.Collections.Generic;
using TypeScriptBuilder;

namespace ApplicationModels.Models.DataViewModels {

    // This is the original Persona Version Id
    using Id = System.String;

    public class PersonaVersionEdit {
        [TSOptional]
        public bool? Archive { get; set; }
        [TSOptional]
        public List<string> AddedAdSets { get; set; }
        [TSOptional]
        public List<string> RemovedAdSets { get; set; }
        public DateTime UpdateDate { get; set; }
        public EditType Flag { get; set; }
    }

    public class PersonaVersionEdits {
        public Dictionary<Id, PersonaVersionEdit> Edits { get; set; }
    }
}
