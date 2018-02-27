using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace DataLakeModels.Migrations.DataLakeYouTubeData
{
    public partial class AddStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statistics",
                schema: "youtube_data_v3",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    CaptureDate = table.Column<DateTime>(type: "date", nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    CommentCount = table.Column<long>(nullable: false),
                    DislikeCount = table.Column<long>(nullable: false),
                    FavoriteCount = table.Column<long>(nullable: false),
                    LikeCount = table.Column<long>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    ViewCount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => new { x.VideoId, x.CaptureDate, x.ValidityStart });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics",
                schema: "youtube_data_v3");
        }
    }
}
