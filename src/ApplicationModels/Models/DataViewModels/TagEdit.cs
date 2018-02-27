using System;
using System.Collections.Generic;
using TypeScriptBuilder;

namespace ApplicationModels.Models.DataViewModels {

    // This is the original tag value or an arbitrary unique string in case of new tags
    using Id = System.String;

    public class TagEdit {
        [TSOptional]
        public string Name { get; set; }
        public DateTime UpdateDate { get; set; }
        public EditType Flag { get; set; }
    }

    public class TagEdits {
        public string Type { get; set; }
        public Dictionary<Id, TagEdit> Edits { get; set; }
    }
}
