using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels.Models.Reels;
using Serilog.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

using Andromeda.Common;
using Andromeda.Common.Extensions;

namespace Jobs.Fetcher.Reels.Helpers {

    public static class ApiDataFetcher {
        public static List<JObject> GetReels(string username, DateTime lastFetch, Logger logger) {
            logger.Information(@"Fetching reels for '" + username + "' after '" + lastFetch + "'.");
            var allPosts = new List<JObject>();
            foreach (var payload in DatabaseManager.GetPayload(username, lastFetch)) {
                allPosts.Add(JObject.Parse(payload));
            }
            logger.Information("Found " + allPosts.Count + " posts.");
            return allPosts;
        }

        public static User GetReelsUserFromJson(JToken userJson) {
            var newUser = new User() {
                Pk = userJson["pk"].ToString(),
                Username = userJson["username"].ToString(),
                FullName = userJson["full_name"].ToString(),
                IsPrivate = userJson["is_private"].ToObject<bool>(),
                IsVerified = userJson["is_verified"].ToObject<bool>(),
                IsUnpublished = userJson["is_unpublished"].ToObject<bool>(),
                ProfilePicId = userJson["profile_pic_id"].ToString(),
                ProfilePicURL = userJson["profile_pic_url"].ToString(),
                HasHighlightReels = userJson["has_highlight_reels"].ToObject<bool>(),
                FollowFrictionType = userJson["follow_friction_type"].ToString(),
                HasAnonymousProfilePicture = userJson["has_anonymous_profile_picture"].ToObject<bool>()
            };

            return newUser;
        }

        public static Caption GetReelsCaptionFromJson(JToken captionJson, string reelId) {
            var newCaption = new Caption() {
                Pk = captionJson["pk"].ToString(),
                Text = captionJson["text"].ToString(),
                Type = captionJson["type"].ToObject<long>(),
                UserId = captionJson["user_id"].ToObject<long>(),
                ReelId = reelId,
                Status = captionJson["status"].ToString(),
                MediaId = captionJson["media_id"].ToString(),
                BitFlags = captionJson["bit_flags"].ToObject<long>(),
                CreatedAt = captionJson["created_at"].ToObject<long>(),
                IsCovered = captionJson["is_covered"].ToObject<bool>(),
                ContentType = captionJson["content_type"].ToString(),
                ShareEnabled = captionJson["share_enabled"].ToObject<bool>(),
                CreatedAtUTC = captionJson["created_at_utc"].ToObject<long>(),
                DidReportAsSpam = captionJson["did_report_as_spam"].ToObject<bool>(),
                PrivateReplyStatus = captionJson["private_reply_status"].ToObject<long>(),
            };

            return newCaption;
        }

        public static ClipsMeta GetReelsClipsFromJson(JToken clipsJson, string reelId) {
            var newClips = new ClipsMeta() {
                ReelId = reelId,
                NuxInfo = clipsJson["nux_info"].ToString(),
                AudioType = clipsJson["audio_type"].ToString(),
                MusicInfo = clipsJson["music_info"].ToString(),
                //MashupInfoId = ,
                ShoppingInfo = clipsJson["shopping_info"].ToString(),
                TemplateInfo = clipsJson["template_info"].ToString(),
                ChallengeInfo = clipsJson["challenge_info"].ToString(),
                FeaturedLabel = clipsJson["featured_label"].ToString(),
                IsSharedToFb = clipsJson["is_shared_to_fb"].ToObject<bool>(),
                AudioRankingClusterId = clipsJson["audio_ranking_info"]["best_audio_cluster_id"].ToString(),
                MusicCanonicalId = clipsJson["music_canonical_id"].ToString(),
                //OriginalSoundInfoId = ,
                AdditionalAudioInfo = clipsJson["additional_audio_info"].ToString(),
                BreakingCreatorInfo = clipsJson["breaking_content_info"].ToString(),
                ReelsOnTheRiseInfo = clipsJson["reels_on_the_rise_info"].ToString(),
                BrandedContentTagInfo = clipsJson["branded_content_tag_info"]["can_add_tag"].ToObject<bool>(),
                AssetRecommendationInfo = clipsJson["asset_recommendation_info"].ToString(),
                ContextualHighlightInfo = clipsJson["contextual_highlight_info"].ToString(),
                ClipsCreationEntryPoint = clipsJson["clips_creation_entry_point"].ToString(),
                ViewerInteractionSettings = clipsJson["viewer_interaction_settings"].ToString()
            };

            return newClips;
        }

