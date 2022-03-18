using Newtonsoft.Json;
using Tweetinvi.Models;

namespace Jobs.Fetcher.Twitter {
    public class TwitterCredentials : Tweetinvi.Models.TwitterCredentials {

        [JsonProperty("Username")] public string Username { get; set; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(ConsumerKey) &&
                   !string.IsNullOrEmpty(ConsumerSecret) &&
                   !string.IsNullOrEmpty(AccessToken) &&
                   !string.IsNullOrEmpty(AccessTokenSecret) &&
                   !string.IsNullOrEmpty(Username);
        }
    }
}
