using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLakeModels.Models.Reels {
    public class Caption : IEquatable<Caption> {
        public string Pk { get; set; }
        public string Text { get; set; }
        public long Type { get; set; }
        public long UserId { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public string Status { get; set; }
        public string MediaId { get; set; }
        public long BitFlags { get; set; }
        public long CreatedAt { get; set; }
        public bool IsCovered { get; set; }
        public string ContentType  { get; set; }
        public bool ShareEnabled { get; set; }
        public long CreatedAtUTC { get; set; }
        public bool DidReportAsSpam { get; set; }
        public long PrivateReplyStatus { get; set; }

        bool IEquatable<Caption>.Equals(Caption other) {
            return Pk == other.Pk &&
                   Text == other.Text &&
                   Type == other.Type &&
                   UserId == other.UserId &&
                   ReelId == other.ReelId &&
                   Status == other.Status &&
                   MediaId == other.MediaId &&
                   BitFlags == other.BitFlags &&
                   IsCovered == other.IsCovered &&
                   ContentType == other.ContentType &&
                   ShareEnabled == other.ShareEnabled &&
                   CreatedAtUTC == other.CreatedAtUTC &&
                   DidReportAsSpam == other.DidReportAsSpam &&
                   PrivateReplyStatus == other.PrivateReplyStatus;
        }
    }
}
