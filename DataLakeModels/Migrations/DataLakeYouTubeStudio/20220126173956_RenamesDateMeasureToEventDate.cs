using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeYouTubeStudio
{
    public partial class RenamesDateMeasureToEventDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos");

            migrationBuilder.RenameColumn(
                name: "DateMeasure",
                schema: "youtube_studio_v1",
                table: "Videos",
                newName: "EventDate");

            migrationBuilder.AlterColumn<string>(
                name: "Metric",
                schema: "youtube_studio_v1",
                table: "Videos",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ChannelId",
                schema: "youtube_studio_v1",
                table: "Videos",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos",
                columns: new[] { "ChannelId", "VideoId", "ValidityStart", "Metric", "EventDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos");

            migrationBuilder.RenameColumn(
                name: "EventDate",
                schema: "youtube_studio_v1",
                table: "Videos",
                newName: "DateMeasure");

            migrationBuilder.AlterColumn<string>(
                name: "Metric",
                schema: "youtube_studio_v1",
                table: "Videos",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ChannelId",
                schema: "youtube_studio_v1",
                table: "Videos",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Videos",
                schema: "youtube_studio_v1",
                table: "Videos",
                columns: new[] { "VideoId", "ValidityStart" });
        }
    }
}
