using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeTwitterData
{
    public partial class AddTwitterData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "twitter_data_v2");

            migrationBuilder.CreateTable(
                name: "Medias",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DurationMs = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    PreviewImageUrl = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Width = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    ProfileImageUrl = table.Column<string>(nullable: true),
                    IsProtected = table.Column<bool>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true),
                    Verified = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaNonPublicMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    Playback0Count = table.Column<int>(nullable: false),
                    Playback25Count = table.Column<int>(nullable: false),
                    Playback50Count = table.Column<int>(nullable: false),
                    Playback75Count = table.Column<int>(nullable: false),
                    Playback100Count = table.Column<int>(nullable: false),
                    MediaId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaNonPublicMetrics", x => new { x.MediaId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_MediaNonPublicMetrics_Medias_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Medias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaOrganicMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    Playback0Count = table.Column<int>(nullable: false),
                    Playback25Count = table.Column<int>(nullable: false),
                    Playback50Count = table.Column<int>(nullable: false),
                    Playback75Count = table.Column<int>(nullable: false),
                    Playback100Count = table.Column<int>(nullable: false),
                    MediaId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaOrganicMetrics", x => new { x.MediaId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_MediaOrganicMetrics_Medias_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Medias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaPromotedMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    Playback0Count = table.Column<int>(nullable: false),
                    Playback25Count = table.Column<int>(nullable: false),
                    Playback50Count = table.Column<int>(nullable: false),
                    Playback75Count = table.Column<int>(nullable: false),
                    Playback100Count = table.Column<int>(nullable: false),
                    MediaId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaPromotedMetrics", x => new { x.MediaId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_MediaPromotedMetrics_Medias_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Medias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaPublicMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    MediaId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    ViewCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaPublicMetrics", x => new { x.MediaId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_MediaPublicMetrics_Medias_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Medias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tweets",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    ConversationId = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    InReplyToUserId = table.Column<string>(nullable: true),
                    Lang = table.Column<string>(nullable: true),
                    PossiblySensitive = table.Column<bool>(nullable: false),
                    Source = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tweets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tweets_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetMedia",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    TweetId = table.Column<string>(nullable: false),
                    MediaId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetMedia", x => new { x.TweetId, x.MediaId });
                    table.ForeignKey(
                        name: "FK_TweetMedia_Medias_MediaId",
                        column: x => x.MediaId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Medias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TweetMedia_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetNonPublicMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    ImpressionCount = table.Column<int>(nullable: false),
                    UrlLinkClicks = table.Column<int>(nullable: false),
                    UserProfileClicks = table.Column<int>(nullable: false),
                    TweetId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetNonPublicMetrics", x => new { x.TweetId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_TweetNonPublicMetrics_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetOrganicMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    ImpressionCount = table.Column<int>(nullable: false),
                    LikeCount = table.Column<int>(nullable: false),
                    ReplyCount = table.Column<int>(nullable: false),
                    RetweetCount = table.Column<int>(nullable: false),
                    UrlLinkClicks = table.Column<int>(nullable: false),
                    UserProfileClicks = table.Column<int>(nullable: false),
                    TweetId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetOrganicMetrics", x => new { x.TweetId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_TweetOrganicMetrics_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetPromotedMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    ImpressionCount = table.Column<int>(nullable: false),
                    LikeCount = table.Column<int>(nullable: false),
                    ReplyCount = table.Column<int>(nullable: false),
                    RetweetCount = table.Column<int>(nullable: false),
                    UrlLinkClicks = table.Column<int>(nullable: false),
                    UserProfileClicks = table.Column<int>(nullable: false),
                    TweetId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetPromotedMetrics", x => new { x.TweetId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_TweetPromotedMetrics_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TweetPublicMetrics",
                schema: "twitter_data_v2",
                columns: table => new
                {
                    LikeCount = table.Column<int>(nullable: false),
                    QuoteCount = table.Column<int>(nullable: false),
                    ReplyCount = table.Column<int>(nullable: false),
                    RetweetCount = table.Column<int>(nullable: false),
                    TweetId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetPublicMetrics", x => new { x.TweetId, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_TweetPublicMetrics_Tweets_TweetId",
                        column: x => x.TweetId,
                        principalSchema: "twitter_data_v2",
                        principalTable: "Tweets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TweetMedia_MediaId",
                schema: "twitter_data_v2",
                table: "TweetMedia",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tweets_UserId",
                schema: "twitter_data_v2",
                table: "Tweets",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaNonPublicMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "MediaOrganicMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "MediaPromotedMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "MediaPublicMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "TweetMedia",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "TweetNonPublicMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "TweetOrganicMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "TweetPromotedMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "TweetPublicMetrics",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "Medias",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "Tweets",
                schema: "twitter_data_v2");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "twitter_data_v2");
        }
    }
}
