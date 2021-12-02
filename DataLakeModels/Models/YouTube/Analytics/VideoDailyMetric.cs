using System;

namespace DataLakeModels.Models.YouTube.Analytics {

    public class VideoDailyMetric : IValidityRange, IEquatable<VideoDailyMetric> {

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public DateTime Date { get; set; }
        public string VideoId { get; set; }
        public long Views { get; set; }
        public long Likes { get; set; }
        public long SubscriberViews { get; set; }
        public long Shares { get; set; }
        public long Comments { get; set; }
        public long AverageViewDuration { get; set; }
        public long Dislikes { get; set; }
        public long SubscribersGained { get; set; }
        public long SubscribersLost { get; set; }
        public long VideosAddedToPlaylists { get; set; }
        public long VideosRemovedFromPlaylists { get; set; }

        bool IEquatable<VideoDailyMetric>.Equals(VideoDailyMetric other) {
            return Views == other.Views &&
                   Likes == other.Likes &&
                   Shares == other.Shares &&
                   Comments == other.Comments &&
                   AverageViewDuration == other.AverageViewDuration &&
                   Dislikes == other.Dislikes &&
                   SubscriberViews == other.SubscriberViews &&
                   SubscribersGained == other.SubscribersGained &&
                   SubscribersLost == other.SubscribersLost &&
                   VideosAddedToPlaylists == other.VideosAddedToPlaylists &&
                   VideosRemovedFromPlaylists == other.VideosRemovedFromPlaylists;
        }
    }
}
