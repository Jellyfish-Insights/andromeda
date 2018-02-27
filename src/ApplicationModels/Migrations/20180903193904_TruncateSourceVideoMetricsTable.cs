using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace ApplicationModels.Migrations {
    public partial class TruncateSourceVideoMetricsTable : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql(@"TRUNCATE TABLE application.""SourceVideoMetrics""", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {}
    }
}
