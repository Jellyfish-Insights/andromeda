using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Music : IEquatable<Music> {

        public string Id { get; set; }
        public string Title { get; set; }
        public string PlayUrl { get; set; }
        public string CoverThumb { get; set; }
        public string CoverMedium { get; set; }
        public string CoverLarge { get; set; }
        public string AuthorName { get; set; }
        public bool Original { get; set; }
        public int Duration { get; set; }
        public string Album { get; set; }
        public ICollection<Post> Posts { get; set; }
        bool IEquatable<Music>.Equals(Music other) {
            return Id == other.Id &&
                   Title == other.Title &&
                   AuthorName == other.AuthorName &&
                   Original == other.Original &&
                   Duration == other.Duration &&
                   Album == other.Album;
        }
    }
}
