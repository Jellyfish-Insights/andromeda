using System;
using System.Linq;
using System.Collections.Generic;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using DataLakeModels;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;

namespace Jobs.Transformation.Facebook {

    public abstract class FacebookJob : TransformationJob {
        public const string PLATFORM_FACEBOOK = "facebook";

        public static IEnumerable<(long, EntityUpdateParams<SourcePlaylist>)> ListPlaylists(NpgsqlConnection conn, JobTrace trace) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        id,
                        title,
                        description,
                        systime,
                        thumbnail
                    FROM
                        video_lists v
                    WHERE
                        systime @> @now::TIMESTAMP
                    ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("video_lists", new RowLog() {
                            Id = new List<JToken>() {
                                JValue.FromObject(reader.Prim<long>("id"))
                            },
                            NewVersion = trace.StartTime,
                            OldVersion = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound,
                        });
                        yield return (reader.Prim<long>("id"), new EntityUpdateParams<SourcePlaylist>() {
                            UpdateFunction = v =>
                            {
                                v.Id = reader.Prim<long>("id").ToString();
                                v.Name = reader.OptClass<string>("title");
                                v.ThumbnailUrl = reader.OptClass<string>("thumbnail");
                                v.Platform = PLATFORM_FACEBOOK;
                                v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                                return v;
                            },
                            MatchFunction = v =>
                                            v.Id == reader.Prim<long>("id").ToString(),
                            Trace = log,
                            ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime")
                        });
                    }
                }
            }
        }

        public static IEnumerable<(long, NpgsqlRange<DateTime>)> ListPlaylistsVideos(NpgsqlConnection conn, JobTrace trace, long playlistId) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        l.id,
                        l.systime
                    FROM
                        video_lists_videos l
                        JOIN videos v ON l.id = v.id
                            AND v.systime && l.systime
                    WHERE
                        v.systime @> @now::TIMESTAMP
                        AND l.systime @> @now::TIMESTAMP
                        AND video_lists_id = @list
                    ";
                cmd.Parameters.AddWithValue("list", playlistId);
                cmd.Parameters.AddWithValue("now", trace.StartTime);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog() {
                            NewVersion = trace.StartTime,
                            OldVersion = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound
                        };
                        log.AddInput("video_lists_videos", MutableEntityExtentions.AutoPK(playlistId, reader.Prim<long>("id")));
                        trace.Add(log);
                        yield return (reader.Prim<long>("id"), reader.Prim<NpgsqlRange<DateTime>>("systime"));
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceVideoMetric>> ListVideoDailyMetrics(NpgsqlConnection conn, JobTrace trace, string video_id, DateTime previous) {
            using (var cmd = conn.CreateCommand())
            {

                cmd.CommandText = @"
                            SELECT
                                v.id,
                                event_date,
                                SUM(views) AS views,
                                SUM(view_time) AS view_time,
                                SUM(comments) AS comments,
                                SUM(likes) AS likes,
                                SUM(shares) AS shares,
                                MAX(max_systime) as max_systime,
                                SUM(reactions) AS reactions
                            FROM
                                videos v
                                JOIN (
                                    SELECT
                                        CAST(p.attachments -> 'data' -> 0 -> 'target' ->> 'id' AS bigint) AS videos_id,
                                        -- Subtract one day because facebook adds one day to the post metric
                                        --- See :  https://developers.facebook.com/docs/platforminsights/page/
                                        date_start :: date  - ('1 day' :: interval) AS event_date,
                                        SUM(pid.post_video_views) AS views,
                                        SUM(pid.post_video_view_time) AS view_time,
                                        NULL::bigint AS comments,
                                        NULL::bigint AS likes,
                                        NULL::bigint AS shares,
                                        MAX(lower(pid.systime)) as max_systime,
                                        NULL::bigint AS reactions
                                    FROM
                                        posts p
                                        JOIN posts_insights_day pid ON pid.posts_id = p.id
                                    WHERE (p.systime @> @now :: timestamp)
                                        AND (pid.systime @> @now :: timestamp)
                                    GROUP BY
                                        CAST(p.attachments -> 'data' -> 0 -> 'target' ->> 'id' AS bigint),
                                        date_start
                                    UNION ALL
                                    SELECT
                                        vc.videos_id,
                                        created_time::DATE AS event_date,
                                        NULL,
                                        NULL,
                                        COUNT(*) AS comments,
                                        NULL,
                                        NULL,
                                        MAX(created_time) as max_systime,
                                        NULL
                                    FROM
                                        videos_comments vc
                                    GROUP BY
                                        vc.videos_id,
                                        vc.created_time::DATE
                                    UNION ALL
                                    SELECT
                                        vc.videos_id,
                                        event_date :: DATE,
                                        NULL,
                                        NULL,
                                        NULL,
                                        COUNT(*) AS likes,
                                        NULL,
                                        MAX(LOWER(vc.systime)) as max_systime,
                                        NULL
                                    FROM
                                        videos_reactions vc
                                    JOIN
                                        videos v
                                        ON v.id = vc.videos_id
                                    JOIN LATERAL
                                        (SELECT
                                            CASE WHEN LOWER(vc.systime) <= @first_deploy_date :: date
                                            then  v.created_time :: DATE
                                            else LOWER(vc.systime)::DATE end
                                            ) a (event_date)
                                             ON true
                                    WHERE
                                        v.systime @> now() :: timestamp
                                        AND
                                        TYPE = 'LIKE'
                                    GROUP BY
                                        vc.videos_id,
                                        event_date :: DATE
                                         UNION ALL
                                    SELECT
                                        vc.videos_id,
                                        event_date :: DATE,
                                        NULL,
                                        NULL,
                                        NULL,
                                        NULL,
                                        NULL,
                                        MAX(LOWER(vc.systime)) as max_systime,
                                        COUNT(*) AS reactions
                                    FROM
                                        videos_reactions vc
                                    JOIN
                                        videos v
                                        ON v.id = vc.videos_id
                                    JOIN LATERAL
                                        (SELECT
                                            CASE WHEN LOWER(vc.systime) <= @first_deploy_date :: date
                                            then  v.created_time :: DATE
                                            else LOWER(vc.systime)::DATE end
                                            ) a (event_date)
                                             ON true
                                    WHERE
                                        v.systime @> now() :: timestamp
                                    GROUP BY
                                        vc.videos_id,
                                        event_date :: DATE
                                    UNION ALL
                                    SELECT
                                        vc.videos_id,
                                        created_time::DATE AS event_date,
                                        NULL,
                                        NULL,
                                        NULL,
                                        NULL,
                                        COUNT(*) AS shares,
                                        MAX(created_time) as max_systime,
                                        NULL
                                    FROM
                                        videos_sharedposts vc
                                    GROUP BY
                                        vc.videos_id,
                                        vc.created_time::DATE) u ON v.id = u.videos_id
                            WHERE (v.systime @> @now :: timestamp and v.id = @video_id )
                            GROUP BY
                                v.id,
                                event_date
                            HAVING MAX(max_systime ) >  @previous
                            ";

                // NOTE: This date is workaround  to reduce spikes created in the first deploy
                // If the system is redeployed with a new first deploy date this should be updated
                cmd.Parameters.AddWithValue("first_deploy_date", "2018-07-13");
                cmd.Parameters.AddWithValue("now", trace.StartTime);
                cmd.Parameters.AddWithValue("previous", previous);
                cmd.Parameters.AddWithValue("video_id", long.Parse(video_id));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("posts_insights_day",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id").ToString(), reader.Prim<DateTime>("event_date")));
                        log.AddInput("videos_sharedposts",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id").ToString(), reader.Prim<DateTime>("event_date")));
                        log.AddInput("videos_reactions",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id").ToString(), reader.Prim<DateTime>("event_date")), "LIKE");
                        log.AddInput("videos_comments",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id").ToString(), reader.Prim<DateTime>("event_date")));

                        yield return (
                            new EntityUpdateParams<SourceVideoMetric>() {
                            UpdateFunction = delegate(SourceVideoMetric v)
                            {
                                v.VideoId = reader.Prim<long>("id").ToString();
                                v.EventDate = reader.Prim<DateTime>("event_date");
                                v.ViewCount = reader.OptPrim<int>("views");
                                v.ViewTime = reader.OptPrim<long>("view_time");
                                v.CommentCount = reader.OptPrim<int>("comments");
                                v.LikeCount = reader.OptPrim<int>("likes");
                                v.ReactionCount = reader.OptPrim<int>("reactions");
                                v.ShareCount = reader.OptPrim<int>("shares");
                                v.UpdateDate = reader.Prim<DateTime>("max_systime");
                                return v;
                            },
                            MatchFunction = v =>
                                            v.VideoId == reader.Prim<long>("id").ToString() &&
                                            v.EventDate == reader.Prim<DateTime>("event_date"),
                            Trace = log,
                            ObjectValidity = new NpgsqlRange<DateTime>(reader.Prim<DateTime>("max_systime"), DateTime.Now)
                        }
                            );
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAdMetric>> ListAdsDailyMetrics(NpgsqlConnection conn, JobTrace trace, string ad_id, DateTime previous) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        ads_id,
                        clicks,
                        impressions,
                        systime,
                        date_start,
                        reach,
                        spend,
                        cpm,
                        cpc,
                        (
                            SELECT
                                SUM(CAST(v ->> 'value' AS bigint))
                            FROM
                                JSON_ARRAY_ELEMENTS(video_10_sec_watched_actions::json) AS v) AS video_view,
                        (
                            SELECT
                                SUM(CAST(v ->> 'value' AS bigint))
                            FROM
                                JSON_ARRAY_ELEMENTS(unique_actions::json) v
                            WHERE
                                v ->> 'action_type' LIKE '%lead%'::TEXT) AS email_capture,
                        (
                            SELECT
                                SUM(CAST(v ->> 'value' AS bigint) * CAST(c ->> 'value' AS double precision)) / SUM(CAST(v ->> 'value' AS double precision))
                            FROM
                                JSON_ARRAY_ELEMENTS(video_10_sec_watched_actions::json) AS v
                                JOIN JSON_ARRAY_ELEMENTS(cost_per_action_type::json) c ON c ->> 'action_type' = v ->> 'action_type') AS cost_per_video_view,
                        (
                            SELECT
                                SUM(CAST(v ->> 'value' AS bigint) * CAST(c ->> 'value' AS double precision))/ SUM(CAST(v ->> 'value' AS double precision))
                            FROM
                                JSON_ARRAY_ELEMENTS(unique_actions::json) v
                                JOIN JSON_ARRAY_ELEMENTS(cost_per_action_type::json) c ON c ->> 'action_type' = v ->> 'action_type'
                            WHERE
                                v ->> 'action_type' LIKE '%lead%'::TEXT) AS cost_per_email_capture
                    FROM
                        ads_insights_day v
                    where (v.systime @> @now :: timestamp)
                            and
                          (v.ads_id = @ad_id)
                            and
                          not (v.systime @> @previous :: timestamp)
                    ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                cmd.Parameters.AddWithValue("previous", previous);
                cmd.Parameters.AddWithValue("ad_id", long.Parse(ad_id));
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var date = reader.Prim<DateTime>("date_start");
                        var range = reader.Prim<NpgsqlRange<DateTime>>("systime");
                        var id = reader.Prim<long>("ads_id").ToString();
                        var log = new RowLog();
                        log.AddInput("ads_insights_day", reader.Prim<long>("ads_id"), range.LowerBound);
                        yield return (
                            new EntityUpdateParams<SourceAdMetric>() {
                            UpdateFunction = delegate(SourceAdMetric v)
                            {
                                v.AdId = reader.Prim<long>("ads_id").ToString();
                                v.Clicks = reader.DefPrim<int>("clicks");
                                v.Impressions = reader.DefPrim<int>("impressions");
                                v.Views = reader.DefPrim<int>("video_view");
                                v.EmailCapture = reader.DefPrim<int>("email_capture");
                                v.Reach = reader.DefPrim<int>("reach");
                                v.Cost = reader.DefPrim<double>("spend");
                                v.CostPerImpression = reader.DefPrim<double>("cpm") / 1000.0;
                                v.CostPerClick = reader.DefPrim<double>("cpc");
                                v.CostPerView = reader.DefPrim<double>("cost_per_video_view");
                                v.CostPerEmailCapture = reader.DefPrim<double>("cost_per_email_capture");
                                v.EventDate = reader.Prim<DateTime>("date_start");
                                v.UpdateDate = range.LowerBound;
                                return v;
                            },
                            MatchFunction = v =>
                                            v.AdId == reader.Prim<long>("ads_id").ToString() &&
                                            v.EventDate == reader.Prim<DateTime>("date_start"),
                            Trace = log,
                            ObjectValidity = range
                        }
                            );
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAudience>> ListAudiences(NpgsqlConnection conn, JobTrace trace) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        v.id,
                        v.NAME,
                        JSON_BUILD_OBJECT(
                            'rule', RULE,
                            'pixel_id', pixel_id,
                            'retention_days', retention_days,
                            'external_event_source', external_event_source,
                            'lookalike_audience_ids', lookalike_audience_ids,
                            'lookalike_spec', lookalike_spec,
                            'data_source', data_source,
                            'subtype', subtype
                        ) AS spec,
                        v.systime
                    FROM
                        customaudiences v
                    WHERE
                        v.systime @> @now::TIMESTAMP
                    ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("customaudiences",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id")));
                        yield return new EntityUpdateParams<SourceAudience>() {
                                   UpdateFunction = v =>
                                   {
                                       v.Id = reader.Prim<long>("id").ToString();
                                       v.Platform = PLATFORM_FACEBOOK;
                                       v.Definition = reader.Prim<string>("spec");
                                       v.Title = reader.Prim<string>("name");
                                       v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                                       return v;
                                   },
                                   Trace = log,
                                   MatchFunction = x => reader.Prim<long>("id").ToString() == x.Id,
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime")
                        };
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceCampaign>> ListCampaigns(NpgsqlConnection conn, JobTrace trace) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        v.id,
                        v.NAME,
                        v.objective,
                        v.start_time,
                        v.stop_time,
                        v.status,
                        v.systime
                    FROM
                        campaigns v
                    WHERE
                        v.systime @> @now::TIMESTAMP
                    ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("campaigns",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id")));
                        yield return new EntityUpdateParams<SourceCampaign>() {
                                   UpdateFunction = v =>
                                   {
                                       v.Id = reader.Prim<long>("id").ToString();
                                       v.Platform = PLATFORM_FACEBOOK;
                                       v.Title = reader.Prim<string>("name");
                                       v.Objective = reader.Prim<string>("objective");
                                       v.Status = reader.Prim<string>("status");
                                       v.StartTime = reader.OptClass<DateTime>("start_time");
                                       v.StopTime = reader.OptClass<DateTime>("stop_time");
                                       v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                                       return v;
                                   },
                                   MatchFunction = x => x.Id == reader.Prim<long>("id").ToString(),
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime")
                        };
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAd>> ListAds(NpgsqlConnection conn, JobTrace trace, ApplicationDbContext context) {
            var sourceVideosId = context.SourceVideos.Where(v => v.Platform == "facebook").Select(v => v.Id);

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $@"
                    SELECT
                        v.id,
                        v.NAME,
                        CAST(v.creative ->> 'video_id' AS bigint) AS video_id,
                        v.systime,
                        v.adset_id,
                        v.campaign_id
                    FROM
                        ads v
                    WHERE
                        v.systime @> @now::TIMESTAMP
                        AND v.creative ->> 'video_id' in (VALUES ('{String.Join("'), ('", sourceVideosId)}'))
                    ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("ads",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id")));
                        yield return new EntityUpdateParams<SourceAd>() {
                                   UpdateFunction = v =>
                                   {
                                       v.Id = reader.Prim<long>("id").ToString();
                                       v.Platform = PLATFORM_FACEBOOK;
                                       v.Title = reader.Prim<string>("name");
                                       v.VideoId = reader.OptPrim<long>("video_id").HasValue ? reader.OptPrim<long>("video_id").ToString() : null;
                                       v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                                       v.AdSetId = reader.Prim<long>("adset_id").ToString();
                                       v.CampaignId = reader.Prim<long>("campaign_id").ToString();
                                       return v;
                                   },
                                   MatchFunction =
                                       x => x.Id == reader.Prim<long>("id").ToString(),
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime")
                        };
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceVideoDemographicMetric>> ListNewVideoMetricsAgeGender(NpgsqlConnection conn, JobTrace trace, long videos) {

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        videos_id,
                        SUBSTRING(view_time.key, 1, 1) AS gender,
                        SUBSTRING(view_time.key, 3) AS age_group,
                        vl.systime AS systime,
                        CASE WHEN UPPER_INF(vl.systime) THEN date '9999-12-31' ELSE UPPER(vl.systime) END AS upper_systime,
                        LOWER(vl.systime) AS lower_systime,
                        view_time.value::bigint AS view_time
                    FROM
                        videos_video_insights_lifetime vl
                        JOIN videos v ON vl.videos_id = v.id
                            AND v.systime @> vl.fetch_time
                        JOIN LATERAL JSONB_EACH_TEXT(total_video_view_time_by_age_bucket_and_gender) view_time ON TRUE
                    WHERE
                        WHERE v.id = @existing_videos
                        AND view_time.value::bigint > 0
                    ";
                cmd.Parameters.AddWithValue("existing_videos", videos);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("videos_video_insights_lifetime",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("videos_id")));
                        log.AddInput("videos",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("videos_id")));
                        yield return new EntityUpdateParams<SourceVideoDemographicMetric>() {
                                   UpdateFunction = v =>
                                   {
                                       v.StartDate = reader.OptClass<DateTime>("lower_systime");
                                       v.EndDate = reader.OptClass<DateTime>("upper_systime");
                                       v.VideoId = reader.Prim<long>("videos_id").ToString();
                                       v.TotalViewTime = reader.Prim<double>("view_time");
                                       v.Gender = reader.Prim<string>("gender");
                                       v.AgeGroup = reader.Prim<string>("age_group");
                                       v.UpdateDate = reader.OptClass<DateTime>("lower_systime");
                                       return v;
                                   },
                                   Trace = log,
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime"),
                                   MatchFunction = v => v.VideoId == reader.Prim<long>("videos_id").ToString() && v.Gender == reader.Prim<string>("gender") && v.AgeGroup == reader.Prim<string>("age_group") && v.StartDate == reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound
                        };
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceVideoDemographicMetric>> ListExistingVideoMetricsAgeGender(NpgsqlConnection conn, JobTrace trace, long video_id, DateTime last_fetch) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        videos_id,
                        SUBSTRING(view_time.key, 1, 1) AS gender,
                        SUBSTRING(view_time.key, 3) AS age_group,
                        vl.systime AS systime,
                        CASE WHEN UPPER_INF(vl.systime) THEN date '9999-12-31' ELSE UPPER(vl.systime) END AS upper_systime,
                        LOWER(vl.systime) AS lower_systime,
                        view_time.value::bigint AS view_time
                    FROM
                        videos_video_insights_lifetime vl
                        JOIN videos v ON vl.videos_id = v.id
                            AND v.systime @> vl.fetch_time
                        JOIN LATERAL JSONB_EACH_TEXT(total_video_view_time_by_age_bucket_and_gender) view_time ON TRUE
                    WHERE
                        view_time.value::bigint > 0
                        AND videos_id = @video_id
                        AND LOWER(vl.systime) > @last_fetch
                    ";
                cmd.Parameters.AddWithValue("last_fetch", last_fetch);
                cmd.Parameters.AddWithValue("video_id", video_id);

                using (var reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("videos_video_insights_lifetime",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("videos_id")));
                        log.AddInput("videos",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("videos_id")));
                        yield return new EntityUpdateParams<SourceVideoDemographicMetric>() {
                                   UpdateFunction = v =>
                                   {
                                       v.StartDate = reader.OptClass<DateTime>("lower_systime");
                                       v.EndDate = reader.OptClass<DateTime>("upper_systime");
                                       v.VideoId = reader.Prim<long>("videos_id").ToString();
                                       v.TotalViewTime = reader.Prim<double>("view_time");
                                       v.Gender = reader.Prim<string>("gender");
                                       v.AgeGroup = reader.Prim<string>("age_group");
                                       v.UpdateDate = reader.OptClass<DateTime>("lower_systime");
                                       return v;
                                   },
                                   Trace = log,
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime"),
                                   MatchFunction = v => v.VideoId == reader.Prim<long>("videos_id").ToString() && v.Gender == reader.Prim<string>("gender") && v.AgeGroup == reader.Prim<string>("age_group") && v.StartDate == reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound
                        };
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceDeltaEncodedVideoMetric>> ListDeltaEncodedVideoMetrics(NpgsqlConnection conn, JobTrace trace) {
            using (var cmd = conn.CreateCommand())
            {
                /**
                 * Query break down:
                 *
                 * - This query joins lifetime metrics with meta data of the video
                 * - This result is partitioned by the video id, and ordered by the fetch_time
                 * - Within a partition we have multiple values of lifetime impressions of a video.
                 *   E.g.:
                 *    Table A
                 *    | video id | fetch_time | systime                  | value |
                 *    | 0        | 2019-01-01 | (2019-01-01, 2019-01-02) | 100   |
                 *    | 0        | 2019-01-02 | (2019-01-02, 2019-01-03) | 110   |
                 *    | 0        | 2019-01-03 | (2019-01-02, 2019-01-05) | 130   |
                 *    | 0        | 2019-01-05 | (2019-01-05, 2019-01-08) | 160   |
                 *    | 0        | 2019-01-08 | (2019-01-08, null)       | 200   |
                 *- From this partition, we'll derive the daily incrementals, which are:
                      Table B
                 *    | video id | systime                          | value |
                 *    | 0        | (v.created_time, 2019-01-01)     | 100   |
                 *    | 0        | (2019-01-01,     2019-01-02)     | 10    |
                 *    | 0        | (2019-01-02,     2019-01-03)     | 20    |
                 *    | 0        | (2019-01-03,     2019-01-05)     | 30    |
                 *    | 0        | (2019-01-05,     2019-01-08)     | 40    |
                 *- We do that by subtracting the consecutive rows of table A
                 *- The "get column X on the previous row in the partition" is done by LAG(X, 1)
                 *- In case we're running the LAG operation on the very 1st row, we need to inform a default value 'x' with LAG(X, 1, '1')
                 */cmd.CommandText = @"
                    SELECT
                        *
                    FROM (
                        SELECT
                            videos_id,
                            TSRANGE(
                                LAG(fetch_time, 1, v.created_time) OVER w,
                                UPPER(LAG(vl.systime, 1, TSRANGE('0001-01-01 00:00:00', fetch_time)) OVER w),
                                '[)'
                            ) AS systime,
                            LAG(total_video_impressions, 0) OVER w - COALESCE(LAG(total_video_impressions) OVER w, 0) AS video_impressions
                        FROM
                            videos_video_insights_lifetime vl
                            JOIN videos v ON vl.videos_id = v.id
                                AND v.systime @> vl.fetch_time WINDOW w AS (PARTITION BY videos_id ORDER BY fetch_time)
                    ) AS w
                    WHERE
                        video_impressions > 0
                    ";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("videos",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("videos_id")));
                        log.AddInput("videos_video_insights_lifetime",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("videos_id")));
                        yield return new EntityUpdateParams<SourceDeltaEncodedVideoMetric>() {
                                   UpdateFunction = v =>
                                   {
                                       v.StartDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                                       v.EndDate = reader.Prim<NpgsqlRange<DateTime>>("systime").UpperBound;
                                       v.VideoId = reader.Prim<long>("videos_id").ToString();
                                       v.ImpressionsCount = reader.Prim<int>("video_impressions");
                                       return v;
                                   },
                                   MatchFunction = x => x.VideoId == reader.Prim<long>("videos_id").ToString() && x.StartDate == reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound,
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime"),
                                   Trace = log
                        };
                    }
                }
            }
        }

        public static IEnumerable<(UpdateEntity<SourceAdSet>, UpdateEntity<SourceAudience>, RowLog, long, string[], NpgsqlRange<DateTime>)> ListAdSets(NpgsqlConnection conn, JobTrace trace) {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT
                        v.id,
                        v.NAME, (
                            SELECT
                                ARRAY_AGG(audience -> 'id')
                            FROM
                                JSON_ARRAY_ELEMENTS(v.targeting -> 'custom_audiences') audience) AS include, (
                            SELECT
                                ARRAY_AGG(audience -> 'id')
                            FROM
                                JSON_ARRAY_ELEMENTS(v.targeting -> 'excluded_custom_audiences') audience) AS exclude,
                        v.targeting,
                        v.targeting ->> 'flexible_spec' AS specs,
                        v.systime
                    FROM
                        adsets v
                    WHERE
                        v.systime @> @NOW::TIMESTAMP
                    ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("adsets", MutableEntityExtentions.AutoPK(reader.Prim<long>("id")));
                        yield return (delegate(SourceAdSet v)
                        {
                            v.Id = reader.Prim<long>("id").ToString();
                            v.Platform = PLATFORM_FACEBOOK;
                            v.Title = reader.Prim<string>("name");
                            v.Definition = reader.Prim<string>("targeting");
                            if (reader.OptClass<string[]>("include") == null) {
                                v.IncludeAudience = new string[] { reader.Prim<long>("id").ToString() };
                            } else {
                                v.IncludeAudience = reader.OptClass<string[]>("include");
                            }
                            v.ExcludeAudience = reader.OptClass<string[]>("exclude");
                            v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                            return v;
                        }
                                      , delegate(SourceAudience v)
                        {
                            v.Id = reader.Prim<long>("id").ToString();
                            v.Platform = PLATFORM_FACEBOOK;
                            v.Definition = reader.OptClass<string>("specs");
                            v.Title = reader.Prim<string>("name");
                            v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                            return v;
                        }
                                      ,
                                      log,
                                      reader.Prim<long>("id"),
                                      reader.OptClass<string[]>("include"),
                                      reader.Prim<NpgsqlRange<DateTime>>("systime")
                                      );
                    }
                }
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceVideo>> ListVideos(NpgsqlConnection conn, JobTrace trace) {
            using (var cmd = conn.CreateCommand())
            {
                // Note on video title:
                // Some Videos that were used as advertisement material get duplicated on Facebook.
                // This is done automatically by FB's API, and the process results in badly named videos with title 'Watch more videos'.
                // To fix these cases, we look for the original video used as source for the advertisement in the creative->object_story_spec->video_data->video_id.
                cmd.CommandText = @"
                    SELECT DISTINCT ON(v.id)
                        v.id,
                        CASE WHEN reference_video IS NOT NULL THEN
                            -- Lookup title in original video referenced by ad
                            (
                                SELECT
                                    v.title
                                FROM
                                    videos v
                                WHERE
                                    v.id = reference_video
                                    AND v.systime @> @NOW::TIMESTAMP)
                            ELSE
                                v.title
                            END AS title,
                        v.description,
                        v.LENGTH,
                        v.picture,
                        v.source,
                        v.created_time,
                        (a.created_time IS NOT NULL
                            OR v.published) AS published,
                        v.systime
                    FROM
                        videos v
                    -- Join with ads that have object_story spec and video_id
                    LEFT JOIN ads a ON v.id = CAST(a.creative ->> 'video_id' AS bigint)
                        AND a.creative ->> 'object_story_spec' IS NOT NULL
                    -- Referenced video in ad
                    LEFT JOIN LATERAL CAST(a.creative -> 'object_story_spec' -> 'video_data' ->> 'video_id' AS bigint) AS rv (reference_video) ON TRUE
                    WHERE
                        v.systime @> @NOW::TIMESTAMP
                        AND
                        -- Filter only when ads exist
                        (a.id IS NULL
                            OR a.systime @> @NOW::TIMESTAMP)
                        ";

                cmd.Parameters.AddWithValue("now", trace.StartTime);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var log = new RowLog();
                        log.AddInput("videos",
                                     MutableEntityExtentions.AutoPK(reader.Prim<long>("id")));

                        yield return new EntityUpdateParams<SourceVideo>() {
                                   UpdateFunction = v =>
                                   {
                                       v.Id = reader.Prim<long>("id").ToString();
                                       v.Platform = PLATFORM_FACEBOOK;
                                       v.Title = reader.OptClass<string>("title") ?? reader.OptClass<string>("description");
                                       v.Description = reader.OptClass<string>("description");
                                       v.VideoLength = reader.Prim<double>("length");
                                       v.ThumbnailUrl = reader.Prim<string>("picture");
                                       v.SourceUrl = reader.OptClass<string>("source");
                                       v.PublishedAt = reader.Prim<DateTime>("created_time");
                                       v.UpdateDate = reader.Prim<NpgsqlRange<DateTime>>("systime").LowerBound;
                                       v.PublishedStatus = reader.Prim<bool>("published");
                                       return v;
                                   },
                                   Trace = log,
                                   MatchFunction = v => v.Id == reader.Prim<long>("id").ToString(),
                                   ObjectValidity = reader.Prim<NpgsqlRange<DateTime>>("systime")
                        };
                    }
                }
            }
        }
    }

    public abstract class BatchedFacebookTransformationJob<K>: FacebookJob where K : class {

        public int BatchSize = 10;
        protected abstract Type TargetTable { get; }

        public override void Run() {
            using (var apContext = new ApplicationDbContext())
            {
                using (var connection = new NpgsqlConnection(FacebookDatabaseManager.ConnectionString()))
                {
                    connection.Open();
                    var hasNext = true;
                    K last = null;
                    var batchNum = 0;
                    while (hasNext)
                    {
                        batchNum++;
                        using (var transaction = apContext.Database.BeginTransaction())
                        {
                            Logger.Debug("Starting batch {BatchNum}", batchNum);
                            var trace = CreateTrace(TargetTable);
                            last = ExecuteJob(apContext, connection, trace, last);
                            if (last == null) {
                                hasNext = false;
                            }
                            trace.EndTime = DateTime.UtcNow;
                            if (trace.Modifications.Count > 0) {
                                apContext.Add(trace);
                                apContext.SaveChanges();
                                transaction.Commit();
                            } else {
                                transaction.Rollback();
                            }
                        }
                    }
                }
            }
        }

        public abstract K ExecuteJob(ApplicationDbContext context, NpgsqlConnection cmd, JobTrace trace, K offset);
    }

    public abstract class TracedFacebookJob : FacebookJob {

        public abstract JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd);

        public override void Run() {
            using (var context = new ApplicationDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                    using (var connection = new NpgsqlConnection(FacebookDatabaseManager.ConnectionString()))
                    {
                        connection.Open();
                        var trace = Job(context, connection);
                        trace.EndTime = DateTime.UtcNow;
                        context.Add(trace);
                        context.SaveChanges();
                        transaction.Commit();
                    }
            }
        }
    }
}
