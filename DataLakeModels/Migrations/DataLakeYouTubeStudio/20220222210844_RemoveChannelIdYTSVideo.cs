using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeYouTubeStudio
{
    public partial class RemoveChannelIdYTSVideo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                schema: "youtube_studio_v1",
                table: "Videos");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDate",
                schema: "youtube_studio_v1",
                table: "Groups",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos",
                columns: new[] { "VideoId", "ValidityStart", "Metric", "EventDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos");

            migrationBuilder.AddColumn<string>(
                name: "ChannelId",
                schema: "youtube_studio_v1",
                table: "Videos",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDate",
                schema: "youtube_studio_v1",
                table: "Groups",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos",
                columns: new[] { "ChannelId", "VideoId", "ValidityStart", "Metric", "EventDate" });
        }
    }
}
