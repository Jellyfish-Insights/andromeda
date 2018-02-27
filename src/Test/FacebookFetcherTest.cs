using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Common;
using Common.Jobs;
using DataLakeModels;
using Npgsql;
using Test.Helpers;
using Xunit;

namespace Test {
    public class FacebookFetcherTest {

        AnalyticsPlatformSteps APS;
        static string[] AllTables = new[] {
            "ads",
            "ads_insights_day",
            "ads_insights_lifetime",
            "adsets",
            "campaigns",
            "customaudiences",

            // DISABLE: Schemas from "instagram_3.3.json" are disable until Fix issue 182.
            // "instagram_insights_day",
            // "instagram_insights_lifetime",
            // "media",
            // "media_insights_lifetime_carousel_album",
            // "media_insights_lifetime_image",
            // "media_insights_lifetime_video",

            "posts",
            "posts_insights_day",
            "posts_insights_lifetime",
            "video_lists",
            "video_lists_videos",
            "videos",
            "videos_comments",
            "videos_crosspost_shared_pages",
            "videos_reactions",
            "videos_sharedposts",
            "videos_video_insights_lifetime",
        };
        static string[] Entities = new[] {
            "ads",
            "adsets",
            "campaigns",
            "customaudiences",

            // Disabled until fix issue 182
            // "media",

            "posts",
            "video_lists",
            "videos",
        };
        static Dictionary<string, string[]> EdgesOfEntities = new Dictionary<string, string[]> {
            { "ads", new[] { "ads_insights_day", "ads_insights_lifetime" } },
            { "adsets", new string[] {} },
            { "campaigns", new string[] {} },
            { "customaudiences", new string[] {} },

            // Disabled until fix issue 182
            // { "media", new[] { "media_insights_lifetime_carousel_album" } },

            { "posts", new[] { "posts_insights_day", "posts_insights_lifetime" } },
            { "video_lists", new[] { "video_lists_videos" } },
            { "videos", new[] { "videos_comments", "videos_crosspost_shared_pages", "videos_reactions", "videos_sharedposts", "videos_video_insights_lifetime" } },
        };
        static string[] DailyMetrics = new[] {
            "ads_insights_day",

            // Disabled until fix issue 182
            // "instagram_insights_day",

            "posts_insights_day",
        };

        static string[] LifetimeMetrics = new[] {
            "ads_insights_lifetime",

            // Disabled until fix issue 182
            // "instagram_insights_lifetime",

            "posts_insights_lifetime",
            "videos_video_insights_lifetime",
        };

        public FacebookFetcherTest() {
            DatabaseReset.Drop(Databases.LakeFacebook);
            APS = new AnalyticsPlatformSteps();
        }

        private static void SetupTest(string testName) {
            FileSystemHelpers.DeleteDirectoryIfExists("cache");
            FileSystemHelpers.DeleteDirectoryIfExists("credentials");
            FileSystemHelpers.DeleteDirectoryIfExists("schema");
            if (File.Exists(JobConstants.JobConfigFile))
                File.Delete(JobConstants.JobConfigFile);
            ZipFile.ExtractToDirectory($"Resources/{testName}.zip", "./");
            DatabaseReset.MigrateFacebook();

            foreach (var f in Directory.EnumerateFiles("cache")) {
                Console.WriteLine(f);
            }
        }

