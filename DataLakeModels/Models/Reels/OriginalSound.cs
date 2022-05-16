using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {

    public class OriginalSound : IEquatable<OriginalSound> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        [Column(TypeName = "text[]")]
        public string[] AudioParts { get; set; }
        public bool IsExplicit { get; set; }
        public long TimeCreated { get; set; }
        public string DashManifest { get; set; }
        public bool HideRemixing { get; set; }
        public long AudioAssetId { get; set; }
        public long DurationInMs { get; set; }
        public ConsumptionInfo ConsumptionInfo { get; set; }
        public string OriginalMediaId { get; set; }
        public bool ShouldMuteAudio { get; set; }
        public string OriginalAudioTitle { get; set; }
        public string OriginalAudioSubtype { get; set; }
        public bool AllowCreatorToRename { get; set; }
        public string ProgressiveDownloadUrl { get; set; }
        public bool CanRemixBeSharedToFb { get; set; }
        public long? FormattedClipsMediaCount { get; set; }
        public bool IsAudioAutomaticallyAttributed { get; set; }

        bool IEquatable<OriginalSound>.Equals(OriginalSound other) {
            return Id == other.Id &&
                   UserId == other.UserId &&
                   AudioParts == other.AudioParts &&
                   IsExplicit == other.IsExplicit &&
                   TimeCreated == other.TimeCreated &&
                   DashManifest == other.DashManifest &&
                   HideRemixing == other.HideRemixing &&
                   AudioAssetId == other.AudioAssetId &&
                   DurationInMs == other.DurationInMs &&
                   ConsumptionInfo == other.ConsumptionInfo &&
                   OriginalMediaId == other.OriginalMediaId &&
                   ShouldMuteAudio == other.ShouldMuteAudio &&
                   OriginalAudioTitle == other.OriginalAudioTitle &&
                   OriginalAudioSubtype == other.OriginalAudioSubtype &&
                   AllowCreatorToRename == other.AllowCreatorToRename &&
                   ProgressiveDownloadUrl == other.ProgressiveDownloadUrl &&
                   CanRemixBeSharedToFb == other.CanRemixBeSharedToFb &&
                   FormattedClipsMediaCount == other.FormattedClipsMediaCount &&
                   IsAudioAutomaticallyAttributed == other.IsAudioAutomaticallyAttributed;
        }
    }
}
