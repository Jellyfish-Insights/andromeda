using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using System.Collections.Generic;

namespace ApplicationModels.Migrations
{
    public partial class AddApplicationViews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE OR REPLACE VIEW application.""ApplicationVideoSourceVideos"" AS
                SELECT g.""ApplicationVideoId"",
                    g.""SourceVideoId"",
                    g.""UpdateDate"",
                    0::double precision AS ""IsUser""
                FROM application.""GeneratedApplicationVideoSourceVideos"" g
                WHERE NOT (EXISTS ( SELECT u.""ApplicationVideoId"",
                            u.""SourceVideoId"",
                            u.""UpdateDate""
                        FROM application.""UserApplicationVideoSourceVideos"" u
                        WHERE u.""SourceVideoId"" = g.""SourceVideoId""))
                UNION
                SELECT uas.""ApplicationVideoId"",
                    uas.""SourceVideoId"",
                    uas.""UpdateDate"",
                    1::double precision AS ""IsUser""
                FROM application.""UserApplicationVideoSourceVideos"" AS uas WHERE NOT ""Suppress""
            ");

            migrationBuilder.Sql(@"
            CREATE OR REPLACE VIEW application.""ApplicationVideoSourceCampaigns"" AS
                SELECT g.""VideoId"",
                    g.""CampaignId"",
                    g.""UpdateDate"",
                    0::double precision AS ""IsUser""
                FROM application.""GeneratedApplicationVideoSourceCampaigns"" g
                WHERE NOT (EXISTS ( SELECT u.""VideoId"",
                            u.""CampaignId"",
                            u.""UpdateDate""
                        FROM application.""UserApplicationVideoSourceCampaigns"" u
                        WHERE u.""CampaignId"" = g.""CampaignId""))
                UNION
                SELECT uas.""VideoId"",
                    uas.""CampaignId"",
                    uas.""UpdateDate"",
                    1::double precision AS ""IsUser""
                FROM application.""UserApplicationVideoSourceCampaigns"" AS uas WHERE NOT ""Suppress""
            ");

            migrationBuilder.Sql(@"
            CREATE OR REPLACE VIEW application.""ApplicationPersonaVersionSourceAdSets"" AS
                SELECT g.""AdSetId"",
                    g.""PersonaVersionId"",
                    g.""UpdateDate"",
                    0::double precision AS ""IsUser""
                FROM application.""GeneratedApplicationPersonaVersionSourceAdSets"" g
                WHERE NOT (EXISTS ( SELECT u.""AdSetId"",
                            u.""PersonaVersionId"",
                            u.""UpdateDate""
                        FROM application.""UserApplicationPersonaVersionSourceAdSets"" u
                        WHERE u.""AdSetId"" = g.""AdSetId""))
                UNION
                SELECT uas.""AdSetId"",
                    uas.""PersonaVersionId"",
                    uas.""UpdateDate"",
                    1::double precision AS ""IsUser""
                FROM application.""UserApplicationPersonaVersionSourceAdSets"" AS uas WHERE NOT ""Suppress""
            ");

            migrationBuilder.Sql(@"
            CREATE OR REPLACE VIEW application.""ApplicationPlaylistSourcePlaylists"" AS
                SELECT g.""SourcePlaylistId"",
                    g.""ApplicationPlaylistId"",
                    g.""UpdateDate"",
                    0::double precision AS ""IsUser""
                FROM application.""GeneratedApplicationPlaylistSourcePlaylists"" g
            ");

            migrationBuilder.Sql(@"
            CREATE OR REPLACE VIEW application.""ApplicationPlaylistApplicationVideos"" AS
                SELECT av.""Id"" AS ""ApplicationVideoId"",
                    a.""Id"" AS ""ApplicationPlaylistId"",
                    count(*) AS ""Count""
                FROM application.""ApplicationPlaylists"" a
                    JOIN application.""ApplicationPlaylistSourcePlaylists"" r ON a.""Id"" = r.""ApplicationPlaylistId""
                    JOIN application.""SourcePlaylists"" s ON s.""Id"" = r.""SourcePlaylistId""
                    JOIN application.""SourcePlaylistSourceVideos"" spsv ON s.""Id"" = spsv.""PlaylistId""
                    JOIN application.""SourceVideos"" sv ON sv.""Id"" = spsv.""VideoId""
                    JOIN application.""ApplicationVideoSourceVideos"" svav ON sv.""Id"" = svav.""SourceVideoId""
                    JOIN application.""ApplicationVideos"" av ON av.""Id"" = svav.""ApplicationVideoId""
                GROUP BY a.""Id"", av.""Id""
                ORDER BY av.""Id""
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW application.""ApplicationPlaylistApplicationVideos""");
            migrationBuilder.Sql(@"DROP VIEW application.""ApplicationVideoSourceVideos""");
            migrationBuilder.Sql(@"DROP VIEW application.""ApplicationPlaylistSourcePlaylists""");
            migrationBuilder.Sql(@"DROP VIEW application.""ApplicationVideoSourceCampaigns""");
            migrationBuilder.Sql(@"DROP VIEW application.""ApplicationPersonaVersionSourceAdSets""");
        }
    }
}
