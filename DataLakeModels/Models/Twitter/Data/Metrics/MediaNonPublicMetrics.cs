using System;

namespace DataLakeModels.Models.Twitter.Data {

    public class MediaNonPublicMetrics : MediaMetrics, IEquatable<MediaNonPublicMetrics> {

        bool IEquatable<MediaNonPublicMetrics>.Equals(MediaNonPublicMetrics other) {

            return MediaId == other.MediaId &&
                   Playback0Count == other.Playback0Count &&
                   Playback25Count == other.Playback25Count &&
                   Playback50Count == other.Playback50Count &&
                   Playback75Count == other.Playback75Count &&
                   Playback100Count == other.Playback100Count;
        }
    }
}
