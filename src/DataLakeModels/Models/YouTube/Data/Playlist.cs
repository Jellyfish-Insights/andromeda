using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLakeModels.Models.YouTube.Data {

    public class Playlist : IValidityRange, IEquatable<Playlist> {

        public string PlaylistId { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Description { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        [Column(TypeName = "text[]")]
        public string[] VideoIds { get; set; }

        bool IEquatable<Playlist>.Equals(Playlist other) {
            return Title == other.Title &&
                   ThumbnailUrl == other.ThumbnailUrl &&
                   ((VideoIds == null && other.VideoIds == null) || other.VideoIds.SequenceEqual(VideoIds));
        }
    }
}
