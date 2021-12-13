using Newtonsoft.Json;

namespace Jobs.Fetcher.TikTok {
    public class TikTokCredentials// : Tweetinvi.Models.TikTokCredentials 
    {

        [JsonProperty("Username")] public string Username { get; set; }

        /*public bool IsValid() {
            return !string.IsNullOrEmpty(ConsumerKey) &&
                   !string.IsNullOrEmpty(ConsumerSecret) &&
                   !string.IsNullOrEmpty(AccessToken) &&
                   !string.IsNullOrEmpty(AccessTokenSecret) &&
                   !string.IsNullOrEmpty(Username);
        }*/
    }
}
