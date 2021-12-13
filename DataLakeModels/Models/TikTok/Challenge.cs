using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Challenge : IEquatable<Challenge> {

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ProfileThumbnail { get; set; }
        public string ProfileMedium { get; set; }
        public string ProfileLarge { get; set; }
        public string CoverThumbnail { get; set; }
        public string CoverMedium { get; set; }
        public string CoverLarge { get; set; }
        public bool IsCommerce { get; set; }
        
        bool IEquatable<Challenge>.Equals(Challenge other) {
            return Id == other.Id &&
                   Title == other.Title &&
                   Description == other.Description &&
                   IsCommerce == other.IsCommerce;
        }
    }
}
