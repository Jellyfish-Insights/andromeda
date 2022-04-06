using System;
using System.Runtime;
using System.Collections.Generic;

namespace DataLakeModels.Models.Twitter.Data {

    public class User : IEquatable<User> {

        /// <summary>
        /// The unique identifier of this user.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The UTC datetime that the user account was created on Twitter.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The location specified in the user's profile, if the user provided one. As this is a freeform value,
        /// it may not indicate a valid location, but it may be fuzzily evaluated when performing searches with location queries.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The name of the user, as they’ve defined it on their profile. Not necessarily a person’s name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The URL to the profile image for this user, as shown on the user's profile.
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// Indicates if this user has chosen to protect their Tweets (in other words, if this user's Tweets are private).
        /// </summary>
        public bool IsProtected { get; set; }

        /// <summary>
        /// The URL specified in the user's profile, if present.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The Twitter screen name, handle, or alias that this user identifies themselves with.
        /// Usernames are unique but subject to change.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Indicates if this user is a verified Twitter User.
        /// </summary>
        public bool Verified { get; set; }

        public ICollection<Tweet> Tweets { get; set; }

        /// <summary>
        bool IEquatable<User>.Equals(User other) {
            return CreatedAt == other.CreatedAt &&
                   Location == other.Location &&
                   Name == other.Name &&
                   ProfileImageUrl == other.ProfileImageUrl &&
                   IsProtected == other.IsProtected &&
                   Url == other.Url &&
                   Username == other.Username &&
                   Verified == other.Verified;
        }

        public override string ToString() {
            return $"Id={Id}, CreatedAt={CreatedAt}, Location={Location}, Name={Name}, "
                + $"ProfileImageUrl={ProfileImageUrl}, IsProtected={IsProtected}, "
                + $"Url={Url}, Username={Username}, Verified={Verified}";
        }
    }
}
