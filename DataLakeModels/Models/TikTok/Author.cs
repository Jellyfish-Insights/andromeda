using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.TikTok {

    public class Author : IEquatable<Author> {

        public string Id { get; set; }
        public string UniqueId { get; set; }
        public string Nickname { get; set; }
        public string AvatarThumbnail { get; set; }
        public string AvatarMedium { get; set; }
        public string AvatarLarger { get; set; }
        public string Signature { get; set; }
        public bool Verified { get; set; }
        public string SecurityUID { get; set; }
        public bool Secret { get; set; }
        public bool FTC  { get; set; }
        public int Relation { get; set; }
        public int OpenFavorite { get; set; }
        public int CommentSetting { get; set; }
        public int DuetSetting { get; set; }
        public int StitchSetting { get; set; }
        public bool privateAccount { get; set; }
        
        bool IEquatable<Author>.Equals(Author other) {
            return Id == other.Id &&
                   UniqueId == other.UniqueId &&
                   Nickname == other.Nickname &&
                   Signature == other.Signature &&
                   Verified == other.Verified &&
                   SecurityUID == other.SecurityUID &&
                   Secret == other.Secret &&
                   FTC == other.FTC &&
                   Relation == other.Relation &&
                   OpenFavorite == other.OpenFavorite &&
                   CommentSetting == other.CommentSetting &&
                   DuetSetting == other.DuetSetting &&
                   StitchSetting == other.StitchSetting &&
                   privateAccount == other.privateAccount;
        }
    }
}
