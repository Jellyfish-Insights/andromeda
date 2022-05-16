using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class ImageVersion : IEquatable<ImageVersion> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public ICollection<Image> Candidates { get; set; }
        public AnimatedThumbnail AnimatedThumbnailSpritesheetInfo { get; set; }

        bool IEquatable<ImageVersion>.Equals(ImageVersion other) {
            return Id == other.Id &&
                   Candidates == other.Candidates &&
                   ReelId == other.ReelId &&
                   AnimatedThumbnailSpritesheetInfo == other.AnimatedThumbnailSpritesheetInfo;
        }
    }

    public class Image : IEquatable<Image> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ImageVersion ImageVersion { get; set; }
        public int ImageVersionId { get; set; }
        public string Url { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }

        bool IEquatable<Image>.Equals(Image other) {
            return Id == other.Id &&
                   ImageVersionId == other.ImageVersionId &&
                   Url == other.Url &&
                   Width == other.Width &&
                   Height == other.Height;
        }
    }
}
