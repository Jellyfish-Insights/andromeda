using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using DataLakeModels;
using Npgsql;
using FF = Jobs.Fetcher.Facebook.FacebookFetcher;

namespace Jobs.Transformation.Facebook {

    public class PlaylistSync : TracedFacebookJob {

        public override List<string> Dependencies() {
            return new List<string>() { FF.IdOf("page", "video_lists") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourcePlaylist));
            foreach (var val in ListPlaylists(cmd, trace)) {
                var existing = context.SourcePlaylists.Where(val.Item2.MatchFunction);
                SaveMutableEntity(context, trace, existing, val.Item2);
            }
            return trace;
        }
    }

    public class PlaylistVideoSync : TracedFacebookJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<PlaylistSync>(), IdOf<VideoSync>(), FF.IdOf("page", "videos") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourcePlaylistSourceVideo));
            foreach (var list in ListPlaylists(cmd, trace).ToList()) {
                var playlistVideos = ListPlaylistsVideos(cmd, trace, list.Item1);
                var hdl_playlist = new HashSet<long>(playlistVideos.Select(x => x.Item1));
                var ap_playlist = context.SourcePlaylistSourceVideos.Where(vi => vi.PlaylistId == list.Item1.ToString());
                var hap_playlist = new HashSet<long>(ap_playlist.Select(x => long.Parse(x.VideoId)));

                var create = hdl_playlist.Except(hap_playlist);
                foreach (var e in create) {
                    var item = playlistVideos.Where(x => x.Item1 == e).First();
                    context.Add(new SourcePlaylistSourceVideo() {
                        PlaylistId = list.Item1.ToString(),
                        VideoId = e.ToString(),
                        UpdateDate = item.Item2.LowerBound
                    });
                    var log = new RowLog() {
                        Id = MutableEntityExtentions.AutoPK(list.Item1, e),
                        NewVersion = item.Item2.LowerBound,
                    };
                    trace.Add(log);
                }

                var delete = hap_playlist.Except(hdl_playlist);
                foreach (var e in delete) {
                    var current = ap_playlist.Where(x => x.VideoId == e.ToString()).First();
                    var log = new RowLog() {
                        Id = MutableEntityExtentions.AutoPK(list.Item1, e),
                        OldVersion = current.UpdateDate,
                    };
                    trace.Add(log);
                }

                var del = context.SourcePlaylistSourceVideos.Where(x => x.PlaylistId == list.Item1.ToString() && delete.Contains(long.Parse(x.VideoId)));
                context.SourcePlaylistSourceVideos.RemoveRange(del);
            }
            return trace;
        }
    }

    public class VideoMetricSync : BatchedFacebookTransformationJob<SourceVideo> {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideoSync>(), FF.IdOf("page", "posts") };
        }

        protected override Type TargetTable {
            get => typeof(SourceVideoMetric);
        }
        public override SourceVideo ExecuteJob(ApplicationDbContext context, NpgsqlConnection cmd, JobTrace trace, SourceVideo previous) {

            IEnumerable<SourceVideo> ads;

            if (previous != null)
                ads = context.SourceVideos.Where(x => x.Platform == PLATFORM_FACEBOOK && x.Id.CompareTo(previous.Id) > 0).OrderBy(x => x.Id).Take(BatchSize);
            else
                ads = context.SourceVideos.Where(x => x.Platform == PLATFORM_FACEBOOK).OrderBy(x => x.Id).Take(BatchSize);
            foreach (var a in ads) {
                var latest = context.SourceVideoMetrics.Where(x => x.VideoId == a.Id)
                                 .Select(x => x.UpdateDate)
                                 .DefaultIfEmpty(DateTime.MinValue)
                                 .Max();

                Logger.Debug("Processing metrics for video {VideoId}, latest date is: {LatestDate}", a.Id, latest);
                foreach (var val in ListVideoDailyMetrics(cmd, trace, a.Id, latest)) {
                    var existing = context.SourceVideoMetrics.Where(val.MatchFunction);
                    SaveMutableEntity(context, trace, existing, val);
                }
            }
            return ads.LastOrDefault();
        }
    }

    public class VideoSync : TracedFacebookJob {
        public override List<string> Dependencies() {
            return new List<string>() { FF.IdOf("page", "videos"), FF.IdOf("adaccount", "ads") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceVideo));
            foreach (var obj in ListVideos(cmd, trace)) {
                var existing = context.SourceVideos.Where(obj.MatchFunction);
                SaveMutableEntity(context, trace, existing, obj);
            }
            return trace;
        }
    }

    public class DeltaEncodedVideoMetricSync : TracedFacebookJob {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideoSync>(), FF.IdOf("page", "videos") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceDeltaEncodedVideoMetric));
            foreach (var obj in ListDeltaEncodedVideoMetrics(cmd, trace)) {
                var existing = context.SourceDeltaEncodedVideoMetrics.Where(obj.MatchFunction);
                SaveMutableEntity<SourceDeltaEncodedVideoMetric>(context, trace, existing, obj);
            }
            return trace;
        }
    }

    public class VideoDemographicMetricSync : BatchedFacebookTransformationJob<SourceVideo> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<VideoSync>(), FF.IdOf("page", "videos") };
        }

        protected override Type TargetTable {
            get => typeof(SourceVideoDemographicMetric);
        }
        public override SourceVideo ExecuteJob(ApplicationDbContext context, NpgsqlConnection cmd, JobTrace trace, SourceVideo previous) {
            IEnumerable<SourceVideo> videos;
            if (previous != null)
                videos = context.SourceVideos.Where(v => v.Platform == PLATFORM_FACEBOOK && v.Id.CompareTo(previous.Id) > 0)
                             .OrderBy(x => x.Id)
                             .Take(BatchSize)
                             .ToList();
            else
                videos = context.SourceVideos.Where(v => v.Platform == PLATFORM_FACEBOOK)
                             .OrderBy(x => x.Id)
                             .Take(BatchSize)
                             .ToList();

            foreach (var video in videos) {
                var latest = context.SourceVideoDemographicMetrics.Where(x => video.Id.ToString() == x.VideoId)
                                 .Select(x => x.UpdateDate)
                                 .DefaultIfEmpty(DateTime.MinValue)
                                 .Max();

                Logger.Debug("Processing demographic metrics for video {VideoId}, latest date is: {LatestDate}", video.Id, latest);
                foreach (var obj in ListExistingVideoMetricsAgeGender(cmd, trace, long.Parse(video.Id), latest)) {
                    var existing = context.SourceVideoDemographicMetrics.Where(obj.MatchFunction);
                    SaveMutableEntity(context, trace, existing, obj);
                }
            }
            return videos.LastOrDefault();
        }
    }
}
