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
                    DateMeasure = table.Column<DateTime>(nullable: false),
                    ChannelId = table.Column<string>(nullable: true),
                    VideoId = table.Column<string>(nullable: false),
                    Metric = table.Column<string>(nullable: true),
                    Value = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => new { x.VideoId, x.ValidityStart });
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
