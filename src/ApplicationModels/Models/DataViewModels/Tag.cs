using System;
using TypeScriptBuilder;

namespace ApplicationModels.Models.DataViewModels {
    public class Tag : IEquatable<Tag> {
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime UpdateDate { get; set; }
        [TSOptional]
        public string Color { get; set; }

        public bool Equals(Tag other) {
            return Type == other.Type && Value == other.Value;
        }

        public override int GetHashCode() {
            return string.Format("{0}_{1}", Type, Value).GetHashCode();
        }
    }
}
