using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class ConsumptionInfo : IEquatable<ConsumptionInfo> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool IsBookmarked { get; set; }
        public bool IsTrendingInClips { get; set; }
        public string ShouldMuteAudioReason { get; set; }

        bool IEquatable<ConsumptionInfo>.Equals(ConsumptionInfo other) {
            return Id == other.Id &&
                   IsBookmarked == other.IsBookmarked &&
                   IsTrendingInClips == other.IsTrendingInClips &&
                   ShouldMuteAudioReason == other.ShouldMuteAudioReason;
        }
    }
}
