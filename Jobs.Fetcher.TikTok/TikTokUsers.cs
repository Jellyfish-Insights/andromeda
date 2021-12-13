using Newtonsoft.Json;

namespace Jobs.Fetcher.Tiktok {
    public class TikTokUsers{

        [JsonProperty("UserId")] public string UserId { get; set; }
        [JsonProperty("Name")] public string Name { get; set; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(UserId) &&
                   !string.IsNullOrEmpty(Name);
        }
    }
}