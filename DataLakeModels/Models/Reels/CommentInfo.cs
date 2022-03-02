using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class CommentInfo : IEquatable<CommentInfo> {
        public string Pk { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public string Text { get; set; }
        public long Type { get; set; }
        public string Status { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string MediaId { get; set; }
        public long BitFlags { get; set; }
        public long CreatedAt { get; set; }
        public bool IsCovered { get; set; }
        public string ContentType { get; set; }
        public bool ShareEnabled { get; set; }
        public long CreatedAtUTC { get; set; }
        public bool DidReportAsSpam { get; set; }
        public long PrivateReplyStatus { get; set; }

        bool IEquatable<CommentInfo>.Equals(CommentInfo other) {
            return Pk == other.Pk &&
                    ReelId == other.ReelId &&
                    Text == other.Text &&
                    Type == other.Type &&
                    Status == other.Status &&
                    UserId == other.UserId &&
                    Username == other.Username &&
                    MediaId == other.MediaId &&
                    BitFlags == other.BitFlags &&
                    CreatedAt == other.CreatedAt &&
                    IsCovered == other.IsCovered &&
                    ContentType == other.ContentType &&
                    ShareEnabled == other.ShareEnabled &&
                    CreatedAtUTC == other.CreatedAtUTC &&
                    DidReportAsSpam == other.DidReportAsSpam &&
                    PrivateReplyStatus == other.PrivateReplyStatus;
        }
    }
}
