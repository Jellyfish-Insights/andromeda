using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeYouTubeAnalytics
{
    public partial class AddSubscriberViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SubscriberViews",
                schema: "youtube_analytics_v2",
                table: "VideoDailyMetrics",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriberViews",
                schema: "youtube_analytics_v2",
                table: "VideoDailyMetrics");
        }
    }
}
