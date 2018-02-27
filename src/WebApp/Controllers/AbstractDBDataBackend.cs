using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.DataViewModels;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers {

    public abstract class AbstractDBDataBackend : AbstractDataBackend {

        public List<string> PersonaList() {
            using (var context = new ApplicationDbContext()) {
                var query = context.ApplicationPersonas.Select(x => x.Name);
                return query.ToList();
            }
        }

        public static string SanitizeTags(string tag, bool lowerCase) {
            return String.Join(" ", (lowerCase ? tag.ToLower() : tag).Split(' ').Where(x => x.Length != 0));
        }

        public List<PersonaVersion> PersonaVersionList(ArchiveMode mode) {
            using (var context = new ApplicationDbContext()) {
                var query = from x in context.ApplicationPersonaVersions
                            where FilterArchive(mode, x.Archived)
                            select new PersonaVersion(){
                    Archived = x.Archived,
                    UpdateDate = x.UpdateDate,
                    Name = x.Persona.Name,
                    AdSets = context.ApplicationPersonaVersionSourceAdSets.Where(a => x.Id == a.PersonaVersionId).Select(z => SourceAdSetObject(context.SourceAdSets.First(y => y.Id == z.AdSetId))).ToList(),
                    Id = x.Id.ToString(),
                    Version = x.Version
                };
                return query.ToList();
            }
        }

        public List<string> PlaylistList() {
            using (var context = new ApplicationDbContext()) {
                var query = context.ApplicationPlaylists.Select(x => x.Title);
                return query.ToList();
            }
        }

        public void ApplyEditGenericTag(ApplicationGenericTag tag, TagEdit edit) {
            if (edit.Name != null) {
                tag.Tag = SanitizeTags(edit.Name, true);
            }
            tag.UpdateDate = DateTime.UtcNow;
        }

        public void ApplyEditMetaTag(ApplicationMetaTag tag, TagEdit edit) {
            if (edit.Name != null) {
                tag.Tag = SanitizeTags(edit.Name, false);
            }
            tag.UpdateDate = DateTime.UtcNow;
        }

        public void AddPersonaAdset(ApplicationDbContext context, ApplicationPersonaVersion persona, string adSetId) {
            var existing = context.UserApplicationPersonaVersionSourceAdSets.FirstOrDefault(x => x.AdSetId == adSetId);
            var updated = existing ?? new UserApplicationPersonaVersionSourceAdSet();
            updated.PersonaVersionId = persona.Id;
            updated.AdSetId = adSetId;
            updated.Suppress = false;
            updated.UpdateDate = DateTime.UtcNow;
            if (existing == null) {
                context.Add(updated);
            }
            context.SaveChanges();
        }

        public void RemovePersonaAdset(ApplicationDbContext context, ApplicationPersonaVersion persona, string adSetId) {
            var user = context.UserApplicationPersonaVersionSourceAdSets.FirstOrDefault(x => x.AdSetId == adSetId);
            // Do Nothing in  the case that there is no tag
            if (user != null) {
                if (user.PersonaVersionId == persona.Id) {
                    if (!user.Suppress) {
                        user.Suppress = true;
                    }
                }
            } else {
                var newConstraint = new UserApplicationPersonaVersionSourceAdSet(){
                    PersonaVersionId = persona.Id,
                    AdSetId = adSetId,
                    Suppress = true,
                    UpdateDate = DateTime.UtcNow
                };
                context.Add(newConstraint);
            }
            context.SaveChanges();
        }

        public bool EditPersonaVersion(ApplicationDbContext context, string key, PersonaVersionEdit edit) {
            switch (edit.Flag) {
                case EditType.Update: {
                    var intKey = int.Parse(key);
                    var existing = context.ApplicationPersonaVersions.First(x => x.Id == intKey);
                    if (edit.UpdateDate == existing.UpdateDate) {
                        if (edit.Archive.HasValue) {
                            existing.Archived = edit.Archive.Value;
                        }
                        if (edit.AddedAdSets != null)
                            foreach (var id in edit.AddedAdSets)
                                AddPersonaAdset(context, existing, id);

                        if (edit.RemovedAdSets != null)
                            foreach (var id in edit.RemovedAdSets)
                                RemovePersonaAdset(context, existing, id);
                    } else {
                        return false;
                    }
                }
                break;
            }
            return true;
        }

        public PersonaVersionEdits EditPersonas(PersonaVersionEdits edits) {
            var failed = new PersonaVersionEdits(){ Edits = new Dictionary<string, PersonaVersionEdit>() };

            using (var context = new ApplicationDbContext()) {
                using (var transaction = context.Database.BeginTransaction()) {
                    try {
                        foreach (var ed  in edits.Edits) {
                            var result = EditPersonaVersion(context, ed.Key, ed.Value);
                            if (!result)
                                failed.Edits[ed.Key] = ed.Value;
                        }
                        context.SaveChanges();
                        transaction.Commit();
                        return failed;
                    }catch (Exception e) {
                        Console.WriteLine(e.Message);
                        transaction.Rollback();
                        return edits;
                    }
                }
            }
        }

        public bool EditGenericTag(ApplicationDbContext context, string key, TagEdit edit) {
            switch (edit.Flag) {
                case EditType.New: {
                    var newType = context.ApplicationGenericTags.FirstOrDefault(x => x.Tag == key);
                    var newVideo = new ApplicationGenericTag(){};
                    ApplyEditGenericTag(newVideo, edit);
                    context.Add(newVideo);
                    context.SaveChanges();
                }
                break;
                case EditType.Update: {
                    var existingTag = context.ApplicationGenericTags.First(x => x.Tag == key);
                    if (edit.UpdateDate == existingTag.UpdateDate) {
                        ApplyEditGenericTag(existingTag, edit);
                    } else {
                        return false;
                    }
                }
                break;
                case EditType.Delete: {
                    var existingTag = context.ApplicationGenericTags.First(x => x.Tag == key);
                    context.Remove(existingTag);
                }
                break;
                default:
                    throw new Exception("No edit flag passed");
            }
            return true;
        }

        public bool EditMetaTag(ApplicationDbContext context, string type, string key, TagEdit edit) {
            switch (edit.Flag) {
                case EditType.New: {
                    var newType = context.ApplicationMetaTagsTypes.FirstOrDefault(x => x.Type == type);
                    var newVideo = new ApplicationMetaTag(){
                        Type = newType,
                    };
                    ApplyEditMetaTag(newVideo, edit);
                    context.Add(newVideo);
                    context.SaveChanges();
                }
                break;
                case EditType.Update: {
                    var existingTag = context.ApplicationMetaTags.First(x => x.Type.Type == type && x.Tag == key);
                    if (edit.UpdateDate == existingTag.UpdateDate) {
                        ApplyEditMetaTag(existingTag, edit);
                    } else {
                        return false;
                    }
                }
                break;
                case EditType.Delete: {
                    var existingTag = context.ApplicationMetaTags.First(x => x.Type.Type == type && x.Tag == key);
                    context.Remove(existingTag);
                }
                break;
                default:
                    throw new Exception("No edit flag passed");
            }

            return true;
        }

        public TagEdits EditTags(TagEdits edits) {
            var failed = new TagEdits(){ Type = edits.Type, Edits = new Dictionary<string, TagEdit>() };

            using (var context = new ApplicationDbContext()) {
                using (var transaction = context.Database.BeginTransaction()) {
                    try {
                        foreach (var ed  in edits.Edits) {
                            bool result;
                            if (edits.Type == Constants.GenericTag) {

                                result = EditGenericTag(context, ed.Key, ed.Value);
                            } else {
                                result = EditMetaTag(context, edits.Type, ed.Key, ed.Value);
                            }
                            if (!result)
                                failed.Edits[ed.Key] = ed.Value;
                        }
                        context.SaveChanges();
                        transaction.Commit();
                        return failed;
                    }catch (Exception e) {
                        Console.WriteLine(e.Message);
                        transaction.Rollback();
                        return edits;
                    }
                }
            }
        }

        public void EditVideoAction(ApplicationVideo video, VideoEdit edit) {
            if (edit.Archive.HasValue) {
                video.Archived = edit.Archive.Value;
            }

            if (edit.Title != null)
                video.Title = edit.Title;
            video.UpdateDate = DateTime.UtcNow;
        }

        public void EditVideoMetaTags(ApplicationDbContext context, ApplicationVideo video, KeyValuePair<string, string> metaTagId) {
            if (metaTagId.Value != null) {
                var refTag = context.ApplicationMetaTags.First(x => x.Tag == metaTagId.Value && x.Type.Type == metaTagId.Key);
                var currentTag = context.ApplicationVideoApplicationMetaTags.FirstOrDefault(x => x.VideoId == video.Id && x.Type.Type == metaTagId.Key);
                if (currentTag == null) {
                    var newTag = new ApplicationVideoApplicationMetaTag(){
                        VideoId = video.Id,
                        TagId = refTag.Id,
                        TypeId = refTag.TypeId
                    };
                    context.ApplicationVideoApplicationMetaTags.Add(newTag);
                } else {
                    currentTag.TagId = refTag.Id;
                    currentTag.TypeId = refTag.TypeId;
                    currentTag.Tag = refTag;
                }
            } else {
                var tag = context.ApplicationVideoApplicationMetaTags.First(x => x.VideoId == video.Id && x.Type.Type == metaTagId.Key);
                // Do Nothing in  the case that there is no tag
                if (tag != null)
                    context.ApplicationVideoApplicationMetaTags.Remove(tag);
            }
        }

        public void RemoveGenericTag(ApplicationDbContext context, ApplicationVideo video, string genericTagId) {
            var tag = context.ApplicationVideoApplicationGenericTags.FirstOrDefault(x => x.VideoId == video.Id && x.Tag.Tag == genericTagId);
            // Do Nothing in  the case that there is no tag
            if (tag != null) {
                context.ApplicationVideoApplicationGenericTags.Remove(tag);
            }
        }

        public void AddGenericTag(ApplicationDbContext context, ApplicationVideo video, string genericTagId) {
            var refTag = context.ApplicationGenericTags.FirstOrDefault(x => x.Tag == genericTagId);
            if (refTag == null) {
                refTag = new ApplicationGenericTag(){
                    Tag = genericTagId
                };
                context.Add(refTag);
                context.SaveChanges();
            }
            var tag = context.ApplicationVideoApplicationGenericTags.FirstOrDefault(x => x.VideoId == video.Id && x.Tag.Tag == genericTagId);

            // Do nothing in the case where the tag already exist
            // Generic tags can't be updated
            if (tag == null) {
                var newTag = new ApplicationVideoApplicationGenericTag(){
                    VideoId = video.Id,
                    TagId = refTag.Id
                };
                context.Add(newTag);
            }
        }

        public void AddVideoCampaign(ApplicationDbContext context, ApplicationVideo video, string campaignId) {
            var existing = context.UserApplicationVideoSourceCampaigns.FirstOrDefault(x => x.CampaignId == campaignId);
            var updated = existing ?? new UserApplicationVideoSourceCampaign();
            updated.CampaignId = campaignId;
            updated.VideoId = video.Id;
            updated.Suppress = false;
            updated.UpdateDate = DateTime.UtcNow;
            if (existing == null) {
                context.Add(updated);
            }
            context.SaveChanges();
        }

        public void RemoveVideoCampaign(ApplicationDbContext context, ApplicationVideo video, string campaignId) {
            var user = context.UserApplicationVideoSourceCampaigns.FirstOrDefault(x => x.CampaignId == campaignId);
            // Do Nothing in  the case that there is no tag
            if (user != null) {
                if (user.VideoId == video.Id)
                    if (!user.Suppress)
                        user.Suppress = true;
            } else {
                var newConstraint = new UserApplicationVideoSourceCampaign(){
                    CampaignId = campaignId,
                    VideoId = video.Id,
                    Suppress = true,
                    UpdateDate = DateTime.UtcNow
                };
                context.Add(newConstraint);
            }
            context.SaveChanges();
        }

        public void AddVideoSourceVideo(ApplicationDbContext context, ApplicationVideo video, string sourceVideoId) {
            var existing = context.UserApplicationVideoSourceVideos.FirstOrDefault(x => x.SourceVideoId == sourceVideoId);
            var updated = existing ?? new UserApplicationVideoSourceVideo();
            updated.SourceVideoId = sourceVideoId;
            updated.ApplicationVideoId = video.Id;
            updated.Suppress = false;
            updated.UpdateDate = DateTime.UtcNow;
            if (existing == null) {
                context.Add(updated);
            }
            context.SaveChanges();
        }

        public void RemoveVideoSourceVideo(ApplicationDbContext context, ApplicationVideo video, string sourceVideoId) {
            var user = context.UserApplicationVideoSourceVideos.FirstOrDefault(x => x.SourceVideoId == sourceVideoId);
            // Do Nothing in  the case that there is no tag
            if (user != null) {
                if (user.ApplicationVideoId == video.Id)
                    if (!user.Suppress)
                        user.Suppress = true;
            } else {
                var suppress = new UserApplicationVideoSourceVideo(){
                    SourceVideoId = sourceVideoId,
                    ApplicationVideoId = video.Id,
                    Suppress = true,
                    UpdateDate = DateTime.UtcNow
                };
                context.Add(suppress);
            }
            context.SaveChanges();
        }

        public void EditVideoRelation(ApplicationDbContext context, ApplicationVideo video, VideoEdit edit) {
            if (edit.MetaTags != null)
                foreach (var etag in edit.MetaTags)
                    EditVideoMetaTags(context, video, etag);
            if (edit.AddedGenericTags != null)
                foreach (var etag in edit.AddedGenericTags)
                    AddGenericTag(context, video, etag);
            if (edit.RemovedGenericTags != null)
                foreach (var etag in edit.RemovedGenericTags)
                    RemoveGenericTag(context, video, etag);
            if (edit.AddedCampaigns != null)
                foreach (var id in edit.AddedCampaigns)
                    AddVideoCampaign(context, video, id);
            if (edit.RemovedCampaigns != null)
                foreach (var id in edit.RemovedCampaigns)
                    RemoveVideoCampaign(context, video, id);
            if (edit.AddedVideos != null)
                foreach (var id in edit.AddedVideos)
                    AddVideoSourceVideo(context, video, id);
            if (edit.RemovedVideos != null)
                foreach (var id in edit.RemovedVideos)
                    RemoveVideoSourceVideo(context, video, id);
        }

        public List<SourceObject> UnAssociatedSources(SourceObjectType type) {
            using (var context = new ApplicationDbContext()) {
                switch (type) {
                    case SourceObjectType.Campaign:
                        var campaignsQ = from vc in context.ApplicationVideoSourceCampaigns
                                         join c in context.SourceCampaigns on vc.CampaignId equals c.Id
                                         select c;
                        return context.SourceCampaigns.Except(campaignsQ)
                                   .Select(SourceCampaignObject).ToList();
                    case SourceObjectType.AdSet:

                        var adsetsQ = from vc in context.ApplicationPersonaVersionSourceAdSets
                                      join c in context.SourceAdSets on vc.AdSetId equals c.Id
                                      select c;
                        return context.SourceAdSets.Except(adsetsQ)
                                   .Select(SourceAdSetObject).ToList();
                    case SourceObjectType.Video:
                        var sourceVideosQ = from avsv in context.ApplicationVideoSourceVideos
                                            join sv in context.SourceVideos on avsv.SourceVideoId equals sv.Id
                                            select sv;
                        return context.SourceVideos.Except(sourceVideosQ)
                                   .Select(SourceVideoObject).ToList();
                    default:
                        throw new Exception("Unsupported Source Type");
                }
            }
        }

        public bool EditVideo(ApplicationDbContext context, string id, VideoEdit edit) {
            switch (edit.Flag) {
                case EditType.New: {
                    if (edit.AddedVideos == null || !edit.AddedVideos.Any()) {
                        return false;
                    }
                    var newVideo = new ApplicationVideo();
                    EditVideoAction(newVideo, edit);
                    context.Add(newVideo);
                    context.SaveChanges();
                    EditVideoRelation(context, newVideo, edit);
                }
                break;
                case EditType.Update: {
                    int intId = int.Parse(id);
                    var existingVideo = context.ApplicationVideos.First(x => x.Id == intId);
                    if (edit.UpdateDate == existingVideo.UpdateDate) {
                        EditVideoAction(existingVideo, edit);
                        EditVideoRelation(context, existingVideo, edit);
                    } else {
                        return false;
                    }
                }
                break;
                case EditType.Delete: {
                    int intId = int.Parse(id);
                    var existingVideo = context.ApplicationVideos.First(x => x.Id == intId);
                    context.ApplicationVideos.Remove(existingVideo);
                }
                break;
                default:
                    throw new Exception("No edit flag passed");
            }
            return true;
        }

        public VideoEdits EditVideos(VideoEdits edits) {
            var failed = new VideoEdits(){ Edits = new Dictionary<string, VideoEdit>() };

            using (var context = new ApplicationDbContext()) {
                using (var transaction = context.Database.BeginTransaction()) {
                    try {
                        foreach (var ed  in edits.Edits) {
                            var result = EditVideo(context, ed.Key, ed.Value);
                            if (!result)
                                failed.Edits[ed.Key] = ed.Value;
                        }
                        context.SaveChanges();
                        transaction.Commit();
                        return failed;
                    }catch (Exception e) {
                        Console.WriteLine(e.Message);
                        transaction.Rollback();
                        return edits;
                    }
                }
            }
        }

        public List<Tag> TagList() {
            using (var context = new ApplicationDbContext()) {
                var metaTags
                    = context.ApplicationMetaTags
                          .Select(x => new Tag() { Type = x.Type.Type, Color = x.Color, UpdateDate = x.UpdateDate, Value = x.Tag });
                var generic
                    = context.ApplicationGenericTags
                          .Select(x => new Tag() { Type = Constants.GenericTag, UpdateDate = x.UpdateDate, Value = x.Tag });
                return generic.Union(metaTags).ToList();
            }
        }

        public IDictionary<string, List<Tag>> MetaTagsList() {
            using (var context = new ApplicationDbContext()) {
                var tags = TagList().GroupBy(x => x.Type).ToDictionary(x => x.Key, y => y.ToList());
                if (!tags.ContainsKey(Constants.GenericTag)) {
                    tags.Add(Constants.GenericTag, new List<Tag>());
                }
                return tags;
            }
        }

        public static IQueryable<ApplicationVideo> ApplyFilters(
            ApplicationDbContext context,
            IQueryable<ApplicationVideo> source,
            Tag[] filters,
            ArchiveMode archiveMode,
            PublishedMode publishedMode = PublishedMode.SomePublished) {
            var publishedStatus = from avsv in context.ApplicationVideoSourceVideos
                                  join sv in context.SourceVideos on avsv.SourceVideoId equals sv.Id
                                  group sv.PublishedStatus by avsv.ApplicationVideoId;

            var videoList = from v in source.Where(x => FilterArchive(archiveMode, x.Archived))
                            join vl in publishedStatus on v.Id equals vl.Key into publishedStatuses
                            from ps in publishedStatuses.DefaultIfEmpty()
                            where AggregatePublishedMode(ps, publishedMode)
                            select v;

            foreach (var tagSet in filters.GroupBy(x => x.Type)) {
                if (tagSet.Count() > 0) {
                    switch (tagSet.Key) {
                        case "platform":
                            // videoList = from v in videoList
                            //             join avsv in context.ApplicationVideoSourceVideos on v.Id equals avsv.ApplicationVideoId
                            //             join sv in context.SourceVideos.Where(x => tagSet.Select(v => v.Value).Contains(x.Platform)) on avsv.SourceVideoId equals sv.Id
                            //             select v;
                            break;
                        case "search":
                            videoList = videoList.Where(x => tagSet.Where(v => v != null).Any(v => EF.Functions.Like(x.Title, "%" + v.Value + "%")));
                            break;
                        case "playlist":
                            videoList = from v in videoList
                                        join vp in context.ApplicationPlaylistApplicationVideos on v.Id equals vp.ApplicationVideoId
                                        join p in context.ApplicationPlaylists.Where(x => tagSet.Select(v => v.Value).Contains(x.Title)) on vp.ApplicationPlaylistId equals p.Id
                                        select v;
                            break;
                        case Constants.GenericTag:
                            videoList = from v in videoList
                                        join vp in context.ApplicationVideoApplicationGenericTags on v.Id equals vp.VideoId
                                        join m in context.ApplicationGenericTags.Where(x => tagSet.Select(v => v.Value).Contains(x.Tag)) on vp.TagId equals m.Id
                                        select v;
                            break;
                        default:
                            videoList = from v in videoList
                                        join vp in context.ApplicationVideoApplicationMetaTags on v.Id equals vp.VideoId
                                        join m in context.ApplicationMetaTags.Where(x => tagSet.Select(v => v.Value).Contains(x.Tag)) on vp.TagId equals m.Id
                                        join mt in context.ApplicationMetaTagsTypes.Where(x => x.Type == tagSet.Key) on m.TypeId equals mt.Id
                                        select v;
                            break;
                    }
                }
            }
            return videoList;
        }

        public static IQueryable<SourceVideo> ApplySourceFilters(
            ApplicationDbContext context,
            Tag[] filters) {

            IQueryable<SourceVideo> sourceVideos = context.SourceVideos;

            foreach (var tagSet in filters.GroupBy(x => x.Type)) {
                if (tagSet.Count() > 0) {
                    switch (tagSet.Key) {
                        case "platform":
                            sourceVideos = context.SourceVideos.Where(x => tagSet.Select(v => v.Value).Contains(x.Platform));
                            break;
                    }
                }
            }
            return sourceVideos;
        }

        public static IQueryable<SourceAd> ApplySourcAdFilters(
            ApplicationDbContext context,
            Tag[] filters) {

            IQueryable<SourceAd> sourceAds = context.SourceAds;

            foreach (var tagSet in filters.GroupBy(x => x.Type)) {
                if (tagSet.Count() > 0) {
                    switch (tagSet.Key) {
                        case "platform":
                            sourceAds = context.SourceAds.Where(x => tagSet.Select(v => v.Value).Contains(x.Platform));
                            break;
                    }
                }
            }
            return sourceAds;
        }

        public static SourceObject SourceCampaignObject(SourceCampaign v) {
            switch (v.Platform) {
                case Constants.YouTubeSource:
                    return new SourceObject() {
                               SourceId = v.Id,
                               SourceName = v.Platform,
                               Title = v.Title,
                               Type = SourceObjectType.Campaign,
                               Links = new List<SourceLink>() {
                                   new SourceLink() {
                                       Type = SourceLinkType.Content,
                                       Link = String.Format("https://adwords.google.com/aw/adgroups?campaignId={0}", v.Id),
                                   }
                               }
                    };
                case Constants.FacebookSource:
                    return new SourceObject() {
                               SourceId = v.Id,
                               SourceName = v.Platform,
                               Title = v.Title,
                               Type = SourceObjectType.Campaign,
                               Links = new List<SourceLink>() {
                                   new SourceLink() {
                                       Type = SourceLinkType.Content,
                                       Link = String.Format("https://www.facebook.com/adsmanager/manage/adsets?act={0}&selected_campaign_ids={1}", "25217845", v.Id),
                                   }
                               }
                    };
                default:
                    throw new Exception("Platform not defined");
            }
        }

        public static List<SourceObject> ListSourceObjects(string platform, IEnumerable<SourceVideo> videos, IEnumerable<SourceCampaign> campaigns) {
            var result = new List<SourceObject>();
            foreach (SourceVideo v in videos) {
                result.Add(SourceVideoObject(v));
            }
            foreach (SourceCampaign c in campaigns) {
                result.Add(SourceCampaignObject(c));
            }
            return result;
        }

        public static SourceObject SourceAdSetObject(SourceAdSet v) {
            return new SourceObject {
                       SourceId = v.Id,
                       SourceName = v.Platform,
                       Title = v.Title,
                       Type = SourceObjectType.AdSet,
                       Links = new List<SourceLink>() {}
            };
        }

        private static List<SourceLink> GetLinksForFacebookObject(SourceVideo v) {
            var Links = new List<SourceLink>() {
                new SourceLink() {
                    Type = SourceLinkType.Content,
                    Link = String.Format("https://www.facebook.com/{0}/videos/{1}/", "feeonline", v.Id),
                }
            };
            if (v.Title != null) {
                Links.Add(
                    new SourceLink() {
                    Type = SourceLinkType.Analytics,
                    Link = String.Format("https://www.facebook.com/{0}/publishing_tools/?section=VIDEOS&filtering[0][field]=name&filtering[0][operator]=CONTAIN&filtering[0][value]={1}&sort[0]=created_time_descending", "feeonline", System.Uri.EscapeDataString(v.Title)),
                });
            }
            return Links;
        }

        public static SourceObject SourceVideoObject(SourceVideo v) {
            switch (v.Platform) {
                case Constants.YouTubeSource:
                    return new SourceObject {
                               SourceId = v.Id,
                               SourceName = v.Platform,
                               Title = v.Title,
                               Type = SourceObjectType.Video,
                               Links = new List<SourceLink>() {
                                   new SourceLink() {
                                       Type = SourceLinkType.Analytics,
                                       Link = String.Format("https://www.youtube.com/analytics?o=U#fi=v-{0}", v.Id),
                                   },
                                   new SourceLink() {
                                       Type = SourceLinkType.Content,
                                       Link = String.Format("https://www.youtube.com/watch?v={0}", v.Id),
                                   }
                               },
                               PublishedStatus = v.PublishedStatus,
                               PublishedAt = v.PublishedAt
                    };
                case Constants.FacebookSource:
                    return new SourceObject {
                               SourceId = v.Id,
                               SourceName = v.Platform,
                               Title = v.Title,
                               Type = SourceObjectType.Video,
                               Links = GetLinksForFacebookObject(v),
                               PublishedStatus = v.PublishedStatus,
                               PublishedAt = v.PublishedAt
                    };
                default:
                    throw new Exception("Platform not defined");
            }
        }

        public List<Video> VideoList(Tag[] filters, ArchiveMode archive) {
            using (var context = new ApplicationDbContext()) {

                var metaTagsQ = from vmt in context.ApplicationVideoApplicationMetaTags
                                join mt in context.ApplicationMetaTags on vmt.TagId equals mt.Id
                                join mtt in context.ApplicationMetaTagsTypes on mt.TypeId equals mtt.Id
                                select new { VideoId = vmt.VideoId, Tag = new Tag() { Type = mtt.Type, Value = mt.Tag } };

                var genericTagsQ = from vmt in context.ApplicationVideoApplicationGenericTags
                                   join mt in context.ApplicationGenericTags on vmt.TagId equals mt.Id
                                   select new { VideoId = vmt.VideoId, Tag = new Tag() { Type = Constants.GenericTag, Value = mt.Tag } };

                var sourceVideosQ = from avsv in context.ApplicationVideoSourceVideos
                                    join sv in context.SourceVideos on avsv.SourceVideoId equals sv.Id
                                    where sv.Title != null
                                    select new { Ap = avsv, Source = sv };

                var playlistsQ = from pv in context.ApplicationPlaylistApplicationVideos
                                 join p in context.ApplicationPlaylists on pv.ApplicationPlaylistId equals p.Id
                                 select new { Ap = pv, Playlist = p };

                var campaignsQ = from vc in context.ApplicationVideoSourceCampaigns
                                 join c in context.SourceCampaigns on vc.CampaignId equals c.Id
                                 select new { Ap = vc, Campaign = c };

                foreach (var tagSet in filters.GroupBy(x => x.Type)) {
                    if (tagSet.Count() > 0) {
                        switch (tagSet.Key) {
                            case "platform":
                                sourceVideosQ = sourceVideosQ.Where(x => tagSet.Select(v => v.Value).Contains(x.Source.Platform));
                                campaignsQ = campaignsQ.Where(x => tagSet.Select(v => v.Value).Contains(x.Campaign.Platform));
                                break;
                        }
                    }
                }

                var query = from v in ApplyFilters(context, context.ApplicationVideos, filters, archive)
                            join sv in sourceVideosQ on v.Id equals sv.Ap.ApplicationVideoId into sourceVideos
                            join pv in playlistsQ on v.Id equals pv.Ap.ApplicationVideoId into playList
                            join mt in metaTagsQ on v.Id equals mt.VideoId into metaTags
                            join gt in genericTagsQ on v.Id equals gt.VideoId into genericTags
                            join sc in campaignsQ on v.Id equals sc.Ap.VideoId into campaings
                            select new Video() {
                    Id = v.Id.ToString(),
                    Title = v.Title,
                    UpdateDate = v.UpdateDate,
                    ThumbnailLink = sourceVideos.OrderByDescending(x => x.Source.Platform).Select(x => x.Source.ThumbnailUrl).FirstOrDefault() ?? Constants.DefaultThumbnail,
                    Playlists = playList.Select(x => x.Playlist.Title).ToList(),
                    Tags = metaTags.Select(x => x.Tag).Union(genericTags.Select(x => x.Tag)).ToList(),
                    Sources = (from p in Constants.Sources.Select(x => new { Name = x })
                               join v in sourceVideos on p.Name equals v.Source.Platform into videos
                               join c in campaings on p.Name equals c.Campaign.Platform into camp
                               where videos.Count() > 0
                               select new Source {
                        SourceName = p.Name,
                        SourceObjects = ListSourceObjects(p.Name, videos.Select(x => x.Source), camp.Select(x => x.Campaign)),
                        VideosCount = videos.Count()
                    }).ToList(),
                    Archived = v.Archived,
                    PublishedAt = sourceVideos.Any() ? sourceVideos.Min(x => x.Source.PublishedAt) : v.UpdateDate,
                };

                foreach (var tagSet in filters.GroupBy(x => x.Type)) {
                    if (tagSet.Count() > 0) {
                        switch (tagSet.Key) {
                            case "platform":
                                query = query.Where(x => x.Sources.Any(y => tagSet.Select(v => v.Value).Contains(y.SourceName)));
                                break;
                        }
                    }
                }

                return query.ToList();
            }
        }
    }

    class Sample<T> {
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public T Group { get; set; }
    }
}
