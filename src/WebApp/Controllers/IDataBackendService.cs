using System;
using System.Collections.Generic;
using ApplicationModels.Models.DataViewModels;

namespace WebApp.Controllers {

    public interface IDataBackend {
        IDictionary<string, List<Tag>> MetaTagsList();
        List<string> PlaylistList();
        List<string> PersonaList();
        List<PersonaVersion> PersonaVersionList(ArchiveMode archived);
        List<Tag> TagList();
        List<string> SourceList();
        List<SourceObject> UnAssociatedSources(SourceObjectType type);
        List<Video> VideoList(Tag[] filters, ArchiveMode archived);
        // Returns failed video edits
        VideoEdits EditVideos(VideoEdits videoEdits);
        TagEdits EditTags(TagEdits videoEdits);
        PersonaVersionEdits EditPersonas(PersonaVersionEdits videoEdits);
        List<VideoMetric> MetricList(DateTime startDate, DateTime endDate, Tag[] filters, List<MetricInfo> metrics, ArchiveMode archived);
        Dictionary<MetricInfo, TimeSeries> ComputeTimeSeries(MetricInfo[] metricInfo, string type, DateTime start, DateTime end, Tag[] filters, ArchiveMode archived);
    }

    public interface IContentDataBackend : IDataBackend {

        IEnumerable<(string Group, string Age, string Gender, double Value)> GetUnstructuredDemographicData(string metric, string metaTagType, DateTime startDate, DateTime endDate, Tag[] filters, ArchiveMode archive);
        Dictionary<int, Dictionary<string, Dictionary<string, double>>> VideoMetricByDay(IEnumerable<int> apVideoIds, DateTime start, DateTime end);
    }

    public interface IMarketingDataBackend : IDataBackend {}
}