        private void AssertQueryReturnsTrue(string query) {
            using (var connection = new NpgsqlConnection(FacebookDatabaseManager.ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = query;
                    Assert.True((bool) cmd.ExecuteScalar(), cmd.CommandText);
                }
        }

        private long CountRowsOnTable(string table) {
            using (var connection = new NpgsqlConnection(FacebookDatabaseManager.ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = $@"
                        SELECT COUNT(*) from {table}
                    ";
                    return (long) cmd.ExecuteScalar();
                }
        }

        [Fact]
        public void TheFollowingTablesExist() {
            SetupTest("clean");
            foreach (var tableName in AllTables) {
                AssertQueryReturnsTrue($@"
                    SELECT EXISTS(
                        SELECT
                            *
                        FROM
                            pg_catalog.pg_tables
                        where
                            schemaname = 'facebook_v3_3'
                            AND tablename = '{tableName}'
                    )");
            }
        }

        [Fact]
        public void CheckPrimeryKeyOfEntities() {
            SetupTest("clean");
            foreach (var tableName in Entities) {
                AssertQueryReturnsTrue($@"
                    SELECT EXISTS(
                        SELECT
                            *
                        FROM
                            information_schema.columns
                        WHERE
                            table_name='{tableName}' AND column_name='id'
                    )");
            }
        }

        [Fact]
        public void CheckPrimaryKeyOfEdges() {
            SetupTest("clean");
            foreach (var(rootName, edges) in EdgesOfEntities) {
                foreach (var tableName in edges) {
                    AssertQueryReturnsTrue($@"
                        SELECT EXISTS(
                            SELECT
                                *
                            FROM
                                information_schema.columns
                            WHERE
                                table_name='{tableName}' AND column_name='{rootName}_id'
                        )");
                }
            }
        }

        [Fact]
        public void CheckPrimaryKeyOfDailyMetrics() {
            SetupTest("clean");
            foreach (var tableName in DailyMetrics) {
                AssertQueryReturnsTrue($@"
                    SELECT
                        COUNT(*) = 4
                    FROM
                        information_schema.columns
                    WHERE
                        table_name='{tableName}'
                        AND column_name in ('date_start', 'date_stop', 'fetch_time', 'systime')");
            }
        }

        [Fact]
        public void CheckPrimaryKeyOfLifetimeMetrics() {
            SetupTest("clean");
            var tables = new string[] {
                "ads_insights_day",
                // Disabled until fix issue 182
                // "instagram_insights_day",
                "posts_insights_day",
            };

            foreach (var tableName in tables) {
                AssertQueryReturnsTrue($@"
                    SELECT
                        COUNT(*) = 4
                    FROM
                        information_schema.columns
                    WHERE
                        table_name='{tableName}'
                        AND column_name in ('date_start', 'date_stop', 'fetch_time', 'systime')");
            }
        }

        [Fact]
        public void CleanRunOnce() {
            SetupTest("tFetcher.sFacebook.max3.run1");
            APS.FacebookFetcherHasRun();
        }

        [Fact]
        public void CleanRunTwice() {
            SetupTest("tFetcher.sFacebook.max3.run2");
            APS.FacebookFetcherHasRun();
            APS.FacebookFetcherHasRun();
        }

        // TODO in order that this test to pass, we need to collect a test execution which is able to fetch all daily metrics without hitting API rate limit
        // [Fact]
        // public void DailyTotalMatchesLifetimeOnPosts() {
        //     SetupTest("tFetcher.sFacebook.max3.deep-first");
        //     var acceptedErrorMargin = 0;
        //     APS.FacebookFetcherHasRun();
        //     var columns = new string[] {
        //         "post_video_views",
        //         "post_video_views_paid",
        //         "post_video_views_10s_paid",
        //         "post_video_view_time",
        //         "post_video_view_time_organic"
        //     };

        //     foreach (var column in columns) {
        //         AssertQueryReturnsTrue($@"
        //             SET search_path = facebook_v3_1;

        //             SELECT NOT EXISTS (
        //                 SELECT
        //                     p.id,
        //                     lf.lifetime_total,
        //                     dt.daily_total
        //                 FROM
        //                     posts AS p
        //                 JOIN
        //                     (
        //                     SELECT
        //                         lf.posts_id,
        //                         lf.{column} as lifetime_total
        //                     FROM
        //                         posts_insights_lifetime AS lf
        //                     WHERE NOW()::timestamp <@ lf.systime
        //                     ) as lf ON lf.posts_id = p.id
        //                 JOIN
        //                 (
        //                     SELECT
        //                         posts_id,
        //                         SUM({column}) AS daily_total
        //                     FROM
        //                         posts_insights_day
        //                     WHERE NOW()::timestamp <@ systime
        //                     GROUP BY posts_id
        //                 ) AS dt ON dt.posts_id = p.id
        //                 WHERE abs(lf.lifetime_total - dt.daily_total) > {acceptedErrorMargin}
        //                 )");
        //     }
        // }
    }
}
