using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.Reels {
    public class Reel : IEquatable<Reel> {
        public string Id { get; set; }
        public string Pk { get; set; }
        public string Code { get; set; }
        public User User { get; set; }
        public string UserId { get; set; }
        public Caption Caption { get; set; }
        public ICollection<CommentInfo> Comments { get; set; }
        public long TakenAt { get; set; }
        public bool HasAudio { get; set; }
        public bool HasLiked { get; set; }
        public long MediaType { get; set; }
        public long FilterType { get; set; }
        public bool PhotoOfYou { get; set; }
        public string ProductType { get; set; }
        public ClipsMeta ClipsMetaData { get; set; }
        public long DeletedReason { get; set; }
        public string MusicMetadata { get; set; }
        public long OriginalWidth { get; set; }
        public double VideoDuration { get; set; }
        public ICollection<VideoVersion> VideoVersions { get; set; }
        public bool CanViewerSave { get; set; }
        public ImageVersion ImageVersions { get; set; }
        public long OriginalHeight { get; set; }
        public string ClientCacheKey { get; set; }
        public long DeviceTimestamp { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsUnifiedVideo { get; set; }
        public ICollection<CommentInfo> PreviewComments { get; set; }
        public bool CaptionIsEdited { get; set; }
        public bool HasMoreComments { get; set; }
        public bool IsInProfileGrid { get; set; }
        public string LoggingInfoToken { get; set; }
        public bool IsPaidPartnership { get; set; }
        public SquareCrop MediaCroppingInfo { get; set; }
        public string CommercialityStatus { get; set; }
        public bool CommentLikesEnabled { get; set; }
        public Friction SharingFrictionInfo { get; set; }
        public string OrganicTrackingToken { get; set; }
        public string CommentInformTreatment { get; set; }
        public bool ShouldHaveInformTreatment { get; set; }
        public bool CanSeeInsightsAsBrand { get; set; }
        public bool CommentThreadingEnabled { get; set; }
        public string IntegrityReviewDecision { get; set; }
        public bool ProfileGridControlEnabled { get; set; }
        public bool LikeAndViewCountsDisabled { get; set; }
        public bool CanViewMorePreviewComments { get; set; }
        public bool HideViewAllCommentEntrypoint { get; set; }
        public long MaxNumVisiblePreviewComments { get; set; }
        public bool OriginalMediaHasVisualReplyMedia { get; set; }
        public bool IsVisualReplyCommenterNoticeEnabled { get; set; }

        public ICollection<ReelStats> Stats { get; set; }

        bool IEquatable<Reel>.Equals(Reel other) {
            return Id == other.Id &&
                   Pk == other.Pk &&
                   Code == other.Code &&
                   UserId == other.UserId &&
                   Caption == other.Caption &&
                   TakenAt == other.TakenAt &&
                   HasAudio == other.HasAudio &&
                   HasLiked == other.HasLiked &&
                   MediaType == other.MediaType &&
                   FilterType == other.FilterType &&
                   PhotoOfYou == other.PhotoOfYou &&
                   ProductType == other.ProductType &&
                   ClipsMetaData == other.ClipsMetaData &&
                   DeletedReason == other.DeletedReason &&
                   MusicMetadata == other.MusicMetadata &&
                   OriginalWidth == other.OriginalWidth &&
                   VideoDuration == other.VideoDuration &&
                   VideoVersions == other.VideoVersions &&
                   CanViewerSave == other.CanViewerSave &&
                   ImageVersions == other.ImageVersions &&
                   OriginalHeight == other.OriginalHeight &&
                   ClientCacheKey == other.ClientCacheKey &&
                   DeviceTimestamp == other.DeviceTimestamp &&
                   IsUnifiedVideo == other.IsUnifiedVideo &&
                   PreviewComments == other.PreviewComments &&
                   CaptionIsEdited == other.CaptionIsEdited &&
                   HasMoreComments == other.HasMoreComments &&
                   IsInProfileGrid == other.IsInProfileGrid &&
                   LoggingInfoToken == other.LoggingInfoToken &&
                   IsPaidPartnership == other.IsPaidPartnership &&
                   MediaCroppingInfo == other.MediaCroppingInfo &&
                   CommercialityStatus == other.CommercialityStatus &&
                   CommentLikesEnabled == other.CommentLikesEnabled &&
                   SharingFrictionInfo == other.SharingFrictionInfo &&
                   OrganicTrackingToken == other.OrganicTrackingToken &&
                   CommentInformTreatment == other.CommentInformTreatment &&
                   ShouldHaveInformTreatment == other.ShouldHaveInformTreatment &&
                   CanSeeInsightsAsBrand == other.CanSeeInsightsAsBrand &&
                   CommentThreadingEnabled == other.CommentThreadingEnabled &&
                   IntegrityReviewDecision == other.IntegrityReviewDecision &&
                   ProfileGridControlEnabled == other.ProfileGridControlEnabled &&
                   LikeAndViewCountsDisabled == other.LikeAndViewCountsDisabled &&
                   CanViewMorePreviewComments == other.CanViewMorePreviewComments &&
                   HideViewAllCommentEntrypoint == other.HideViewAllCommentEntrypoint &&
                   MaxNumVisiblePreviewComments == other.MaxNumVisiblePreviewComments &&
                   OriginalMediaHasVisualReplyMedia == other.OriginalMediaHasVisualReplyMedia &&
                   IsVisualReplyCommenterNoticeEnabled == other.IsVisualReplyCommenterNoticeEnabled;
        }
    }
}
