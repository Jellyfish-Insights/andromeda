using System;
using System.Collections.Generic;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceDeltaEncodedVideoMetric : IMutableEntity {
        public string VideoId { get; set; }
        public SourceVideo Video { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /**
        ** Impressions

           Impressions is the number of times a post from your page is displayed,
           whether the post is clicked or not. People may see multiple
           impressions of the same post. Do not mistake it with ~reach~. Reach is
           the the number of unique people who received impressions of a page
           post. Reach might be less than impressions since one person can see
           multiple impressions.

           - Facebook :: The number of impressions of the video (lifetime). Got from ~total_video_impressions~ on [[https://developers.facebook.com/docs/graph-api/reference/v2.12/video/video_insights][video_insights]].
           - YouTube :: N/A.
        */
        public long? ImpressionsCount { get; set; }

        DateTime IMutableEntity.UpdateDate { get => StartDate; }
        List<JToken> IMutableEntity.PrimaryKey { get => MutableEntityExtentions.AutoPK(VideoId); }
    }
}
