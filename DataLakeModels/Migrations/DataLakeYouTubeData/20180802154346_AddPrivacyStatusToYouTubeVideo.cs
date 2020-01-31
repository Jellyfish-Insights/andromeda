using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace DataLakeModels.Migrations.DataLakeYouTubeData
{
    public partial class AddPrivacyStatusToYouTubeVideo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivacyStatus",
                schema: "youtube_data_v3",
                table: "Videos",
                nullable: true,
                defaultValue: "public");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyStatus",
                schema: "youtube_data_v3",
                table: "Videos");
        }
    }
}