        public static CommentInfo GetReelsCommentFromJson(JToken commentJson, string reelId) {
            var newComment = new CommentInfo() {
                Pk = commentJson["pk"].ToString(),
                Text = commentJson["text"].ToString(),
                Type = commentJson["type"].ToObject<long>(),
                UserId = commentJson["user_id"].ToObject<long>(),
                Username = commentJson["user"]["username"].ToString(),
                ReelId = reelId,
                Status = commentJson["status"].ToString(),
                MediaId = commentJson["media_id"].ToString(),
                BitFlags = commentJson["bit_flags"].ToObject<long>(),
                CreatedAt = commentJson["created_at"].ToObject<long>(),
                IsCovered = commentJson["is_covered"].ToObject<bool>(),
                ContentType = commentJson["content_type"].ToString(),
                ShareEnabled = commentJson["share_enabled"].ToObject<bool>(),
                CreatedAtUTC = commentJson["created_at_utc"].ToObject<long>(),
                DidReportAsSpam = commentJson["did_report_as_spam"].ToObject<bool>(),
                PrivateReplyStatus = commentJson["private_reply_status"].ToObject<long>(),
            };

            return newComment;
        }

        public static ConsumptionInfo GetReelsConsumptionInfoFromJson(JToken consumptionInfoJson) {
            var newConsumptionInfo = new ConsumptionInfo() {
                IsBookmarked = consumptionInfoJson["is_bookmarked"].ToObject<bool>(),
                IsTrendingInClips = consumptionInfoJson["is_bookmarked"].ToObject<bool>(),
                ShouldMuteAudioReason = consumptionInfoJson["is_bookmarked"].ToString()
            };
            return newConsumptionInfo;
        }

        public static Friction GetReelsFrictionFromJson(JToken frictionJson, string reelId) {
            var newFriction = new Friction() {
                ReelId = reelId,
                BloksAppUrl = frictionJson["bloks_app_url"].ToString(),
                ShouldHaveSharingFriction = frictionJson["should_have_sharing_friction"].ToObject<long>()
            };

            return newFriction;
        }

        public static ImageVersion GetReelsImageVersionFromJson(JToken imageVersionJson, string reelId) {
            var newImageVersion = new ImageVersion() {
                ReelId = reelId
            };

            return newImageVersion;
        }

        public static Image GetReelsImageFromJson(JToken imageJson, int imageVersionId) {
            var newImage = new Image() {
                ImageVersionId = imageVersionId,
                Url = imageJson["url"].ToString(),
                Width = imageJson["width"].ToObject<long>(),
                Height = imageJson["height"].ToObject<long>()
            };

            return newImage;
        }

        public static AnimatedThumbnail GetReelsAnimatedThumbnailFromJson(JToken thumbnailJson, int imageVersionId) {
            var spriteUrls = new List<string>();
            foreach (var url in thumbnailJson["sprite_urls"]) {
                spriteUrls.Add(url.ToString());
            }
            var newAnimatedThumbnail = new AnimatedThumbnail() {
                ImageVersionId = imageVersionId,
                SpriteUrls = spriteUrls.ToArray(),
                FileSizeKb = thumbnailJson["file_size_kb"].ToObject<long>(),
                SpriteWidth = thumbnailJson["sprite_width"].ToObject<long>(),
                VideoLength = thumbnailJson["video_length"].ToObject<double>(),
                SpriteHeight = thumbnailJson["sprite_height"].ToObject<long>(),
                RenderedWidth = thumbnailJson["rendered_width"].ToObject<long>(),
                ThumbnailWidth = thumbnailJson["thumbnail_width"].ToObject<long>(),
                ThumbnailHeight = thumbnailJson["thumbnail_height"].ToObject<long>(),
                ThumbnailDuration = thumbnailJson["thumbnail_duration"].ToObject<double>(),
                ThumbnailsPerRow = thumbnailJson["thumbnails_per_row"].ToObject<long>(),
                MaxThumbnailsPerSprite = thumbnailJson["max_thumbnails_per_sprite"].ToObject<long>(),
                TotalThumbnailNumPerSprite = thumbnailJson["total_thumbnail_num_per_sprite"].ToObject<long>()
            };

            return newAnimatedThumbnail;
        }

