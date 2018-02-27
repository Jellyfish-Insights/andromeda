using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace DataLakeModels.Migrations.DataLakeAdWords
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "adwords_v201802");

            migrationBuilder.CreateTable(
                name: "AdPerformanceReports",
                schema: "adwords_v201802",
                columns: table => new
                {
                    AdId = table.Column<string>(nullable: false),
                    Date = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    AdGroupId = table.Column<string>(nullable: true),
                    AverageCpc = table.Column<string>(nullable: true),
                    AverageCpe = table.Column<string>(nullable: true),
                    AverageCpm = table.Column<string>(nullable: true),
                    AverageCpv = table.Column<string>(nullable: true),
                    CampaignId = table.Column<string>(nullable: true),
                    Clicks = table.Column<string>(nullable: true),
                    Cost = table.Column<string>(nullable: true),
                    Engagements = table.Column<string>(nullable: true),
                    Headline = table.Column<string>(nullable: true),
                    Impressions = table.Column<string>(nullable: true),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    VideoViews = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdPerformanceReports", x => new { x.AdId, x.Date, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "StructuralCampaignPerformanceReports",
                schema: "adwords_v201802",
                columns: table => new
                {
                    CampaignId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    BiddingStrategyId = table.Column<string>(nullable: true),
                    BiddingStrategyName = table.Column<string>(nullable: true),
                    BiddingStrategyType = table.Column<string>(nullable: true),
                    CampaignName = table.Column<string>(nullable: true),
                    CampaignStatus = table.Column<string>(nullable: true),
                    EndDate = table.Column<string>(nullable: true),
                    ServingStatus = table.Column<string>(nullable: true),
                    StartDate = table.Column<string>(nullable: true),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructuralCampaignPerformanceReports", x => new { x.CampaignId, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "StructuralCriteriaPerformanceReports",
                schema: "adwords_v201802",
                columns: table => new
                {
                    AdGroupId = table.Column<string>(nullable: false),
                    CriteriaType = table.Column<string>(nullable: false),
                    Criteria = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    AdGroupName = table.Column<string>(nullable: true),
                    CampaignId = table.Column<string>(nullable: true),
                    DisplayName = table.Column<string>(nullable: true),
                    IsNegative = table.Column<string>(nullable: true),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructuralCriteriaPerformanceReports", x => new { x.AdGroupId, x.CriteriaType, x.Criteria, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "StructuralVideoPerformanceReports",
                schema: "adwords_v201802",
                columns: table => new
                {
                    CreativeId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    VideoId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructuralVideoPerformanceReports", x => new { x.CreativeId, x.ValidityStart });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdPerformanceReports",
                schema: "adwords_v201802");

            migrationBuilder.DropTable(
                name: "StructuralCampaignPerformanceReports",
                schema: "adwords_v201802");

            migrationBuilder.DropTable(
                name: "StructuralCriteriaPerformanceReports",
                schema: "adwords_v201802");

            migrationBuilder.DropTable(
                name: "StructuralVideoPerformanceReports",
                schema: "adwords_v201802");
        }
    }
}
