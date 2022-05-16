using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class ClipsMeta : IEquatable<ClipsMeta> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public string NuxInfo { get; set; }
        public string AudioType { get; set; }
        public string MusicInfo { get; set; }
        public MashupInfo MashupInfo { get; set; }
        public string ShoppingInfo { get; set; }
        public string TemplateInfo { get; set; }
        public string ChallengeInfo { get; set; }
        public string FeaturedLabel { get; set; }
        public bool IsSharedToFb { get; set; }
        public string AudioRankingClusterId { get; set; }
        public string MusicCanonicalId { get; set; }
        public OriginalSound OriginalSoundInfo { get; set; }
        public string AdditionalAudioInfo { get; set; }
        public string BreakingContentInfo { get; set; }
        public string BreakingCreatorInfo { get; set; }
        public string ReelsOnTheRiseInfo { get; set; }
        public bool BrandedContentTagInfo { get; set; }
        public string AssetRecommendationInfo { get; set; }
        public string ContextualHighlightInfo { get; set; }
        public string ClipsCreationEntryPoint { get; set; }
        public string ViewerInteractionSettings { get; set; }

        bool IEquatable<ClipsMeta>.Equals(ClipsMeta other) {
            return Id == other.Id &&
                   ReelId == other.ReelId &&
                   NuxInfo == other.NuxInfo &&
                   AudioType == other.AudioType &&
                   MusicInfo == other.MusicInfo &&
                   MashupInfo == other.MashupInfo &&
                   ShoppingInfo == other.ShoppingInfo &&
                   TemplateInfo == other.TemplateInfo &&
                   ChallengeInfo == other.ChallengeInfo &&
                   FeaturedLabel == other.FeaturedLabel &&
                   IsSharedToFb == other.IsSharedToFb &&
                   AudioRankingClusterId == other.AudioRankingClusterId &&
                   MusicCanonicalId == other.MusicCanonicalId &&
                   OriginalSoundInfo == other.OriginalSoundInfo;
        }
    }
}