        public static MashupInfo GetReelsMashupInfoFromJson(JToken mashupInfoJson, int clipsId) {
            var newMashupInfo = new MashupInfo() {
                ClipsId = clipsId,
                OriginalMedia = mashupInfoJson["original_media"].ToString(),
                MashupsAllowed = mashupInfoJson["mashups_allowed"].ToObject<bool>(),
                HasBeenMashedUp = mashupInfoJson["has_been_mashed_up"].ToObject<bool>(),
                FormattedMashupsCount = mashupInfoJson["formatted_mashups_count"].ToObject<long?>(),
                CanToggleMashupsAllowed = mashupInfoJson["can_toggle_mashups_allowed"].ToObject<bool>(),
                NonPrivacyFilteredMashupsMediaCount = mashupInfoJson["non_privacy_filtered_mashups_media_count"].ToObject<long>()
            };

            return newMashupInfo;
        }

        public static OriginalSound GetReelsOriginalSoundFromJson(JToken originalSoundJson, string reelId) {
            var audioParts = new List<string>();
            foreach (var part in originalSoundJson["audio_parts"]) {
                audioParts.Add(part.ToString());
            }
            var newOriginalSound = new OriginalSound() {
                UserId = reelId,
                AudioParts = audioParts.ToArray(),
                IsExplicit = originalSoundJson["is_explicit"].ToObject<bool>(),
                TimeCreated = originalSoundJson["time_created"].ToObject<long>(),
                DashManifest = originalSoundJson["dash_manifest"].ToString(),
                HideRemixing = originalSoundJson["hide_remixing"].ToObject<bool>(),
                AudioAssetId = originalSoundJson["audio_asset_id"].ToObject<long>(),
                DurationInMs = originalSoundJson["duration_in_ms"].ToObject<long>(),
                //ConsumptionInfo = originalSoundJson["is_shared_to_fb"].ToObject<bool>(),
                OriginalMediaId = originalSoundJson["original_media_id"].ToString(),
                ShouldMuteAudio = originalSoundJson["should_mute_audio"].ToObject<bool>(),
                OriginalAudioTitle = originalSoundJson["original_audio_title"].ToString(),
                OriginalAudioSubtype = originalSoundJson["original_audio_subtype"].ToString(),
                AllowCreatorToRename = originalSoundJson["allow_creator_to_rename"].ToObject<bool>(),
                ProgressiveDownloadUrl = originalSoundJson["progressive_download_url"].ToString(),
                CanRemixBeSharedToFb = originalSoundJson["can_remix_be_shared_to_fb"].ToObject<bool>(),
                FormattedClipsMediaCount = originalSoundJson["formatted_clips_media_count"].ToObject<long?>(),
                IsAudioAutomaticallyAttributed = originalSoundJson["is_audio_automatically_attributed"].ToObject<bool>()
            };

            return newOriginalSound;
        }

