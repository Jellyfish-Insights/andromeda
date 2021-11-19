using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeTwitterAds
{
    public partial class AddTwitterAds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "twitter_ads_v10");

            migrationBuilder.CreateTable(
                name: "AdsAccounts",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    BusinessName = table.Column<string>(nullable: true),
                    TimeZone = table.Column<string>(nullable: true),
                    TimeZoneSwitchAt = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    BusinessId = table.Column<string>(nullable: true),
                    ApprovalStatus = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdsAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomAudiences",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Targetable = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    TargetableTypes = table.Column<string[]>(nullable: true),
                    AudienceType = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    PermissionLevel = table.Column<string>(nullable: true),
                    OwnerAccountId = table.Column<string>(nullable: true),
                    ReasonsNotTargetable = table.Column<string[]>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    PartnerSource = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    AudienceSize = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganicTweetDailyMetrics",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Date = table.Column<DateTime>(nullable: false),
                    Engagements = table.Column<int>(nullable: false),
                    Impressions = table.Column<int>(nullable: false),
                    Retweets = table.Column<int>(nullable: false),
                    Replies = table.Column<int>(nullable: false),
                    Likes = table.Column<int>(nullable: false),
                    Follows = table.Column<int>(nullable: false),
                    CardEngagements = table.Column<int>(nullable: false),
                    Clicks = table.Column<int>(nullable: false),
                    AppClicks = table.Column<int>(nullable: false),
                    UrlClicks = table.Column<int>(nullable: false),
                    QualifiedImpressions = table.Column<int>(nullable: false),
                    VideoTotalViews = table.Column<int>(nullable: false),
                    VideoViews25 = table.Column<int>(nullable: false),
                    VideoViews50 = table.Column<int>(nullable: false),
                    VideoViews75 = table.Column<int>(nullable: false),
                    VideoViews100 = table.Column<int>(nullable: false),
                    VideoCtaClicks = table.Column<int>(nullable: false),
                    VideoContentStarts = table.Column<int>(nullable: false),
                    Video3s100pctViews = table.Column<int>(nullable: false),
                    Video6sViews = table.Column<int>(nullable: false),
                    Video15sViews = table.Column<int>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    TweetId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganicTweetDailyMetrics", x => new { x.TweetId, x.Date, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "PromotedTweetDailyMetrics",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Date = table.Column<DateTime>(nullable: false),
                    Engagements = table.Column<int>(nullable: false),
                    Impressions = table.Column<int>(nullable: false),
                    Retweets = table.Column<int>(nullable: false),
                    Replies = table.Column<int>(nullable: false),
                    Likes = table.Column<int>(nullable: false),
                    Follows = table.Column<int>(nullable: false),
                    CardEngagements = table.Column<int>(nullable: false),
                    Clicks = table.Column<int>(nullable: false),
                    AppClicks = table.Column<int>(nullable: false),
                    UrlClicks = table.Column<int>(nullable: false),
                    QualifiedImpressions = table.Column<int>(nullable: false),
                    VideoTotalViews = table.Column<int>(nullable: false),
                    VideoViews25 = table.Column<int>(nullable: false),
                    VideoViews50 = table.Column<int>(nullable: false),
                    VideoViews75 = table.Column<int>(nullable: false),
                    VideoViews100 = table.Column<int>(nullable: false),
                    VideoCtaClicks = table.Column<int>(nullable: false),
                    VideoContentStarts = table.Column<int>(nullable: false),
                    Video3s100pctViews = table.Column<int>(nullable: false),
                    Video6sViews = table.Column<int>(nullable: false),
                    Video15sViews = table.Column<int>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    PromotedTweetId = table.Column<string>(nullable: false),
                    BilledEngagements = table.Column<int>(nullable: false),
                    BilledChargeLocalMicro = table.Column<long>(nullable: false),
                    MediaViews = table.Column<int>(nullable: false),
                    MediaEngagements = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotedTweetDailyMetrics", x => new { x.PromotedTweetId, x.Date, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "VideoLibraries",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    PosterMediaKey = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    MediaStatus = table.Column<string>(nullable: true),
                    MediaUrl = table.Column<string>(nullable: true),
                    PosterMediaUrl = table.Column<string>(nullable: true),
                    AspectRatio = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Tweeted = table.Column<bool>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    Username = table.Column<string>(nullable: true),
                    Duration = table.Column<long>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoLibraries", x => new { x.Id, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AdsAccountId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    StartTime = table.Column<DateTimeOffset>(nullable: false),
                    EndTime = table.Column<DateTimeOffset>(nullable: true),
                    Servable = table.Column<bool>(nullable: false),
                    PurchaseOrderNumber = table.Column<string>(nullable: true),
                    EffectiveStatus = table.Column<string>(nullable: true),
                    DailyBudgetAmountLocalMicro = table.Column<long>(nullable: false),
                    FundingInstrumentId = table.Column<string>(nullable: true),
                    DurationInDays = table.Column<int>(nullable: true),
                    StandardDelivery = table.Column<bool>(nullable: false),
                    TotalBudgetAmountLocalMicro = table.Column<long>(nullable: true),
                    EntityStatus = table.Column<string>(nullable: true),
                    FrequencyCap = table.Column<int>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => new { x.Id, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_Campaigns_AdsAccounts_AdsAccountId",
                        column: x => x.AdsAccountId,
                        principalSchema: "twitter_ads_v10",
                        principalTable: "AdsAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineItems",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CampaignId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    StartTime = table.Column<DateTimeOffset>(nullable: true),
                    BidAmountLocalMicro = table.Column<long>(nullable: true),
                    AdvertiserDomain = table.Column<string>(nullable: true),
                    TargetCpaLocalMicro = table.Column<long>(nullable: true),
                    PrimaryWebEventTag = table.Column<string>(nullable: true),
                    Goal = table.Column<string>(nullable: true),
                    ProductType = table.Column<string>(nullable: true),
                    EndTime = table.Column<DateTimeOffset>(nullable: true),
                    BidStrategy = table.Column<string>(nullable: true),
                    DurationInDays = table.Column<int>(nullable: true),
                    TotalBudgetAmountLocalMicro = table.Column<long>(nullable: true),
                    Objective = table.Column<string>(nullable: true),
                    EntityStatus = table.Column<string>(nullable: true),
                    FrequencyCap = table.Column<int>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    PayBy = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    CreativeSource = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    CampaignId1 = table.Column<string>(nullable: true),
                    CampaignValidityStart = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItems", x => new { x.Id, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_LineItems_Campaigns_CampaignId1_CampaignValidityStart",
                        columns: x => new { x.CampaignId1, x.CampaignValidityStart },
                        principalSchema: "twitter_ads_v10",
                        principalTable: "Campaigns",
                        principalColumns: new[] { "Id", "ValidityStart" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotedTweets",
                schema: "twitter_ads_v10",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    LineItemId = table.Column<string>(nullable: true),
                    TweetId = table.Column<string>(nullable: true),
                    EntityStatus = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    TweetIdSt = table.Column<string>(nullable: true),
                    ApprovalStatus = table.Column<string>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    CampaignId = table.Column<string>(nullable: true),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    LineItemId1 = table.Column<string>(nullable: true),
                    LineItemValidityStart = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotedTweets", x => new { x.Id, x.ValidityStart });
                    table.ForeignKey(
                        name: "FK_PromotedTweets_LineItems_LineItemId1_LineItemValidityStart",
                        columns: x => new { x.LineItemId1, x.LineItemValidityStart },
                        principalSchema: "twitter_ads_v10",
                        principalTable: "LineItems",
                        principalColumns: new[] { "Id", "ValidityStart" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_AdsAccountId",
                schema: "twitter_ads_v10",
                table: "Campaigns",
                column: "AdsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_CampaignId1_CampaignValidityStart",
                schema: "twitter_ads_v10",
                table: "LineItems",
                columns: new[] { "CampaignId1", "CampaignValidityStart" });

            migrationBuilder.CreateIndex(
                name: "IX_PromotedTweets_LineItemId1_LineItemValidityStart",
                schema: "twitter_ads_v10",
                table: "PromotedTweets",
                columns: new[] { "LineItemId1", "LineItemValidityStart" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomAudiences",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "OrganicTweetDailyMetrics",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "PromotedTweetDailyMetrics",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "PromotedTweets",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "VideoLibraries",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "LineItems",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "Campaigns",
                schema: "twitter_ads_v10");

            migrationBuilder.DropTable(
                name: "AdsAccounts",
                schema: "twitter_ads_v10");
        }
    }
}
