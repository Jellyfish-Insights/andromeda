using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeYouTubeStudio
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "youtube_studio_v1");

            migrationBuilder.CreateTable(
                name: "Videos",
                schema: "youtube_studio_v1",
                columns: table => new
                {
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    EventDate = table.Column<DateTime>(type: "date", nullable: false),
                    ChannelId = table.Column<string>(nullable: false),
                    VideoId = table.Column<string>(nullable: false),
                    Metric = table.Column<string>(nullable: false),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => new { x.ChannelId, x.VideoId, x.ValidityStart, x.Metric, x.EventDate });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Videos",
                schema: "youtube_studio_v1");
        }
    }
}
