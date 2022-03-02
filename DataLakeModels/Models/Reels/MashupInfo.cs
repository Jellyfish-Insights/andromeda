using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class MashupInfo : IEquatable<MashupInfo> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ClipsMeta Clip { get; set; }
        public int ClipsId { get; set; }
        public string OriginalMedia { get; set; }
        public bool MashupsAllowed { get; set; }
        public bool HasBeenMashedUp { get; set; }
        public long? FormattedMashupsCount { get; set; }
        public bool CanToggleMashupsAllowed { get; set; }
        public long NonPrivacyFilteredMashupsMediaCount { get; set; }

        bool IEquatable<MashupInfo>.Equals(MashupInfo other) {
            return Id == other.Id &&
                   OriginalMedia == other.OriginalMedia &&
                   MashupsAllowed == other.MashupsAllowed &&
                   HasBeenMashedUp == other.HasBeenMashedUp &&
                   FormattedMashupsCount == other.FormattedMashupsCount &&
                   CanToggleMashupsAllowed == other.CanToggleMashupsAllowed &&
                   NonPrivacyFilteredMashupsMediaCount == other.NonPrivacyFilteredMashupsMediaCount;
        }
    }
}
