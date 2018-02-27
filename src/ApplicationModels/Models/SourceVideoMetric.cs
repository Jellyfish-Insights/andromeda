using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceVideoMetric : IMutableEntity {
        public string VideoId { get; set; }
        public SourceVideo Video { get; set; }

        [Column("EventDate", TypeName = "date")]
        public DateTime EventDate { get; set; }

        /**
         * Content Metrics

         ** Comments
           - Facebook :: Count of comments got from end point [[https://developers.facebook.com/docs/graph-api/reference/video/comments/][video/comments]].
           - YouTube :: The number of times that users commented on a video. See [[https://developers.google.com/youtube/analytics/metrics#comments][comments]].
         */
        public long? CommentCount { get; set; }

        /**
        ** Dislikes
           - Facebook :: N/A.
           - YouTube :: The number of times that users indicated that they disliked a video by giving it a negative rating. See [[https://developers.google.com/youtube/analytics/metrics#dislikes][dislikes]].

             Note that the YouTube Analytics API sometimes returns a negative value.
        */
        public long? DislikeCount { get; set; }

        /**
        ** Likes
           - Facebook :: Number of "video reactions" where type is "LIKE". See [[https://developers.facebook.com/docs/graph-api/reference/video/reactions/][reactions]].
           - YouTube :: The number of times that users indicated that they liked a video by giving it a positive rating. See [[https://developers.google.com/youtube/analytics/metrics#likes][likes]].

             Note that the YouTube Analytics API sometimes returns a negative value.
        */
        public long? LikeCount { get; set; }

        /**
        ** Reactions
           - Facebook :: Number of "video reactions" of any type. See [[https://developers.facebook.com/docs/graph-api/reference/video/reactions/][reactions]].
           - YouTube :: The number of times that users indicated that they liked a video by giving it a positive rating. See [[https://developers.google.com/youtube/analytics/metrics#likes][likes]].

           Note that the YouTube Analytics API sometimes returns a negative value.
        */
        public long? ReactionCount { get; set; }

        /**
        ** Shares
           - Facebook :: Count of public sharedposts that shared this video. Posts are got from end point [[https://developers.facebook.com/docs/graph-api/reference/video/sharedposts/][video/sharedposts]].
           - YouTube :: The number of times that users shared a video through the Share button. [[https://developers.google.com/youtube/analytics/metrics#shares][shares]].
        */
        public long? ShareCount { get; set; }

        /**
        ** Views
           - Facebook :: The (daily) number of times your video was watched for an aggregate of at least 3 seconds, or for nearly its total length, whichever happened first. Crossposted video views are not included in this counter. Got from ~post_video_views~ [[https://developers.facebook.com/docs/graph-api/reference/v2.12/insights][post insights]].
           - YouTube :: The number of times that a video was viewed. See [[https://developers.google.com/youtube/analytics/metrics#views][views]].
        */
        public long? ViewCount { get; set; }

        /**
        ** View Time
           - Facebook :: The (daily) total number of milliseconds your video was watched, including replays and views less than 3 seconds. Crossposted video view time are not included in this counter . Got from ~post_video_view_time~ on [[https://developers.facebook.com/docs/graph-api/reference/v2.12/insights][post insights]].
           - YouTube :: The average length, in milliseconds, of video playbacks times the number of views. See [[https://developers.google.com/youtube/analytics/metrics#averageViewDuration][averageViewDuration]] and [[https://developers.google.com/youtube/analytics/metrics#views][views]]. This value is fetched as seconds and converted to milliseconds for compatibility with FB's data.
        */
        public long? ViewTime { get; set; }

        /**
        ** Average View Time
           This metric is computed by dividing the total View Time by the total View Count.
        */

        public DateTime UpdateDate { get; set; }

        List<JToken> IMutableEntity.PrimaryKey { get => MutableEntityExtentions.AutoPK(VideoId, EventDate); }
        DateTime IMutableEntity.UpdateDate => UpdateDate;
    }
}
