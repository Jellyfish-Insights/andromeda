using System;
using System.Collections.Generic;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceVideoDemographicMetric : IMutableEntity {
        public string VideoId { get; set; }
        public SourceVideo Video { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AgeGroup { get; set; }
        public string Gender { get; set; }

        /**
        ** Demographics Total View Time Percentage
           - Facebook :: N/A.
           - YouTube :: Percentage of viewers who were logged in when watching the video, by age-group, and gender; lifetime value at the queried date. See [[https://developers.google.com/youtube/analytics/channel_reports#demographic-reports][demographics-report]].
        */
        public double? ViewerPercentage { get; set; }

        /**
        ** Demographics Total View Time Miliseconds
           - Facebook :: Total time (in milliseconds) video has been viewed by Top Audiences (lifetime). Taken from ~total_video_view_time_by_age_bucket_and_gender~ on [[https://developers.facebook.com/docs/graph-api/reference/v2.12/video/video_insights][video_insights]].
           - YouTube :: N/A.
        */
        public double? TotalViewTime { get; set; }

        /**
        ** Demographics Total Approximate Views Distribution
           - Facebook :: N/A.
           - YouTube :: Number of viewers who were logged in when watching the video, by age-group, and gender; lifetime value at the queried date. See [[https://developers.google.com/youtube/analytics/channel_reports#demographic-reports][demographics-report]]..
        */
        public double? TotalViewCount { get; set; }

        public DateTime UpdateDate { get; set; }

        DateTime IMutableEntity.UpdateDate { get => UpdateDate; }
        List<JToken> IMutableEntity.PrimaryKey { get => MutableEntityExtentions.AutoPK(VideoId, StartDate, AgeGroup, Gender); }
    }
}
