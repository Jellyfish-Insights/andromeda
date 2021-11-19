using System;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Data {

    public class Media : IEquatable<Media> {

        /// <summary>
        /// Unique identifier of the expanded media content.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Available when type is video. Duration in milliseconds of the video.
        /// </summary>
        public int DurationMs { get; set; }

        /// <summary>
        /// Height of this content in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// URL to the static placeholder preview of this content.
        /// </summary>
        public string PreviewImageUrl { get; set; }

        /// <summary>
        /// Type of content (animated_gif, photo, video).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Url to access the media
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Width of this content in pixels.
        /// </summary>
        public int Width { get; set; }

        /************* METRICS ************/

        /// <summary>
        /// Non-public engagement metrics for the media content at the time of the request.
        /// </summary>
        public ICollection<MediaNonPublicMetrics> NonPublicMetrics { get; set; }

        /// <summary>
        /// Engagement metrics for the media content, tracked in an organic context, at the time of the request.
        /// </summary>
        public ICollection<MediaOrganicMetrics> OrganicMetrics { get; set; }

        /// <summary>
        /// Engagement metrics for the media content, tracked in a promoted context, at the time of the request.
        /// </summary>
        public ICollection<MediaPromotedMetrics> PromotedMetrics { get; set; }

        /// <summary>
        /// Public engagement metrics for the media content at the time of the request.
        /// </summary>
        public ICollection<MediaPublicMetrics> PublicMetrics { get; set; }

        /// <summary>
        /// A navigation property that maps the media to all of its associated tweets.
        /// </summary>
        public ICollection<TweetMedia> TweetMedia { get; set; }

        bool IEquatable<Media>.Equals(Media other) {
            return Id == other.Id &&
                   Height == other.Height &&
                   PreviewImageUrl == other.PreviewImageUrl &&
                   Type == other.Type &&
                   Url == other.Url &&
                   Width == other.Width;
        }
    }
}
