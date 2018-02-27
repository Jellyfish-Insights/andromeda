using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace ApplicationModels.Migrations
{
    public partial class AddUpdateDateToVideoMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                schema: "application",
                table: "SourceVideoMetrics",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDate",
                schema: "application",
                table: "SourceVideoDemographicMetrics",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateDate",
                schema: "application",
                table: "SourceVideoMetrics");

            migrationBuilder.DropColumn(
                name: "UpdateDate",
                schema: "application",
                table: "SourceVideoDemographicMetrics");
        }
    }
}
