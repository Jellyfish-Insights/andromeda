using System;
using TypeScriptBuilder;
using System.Collections.Generic;

namespace ApplicationModels.Models.DataViewModels {

    public enum SourceLinkType {
        Content = 0,
        Analytics = 1
    }

    public class SourceLink {
        public SourceLinkType Type { get; set; }
        public string Link { get; set; }
    }

    public class Source {
        public string SourceName { get; set; }
        public int VideosCount { get; set; }
        public List<SourceObject> SourceObjects { get; set; }
    }

    public enum SourceObjectType {
        Video = 0,
        Campaign = 1,
        AdSet = 2
    }

    public class SourceObject {
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string Title { get; set; }
        public string ThumbnailLink { get; set; } = Constants.DefaultThumbnail;
        public SourceObjectType Type { get; set; }
        public List<SourceLink> Links { get; set; }
        // Nullable because if the source object is not a video, does not have length
        [TSOptional]
        public int? LengthInSeconds { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool PublishedStatus { get; set; }
    }

    public class Video {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool Archived { get; set; } = false;
        public DateTime UpdateDate { get; set; }
        public List<string> Playlists { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Source> Sources { get; set; }
        public string ThumbnailLink { get; set; } = Constants.DefaultThumbnail;
        public DateTime PublishedAt { get; set; }
    }
}
