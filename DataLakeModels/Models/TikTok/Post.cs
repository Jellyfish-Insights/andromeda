using System;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Post : IEquatable<Post>, IValidityRange {

        /// <summary>
        /// Unique identifier of the post.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string VideoId { get; set; }
        public Video Video { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string AuthorId { get; set; }
        public Author Author { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string MusicId { get; set; }
        public Music Music { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public List<string> ChallengeIds { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public ICollection<PostStats> Stats { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string DuetInfo { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool OriginalItem { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool OfficialItem { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public List<string> TagIds { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool Secret { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool ForFriend { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool Digged { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int ItemCommentStatus { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool ShowNotPass { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool VL1 { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool ItemMute { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public List<string> EffectStickerIds { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool Private { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool DuetEnabled { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool StitchEnabled { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool ShareEnabled { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsAd { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int DuetDisplay { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int StitchDisplay { get; set; }
        
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        /************* METRICS ************/

        bool IEquatable<Post>.Equals(Post other) {
            return Id == other.Id &&
                   Description == other.Description &&
                   CreateTime == other.CreateTime &&
                   OriginalItem == other.OriginalItem &&
                   OfficialItem == other.OfficialItem &&
                   Secret == other.Secret &&
                   ForFriend == other.ForFriend &&
                   Digged == other.Digged &&
                   ItemCommentStatus == other.ItemCommentStatus &&
                   VL1 == other.VL1 &&
                   ItemMute == other.ItemMute &&
                   Private == other.Private &&
                   DuetEnabled == other.DuetEnabled &&
                   StitchEnabled == other.StitchEnabled &&
                   ShareEnabled == other.ShareEnabled &&
                   IsAd == other.IsAd &&
                   DuetDisplay == other.DuetDisplay &&
                   StitchDisplay == other.StitchDisplay;
        }
    }
}
