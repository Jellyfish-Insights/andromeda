using System;
using System.Collections.Generic;
using TypeScriptBuilder;

namespace ApplicationModels.Models.DataViewModels {

    using TagType = System.String;
    using TagValue = System.String;
    using SourceCampaignId = System.String;
    using SourceVideoId = System.String;
    // This is the original Applciation Video Id or an arbitrary unique string for new Videos
    using Id = System.String;

    public class VideoEdit {
        [TSOptional]
        public String Title { get; set; }
        [TSOptional]
        public bool? Archive { get; set; }
        public DateTime UpdateDate { get; set; }

        [TSOptional]
        public Dictionary<TagType, TagValue> MetaTags { get; set; }

        [TSOptional]
        public List<string> AddedGenericTags { get; set; }

        [TSOptional]
        public List<string> RemovedGenericTags { get; set; }

        [TSOptional]
        public List<SourceCampaignId> AddedCampaigns { get; set; }
        [TSOptional]
        public List<SourceCampaignId> RemovedCampaigns { get; set; }
        [TSOptional]

        public List<SourceVideoId> AddedVideos { get; set; }
        [TSOptional]
        public List<SourceVideoId> RemovedVideos { get; set; }
        public EditType Flag { get; set; }
    }

    public class VideoEdits {
        public Dictionary<Id, VideoEdit> Edits { get; set; }
    }
}
