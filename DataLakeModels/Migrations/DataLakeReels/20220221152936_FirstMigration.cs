using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataLakeModels.Migrations.DataLakeReels
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reels_v1");

            migrationBuilder.CreateTable(
                name: "ConsumptionInfos",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    IsBookmarked = table.Column<bool>(nullable: false),
                    IsTrendingInClips = table.Column<bool>(nullable: false),
                    ShouldMuteAudioReason = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "reels_v1",
                columns: table => new
                {
                    Pk = table.Column<string>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    IsPrivate = table.Column<bool>(nullable: false),
                    IsVerified = table.Column<bool>(nullable: false),
                    IsUnpublished = table.Column<bool>(nullable: false),
                    ProfilePicId = table.Column<string>(nullable: true),
                    ProfilePicURL = table.Column<string>(nullable: true),
                    HasHighlightReels = table.Column<bool>(nullable: false),
                    FollowFrictionType = table.Column<string>(nullable: true),
                    HasAnonymousProfilePicture = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Pk);
                });

            migrationBuilder.CreateTable(
                name: "OriginalSounds",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    UserId = table.Column<string>(nullable: true),
                    AudioParts = table.Column<string[]>(type: "text[]", nullable: true),
                    IsExplicit = table.Column<bool>(nullable: false),
                    TimeCreated = table.Column<long>(nullable: false),
                    DashManifest = table.Column<string>(nullable: true),
                    HideRemixing = table.Column<bool>(nullable: false),
                    AudioAssetId = table.Column<long>(nullable: false),
                    DurationInMs = table.Column<long>(nullable: false),
                    ConsumptionInfoId = table.Column<int>(nullable: true),
                    OriginalMediaId = table.Column<string>(nullable: true),
                    ShouldMuteAudio = table.Column<bool>(nullable: false),
                    OriginalAudioTitle = table.Column<string>(nullable: true),
                    OriginalAudioSubtype = table.Column<string>(nullable: true),
                    AllowCreatorToRename = table.Column<bool>(nullable: false),
                    ProgressiveDownloadUrl = table.Column<string>(nullable: true),
                    CanRemixBeSharedToFb = table.Column<bool>(nullable: false),
                    FormattedClipsMediaCount = table.Column<long>(nullable: true),
                    IsAudioAutomaticallyAttributed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OriginalSounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OriginalSounds_ConsumptionInfos_ConsumptionInfoId",
                        column: x => x.ConsumptionInfoId,
                        principalSchema: "reels_v1",
                        principalTable: "ConsumptionInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OriginalSounds_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "reels_v1",
                        principalTable: "Users",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reels",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Pk = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    TakenAt = table.Column<long>(nullable: false),
                    HasAudio = table.Column<bool>(nullable: false),
                    HasLiked = table.Column<bool>(nullable: false),
                    MediaType = table.Column<long>(nullable: false),
                    FilterType = table.Column<long>(nullable: false),
                    PhotoOfYou = table.Column<bool>(nullable: false),
                    ProductType = table.Column<string>(nullable: true),
                    DeletedReason = table.Column<long>(nullable: false),
                    MusicMetadata = table.Column<string>(nullable: true),
                    OriginalWidth = table.Column<long>(nullable: false),
                    VideoDuration = table.Column<double>(nullable: false),
                    CanViewerSave = table.Column<bool>(nullable: false),
                    OriginalHeight = table.Column<long>(nullable: false),
                    ClientCacheKey = table.Column<string>(nullable: true),
                    DeviceTimestamp = table.Column<long>(nullable: false),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    IsUnifiedVideo = table.Column<bool>(nullable: false),
                    CaptionIsEdited = table.Column<bool>(nullable: false),
                    HasMoreComments = table.Column<bool>(nullable: false),
                    IsInProfileGrid = table.Column<bool>(nullable: false),
                    LoggingInfoToken = table.Column<string>(nullable: true),
                    IsPaidPartnership = table.Column<bool>(nullable: false),
                    CommercialityStatus = table.Column<string>(nullable: true),
                    CommentLikesEnabled = table.Column<bool>(nullable: false),
                    OrganicTrackingToken = table.Column<string>(nullable: true),
                    CommentInformTreatment = table.Column<string>(nullable: true),
                    ShouldHaveInformTreatment = table.Column<bool>(nullable: false),
                    CanSeeInsightsAsBrand = table.Column<bool>(nullable: false),
                    CommentThreadingEnabled = table.Column<bool>(nullable: false),
                    IntegrityReviewDecision = table.Column<string>(nullable: true),
                    ProfileGridControlEnabled = table.Column<bool>(nullable: false),
                    LikeAndViewCountsDisabled = table.Column<bool>(nullable: false),
                    CanViewMorePreviewComments = table.Column<bool>(nullable: false),
                    HideViewAllCommentEntrypoint = table.Column<bool>(nullable: false),
                    MaxNumVisiblePreviewComments = table.Column<long>(nullable: false),
                    OriginalMediaHasVisualReplyMedia = table.Column<bool>(nullable: false),
                    IsVisualReplyCommenterNoticeEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reels_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "reels_v1",
                        principalTable: "Users",
                        principalColumn: "Pk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Captions",
                schema: "reels_v1",
                columns: table => new
                {
                    Pk = table.Column<string>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    Type = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    ReelId = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    MediaId = table.Column<string>(nullable: true),
                    BitFlags = table.Column<long>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false),
                    IsCovered = table.Column<bool>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    ShareEnabled = table.Column<bool>(nullable: false),
                    CreatedAtUTC = table.Column<long>(nullable: false),
                    DidReportAsSpam = table.Column<bool>(nullable: false),
                    PrivateReplyStatus = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Captions", x => x.Pk);
                    table.ForeignKey(
                        name: "FK_Captions_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClipsMetas",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ReelId = table.Column<string>(nullable: true),
                    NuxInfo = table.Column<string>(nullable: true),
                    AudioType = table.Column<string>(nullable: true),
                    MusicInfo = table.Column<string>(nullable: true),
                    ShoppingInfo = table.Column<string>(nullable: true),
                    TemplateInfo = table.Column<string>(nullable: true),
                    ChallengeInfo = table.Column<string>(nullable: true),
                    FeaturedLabel = table.Column<string>(nullable: true),
                    IsSharedToFb = table.Column<bool>(nullable: false),
                    AudioRankingClusterId = table.Column<string>(nullable: true),
                    MusicCanonicalId = table.Column<string>(nullable: true),
                    OriginalSoundInfoId = table.Column<int>(nullable: true),
                    AdditionalAudioInfo = table.Column<string>(nullable: true),
                    BreakingContentInfo = table.Column<string>(nullable: true),
                    BreakingCreatorInfo = table.Column<string>(nullable: true),
                    ReelsOnTheRiseInfo = table.Column<string>(nullable: true),
                    BrandedContentTagInfo = table.Column<bool>(nullable: false),
                    AssetRecommendationInfo = table.Column<string>(nullable: true),
                    ContextualHighlightInfo = table.Column<string>(nullable: true),
                    ClipsCreationEntryPoint = table.Column<string>(nullable: true),
                    ViewerInteractionSettings = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipsMetas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClipsMetas_OriginalSounds_OriginalSoundInfoId",
                        column: x => x.OriginalSoundInfoId,
                        principalSchema: "reels_v1",
                        principalTable: "OriginalSounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClipsMetas_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "reels_v1",
                columns: table => new
                {
                    Pk = table.Column<string>(nullable: false),
                    ReelId = table.Column<string>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Type = table.Column<long>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    MediaId = table.Column<string>(nullable: true),
                    BitFlags = table.Column<long>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false),
                    IsCovered = table.Column<bool>(nullable: false),
                    ContentType = table.Column<string>(nullable: true),
                    ShareEnabled = table.Column<bool>(nullable: false),
                    CreatedAtUTC = table.Column<long>(nullable: false),
                    DidReportAsSpam = table.Column<bool>(nullable: false),
                    PrivateReplyStatus = table.Column<long>(nullable: false),
                    ReelId1 = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Pk);
                    table.ForeignKey(
                        name: "FK_Comments_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Reels_ReelId1",
                        column: x => x.ReelId1,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Frictions",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ReelId = table.Column<string>(nullable: true),
                    BloksAppUrl = table.Column<string>(nullable: true),
                    ShouldHaveSharingFriction = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Frictions_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageVersions",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ReelId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageVersions_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReelStats",
                schema: "reels_v1",
                columns: table => new
                {
                    ReelId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    LikeCount = table.Column<long>(nullable: false),
                    PlayCount = table.Column<long>(nullable: false),
                    ViewCount = table.Column<long>(nullable: false),
                    CommentCount = table.Column<long>(nullable: false),
                    EventDate = table.Column<DateTime>(type: "date", nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReelStats", x => new { x.ReelId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_ReelStats_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SquareCrops",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ReelId = table.Column<string>(nullable: true),
                    Top = table.Column<double>(nullable: false),
                    Left = table.Column<double>(nullable: false),
                    Right = table.Column<double>(nullable: false),
                    Bottom = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SquareCrops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SquareCrops_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoVersions",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ReelId = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Type = table.Column<long>(nullable: false),
                    Width = table.Column<long>(nullable: false),
                    Height = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoVersions_Reels_ReelId",
                        column: x => x.ReelId,
                        principalSchema: "reels_v1",
                        principalTable: "Reels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MashupInfos",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClipsId = table.Column<int>(nullable: false),
                    OriginalMedia = table.Column<string>(nullable: true),
                    MashupsAllowed = table.Column<bool>(nullable: false),
                    HasBeenMashedUp = table.Column<bool>(nullable: false),
                    FormattedMashupsCount = table.Column<long>(nullable: true),
                    CanToggleMashupsAllowed = table.Column<bool>(nullable: false),
                    NonPrivacyFilteredMashupsMediaCount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MashupInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MashupInfos_ClipsMetas_ClipsId",
                        column: x => x.ClipsId,
                        principalSchema: "reels_v1",
                        principalTable: "ClipsMetas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimatedThumbnails",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ImageVersionId = table.Column<int>(nullable: false),
                    SpriteUrls = table.Column<string[]>(type: "text[]", nullable: true),
                    FileSizeKb = table.Column<long>(nullable: false),
                    SpriteWidth = table.Column<long>(nullable: false),
                    VideoLength = table.Column<double>(nullable: false),
                    SpriteHeight = table.Column<long>(nullable: false),
                    RenderedWidth = table.Column<long>(nullable: false),
                    ThumbnailWidth = table.Column<long>(nullable: false),
                    ThumbnailHeight = table.Column<long>(nullable: false),
                    ThumbnailDuration = table.Column<double>(nullable: false),
                    ThumbnailsPerRow = table.Column<long>(nullable: false),
                    MaxThumbnailsPerSprite = table.Column<long>(nullable: false),
                    TotalThumbnailNumPerSprite = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimatedThumbnails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimatedThumbnails_ImageVersions_ImageVersionId",
                        column: x => x.ImageVersionId,
                        principalSchema: "reels_v1",
                        principalTable: "ImageVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                schema: "reels_v1",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ImageVersionId = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Width = table.Column<long>(nullable: false),
                    Height = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_ImageVersions_ImageVersionId",
                        column: x => x.ImageVersionId,
                        principalSchema: "reels_v1",
                        principalTable: "ImageVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnimatedThumbnails_ImageVersionId",
                schema: "reels_v1",
                table: "AnimatedThumbnails",
                column: "ImageVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Captions_ReelId",
                schema: "reels_v1",
                table: "Captions",
                column: "ReelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClipsMetas_OriginalSoundInfoId",
                schema: "reels_v1",
                table: "ClipsMetas",
                column: "OriginalSoundInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipsMetas_ReelId",
                schema: "reels_v1",
                table: "ClipsMetas",
                column: "ReelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReelId",
                schema: "reels_v1",
                table: "Comments",
                column: "ReelId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReelId1",
                schema: "reels_v1",
                table: "Comments",
                column: "ReelId1");

            migrationBuilder.CreateIndex(
                name: "IX_Frictions_ReelId",
                schema: "reels_v1",
                table: "Frictions",
                column: "ReelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImageVersionId",
                schema: "reels_v1",
                table: "Images",
                column: "ImageVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageVersions_ReelId",
                schema: "reels_v1",
                table: "ImageVersions",
                column: "ReelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MashupInfos_ClipsId",
                schema: "reels_v1",
                table: "MashupInfos",
                column: "ClipsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OriginalSounds_ConsumptionInfoId",
                schema: "reels_v1",
                table: "OriginalSounds",
                column: "ConsumptionInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginalSounds_UserId",
                schema: "reels_v1",
                table: "OriginalSounds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reels_UserId",
                schema: "reels_v1",
                table: "Reels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SquareCrops_ReelId",
                schema: "reels_v1",
                table: "SquareCrops",
                column: "ReelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoVersions_ReelId",
                schema: "reels_v1",
                table: "VideoVersions",
                column: "ReelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimatedThumbnails",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "Captions",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "Frictions",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "Images",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "MashupInfos",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "ReelStats",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "SquareCrops",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "VideoVersions",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "ImageVersions",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "ClipsMetas",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "OriginalSounds",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "Reels",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "ConsumptionInfos",
                schema: "reels_v1");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "reels_v1");
        }
    }
}
