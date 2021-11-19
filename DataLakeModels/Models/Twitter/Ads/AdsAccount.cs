using System;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Ads {

    public class AdsAccount : FlycatcherAds.Models.AdsAccount, IEquatable<AdsAccount> {

        public string UserId { get; set; }
        public string Username { get; set; }
        public ICollection<Campaign> Campaigns { get; set; }

        bool IEquatable<AdsAccount>.Equals(AdsAccount other) {
            return Id == other.Id &&
                   UserId == other.UserId &&
                   Username == other.Username &&
                   Name == other.Name &&
                   BusinessName == other.BusinessName &&
                   TimeZone == other.TimeZone &&
                   TimeZoneSwitchAt == other.TimeZoneSwitchAt &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   BusinessId == other.BusinessId &&
                   ApprovalStatus == other.ApprovalStatus &&
                   Deleted == other.Deleted;
        }
    }
}
