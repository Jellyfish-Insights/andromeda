using Newtonsoft.Json;

namespace Jobs.Fetcher.Reels {
    public class ReelsUsers {

        [JsonProperty("Name")] public string Name { get; set; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
