using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace DataLakeModels.Migrations.DataLakeYouTubeAnalytics
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "youtube_analytics_v2");

            migrationBuilder.CreateTable(
                name: "VideoDailyMetrics",
                schema: "youtube_analytics_v2",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    AverageViewDuration = table.Column<long>(nullable: false),
                    Comments = table.Column<long>(nullable: false),
                    Dislikes = table.Column<long>(nullable: false),
                    Likes = table.Column<long>(nullable: false),
                    Shares = table.Column<long>(nullable: false),
                    SubscribersGained = table.Column<long>(nullable: false),
                    SubscribersLost = table.Column<long>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    VideosAddedToPlaylists = table.Column<long>(nullable: false),
                    VideosRemovedFromPlaylists = table.Column<long>(nullable: false),
                    Views = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoDailyMetrics", x => new { x.VideoId, x.Date, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "ViewerPercentageMetric",
                schema: "youtube_analytics_v2",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<string>(nullable: false),
                    AgeGroup = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViewerPercentageMetric", x => new { x.VideoId, x.StartDate, x.Gender, x.AgeGroup, x.ValidityStart });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoDailyMetrics",
                schema: "youtube_analytics_v2");

            migrationBuilder.DropTable(
                name: "ViewerPercentageMetric",
                schema: "youtube_analytics_v2");
        }
    }
}
