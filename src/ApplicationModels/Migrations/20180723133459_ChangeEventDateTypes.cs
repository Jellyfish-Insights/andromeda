using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace ApplicationModels.Migrations
{
    public partial class ChangeEventDateTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                schema: "application",
                table: "SourceVideoMetrics",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                schema: "application",
                table: "SourceAdMetrics",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                schema: "application",
                table: "SourceVideoMetrics",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                schema: "application",
                table: "SourceAdMetrics",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
