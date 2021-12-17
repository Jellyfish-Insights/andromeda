using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeTikTok
{
    public partial class InitialTikTokModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tiktok_v1");

            migrationBuilder.CreateTable(
                name: "Authors",
                schema: "tiktok_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UniqueId = table.Column<string>(nullable: true),
                    Nickname = table.Column<string>(nullable: true),
                    AvatarThumbnail = table.Column<string>(nullable: true),
                    AvatarMedium = table.Column<string>(nullable: true),
                    AvatarLarge = table.Column<string>(nullable: true),
                    Signature = table.Column<string>(nullable: true),
                    Verified = table.Column<bool>(nullable: false),
                    SecurityUId = table.Column<string>(nullable: true),
                    Secret = table.Column<bool>(nullable: false),
                    FTC = table.Column<bool>(nullable: false),
                    Relation = table.Column<int>(nullable: false),
                    OpenFavorite = table.Column<int>(nullable: false),
                    CommentSetting = table.Column<int>(nullable: false),
                    DuetSetting = table.Column<int>(nullable: false),
                    StitchSetting = table.Column<int>(nullable: false),
                    PrivateAccount = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                schema: "tiktok_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ProfileThumbnail = table.Column<string>(nullable: true),
                    ProfileMedium = table.Column<string>(nullable: true),
                    ProfileLarge = table.Column<string>(nullable: true),
                    CoverThumbnail = table.Column<string>(nullable: true),
                    CoverMedium = table.Column<string>(nullable: true),
                    CoverLarge = table.Column<string>(nullable: true),
                    IsCommerce = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EffectStickers",
                schema: "tiktok_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EffectStickers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Music",
                schema: "tiktok_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    PlayUrl = table.Column<string>(nullable: true),
                    CoverThumb = table.Column<string>(nullable: true),
                    CoverMedium = table.Column<string>(nullable: true),
                    CoverLarge = table.Column<string>(nullable: true),
                    AuthorName = table.Column<string>(nullable: true),
                    Original = table.Column<bool>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    Album = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Music", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "tiktok_v1",
                columns: table => new
                {
                    AweMeId = table.Column<string>(nullable: true),
                    Start = table.Column<int>(nullable: false),
                    End = table.Column<int>(nullable: false),
                    HashtagName = table.Column<string>(nullable: true),
                    HashtagId = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    IsCommerce = table.Column<bool>(nullable: false),
                    UserUniqueId = table.Column<string>(nullable: true),
                    SecureUId = table.Column<string>(nullable: true),
                    SubType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.HashtagId);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                schema: "tiktok_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    Width = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    Ratio = table.Column<string>(nullable: true),
                    Cover = table.Column<string>(nullable: true),
                    OriginCover = table.Column<string>(nullable: true),
                    DynamicCover = table.Column<string>(nullable: true),
                    PlayAddress = table.Column<string>(nullable: true),
                    DownloadAddress = table.Column<string>(nullable: true),
                    ShareCover = table.Column<List<string>>(nullable: true),
                    ReflowCover = table.Column<string>(nullable: true),
                    BitRate = table.Column<int>(nullable: false),
                    EncodedType = table.Column<string>(nullable: true),
                    Format = table.Column<string>(nullable: true),
                    VideoQuality = table.Column<string>(nullable: true),
                    EncodedUserTag = table.Column<string>(nullable: true),
                    CodecType = table.Column<string>(nullable: true),
                    Definition = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorStats",
                schema: "tiktok_v1",
                columns: table => new
                {
                    AuthorId = table.Column<string>(nullable: false),
                    FollowingCount = table.Column<long>(nullable: false),
                    FollowerCount = table.Column<long>(nullable: false),
                    HeartCount = table.Column<long>(nullable: false),
                    VideoCount = table.Column<long>(nullable: false),
                    DiggCount = table.Column<long>(nullable: false),
                    Heart = table.Column<long>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorStats", x => new { x.AuthorId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_AuthorStats_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "tiktok_v1",
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "tiktok_v1",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    VideoId = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    MusicId = table.Column<string>(nullable: true),
                    ChallengeIds = table.Column<List<string>>(nullable: true),
                    DuetInfo = table.Column<string>(nullable: true),
                    OriginalItem = table.Column<bool>(nullable: false),
                    OfficialItem = table.Column<bool>(nullable: false),
                    TagIds = table.Column<List<string>>(nullable: true),
                    Secret = table.Column<bool>(nullable: false),
                    ForFriend = table.Column<bool>(nullable: false),
                    Digged = table.Column<bool>(nullable: false),
                    ItemCommentStatus = table.Column<int>(nullable: false),
                    ShowNotPass = table.Column<bool>(nullable: false),
                    VL1 = table.Column<bool>(nullable: false),
                    ItemMute = table.Column<bool>(nullable: false),
                    EffectStickerIds = table.Column<List<string>>(nullable: true),
                    Private = table.Column<bool>(nullable: false),
                    DuetEnabled = table.Column<bool>(nullable: false),
                    StitchEnabled = table.Column<bool>(nullable: false),
                    ShareEnabled = table.Column<bool>(nullable: false),
                    IsAd = table.Column<bool>(nullable: false),
                    DuetDisplay = table.Column<int>(nullable: false),
                    StitchDisplay = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalSchema: "tiktok_v1",
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Posts_Music_MusicId",
                        column: x => x.MusicId,
                        principalSchema: "tiktok_v1",
                        principalTable: "Music",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Posts_Videos_VideoId",
                        column: x => x.VideoId,
                        principalSchema: "tiktok_v1",
                        principalTable: "Videos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stats",
                schema: "tiktok_v1",
                columns: table => new
                {
                    DiggCount = table.Column<long>(nullable: false),
                    ShareCount = table.Column<long>(nullable: false),
                    CommentCount = table.Column<long>(nullable: false),
                    PlayCount = table.Column<long>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    PostId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => new { x.PostId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_Stats_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "tiktok_v1",
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                schema: "tiktok_v1",
                table: "Posts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_MusicId",
                schema: "tiktok_v1",
                table: "Posts",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_VideoId",
                schema: "tiktok_v1",
                table: "Posts",
                column: "VideoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorStats",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Challenges",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "EffectStickers",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Stats",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Authors",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Music",
                schema: "tiktok_v1");

            migrationBuilder.DropTable(
                name: "Videos",
                schema: "tiktok_v1");
        }
    }
}
