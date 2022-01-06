using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class EffectSticker : IEquatable<EffectSticker> {

        public string Id { get; set; }
        public string Name { get; set; }
        bool IEquatable<EffectSticker>.Equals(EffectSticker other) {
            return Id == other.Id &&
                   Name == other.Name;
        }
    }
}
