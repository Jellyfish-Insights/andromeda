using Newtonsoft.Json.Linq;

namespace Jobs.Fetcher.Facebook {
    public static class JObjectExtensions {
        // Index a path in the JObject return a default value if it fails
        public static T IndexPathOrDefault<T>(this JObject obj, string[] path, T def) {
            return ((JToken) obj).IndexPathOrDefault(path, def);
        }

        public static T IndexPathOrDefault<T>(this JToken obj, string[] path, T def) {
            if (path == null)
                return def;
            var result = obj;
            foreach (string p in path) {
                if (result[p] != null) {
                    result = result[p];
                } else {
                    return def;
                }
            }
            return result.ToObject<T>();
        }

        public static T IndexPathOrDefault<T>(this JToken obj, string path, T def) {
            return obj.IndexPathOrDefault(path.Split("."), def);
        }
    }
}
