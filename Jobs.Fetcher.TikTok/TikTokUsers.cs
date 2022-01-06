using Newtonsoft.Json;

namespace Jobs.Fetcher.TikTok {
<<<<<<< HEAD
    public class TikTokUsers {
=======
    public class TikTokUsers{
>>>>>>> e4dd7bc49c63c39cdc7e4a2b044703e450b59e12

        [JsonProperty("Name")] public string Name { get; set; }

        public bool IsValid() {
            return !string.IsNullOrEmpty(Name);
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> e4dd7bc49c63c39cdc7e4a2b044703e450b59e12
