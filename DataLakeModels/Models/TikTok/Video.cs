using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Video : IEquatable<Video> {

        public string Id { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Duration { get; set; }
        public string Ratio { get; set; }
        public string Cover { get; set; }
        public string OriginCover { get; set; }
        public string DynamicCover { get; set; }
        public string PlayAddress { get; set; }
        public string DownloadAddress { get; set; }
        public List<string> ShareCover  { get; set; }
        public string ReflowCover { get; set; }
        public int BitRate { get; set; }
        public string EncodedType { get; set; }
        public string Format { get; set; }
        public string VideoQuality { get; set; }
        public string EncodedUserTag { get; set; }
        public string CodecType { get; set; }
        public string Definition { get; set; }
        public ICollection<Post> Posts { get; set; }

        bool IEquatable<Video>.Equals(Video other) {
            return Id == other.Id &&
                   Height == other.Height &&
                   Width == other.Width &&
                   Duration == other.Duration &&
                   Ratio == other.Ratio &&
                   Cover == other.Cover &&
                   PlayAddress == other.PlayAddress &&
                   DownloadAddress == other.DownloadAddress &&
                   BitRate == other.BitRate &&
                   EncodedType == other.EncodedType &&
                   Format == other.Format &&
                   VideoQuality == other.VideoQuality &&
                   EncodedUserTag == other.EncodedUserTag &&
                   CodecType == other.CodecType &&
                   Definition == other.Definition;
        }
    }
}
