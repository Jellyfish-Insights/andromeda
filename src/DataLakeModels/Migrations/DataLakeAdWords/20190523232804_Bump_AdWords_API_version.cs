using Microsoft.EntityFrameworkCore.Migrations;

namespace DataLakeModels.Migrations.DataLakeAdWords
{
    public partial class Bump_AdWords_API_version : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StructuralCriteriaPerformanceReports",
                schema: "adwords_v201802",
                table: "StructuralCriteriaPerformanceReports");

            migrationBuilder.EnsureSchema(
                name: "adwords_v201809");

            migrationBuilder.RenameTable(
                name: "StructuralVideoPerformanceReports",
                schema: "adwords_v201802",
                newSchema: "adwords_v201809");

            migrationBuilder.RenameTable(
                name: "StructuralCriteriaPerformanceReports",
                schema: "adwords_v201802",
                newSchema: "adwords_v201809");

            migrationBuilder.RenameTable(
                name: "StructuralCampaignPerformanceReports",
                schema: "adwords_v201802",
                newSchema: "adwords_v201809");

            migrationBuilder.RenameTable(
                name: "AdPerformanceReports",
                schema: "adwords_v201802",
                newSchema: "adwords_v201809");

            migrationBuilder.AddColumn<string>(
                name: "KeywordId",
                schema: "adwords_v201809",
                table: "StructuralCriteriaPerformanceReports",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StructuralCriteriaPerformanceReports",
                schema: "adwords_v201809",
                table: "StructuralCriteriaPerformanceReports",
                columns: new[] { "KeywordId", "AdGroupId", "CriteriaType", "Criteria", "ValidityStart" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StructuralCriteriaPerformanceReports",
                schema: "adwords_v201809",
                table: "StructuralCriteriaPerformanceReports");

            migrationBuilder.DropColumn(
                name: "KeywordId",
                schema: "adwords_v201809",
                table: "StructuralCriteriaPerformanceReports");

            migrationBuilder.EnsureSchema(
                name: "adwords_v201802");

            migrationBuilder.RenameTable(
                name: "StructuralVideoPerformanceReports",
                schema: "adwords_v201809",
                newSchema: "adwords_v201802");

            migrationBuilder.RenameTable(
                name: "StructuralCriteriaPerformanceReports",
                schema: "adwords_v201809",
                newSchema: "adwords_v201802");

            migrationBuilder.RenameTable(
                name: "StructuralCampaignPerformanceReports",
                schema: "adwords_v201809",
                newSchema: "adwords_v201802");

            migrationBuilder.RenameTable(
                name: "AdPerformanceReports",
                schema: "adwords_v201809",
                newSchema: "adwords_v201802");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StructuralCriteriaPerformanceReports",
                schema: "adwords_v201802",
                table: "StructuralCriteriaPerformanceReports",
                columns: new[] { "AdGroupId", "CriteriaType", "Criteria", "ValidityStart" });
        }
    }
}
