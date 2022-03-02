﻿// <auto-generated />
using System;
using DataLakeModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataLakeModels.Migrations.DataLakeReels
{
    [DbContext(typeof(DataLakeReelsContext))]
    [Migration("20220221152936_FirstMigration")]
    partial class FirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("reels_v1")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DataLakeModels.Models.Reels.AnimatedThumbnail", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("FileSizeKb");

                    b.Property<int>("ImageVersionId");

                    b.Property<long>("MaxThumbnailsPerSprite");

                    b.Property<long>("RenderedWidth");

                    b.Property<long>("SpriteHeight");

                    b.Property<string[]>("SpriteUrls")
                        .HasColumnType("text[]");

                    b.Property<long>("SpriteWidth");

                    b.Property<double>("ThumbnailDuration");

                    b.Property<long>("ThumbnailHeight");

                    b.Property<long>("ThumbnailWidth");

                    b.Property<long>("ThumbnailsPerRow");

                    b.Property<long>("TotalThumbnailNumPerSprite");

                    b.Property<double>("VideoLength");

                    b.HasKey("Id");

                    b.HasIndex("ImageVersionId")
                        .IsUnique();

                    b.ToTable("AnimatedThumbnails");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Caption", b =>
                {
                    b.Property<string>("Pk")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("BitFlags");

                    b.Property<string>("ContentType");

                    b.Property<long>("CreatedAt");

                    b.Property<long>("CreatedAtUTC");

                    b.Property<bool>("DidReportAsSpam");

                    b.Property<bool>("IsCovered");

                    b.Property<string>("MediaId");

                    b.Property<long>("PrivateReplyStatus");

                    b.Property<string>("ReelId");

                    b.Property<bool>("ShareEnabled");

                    b.Property<string>("Status");

                    b.Property<string>("Text");

                    b.Property<long>("Type");

                    b.Property<long>("UserId");

                    b.HasKey("Pk");

                    b.HasIndex("ReelId")
                        .IsUnique();

                    b.ToTable("Captions");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ClipsMeta", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AdditionalAudioInfo");

                    b.Property<string>("AssetRecommendationInfo");

                    b.Property<string>("AudioRankingClusterId");

                    b.Property<string>("AudioType");

                    b.Property<bool>("BrandedContentTagInfo");

                    b.Property<string>("BreakingContentInfo");

                    b.Property<string>("BreakingCreatorInfo");

                    b.Property<string>("ChallengeInfo");

                    b.Property<string>("ClipsCreationEntryPoint");

                    b.Property<string>("ContextualHighlightInfo");

                    b.Property<string>("FeaturedLabel");

                    b.Property<bool>("IsSharedToFb");

                    b.Property<string>("MusicCanonicalId");

                    b.Property<string>("MusicInfo");

                    b.Property<string>("NuxInfo");

                    b.Property<int?>("OriginalSoundInfoId");

                    b.Property<string>("ReelId");

                    b.Property<string>("ReelsOnTheRiseInfo");

                    b.Property<string>("ShoppingInfo");

                    b.Property<string>("TemplateInfo");

                    b.Property<string>("ViewerInteractionSettings");

                    b.HasKey("Id");

                    b.HasIndex("OriginalSoundInfoId");

                    b.HasIndex("ReelId")
                        .IsUnique();

                    b.ToTable("ClipsMetas");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.CommentInfo", b =>
                {
                    b.Property<string>("Pk")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("BitFlags");

                    b.Property<string>("ContentType");

                    b.Property<long>("CreatedAt");

                    b.Property<long>("CreatedAtUTC");

                    b.Property<bool>("DidReportAsSpam");

                    b.Property<bool>("IsCovered");

                    b.Property<string>("MediaId");

                    b.Property<long>("PrivateReplyStatus");

                    b.Property<string>("ReelId");

                    b.Property<string>("ReelId1");

                    b.Property<bool>("ShareEnabled");

                    b.Property<string>("Status");

                    b.Property<string>("Text");

                    b.Property<long>("Type");

                    b.Property<long>("UserId");

                    b.Property<string>("Username");

                    b.HasKey("Pk");

                    b.HasIndex("ReelId");

                    b.HasIndex("ReelId1");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ConsumptionInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsBookmarked");

                    b.Property<bool>("IsTrendingInClips");

                    b.Property<string>("ShouldMuteAudioReason");

                    b.HasKey("Id");

                    b.ToTable("ConsumptionInfos");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Friction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BloksAppUrl");

                    b.Property<string>("ReelId");

                    b.Property<long>("ShouldHaveSharingFriction");

                    b.HasKey("Id");

                    b.HasIndex("ReelId")
                        .IsUnique();

                    b.ToTable("Frictions");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Image", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Height");

                    b.Property<int>("ImageVersionId");

                    b.Property<string>("Url");

                    b.Property<long>("Width");

                    b.HasKey("Id");

                    b.HasIndex("ImageVersionId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ImageVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ReelId");

                    b.HasKey("Id");

                    b.HasIndex("ReelId")
                        .IsUnique();

                    b.ToTable("ImageVersions");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.MashupInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CanToggleMashupsAllowed");

                    b.Property<int>("ClipsId");

                    b.Property<long?>("FormattedMashupsCount");

                    b.Property<bool>("HasBeenMashedUp");

                    b.Property<bool>("MashupsAllowed");

                    b.Property<long>("NonPrivacyFilteredMashupsMediaCount");

                    b.Property<string>("OriginalMedia");

                    b.HasKey("Id");

                    b.HasIndex("ClipsId")
                        .IsUnique();

                    b.ToTable("MashupInfos");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.OriginalSound", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("AllowCreatorToRename");

                    b.Property<long>("AudioAssetId");

                    b.Property<string[]>("AudioParts")
                        .HasColumnType("text[]");

                    b.Property<bool>("CanRemixBeSharedToFb");

                    b.Property<int?>("ConsumptionInfoId");

                    b.Property<string>("DashManifest");

                    b.Property<long>("DurationInMs");

                    b.Property<long?>("FormattedClipsMediaCount");

                    b.Property<bool>("HideRemixing");

                    b.Property<bool>("IsAudioAutomaticallyAttributed");

                    b.Property<bool>("IsExplicit");

                    b.Property<string>("OriginalAudioSubtype");

                    b.Property<string>("OriginalAudioTitle");

                    b.Property<string>("OriginalMediaId");

                    b.Property<string>("ProgressiveDownloadUrl");

                    b.Property<bool>("ShouldMuteAudio");

                    b.Property<long>("TimeCreated");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ConsumptionInfoId");

                    b.HasIndex("UserId");

                    b.ToTable("OriginalSounds");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Reel", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CanSeeInsightsAsBrand");

                    b.Property<bool>("CanViewMorePreviewComments");

                    b.Property<bool>("CanViewerSave");

                    b.Property<bool>("CaptionIsEdited");

                    b.Property<string>("ClientCacheKey");

                    b.Property<string>("Code");

                    b.Property<string>("CommentInformTreatment");

                    b.Property<bool>("CommentLikesEnabled");

                    b.Property<bool>("CommentThreadingEnabled");

                    b.Property<string>("CommercialityStatus");

                    b.Property<DateTime>("CreateTime");

                    b.Property<long>("DeletedReason");

                    b.Property<long>("DeviceTimestamp");

                    b.Property<long>("FilterType");

                    b.Property<bool>("HasAudio");

                    b.Property<bool>("HasLiked");

                    b.Property<bool>("HasMoreComments");

                    b.Property<bool>("HideViewAllCommentEntrypoint");

                    b.Property<string>("IntegrityReviewDecision");

                    b.Property<bool>("IsInProfileGrid");

                    b.Property<bool>("IsPaidPartnership");

                    b.Property<bool>("IsUnifiedVideo");

                    b.Property<bool>("IsVisualReplyCommenterNoticeEnabled");

                    b.Property<bool>("LikeAndViewCountsDisabled");

                    b.Property<string>("LoggingInfoToken");

                    b.Property<long>("MaxNumVisiblePreviewComments");

                    b.Property<long>("MediaType");

                    b.Property<string>("MusicMetadata");

                    b.Property<string>("OrganicTrackingToken");

                    b.Property<long>("OriginalHeight");

                    b.Property<bool>("OriginalMediaHasVisualReplyMedia");

                    b.Property<long>("OriginalWidth");

                    b.Property<bool>("PhotoOfYou");

                    b.Property<string>("Pk");

                    b.Property<string>("ProductType");

                    b.Property<bool>("ProfileGridControlEnabled");

                    b.Property<bool>("ShouldHaveInformTreatment");

                    b.Property<long>("TakenAt");

                    b.Property<string>("UserId");

                    b.Property<double>("VideoDuration");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Reels");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ReelStats", b =>
                {
                    b.Property<string>("ReelId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<long>("CommentCount");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("date");

                    b.Property<long>("LikeCount");

                    b.Property<long>("PlayCount");

                    b.Property<string>("UserId");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<long>("ViewCount");

                    b.HasKey("ReelId", "ValidityStart");

                    b.ToTable("ReelStats");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.SquareCrop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Bottom");

                    b.Property<double>("Left");

                    b.Property<string>("ReelId");

                    b.Property<double>("Right");

                    b.Property<double>("Top");

                    b.HasKey("Id");

                    b.HasIndex("ReelId")
                        .IsUnique();

                    b.ToTable("SquareCrops");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.User", b =>
                {
                    b.Property<string>("Pk")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FollowFrictionType");

                    b.Property<string>("FullName");

                    b.Property<bool>("HasAnonymousProfilePicture");

                    b.Property<bool>("HasHighlightReels");

                    b.Property<bool>("IsPrivate");

                    b.Property<bool>("IsUnpublished");

                    b.Property<bool>("IsVerified");

                    b.Property<string>("ProfilePicId");

                    b.Property<string>("ProfilePicURL");

                    b.Property<string>("Username");

                    b.HasKey("Pk");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.VideoVersion", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Height");

                    b.Property<string>("ReelId");

                    b.Property<long>("Type");

                    b.Property<string>("Url");

                    b.Property<long>("Width");

                    b.HasKey("Id");

                    b.HasIndex("ReelId");

                    b.ToTable("VideoVersions");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.AnimatedThumbnail", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.ImageVersion", "ImageVersion")
                        .WithOne("AnimatedThumbnailSpritesheetInfo")
                        .HasForeignKey("DataLakeModels.Models.Reels.AnimatedThumbnail", "ImageVersionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Caption", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithOne("Caption")
                        .HasForeignKey("DataLakeModels.Models.Reels.Caption", "ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ClipsMeta", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.OriginalSound", "OriginalSoundInfo")
                        .WithMany()
                        .HasForeignKey("OriginalSoundInfoId");

                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithOne("ClipsMetaData")
                        .HasForeignKey("DataLakeModels.Models.Reels.ClipsMeta", "ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.CommentInfo", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithMany("Comments")
                        .HasForeignKey("ReelId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataLakeModels.Models.Reels.Reel")
                        .WithMany("PreviewComments")
                        .HasForeignKey("ReelId1");
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Friction", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithOne("SharingFrictionInfo")
                        .HasForeignKey("DataLakeModels.Models.Reels.Friction", "ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Image", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.ImageVersion", "ImageVersion")
                        .WithMany("Candidates")
                        .HasForeignKey("ImageVersionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ImageVersion", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithOne("ImageVersions")
                        .HasForeignKey("DataLakeModels.Models.Reels.ImageVersion", "ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.MashupInfo", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.ClipsMeta", "Clip")
                        .WithOne("MashupInfo")
                        .HasForeignKey("DataLakeModels.Models.Reels.MashupInfo", "ClipsId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.OriginalSound", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.ConsumptionInfo", "ConsumptionInfo")
                        .WithMany()
                        .HasForeignKey("ConsumptionInfoId");

                    b.HasOne("DataLakeModels.Models.Reels.User", "User")
                        .WithMany("Sounds")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.Reel", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.User", "User")
                        .WithMany("Reels")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.ReelStats", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithMany("Stats")
                        .HasForeignKey("ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.SquareCrop", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithOne("MediaCroppingInfo")
                        .HasForeignKey("DataLakeModels.Models.Reels.SquareCrop", "ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Reels.VideoVersion", b =>
                {
                    b.HasOne("DataLakeModels.Models.Reels.Reel", "Reel")
                        .WithMany("VideoVersions")
                        .HasForeignKey("ReelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
