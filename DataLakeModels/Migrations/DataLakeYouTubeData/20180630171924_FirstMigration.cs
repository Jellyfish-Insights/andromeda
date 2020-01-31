using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace DataLakeModels.Migrations.DataLakeYouTubeData
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "youtube_data_v3");

            migrationBuilder.CreateTable(
                name: "Playlists",
                schema: "youtube_data_v3",
                columns: table => new
                {
                    PlaylistId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    ValidityEnd = table.Column<DateTime>(nullable: false),
                    VideoIds = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => new { x.PlaylistId, x.ValidityStart });
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                schema: "youtube_data_v3",
                columns: table => new
                {
                    VideoId = table.Column<string>(nullable: false),
                    ValidityStart = table.Column<DateTime>(nullable: false),
                    ChannelId = table.Column<string>(nullable: true),
                    Duration = table.Column<string>(nullable: true),
                    PublishedAt = table.Column<DateTime>(nullable: false),
                    Tags = table.Column<string[]>(type: "text[]", nullable: true),
                    ThumbnailUrl = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    ValidityEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => new { x.VideoId, x.ValidityStart });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Playlists",
                schema: "youtube_data_v3");

            migrationBuilder.DropTable(
                name: "Videos",
                schema: "youtube_data_v3");
        }
    }
}
