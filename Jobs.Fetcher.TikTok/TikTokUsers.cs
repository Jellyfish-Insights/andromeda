using Newtonsoft.Json;

namespace Jobs.Fetcher.TikTok {
    public class TikTokUsers{

        [JsonProperty("Name")] public string Name { get; set; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
