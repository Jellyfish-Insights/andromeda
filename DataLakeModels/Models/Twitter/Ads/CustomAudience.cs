using System;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Ads {

    public class CustomAudience : FlycatcherAds.Models.CustomAudience, IEquatable<CustomAudience> {

        bool IEquatable<CustomAudience>.Equals(CustomAudience other) {
            return Id == other.Id &&
                   Targetable == other.Targetable &&
                   Name == other.Name &&
                   TargetableTypes == other.TargetableTypes &&
                   AudienceType == other.AudienceType &&
                   Description == other.Description &&
                   PermissionLevel == other.PermissionLevel &&
                   OwnerAccountId == other.OwnerAccountId &&
                   ReasonsNotTargetable == other.ReasonsNotTargetable &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt &&
                   PartnerSource == other.PartnerSource &&
                   Deleted == other.Deleted &&
                   AudienceSize == other.AudienceSize;
        }
    }
}
