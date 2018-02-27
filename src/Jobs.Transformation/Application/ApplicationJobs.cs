using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Jobs.Transformation;

namespace Jobs.Transformation.Application {

    public class UpsertApplicationPlaylists : ApplicationTransformationJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Facebook.PlaylistSync>(), IdOf<YouTube.PlaylistSync>() };
        }

        public override Type Target { get => typeof(ApplicationPlaylist); }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            List<(ApplicationPlaylist, RowLog)> newPlaylists = new List<(ApplicationPlaylist, RowLog)>();
            foreach (var val in context.SourcePlaylists) {
                var vl = context.ApplicationPlaylists.Where(x => x.Title == val.Name);
                if (!vl.Any()) {
                    var log = new RowLog();
                    log.AddInput(typeof(SourcePlaylist), val.PrimaryKey);
                    var apPlaylist = new ApplicationPlaylist() {
                        Title = val.Name,
                        ThumbnailUrl = val.ThumbnailUrl,
                        CreateDate = val.UpdateDate,
                        UpdateDate = val.UpdateDate
                    };
                    log.NewVersion = apPlaylist.UpdateDate;
                    if (!newPlaylists.Where(x => x.Item1.Title == apPlaylist.Title).Any())
                        newPlaylists.Add((apPlaylist, log));
                }
            }
            newPlaylists.ForEach(x => context.Add(x.Item1));
            context.SaveChanges();
            newPlaylists.ForEach(x => {
                x.Item2.Id = x.Item1.PrimaryKey;
                trace.Add(x.Item2);
            });
        }
    }

    public class UpsertPlaylistAssociations : ApplicationTransformationJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Facebook.PlaylistSync>(), IdOf<UpsertApplicationPlaylists>() };
        }

        public override Type Target { get => typeof(GeneratedApplicationPlaylistSourcePlaylist); }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            foreach (var val in context.SourcePlaylists) {
                var vl = context.ApplicationPlaylists.Where(x => x.Title == val.Name);
                if (vl.Count() > 0) {
                    var v = vl.First();
                    var rell = context.GeneratedApplicationPlaylistSourcePlaylists.Where(x => x.ApplicationPlaylistId == v.Id && x.SourcePlaylistId == val.Id);
                    if (!rell.Any()) {
                        var log = new RowLog();
                        var gen = new GeneratedApplicationPlaylistSourcePlaylist() {
                            ApplicationPlaylistId = v.Id,
                            SourcePlaylistId = val.Id,
                            UpdateDate = v.UpdateDate
                        };
                        context.Add(gen);
                        log.AddInput(typeof(SourcePlaylist), val.PrimaryKey);
                        log.Id = gen.PrimaryKey;
                        log.NewVersion = gen.UpdateDate;
                        trace.Add(log);
                    }
                }
            }
        }
    }

    public class UpsertVideoAssociations : ApplicationTransformationJob {
        /**
           This job creates applications videos for non associated source videos.

           If an existing application video matches the title of the source video, a connection
           will be created between the existing objects.

           Automatic merging of many source videos into one AP video is only
           enabled for Facebook videos.
         */
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Facebook.VideoSync>(), IdOf<YouTube.VideoSync>() };
        }

        public const double TITLE_SIMILARITY_THRESHOLD = 0.8;

        public override Type Target { get => typeof(GeneratedApplicationVideoSourceVideo); }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            foreach (var val in context.SourceVideos) {
                var vl = context.ApplicationVideos.FromSql(@"
                                                           SET search_path TO application, public;
                                                           SELECT
                                                               a.*
                                                           FROM
                                                               ""ApplicationVideos"" a
                                                               LEFT JOIN ""ApplicationVideoSourceVideos"" ap ON ap.""ApplicationVideoId"" = a.""Id""
                                                               LEFT JOIN ""SourceVideos"" s ON s.""Id"" = ap.""SourceVideoId""
                                                           WHERE ((SIMILARITY(s.""Title"", {0}) > {1}
                                                                    OR {0} LIKE '%' || s.""Title"" || '%'
                                                                    OR s.""Title"" LIKE '%' || {0} || '%'
                                                                    OR s.""SourceUrl"" = {2})
                                                               OR (SIMILARITY(a.""Title"", {0}) > {1}
                                                                    OR {0} LIKE '%' || a.""Title"" || '%'
                                                                    OR a.""Title"" LIKE '%' || {0} || '%'))
                                                               AND ({3} = 'facebook'
                                                               OR NOT EXISTS(
                                                                  SELECT
                                                                      *
                                                                   FROM
                                                                      ""ApplicationVideoSourceVideos"" ap
                                                                      JOIN ""SourceVideos"" sv ON sv.""Id"" = ap.""SourceVideoId""
                                                                  WHERE
                                                                      ap.""ApplicationVideoId"" = a.""Id""
                                                                      AND sv.""Platform"" = 'youtube'
                                                                      AND sv.""Id"" <> {4}))
                                                           ORDER BY
                                                               GREATEST (ap.""IsUser"",
                                                                   SIMILARITY(s.""Title"", {0}),
                                                                   SIMILARITY(a.""Title"", {0}))
                                                               DESC
                                                           LIMIT 1
                                                           ",
                                                           val.Title,
                                                           TITLE_SIMILARITY_THRESHOLD,
                                                           val.SourceUrl,
                                                           val.Platform,
                                                           val.Id);
                ApplicationVideo v;
                if (!vl.Any()) {
                    var aplog = new RowLog();
                    aplog.AddInput(typeof(SourceVideo), val.PrimaryKey);
                    v = new ApplicationVideo() {
                        Title = val.Title,
                        CreateDate = val.UpdateDate,
                        UpdateDate = val.UpdateDate
                    };
                    aplog.NewVersion = v.UpdateDate;
                    context.Add(v);
                    context.SaveChanges();
                    aplog.Id = v.PrimaryKey;
                    trace.Add(aplog);
                } else {
                    v = vl.First();
                }
                var rell = context.GeneratedApplicationVideoSourceVideos.Where(x => x.SourceVideoId == val.Id);
                var log = new RowLog();
                var updateParams = new EntityUpdateParams<GeneratedApplicationVideoSourceVideo>() {
                    UpdateFunction = n => {
                        n.ApplicationVideoId = v.Id;
                        n.SourceVideoId = val.Id;
                        n.UpdateDate = val.UpdateDate;
                        return n;
                    },
                    ObjectValidity = new NpgsqlRange<DateTime>(val.UpdateDate, DateTime.UtcNow),
                    Trace = log,
                };
                SaveMutableEntity(context, trace, rell, updateParams);
                context.SaveChanges();
            }
        }
    }

    public class CreatePersonas : ApplicationTransformationJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Facebook.AdSetSync>(), IdOf<AdWords.AdSetSync>() };
        }

        public override Type Target { get => typeof(ApplicationPersona); }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            var newPersonas = new List<(ApplicationPersona, RowLog)>();
            foreach (var adSet in context.SourceAdSets) {
                var persona = PersonaHelper.ParsePersona(adSet.Title);
                if (persona.HasValue) {
                    var(personaName, personaVersion) = persona.Value;
                    var existingPersonas = context.ApplicationPersonas.Where(x => x.Name == personaName);
                    if (!existingPersonas.Any()) {
                        var log = new RowLog();
                        log.AddInput(typeof(SourceAdSet), adSet.PrimaryKey);
                        log.NewVersion = adSet.UpdateDate;
                        var personasToCreate = newPersonas.Where(x => x.Item1.Name == personaName);
                        if (!personasToCreate.Any())
                            newPersonas.Add((new ApplicationPersona(){ Name = personaName }, log));
                    }
                }
            }
            context.AddRange(newPersonas.Select(x => x.Item1));
            context.SaveChanges();
            newPersonas.ForEach(x => {
                x.Item2.Id = x.Item1.PrimaryKey;
                trace.Add(x.Item2);
            });
        }
    }

    public class CreatePersonaVersions : ApplicationTransformationJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<CreatePersonas>() };
        }

        public override Type Target { get => typeof(ApplicationPersonaVersion); }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            var newVersions = new List<(ApplicationPersonaVersion, RowLog)>();
            foreach (var adSet in context.SourceAdSets) {
                var persona = PersonaHelper.ParsePersona(adSet.Title);
                if (persona.HasValue) {
                    var(personaName, personaVersion) = persona.Value;
                    var existingVersions = context.ApplicationPersonaVersions.Where(x => x.Version == personaVersion && x.Persona.Name == personaName);
                    if (!existingVersions.Any()) {
                        var log = new RowLog();
                        log.AddInput(typeof(SourceAdSet), adSet.PrimaryKey);
                        log.NewVersion = adSet.UpdateDate;
                        var personaId = context.ApplicationPersonas.Where(x => x.Name == personaName).Single().Id;
                        var versionsToCreate = newVersions.Where(x => x.Item1.Version == personaVersion && x.Item1.PersonaId == personaId);
                        if (!versionsToCreate.Any())
                            newVersions.Add((new ApplicationPersonaVersion() { Version = personaVersion, PersonaId = personaId }, log));
                    }
                }
            }
            context.AddRange(newVersions.Select(x => x.Item1));
            context.SaveChanges();
            newVersions.ForEach(x => {
                x.Item2.Id = x.Item1.PrimaryKey;
                trace.Add(x.Item2);
            });
        }
    }

    public class UpsertPersonaVersionAdSetRelation : ApplicationTransformationJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<CreatePersonaVersions>() };
        }

        public override Type Target { get => typeof(GeneratedApplicationPersonaVersionSourceAdSet); }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            foreach (var adSet in context.SourceAdSets) {
                var persona = PersonaHelper.ParsePersona(adSet.Title);
                if (persona.HasValue) {
                    var(personaName, personaVersion) = persona.Value;
                    var existingVersions = context.ApplicationPersonaVersions.Where(x => x.Version == personaVersion && x.Persona.Name == personaName);
                    if (existingVersions.Any()) {
                        var version = existingVersions.Single();
                        var relation = context.GeneratedApplicationPersonaVersionSourceAdSets.Where(x => x.AdSetId == adSet.Id);
                        if (!relation.Any()) {
                            var log = new RowLog();
                            var newRelation = new GeneratedApplicationPersonaVersionSourceAdSet() {
                                AdSetId = adSet.Id,
                                PersonaVersionId = version.Id,
                                UpdateDate = adSet.UpdateDate,
                            };
                            context.Add(newRelation);
                            log.AddInput(typeof(SourceAdSet), adSet.PrimaryKey);
                            log.Id = newRelation.PrimaryKey;
                            log.NewVersion = newRelation.UpdateDate;
                            trace.Add(log);
                        }
                    }
                }
            }
        }
    }

    public class AutomaticVideoLengthTagging : ApplicationTransformationJob {
        public override Type Target => typeof(ApplicationMetaTag);

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<YouTube.VideoSync>(), IdOf<UpsertVideoAssociations>() };
        }

        private string GetDurationString(double seconds) {
            var minutes = seconds / 60.0;
            if (minutes <= 2.0) {
                return "0-2min";
            } else if (minutes <= 5.0) {
                return "3-5min";
            } else if (minutes <= 15.0) {
                return "6-15min";
            } else if (minutes <= 44.0) {
                return "16-44min";
            } else {
                return "45+min";
            }
        }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            var lengthMetaType = context.ApplicationMetaTagsTypes.Single(x => x.Type == "Length");
            var videos = from a in context.GeneratedApplicationVideoSourceVideos
                         join b in context.SourceVideos on a.SourceVideoId equals b.Id
                         select new {
                APVideoId = a.ApplicationVideoId,
                SourceVideoId = b.Id,
                DurationSecond = b.VideoLength
            };

            foreach (var v in videos.GroupBy(x => x.APVideoId)) {
                if (!context.ApplicationVideoApplicationMetaTags.Where(x => x.VideoId == v.Key && x.TypeId == lengthMetaType.Id).Any()) {
                    var durationString = GetDurationString(v.Select(x => x.DurationSecond).Max());
                    context.Add(
                        new ApplicationVideoApplicationMetaTag() {
                        TypeId = lengthMetaType.Id,
                        Tag = context.ApplicationMetaTags.Single(x => x.Type == lengthMetaType && x.Tag == durationString),
                        VideoId = v.Key
                    });
                }
            }

            context.SaveChanges();
        }
    }

    public class UpsertCampaignAssociations : ApplicationTransformationJob {
        public override Type Target => typeof(GeneratedApplicationVideoSourceCampaign);

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<UpsertVideoAssociations>(), IdOf<Facebook.AdSync>(), IdOf<AdWords.AdSync>() };
        }

        public override void Run(ApplicationDbContext context, JobTrace trace) {
            // Select all SourceCampaigns and ApplicationVideos
            // where SourceAds have SourceVideos
            // and all SourceVideos have exactly one ApVideo

            var campaignsVideos = from ad in context.SourceAds
                                  join r in context.ApplicationVideoSourceVideos on ad.VideoId equals r.SourceVideoId
                                  group new { Video = r, Ad = ad } by ad.CampaignId into gv
            where gv.Select(x => x.Video.ApplicationVideoId).Distinct().Count() == 1
            select new {
                Relation = new GeneratedApplicationVideoSourceCampaign {
                    CampaignId = gv.Key,
                    UpdateDate = MutableEntityExtentions.Max(gv.Max(x => x.Ad.UpdateDate), gv.Max(x => x.Video.UpdateDate)),
                    VideoId = gv.Select(x => x.Video.ApplicationVideoId).Distinct().First()
                },
                AdLog = gv.Select(x => x.Ad),
                VideoLog = gv.Select(x => x.Video)
            };

            foreach (var item in campaignsVideos) {
                var camp = item.Relation;
                var log = new RowLog() {
                    Id = camp.PrimaryKey,
                    NewVersion = camp.UpdateDate
                };
                log.AddInput(typeof(SourceAd).Name, JToken.FromObject(item.AdLog.Select(x => x.PrimaryKey)));
                log.AddInput(typeof(GeneratedApplicationVideoSourceVideo).Name, JToken.FromObject(item.VideoLog.Select(x => x.PrimaryKey)));
                var update = new EntityUpdateParams<GeneratedApplicationVideoSourceCampaign>() {
                    UpdateFunction = x => {
                        x.CampaignId = camp.CampaignId;
                        x.VideoId = camp.VideoId;
                        x.UpdateDate = camp.UpdateDate;
                        return x;
                    },
                    MatchFunction = x => x.CampaignId == item.Relation.CampaignId,
                    Trace = log,
                    ObjectValidity = new NpgsqlRange<DateTime>(item.Relation.UpdateDate, DateTime.UtcNow)
                };

                SaveMutableEntity(context, trace, context.GeneratedApplicationVideoSourceCampaigns.Where(update.MatchFunction), update);
            }
        }
    }
}
