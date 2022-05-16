using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class AnimatedThumbnail : IEquatable<AnimatedThumbnail> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public ImageVersion ImageVersion { get; set; }
        public int ImageVersionId { get; set; }
        [Column(TypeName = "text[]")]
        public string[] SpriteUrls { get; set; }
        public long FileSizeKb { get; set; }
        public long SpriteWidth { get; set; }
        public double VideoLength { get; set; }
        public long SpriteHeight { get; set; }
        public long RenderedWidth { get; set; }
        public long ThumbnailWidth { get; set; }
        public long ThumbnailHeight { get; set; }
        public double ThumbnailDuration { get; set; }
        public long ThumbnailsPerRow { get; set; }
        public long MaxThumbnailsPerSprite  { get; set; }
        public long TotalThumbnailNumPerSprite { get; set; }

        bool IEquatable<AnimatedThumbnail>.Equals(AnimatedThumbnail other) {
            return Id == other.Id &&
                   SpriteUrls == other.SpriteUrls &&
                   FileSizeKb == other.FileSizeKb &&
                   SpriteWidth == other.SpriteWidth &&
                   VideoLength == other.VideoLength &&
                   SpriteHeight == other.SpriteHeight &&
                   RenderedWidth == other.RenderedWidth &&
                   ThumbnailWidth == other.ThumbnailWidth &&
                   ThumbnailHeight == other.ThumbnailHeight &&
                   ThumbnailDuration == other.ThumbnailDuration &&
                   ThumbnailsPerRow == other.ThumbnailsPerRow &&
                   MaxThumbnailsPerSprite == other.MaxThumbnailsPerSprite &&
                   TotalThumbnailNumPerSprite == other.TotalThumbnailNumPerSprite;
        }
    }
}
