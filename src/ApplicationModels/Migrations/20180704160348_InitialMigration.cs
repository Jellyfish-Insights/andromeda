using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ApplicationModels.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "application");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", "'pg_trgm', '', ''");

            migrationBuilder.CreateTable(
                name: "ApplicationGenericTags",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Tag = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationGenericTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationMetaTagsTypes",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationMetaTagsTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationPersonas",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Name = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPersonas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationPlaylists",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPlaylists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVideos",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Archived = table.Column<bool>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVideos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTraces",
                schema: "application",
                columns: table => new
                {
                    Table = table.Column<string>(nullable: false),
                    JobName = table.Column<string>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    GitCommitHash = table.Column<string>(nullable: true),
                    Modifications = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTraces", x => new { x.Table, x.JobName, x.StartTime });
                });

            migrationBuilder.CreateTable(
                name: "RuntimeLog",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    Exception = table.Column<string>(nullable: true),
                    Level = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    When = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuntimeLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceAdSets",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Definition = table.Column<string>(type: "jsonb", nullable: true),
                    ExcludeAudience = table.Column<string[]>(type: "text[]", nullable: true),
                    IncludeAudience = table.Column<string[]>(type: "text[]", nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceAdSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceAudiences",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Definition = table.Column<string>(type: "jsonb", nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceCampaigns",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Objective = table.Column<string>(nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    StopTime = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourcePlaylists",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourcePlaylists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SourceVideos",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    PublishedAt = table.Column<DateTime>(nullable: false),
                    SourceUrl = table.Column<string>(nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    VideoLength = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceVideos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationMetaTags",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Color = table.Column<string>(nullable: true),
                    Tag = table.Column<string>(nullable: true),
                    TypeId = table.Column<int>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationMetaTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationMetaTags_ApplicationMetaTagsTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "application",
                        principalTable: "ApplicationMetaTagsTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationPersonaVersions",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Archived = table.Column<bool>(nullable: false),
                    PersonaId = table.Column<int>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    Version = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationPersonaVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationPersonaVersions_ApplicationPersonas_PersonaId",
                        column: x => x.PersonaId,
                        principalSchema: "application",
                        principalTable: "ApplicationPersonas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedApplicationPlaylistSourcePlaylists",
                schema: "application",
                columns: table => new
                {
                    ApplicationPlaylistId = table.Column<int>(nullable: false),
                    SourcePlaylistId = table.Column<string>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedApplicationPlaylistSourcePlaylists", x => new { x.ApplicationPlaylistId, x.SourcePlaylistId });
                    table.ForeignKey(
                        name: "FK_GeneratedApplicationPlaylistSourcePlaylists_ApplicationPlaylists_ApplicationPlaylistId",
                        column: x => x.ApplicationPlaylistId,
                        principalSchema: "application",
                        principalTable: "ApplicationPlaylists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVideoApplicationGenericTags",
                schema: "application",
                columns: table => new
                {
                    TagId = table.Column<int>(nullable: false),
                    VideoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVideoApplicationGenericTags", x => new { x.TagId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_ApplicationVideoApplicationGenericTags_ApplicationGenericTags_TagId",
                        column: x => x.TagId,
                        principalSchema: "application",
                        principalTable: "ApplicationGenericTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationVideoApplicationGenericTags_ApplicationVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "ApplicationVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedApplicationVideoSourceCampaigns",
                schema: "application",
                columns: table => new
                {
                    CampaignId = table.Column<string>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    VideoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedApplicationVideoSourceCampaigns", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_GeneratedApplicationVideoSourceCampaigns_ApplicationVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "ApplicationVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedApplicationVideoSourceVideos",
                schema: "application",
                columns: table => new
                {
                    SourceVideoId = table.Column<string>(nullable: false),
                    ApplicationVideoId = table.Column<int>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedApplicationVideoSourceVideos", x => x.SourceVideoId);
                    table.ForeignKey(
                        name: "FK_GeneratedApplicationVideoSourceVideos_ApplicationVideos_ApplicationVideoId",
                        column: x => x.ApplicationVideoId,
                        principalSchema: "application",
                        principalTable: "ApplicationVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserApplicationVideoSourceCampaigns",
                schema: "application",
                columns: table => new
                {
                    CampaignId = table.Column<string>(nullable: false),
                    Suppress = table.Column<bool>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    VideoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplicationVideoSourceCampaigns", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_UserApplicationVideoSourceCampaigns_ApplicationVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "ApplicationVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserApplicationVideoSourceVideos",
                schema: "application",
                columns: table => new
                {
                    SourceVideoId = table.Column<string>(nullable: false),
                    ApplicationVideoId = table.Column<int>(nullable: false),
                    Suppress = table.Column<bool>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplicationVideoSourceVideos", x => x.SourceVideoId);
                    table.ForeignKey(
                        name: "FK_UserApplicationVideoSourceVideos_ApplicationVideos_ApplicationVideoId",
                        column: x => x.ApplicationVideoId,
                        principalSchema: "application",
                        principalTable: "ApplicationVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "application",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "application",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "application",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "application",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "application",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "application",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "application",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "application",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "application",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourceAds",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AdSetId = table.Column<string>(nullable: true),
                    CampaignId = table.Column<string>(nullable: true),
                    Platform = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    VideoId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceAds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceAds_SourceAdSets_AdSetId",
                        column: x => x.AdSetId,
                        principalSchema: "application",
                        principalTable: "SourceAdSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SourceAds_SourceCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalSchema: "application",
                        principalTable: "SourceCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SourceAds_SourceVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "SourceVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SourceDeltaEncodedVideoMetrics",
                schema: "application",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    ImpressionsCount = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceDeltaEncodedVideoMetrics", x => new { x.VideoId, x.StartDate });
                    table.ForeignKey(
                        name: "FK_SourceDeltaEncodedVideoMetrics_SourceVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "SourceVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourcePlaylistSourceVideos",
                schema: "application",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    PlaylistId = table.Column<string>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourcePlaylistSourceVideos", x => new { x.VideoId, x.PlaylistId });
                    table.ForeignKey(
                        name: "FK_SourcePlaylistSourceVideos_SourcePlaylists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalSchema: "application",
                        principalTable: "SourcePlaylists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SourcePlaylistSourceVideos_SourceVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "SourceVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourceVideoDemographicMetrics",
                schema: "application",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<string>(nullable: false),
                    AgeGroup = table.Column<string>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    TotalViewCount = table.Column<double>(nullable: true),
                    TotalViewTime = table.Column<double>(nullable: true),
                    ViewerPercentage = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceVideoDemographicMetrics", x => new { x.VideoId, x.StartDate, x.Gender, x.AgeGroup });
                    table.ForeignKey(
                        name: "FK_SourceVideoDemographicMetrics_SourceVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "SourceVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourceVideoMetrics",
                schema: "application",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    CommentCount = table.Column<long>(nullable: true),
                    DislikeCount = table.Column<long>(nullable: true),
                    LikeCount = table.Column<long>(nullable: true),
                    ShareCount = table.Column<long>(nullable: true),
                    ViewCount = table.Column<long>(nullable: true),
                    ViewTime = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceVideoMetrics", x => new { x.VideoId, x.EventDate });
                    table.ForeignKey(
                        name: "FK_SourceVideoMetrics_SourceVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "SourceVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationVideoApplicationMetaTags",
                schema: "application",
                columns: table => new
                {
                    TypeId = table.Column<int>(nullable: false),
                    VideoId = table.Column<int>(nullable: false),
                    TagId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationVideoApplicationMetaTags", x => new { x.TypeId, x.VideoId });
                    table.ForeignKey(
                        name: "FK_ApplicationVideoApplicationMetaTags_ApplicationMetaTags_TagId",
                        column: x => x.TagId,
                        principalSchema: "application",
                        principalTable: "ApplicationMetaTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationVideoApplicationMetaTags_ApplicationMetaTagsTypes_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "application",
                        principalTable: "ApplicationMetaTagsTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationVideoApplicationMetaTags_ApplicationVideos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "application",
                        principalTable: "ApplicationVideos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedApplicationPersonaVersionSourceAdSets",
                schema: "application",
                columns: table => new
                {
                    AdSetId = table.Column<string>(nullable: false),
                    PersonaVersionId = table.Column<int>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedApplicationPersonaVersionSourceAdSets", x => x.AdSetId);
                    table.ForeignKey(
                        name: "FK_GeneratedApplicationPersonaVersionSourceAdSets_ApplicationPersonaVersions_PersonaVersionId",
                        column: x => x.PersonaVersionId,
                        principalSchema: "application",
                        principalTable: "ApplicationPersonaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserApplicationPersonaVersionSourceAdSets",
                schema: "application",
                columns: table => new
                {
                    AdSetId = table.Column<string>(nullable: false),
                    PersonaVersionId = table.Column<int>(nullable: false),
                    Suppress = table.Column<bool>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApplicationPersonaVersionSourceAdSets", x => x.AdSetId);
                    table.ForeignKey(
                        name: "FK_UserApplicationPersonaVersionSourceAdSets_ApplicationPersonaVersions_PersonaVersionId",
                        column: x => x.PersonaVersionId,
                        principalSchema: "application",
                        principalTable: "ApplicationPersonaVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourceAdMetrics",
                schema: "application",
                columns: table => new
                {
                    AdId = table.Column<string>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false),
                    Clicks = table.Column<int>(nullable: false),
                    Cost = table.Column<double>(nullable: false),
                    CostPerClick = table.Column<double>(nullable: false),
                    CostPerEmailCapture = table.Column<double>(nullable: false),
                    CostPerEngagement = table.Column<double>(nullable: false),
                    CostPerImpression = table.Column<double>(nullable: false),
                    CostPerView = table.Column<double>(nullable: false),
                    EmailCapture = table.Column<int>(nullable: true),
                    Engagements = table.Column<int>(nullable: true),
                    Impressions = table.Column<int>(nullable: false),
                    Reach = table.Column<int>(nullable: true),
                    Views = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceAdMetrics", x => new { x.AdId, x.EventDate });
                    table.ForeignKey(
                        name: "FK_SourceAdMetrics_SourceAds_AdId",
                        column: x => x.AdId,
                        principalSchema: "application",
                        principalTable: "SourceAds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMetaTags_TypeId",
                schema: "application",
                table: "ApplicationMetaTags",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationPersonaVersions_PersonaId",
                schema: "application",
                table: "ApplicationPersonaVersions",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVideoApplicationGenericTags_VideoId",
                schema: "application",
                table: "ApplicationVideoApplicationGenericTags",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVideoApplicationMetaTags_TagId",
                schema: "application",
                table: "ApplicationVideoApplicationMetaTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationVideoApplicationMetaTags_VideoId",
                schema: "application",
                table: "ApplicationVideoApplicationMetaTags",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "application",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "application",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "application",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "application",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "application",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "application",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "application",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedApplicationPersonaVersionSourceAdSets_PersonaVersionId",
                schema: "application",
                table: "GeneratedApplicationPersonaVersionSourceAdSets",
                column: "PersonaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedApplicationVideoSourceCampaigns_VideoId",
                schema: "application",
                table: "GeneratedApplicationVideoSourceCampaigns",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedApplicationVideoSourceVideos_ApplicationVideoId",
                schema: "application",
                table: "GeneratedApplicationVideoSourceVideos",
                column: "ApplicationVideoId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceAds_AdSetId",
                schema: "application",
                table: "SourceAds",
                column: "AdSetId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceAds_CampaignId",
                schema: "application",
                table: "SourceAds",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SourceAds_VideoId",
                schema: "application",
                table: "SourceAds",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_SourcePlaylistSourceVideos_PlaylistId",
                schema: "application",
                table: "SourcePlaylistSourceVideos",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationPersonaVersionSourceAdSets_PersonaVersionId",
                schema: "application",
                table: "UserApplicationPersonaVersionSourceAdSets",
                column: "PersonaVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationVideoSourceCampaigns_VideoId",
                schema: "application",
                table: "UserApplicationVideoSourceCampaigns",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApplicationVideoSourceVideos_ApplicationVideoId",
                schema: "application",
                table: "UserApplicationVideoSourceVideos",
                column: "ApplicationVideoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationVideoApplicationGenericTags",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationVideoApplicationMetaTags",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "application");

            migrationBuilder.DropTable(
                name: "GeneratedApplicationPersonaVersionSourceAdSets",
                schema: "application");

            migrationBuilder.DropTable(
                name: "GeneratedApplicationPlaylistSourcePlaylists",
                schema: "application");

            migrationBuilder.DropTable(
                name: "GeneratedApplicationVideoSourceCampaigns",
                schema: "application");

            migrationBuilder.DropTable(
                name: "GeneratedApplicationVideoSourceVideos",
                schema: "application");

            migrationBuilder.DropTable(
                name: "JobTraces",
                schema: "application");

            migrationBuilder.DropTable(
                name: "RuntimeLog",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceAdMetrics",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceAudiences",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceDeltaEncodedVideoMetrics",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourcePlaylistSourceVideos",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceVideoDemographicMetrics",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceVideoMetrics",
                schema: "application");

            migrationBuilder.DropTable(
                name: "UserApplicationPersonaVersionSourceAdSets",
                schema: "application");

            migrationBuilder.DropTable(
                name: "UserApplicationVideoSourceCampaigns",
                schema: "application");

            migrationBuilder.DropTable(
                name: "UserApplicationVideoSourceVideos",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationGenericTags",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationMetaTags",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "application");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationPlaylists",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceAds",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourcePlaylists",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationPersonaVersions",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationVideos",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationMetaTagsTypes",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceAdSets",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceCampaigns",
                schema: "application");

            migrationBuilder.DropTable(
                name: "SourceVideos",
                schema: "application");

            migrationBuilder.DropTable(
                name: "ApplicationPersonas",
                schema: "application");
        }
    }
}
