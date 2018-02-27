using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {

    public abstract class AbstractMockDataBackend : AbstractDataBackend {
        public static Random rnd;
        public static List<Video> Videos;
        private static List<SourceObject> SourceObjects;
        public static List<PersonaVersion> Personas;
        public static Video GetVideo(
            string id,
            string title,
            List<string> playlists,
            List<Source> sources,
            List<Tag> tags
            ) {
            Tag lengthTag = GetLengthTag(rnd.Next(1, 7200));
            tags.Add(lengthTag);
            return new Video {
                       Id = id,
                       Title = title,
                       UpdateDate = DateTime.UtcNow,
                       Playlists = playlists,
                       Tags = tags,
                       Sources = sources,
                       PublishedAt = DateTime.Today.AddDays(-1 * rnd.Next(1, 1500))
            };
        }

        public List<PersonaVersion> PersonaVersionList(ArchiveMode mode) {
            return Personas.Where(x => FilterArchive(mode, x.Archived)).ToList();
        }

        public void RemoveVideoSourceObject(Video v, string removedId) {
            var obj = SourceObjects.Find(x => x.SourceId == removedId);
            var source = v.Sources.Find(x => x.SourceName == obj.SourceName);
            if (source != null) {

                if (obj.Type == SourceObjectType.Video)
                    source.VideosCount--;
                source.SourceObjects.Remove(obj);
            }
        }

        public PersonaVersionEdits EditPersonas(PersonaVersionEdits edits) {
            var failed = new PersonaVersionEdits(){ Edits = new Dictionary<string, PersonaVersionEdit>() };
            foreach (var etag in edits.Edits) {
                switch (etag.Value.Flag) {
                    case EditType.Update:
                        var current = Personas.Find(x => x.Id == etag.Key);
                        foreach (var edit  in edits.Edits) {
                            if (current.UpdateDate == edit.Value.UpdateDate) {
                                if (edit.Value.Archive.HasValue) {
                                    current.Archived = edit.Value.Archive.Value;
                                }
                                if (edit.Value.AddedAdSets != null) {
                                    foreach (var item in edit.Value.AddedAdSets)
                                        current.AdSets.Add(SourceObjects.Find(x => x.Type == SourceObjectType.AdSet && x.SourceId == item));
                                }
                                if (edit.Value.RemovedAdSets != null) {
                                    foreach (var item in edit.Value.RemovedAdSets)
                                        current.AdSets.RemoveAll(x => x.SourceId == item);
                                }
                            } else {
                                failed.Edits.Add(edit.Key, edit.Value);
                            }
                        }
                        break;
                    default:
                        throw new Exception("Unsupported operation Exception");
                }
            }
            return failed;
        }

        public TagEdits EditTags(TagEdits edit) {
            var failed = new TagEdits(){ Edits = new Dictionary<string, TagEdit>() };
            foreach (var etag in edit.Edits) {
                switch (etag.Value.Flag) {
                    case EditType.New:
                        var newTag = new Tag(){
                            Type = edit.Type,
                            Value = etag.Value.Name
                        };
                        MetaTags[edit.Type].Add(newTag);
                        break;
                    case EditType.Update:
                        var current = MetaTags[edit.Type].Find(x => x.Value == etag.Key);
                        if (current.UpdateDate == etag.Value.UpdateDate) {
                            current.Value = etag.Value.Name;
                        } else {
                            failed.Edits.Add(etag.Key, etag.Value);
                        }
                        break;
                    case EditType.Delete:
                        MetaTags[edit.Type].RemoveAll(x => x.Value == etag.Key);
                        break;
                }
            }
            return failed;
        }

        public static void AddVideoSourceObject(Video v, string addedId) {
            var obj = SourceObjects.Find(x => x.SourceId == addedId);
            var source = v.Sources.FirstOrDefault(x => x.SourceName == obj.SourceName) ?? new Source(){
                SourceName = obj.SourceName,
                VideosCount = 0,
                SourceObjects = new List<SourceObject>()
            };
            if (obj.Type == SourceObjectType.Video)
                source.VideosCount++;
            source.SourceObjects.Add(obj);
        }

        public void EditVideoAction(Video v, VideoEdit edit) {
            if (edit.Archive.HasValue) {
                v.Archived = edit.Archive.Value;
            }
            if (edit.Title != null) {
                v.Title = edit.Title;
            }
            if (edit.MetaTags != null) {
                foreach (var etag in edit.MetaTags) {
                    var value = etag.Value;
                    if (value == null) {
                        v.Tags.RemoveAll(x => x.Type == etag.Key);
                    } else {
                        v.Tags.RemoveAll(x => x.Type == etag.Key);
                        v.Tags.Add(new Tag(){ Type = etag.Key, Value = etag.Value });
                    }
                }
            }
            if (edit.AddedGenericTags != null) {
                v.Tags.AddRange(edit.AddedGenericTags.Select(x => new Tag(){ Type = Constants.GenericTag, Value = x }));
            }
            if (edit.RemovedGenericTags != null) {
                v.Tags.RemoveAll(x => edit.RemovedGenericTags.Contains(x.Value));
            }
            if (edit.AddedVideos != null) {
                foreach (var id in edit.AddedVideos) {
                    AddVideoSourceObject(v, id);
                }
            }
            if (edit.RemovedVideos != null) {
                foreach (var id in edit.RemovedVideos) {
                    RemoveVideoSourceObject(v, id);
                }
            }
            if (edit.AddedCampaigns != null) {
                foreach (var id in edit.AddedCampaigns) {
                    AddVideoSourceObject(v, id);
                }
            }
            if (edit.RemovedCampaigns != null) {
                foreach (var id in edit.RemovedCampaigns) {
                    RemoveVideoSourceObject(v, id);
                }
            }
        }

        public bool EditVideo(string id, VideoEdit edit) {
            switch (edit.Flag) {
                case EditType.New:
                    var newVideo = new Video();
                    EditVideoAction(newVideo, edit);
                    Videos.Add(newVideo);
                    break;
                case EditType.Update:
                    var existingVideo = Videos.Find(x => x.Id == id.ToString());
                    if (existingVideo.UpdateDate == edit.UpdateDate) {
                        EditVideoAction(existingVideo, edit);
                    } else {
                        return false;
                    }
                    break;
                case EditType.Delete:
                    Videos.RemoveAll(x => x.Id == id.ToString());
                    break;
            }
            return true;
        }

        public VideoEdits EditVideos(VideoEdits videoEdits) {
            var failed = new VideoEdits(){ Edits = new Dictionary<string, VideoEdit>() };

            foreach (var edit in videoEdits.Edits) {
                var result = EditVideo(edit.Key, edit.Value);
                if (!result)
                    failed.Edits[edit.Key] = edit.Value;
            }
            return failed;
        }

        public static Tag ToneHappy = new Tag { Type = "Tone", Value = "Happy" };
        public static Tag ToneSad = new Tag { Type = "Tone", Value = "Sad" };
        public static Tag ToneAngry = new Tag { Type = "Tone", Value = "Angry" };
        public static Tag ToneMelancholic = new Tag { Type = "Tone", Value = "Melancholic" };
        public static Tag ToneFrustrated = new Tag { Type = "Tone", Value = "Frustrated" };
        public static List<Tag> Tones = new List<Tag>() { ToneHappy, ToneSad, ToneAngry, ToneMelancholic, ToneFrustrated };

        public static Tag Topic1 = new Tag { Type = "Topic", Value = "Topic 1" };
        public static Tag Topic2 = new Tag { Type = "Topic", Value = "Topic 2" };
        public static List<Tag> Topics = new List<Tag>() { Topic1, Topic2 };
        public static Tag Medium1 = new Tag { Type = "Medium", Value = "Medium 1" };
        public static Tag Medium2 = new Tag { Type = "Medium", Value = "Medium 2" };
        public static List<Tag> Mediums = new List<Tag>() { Medium1, Medium2 };

        public static string PlaylistA = "Playlist A";
        public static string PlaylistB = "Playlist B";
        public static string PlaylistC = "Playlist C";
        public static string PlaylistD = "Playlist D";
        public List<string> PlaylistList() {
            return new List<string>(){ PlaylistA, PlaylistB, PlaylistC, PlaylistD };
        }

        public List<Tag> TagList() {
            var VideoTags = Videos.SelectMany(v => v.Tags);
            var MetaTags = Tones.Concat(Lengths).Concat(Topics).Concat(Mediums);
            return MetaTags.Concat(VideoTags).Distinct().ToList();
        }

        public static List<Tag> Lengths = new List<Tag>() {
            new Tag { Type = "Length", Value = "0-2min" },
            new Tag { Type = "Length", Value = "3-5min" },
            new Tag { Type = "Length", Value = "6-15min" },
            new Tag { Type = "Length", Value = "16-44min" },
            new Tag { Type = "Length", Value = "45+min" },
            new Tag { Type = "Length", Value = "22-24min" },
            new Tag { Type = "Length", Value = "44-48min" },
        };
        public IDictionary<string, List<Tag>> MetaTags = new Dictionary<string, List<Tag>>() {
            { Tones[0].Type.ToLower(), Tones },
            { Topics[0].Type.ToLower(), Topics },
            { Mediums[0].Type.ToLower(), Mediums },
            { Lengths[0].Type.ToLower(), Lengths },
        };
        public IDictionary<string, List<Tag>> MetaTagsList() {
            return MetaTags;
        }

        public static Tag GetLengthTag(int durationInSeconds) {
            const int secondsInMinute = 60;
            string tagString = "0-2min";
            switch (durationInSeconds) {
                case int v when v < 3 * secondsInMinute:
                    tagString = "0-2min";
                    break;
                case int v when v >= 3 * secondsInMinute && v < 6 * secondsInMinute:
                    tagString = "3-5min";
                    break;
                case int v when v >= 6 * secondsInMinute && v < 16 * secondsInMinute:
                    tagString = "6-15min";
                    break;
                case int v when v >= 16 * secondsInMinute && v < 45 * secondsInMinute:
                    tagString = "16-44min";
                    break;
                case int v when v >= 45 * secondsInMinute:
                    tagString = "45+min";
                    break;
            }
            return new Tag { Type = "Length", Value = tagString };
        }

        public List<string> PersonaList() {
            return Personas.Select(x => x.Name).Distinct().ToList();
        }

        public static List<PersonaVersion> GeneratePersonas() {
            int totalPersonas = 5;
            var personas = new List<PersonaVersion>();
            for (int i = 0; i < totalPersonas; i++) {
                personas.Add(new PersonaVersion(){
                    Name = "Persona " + i,
                    Version = "0"
                });
            }
            return personas;
        }

        public static List<Metric> GetMetric(List<MetricInfo> metricInfo) {
            var metrics = new List<Metric>();
            foreach (var m in metricInfo) {
                metrics.Add(new Metric { Type = m.Type, Value = rnd.Next(1, 300) * 1.0 });
            }
            return metrics;
        }

        public static PersonaMetric GetPersonaMetric(List<MetricInfo> metricInfo, string persona) {

            return new PersonaMetric { Persona = persona, Metrics = GetMetric(metricInfo) };
        }

        public VideoMetric GetVideoMetric(List<MetricInfo> metricInfo, string id) {
            var metrics = new List<PersonaMetric>();
            List<string> personas = PersonaList();
            foreach (string p in personas) {
                metrics.Add(GetPersonaMetric(metricInfo, p));
            }
            PersonaMetric GrandTotal = new PersonaMetric { Persona = Constants.GrandTotalName, Metrics = GetMetric(metricInfo) };
            // This is just a mock aggregation of the metrics, which sums them
            // This does not mean that the real aggregation of values for a metric will be a sum
            foreach (var metric in metricInfo) {
                var metricName = metric.Type;
                var total = 0.0;
                foreach (var personaMetric in metrics) {
                    total += personaMetric.Metrics.Find(x => x.Type == metricName).Value;
                }
                GrandTotal.Metrics.Add(new Metric { Type = metricName, Value = total });
            }
            metrics.Add(GrandTotal);
            return new VideoMetric {
                       Id = id,
                       TotalMetrics = GetMetric(metricInfo),
                       MetricsPerPersona = metrics,
            };
        }

        public List<VideoMetric> GenerateVideoMetrics(List<MetricInfo> metricInfo, List<Video> videos) {
            return videos.Select(x => GetVideoMetric(metricInfo, x.Id)).ToList();
        }

        public List<VideoMetric> MetricList(DateTime startDate, DateTime endDate, Tag[] filters, List<MetricInfo> metrics, ArchiveMode archive) {
            return GenerateVideoMetrics(metrics, ApplyFilters(Videos, filters, archive).ToList());
        }

        public override Dictionary<MetricType, TimeSeries> ComputePrimitiveTimeSeries(MetricType[] metrics, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archive) {
            var store = new Dictionary<MetricType, TimeSeries>();
            var dates = DateUtilities.GetDatesBetween(start, end);
            foreach (var metric in metrics) {
                var Values = new List<TimeSeriesDataGroup>();
                IEnumerable<string> groups;
                // Marketing ignores type
                if (type == null) {
                    groups = PersonaList();
                } else {
                    groups = ApplyFilters(Videos, filters, archive)
                                 .Select(x => x.Tags.Where(y => y.Type.ToLower() == type.ToLower()).Select(y => y.Value))
                                 .Where(v => v.Any())
                                 .Aggregate(new List<string>().AsEnumerable(), (x, y) => x.Concat(y));
                }
                foreach (var group in groups.Distinct()) {
                    var values = dates.Select(x => (rnd.Next(1, 300)) * 1.0).ToArray();
                    Values.Add(new TimeSeriesDataGroup { GroupName = group, Values = values });
                }
                store.Add(metric, ReduceTimeSeries(dates, Values));
            }
            return store;
        }

        public static IEnumerable<Video> ApplyFilters(IEnumerable<Video> source, Tag[] filters, ArchiveMode archive) {
            var videoList = source.Where(x => FilterArchive(archive, x.Archived));
            foreach (var tagSet in  filters.GroupBy(x => x.Type)) {
                switch (tagSet.Key) {
                    case "search":
                        if (tagSet.Count() > 0) {
                            videoList = videoList.Where(x => tagSet.Any(v => x.Title.Contains(v.Value)));
                        }
                        break;
                    case "playlist":
                        if (tagSet.Count() > 0) {
                            videoList = videoList.Where(x => x.Playlists.Any(y => tagSet.Select(v => v.Value).Contains(y)));
                        }
                        break;
                    default:
                        if (tagSet.Count() > 0) {
                            videoList = videoList.Where(x => x.Tags.Any(y => tagSet.Key == y.Type && tagSet.Select(v => v.Value).Contains(y.Value)));
                        }
                        break;
                }
            }
            return videoList;
        }

        public List<SourceObject> UnAssociatedSources(SourceObjectType type) {
            return StaticUnAssociatedSources(type);
        }

        static List<SourceObject> StaticUnAssociatedSources(SourceObjectType type) {
            var associated = Videos.SelectMany(x => x.Sources != null ? x.Sources.SelectMany(y => y.SourceObjects) : new List<SourceObject>());
            return SourceObjects.Where(x => x.Type == type && !associated.Contains(x)).ToList();
        }

        public List<Video> VideoList(Tag[] filters, ArchiveMode archive) {
            return ApplyFilters(Videos, filters, archive).ToList();
        }

        public static List<Source> GenerateSourceList() {
            return new[] { Constants.YouTubeSource, Constants.FacebookSource }.Select(x => GenerateSource(x)).Where(x => x.VideosCount > 0).ToList();
        }

        public static List<T> SelectItems<T>(List<T> items, int k) {
            var selected = new List<T>();
            var needed = k;
            var available = items.Count;
            if (available > 0 && needed > 0) {
                while (selected.Count < k) {
                    if (rnd.NextDouble() < needed / available) {
                        selected.Add(items[available - 1]);
                        needed--;
                    }
                    available--;
                }
            }
            return selected;
        }

        public static Source GenerateSource(string source) {
            var refsVideo = SelectItems(StaticUnAssociatedSources(SourceObjectType.Video).FindAll(x => x.SourceName == source), 1 + rnd.Next() % 2);
            var refsCampaign = SelectItems(StaticUnAssociatedSources(SourceObjectType.Campaign).FindAll(x => x.SourceName == source), refsVideo.Count() * (rnd.Next() % 2));
            return new Source {
                       SourceName = source,
                       VideosCount = refsVideo.Count(),
                       SourceObjects = refsCampaign.Union(refsVideo).ToList()
            };
        }

        public static void GenerateVideos() {
            Videos = new List<Video>();
            Videos.Add(GetVideo(
                           "1",
                           "This is a Video Title 1",
                           new List<string> { PlaylistA },
                           GenerateSourceList(),
                           new List<Tag> { ToneAngry, Topic1 }));
            Videos.Add(GetVideo(
                           "2",
                           "This is a Video Title 2",
                           new List<string> { PlaylistA },
                           GenerateSourceList(),
                           new List<Tag> { ToneHappy, Topic1, Medium2 }));
            Videos.Add(GetVideo(
                           "3",
                           "This is a Video Title 3",
                           new List<string> { PlaylistA },
                           GenerateSourceList(),
                           new List<Tag> { ToneMelancholic, Topic2 }));
            Videos.Add(GetVideo(
                           "4",
                           "This is a Video Title 4",
                           new List<string> { PlaylistB },
                           GenerateSourceList(),
                           new List<Tag> { ToneFrustrated, Medium1 }));
            Videos.Add(GetVideo(
                           "5",
                           "This is a Video Title 5",
                           new List<string> { PlaylistB },
                           GenerateSourceList(),
                           new List<Tag> { ToneAngry, Topic2 }));
            Videos.Add(GetVideo(
                           "6",
                           "This is a Video Title 6",
                           new List<string> { PlaylistB },
                           GenerateSourceList(),
                           new List<Tag> { ToneFrustrated, Topic2 }));
            Videos.Add(GetVideo(
                           "7",
                           "This is a Video Title 7",
                           new List<string> { PlaylistB, PlaylistA },
                           GenerateSourceList(),
                           new List<Tag> { ToneSad, Topic1 }));
            Videos.Add(GetVideo(
                           "8",
                           "This is a Video Title Super 8",
                           new List<string> { PlaylistC },
                           GenerateSourceList(),
                           new List<Tag> { ToneMelancholic, Topic1, Medium1 }));
            Videos.Add(GetVideo(
                           "9",
                           "This is a Video Title 9",
                           new List<string> { PlaylistC },
                           GenerateSourceList(),
                           new List<Tag> { ToneMelancholic, Topic1, Medium2 }));
            Videos.Add(GetVideo(
                           "10",
                           "This is a Video Title 10",
                           new List<string> { PlaylistD },
                           GenerateSourceList(),
                           new List<Tag> { ToneAngry, Medium2 }));
            Videos.Add(GetVideo(
                           "11",
                           "This is a Video Title 11",
                           new List<string> { PlaylistD },
                           GenerateSourceList(),
                           new List<Tag> { Topic2, ToneHappy, Medium1 }));
            Videos.Add(GetVideo(
                           "12",
                           "This is a Video Title 12",
                           new List<string> { PlaylistD },
                           GenerateSourceList(),
                           new List<Tag> { ToneSad, Topic1 }));
        }

        public static List<SourceObject> GenerateSourceObjects(int size) {
            int i = 0;
            var list = new List<SourceObject>();
            while (i < size) {
                list.Add(GenerateSourceObject(i++.ToString(), SourceObjectType.Video, Constants.YouTubeSource));
                list.Add(GenerateSourceObject(i++.ToString(), SourceObjectType.Video, Constants.FacebookSource));
                list.Add(GenerateSourceObject(i++.ToString(), SourceObjectType.Campaign, Constants.YouTubeSource));
                list.Add(GenerateSourceObject(i++.ToString(), SourceObjectType.Campaign, Constants.FacebookSource));
                list.Add(GenerateSourceObject(i++.ToString(), SourceObjectType.AdSet, Constants.FacebookSource));
                list.Add(GenerateSourceObject(i++.ToString(), SourceObjectType.AdSet, Constants.YouTubeSource));
            }
            return list;
        }

        public static SourceObject GenerateSourceObject(string id, SourceObjectType type, string source) {
            string name;
            switch (type) {
                case SourceObjectType.Campaign:
                    name = "Campaign Title " + id;
                    break;
                case SourceObjectType.AdSet:
                    name = "AdSet Title " + id;
                    break;
                case SourceObjectType.Video:
                    name = "Video Title " + id;
                    break;
                default:
                    throw new Exception("no source");
            }
            return new SourceObject {
                       SourceId = id,
                       Type = type,
                       SourceName = source,
                       Title = name,
                       Links = new List<SourceLink>() {
                           new SourceLink {
                               Type = SourceLinkType.Content,
                               Link = "https://fee.org",
                           }
                       },
            };
        }

        static AbstractMockDataBackend() {
            rnd = new Random(1234124124);
            SourceObjects = GenerateSourceObjects(90);
            GenerateVideos();
            Personas = GeneratePersonas();
        }

        public IEnumerable<(string Group, string Age, string Gender, double Value)> GetUnstructuredDemographicData(string metric, string metaTagType, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive) {
            var metricType = Constants.ContentMetrics.Where(x => x.Type == metric).First().TypeId;

            if (!IsDemographicMetricType(metricType)) {
                return Enumerable.Empty<(string Group, string Age, string Gender, double Value)>();
            }

            var metaTagValues = MetaTagsList()[metaTagType].Select(x => x.Value).ToList();

            var unstructuredData =
                from groupName in metaTagValues
                from gender in Constants.GenderGroups
                from age in Constants.AgeGroups
                select(
                    Group : groupName,
                    Age : age,
                    Gender : gender,
                    Value : rnd.Next(1, 300) * 1.0
                    );

            return unstructuredData;
        }
    }
}