        public static Reel GetReelsFromJson(JToken reelJson, string userId) {
            return new Reel() {
                       Id = reelJson["id"].ToString(),
                       Pk = reelJson["pk"].ToString(),
                       Code = reelJson["code"].ToString(),
                       UserId = userId,
                       //User
                       //Caption
                       //Comments
                       TakenAt = reelJson["taken_at"].ToObject<long>(),
                       HasAudio = reelJson["has_audio"].ToObject<bool>(),
                       HasLiked = reelJson["has_liked"].ToObject<bool>(),
                       //AuthorName = reelJson["like_count"].ToObject<long>(),
                       MediaType = reelJson["media_type"].ToObject<long>(),
                       //Duration = reelJson["play_count"].ToObject<long>(),
                       //Album = reelJson["view_count"].ToObject<long>(),
                       FilterType = reelJson["filter_type"].ToObject<long>(),
                       PhotoOfYou = reelJson["photo_of_you"].ToObject<bool>(),
                       ProductType = reelJson["product_type"].ToString(),
                       //AuthorName = reelJson["comment_count"].ToObject<long>(),
                       //Clips Metadata
                       DeletedReason = reelJson["deleted_reason"].ToObject<long>(),
                       MusicMetadata = reelJson["music_metadata"] == null ? "" : reelJson["music_metadata"].ToString(),
                       OriginalWidth = reelJson["original_width"].ToObject<long>(),
                       VideoDuration = reelJson["video_duration"].ToObject<double>(),
                       //video versions
                       CanViewerSave = reelJson["can_viewer_save"].ToObject<bool>(),
                       // Image Versions
                       OriginalHeight = reelJson["original_height"].ToObject<long>(),
                       ClientCacheKey = reelJson["client_cache_key"].ToString(),
                       DeviceTimestamp = reelJson["device_timestamp"].ToObject<long>(),
                       CreateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(reelJson["device_timestamp"].ToObject<long>() / 1000),
                       IsUnifiedVideo = reelJson["is_unified_video"].ToObject<bool>(),
                       //Preview Comments
                       CaptionIsEdited = reelJson["caption_is_edited"].ToObject<bool>(),
                       HasMoreComments = reelJson["has_more_comments"].ToObject<bool>(),
                       IsInProfileGrid = reelJson["is_in_profile_grid"].ToObject<bool>(),
                       LoggingInfoToken = reelJson["logging_info_token"].ToString(),
                       IsPaidPartnership = reelJson["is_paid_partnership"].ToObject<bool>(),
                       //Cropping info
                       CommercialityStatus = reelJson["commerciality_status"].ToString(),
                       CommentLikesEnabled = reelJson["comment_likes_enabled"].ToObject<bool>(),
                       //Sharing Friction
                       OrganicTrackingToken = reelJson["organic_tracking_token"].ToString(),
                       //Comment inform treatment
                       CanSeeInsightsAsBrand = reelJson["can_see_insights_as_brand"].ToObject<bool>(),
                       CommentThreadingEnabled = reelJson["comment_threading_enabled"].ToObject<bool>(),
                       IntegrityReviewDecision = reelJson["integrity_review_decision"].ToString(),
                       ProfileGridControlEnabled = reelJson["profile_grid_control_enabled"].ToObject<bool>(),
                       LikeAndViewCountsDisabled = reelJson["like_and_view_counts_disabled"].ToObject<bool>(),
                       CanViewMorePreviewComments = reelJson["can_view_more_preview_comments"].ToObject<bool>(),
                       HideViewAllCommentEntrypoint = reelJson["hide_view_all_comment_entrypoint"].ToObject<bool>(),
                       MaxNumVisiblePreviewComments = reelJson["max_num_visible_preview_comments"].ToObject<long>(),
                       OriginalMediaHasVisualReplyMedia = reelJson["original_media_has_visual_reply_media"].ToObject<bool>(),
                       IsVisualReplyCommenterNoticeEnabled = reelJson["is_visual_reply_commenter_notice_enabled"].ToObject<bool>()
            };
        }

        public static ReelStats GetReelsStatsFromJson(JToken reelJson, string userId) {
            return new ReelStats() {
                       ReelId = reelJson["id"].ToString(),
                       UserId = userId,
                       LikeCount = reelJson["like_count"].ToObject<long>(),
                       PlayCount = reelJson["play_count"].ToObject<long>(),
                       ViewCount = reelJson["view_count"].ToObject<long>(),
                       CommentCount = reelJson["comment_count"].ToObject<long>(),
                       EventDate = DateTime.Today,
                       ValidityStart = DateTime.UtcNow,
                       ValidityEnd = DateTime.MaxValue,
            };
        }

        public static SquareCrop GetReelsCropInfoFromJson(JToken cropInfoJson, string reelId) {
            var newSquareCrop = new SquareCrop() {
                ReelId = reelId,
                Top = cropInfoJson["crop_top"].ToObject<double>(),
                Left = cropInfoJson["crop_left"].ToObject<double>(),
                Right = cropInfoJson["crop_right"].ToObject<double>(),
                Bottom = cropInfoJson["crop_bottom"].ToObject<double>()
            };

            return newSquareCrop;
        }

        public static VideoVersion GetReelsVideoVersionFromJson(JToken videoVersionJson, string reelId) {
            var newVideoVersion = new VideoVersion() {
                ReelId = reelId,
                Id = videoVersionJson["id"].ToString(),
                Url = videoVersionJson["url"].ToString(),
                Type = videoVersionJson["type"].ToObject<long>(),
                Width = videoVersionJson["width"].ToObject<long>(),
                Height = videoVersionJson["height"].ToObject<long>()
            };

            return newVideoVersion;
        }
    }
}
