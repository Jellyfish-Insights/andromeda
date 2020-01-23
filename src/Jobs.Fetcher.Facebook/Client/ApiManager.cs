using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataLakeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Common.Logging;
using Serilog;
using System.Text.RegularExpressions;
using Common;

namespace Jobs.Fetcher.Facebook {
    public class ApiManager {
        const string BASE_URL = "graph.facebook.com";
        // values taken from: https://developers.facebook.com/docs/graph-api/using-graph-api/error-handling/
        const int APPLICATION_LEVEL_THROTTLING = 4;
        const int APPLICATION_QUOTA_WINDOW = 60 * 60; // seconds
        const int ACCOUNT_LEVEL_THROTTLING = 17;
        const int ACCOUNT_QUOTE_WINDOW = 24 * 60 * 60; // seconds
        const int WAIT_AFTER_USER_LIMIT_REACHED = 410; // seconds
        public DateTime? DefaultNowDate { get; set; }
        public bool IgnoreAPI { get; set; }
        public bool IgnoreCache { get; set; }
        public bool IgnoreTTL { get; set; }
        public bool ShouldPaginate { get; set; }
        int RequestDelay;
        public string CacheDirectory { get; set; }
        string ApiVersion;

        public int FailureTimeout { get; set; }
        public int SuccessTimeout { get; set; }
        public Credentials Secret { get; set; }
        // age after which cache is discarded (in unit of hours)
        public int CacheTTL { get; set; }

        private static readonly HttpClient client = new HttpClient();

        private ILogger Logger { get => Log.ForContext<ApiManager>(); }

        public ApiManager(
            Credentials secret,
            bool ignoreCache,
            bool ignoreAPI,
            bool ignoreTTL,
            bool shouldPaginate,
            int requestDelay,
            string cacheDirectory,
            DateTime? defaultNowDate
            ) {
            FailureTimeout = 60000;
            SuccessTimeout = 1000;
            CacheTTL = 8;
            Secret = secret;
            ApiVersion = FacebookDatabaseManager.ApiVersion;
            IgnoreCache = ignoreCache;
            IgnoreAPI = ignoreAPI;
            IgnoreTTL = ignoreTTL;
            RequestDelay = requestDelay;
            CacheDirectory = cacheDirectory;
            ShouldPaginate = shouldPaginate;
            DefaultNowDate = defaultNowDate;

            if (!Directory.Exists(CacheDirectory)) {
                Logger.Information("Creating cache directory {CacheDir}", CacheDirectory);
                Directory.CreateDirectory(CacheDirectory);
            }
        }

        private static JObject DecodeEndpoint(Stream result) {
            using (var msg = new StreamReader(result)) {
                using (var reader = new JsonTextReader(msg)) {
                    return (JObject) JObject.ReadFrom(reader);
                }
            }
        }

        public DateTime GetUtcTime() {
            if (this.DefaultNowDate.HasValue) {
                return this.DefaultNowDate.Value;
            } else {
                return DateTime.UtcNow;
            }
        }

        private static void EncodeEndpoint(Stream stream, JObject result) {
            using (var file = new StreamWriter(stream)) {
                using (var writer = new JsonTextWriter(file)) {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, result);
                }
            }
        }

        private async Task<JObject> RequestUrl(HttpClient client, int retries, String path, String url) {
            if (IgnoreAPI) {
                throw new FacebookApiException("Cannot Reach Api");
            }
            var before = this.GetUtcTime();
            Logger.Verbose("Fetching url in path {Path}: {Url}", path, url);
            HttpResponseMessage response;
            try {
                response = await client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
            } catch (Exception e) {
                throw new FacebookApiException($"Error fetching url: {url}", e);
            }
            Logger.Verbose("Api returned");
            Logger.Verbose("Sleeping for {SLEEP_TIME}s", RequestDelay);
            System.Threading.Thread.Sleep(RequestDelay * 1000);
            var stream = await response.Content.ReadAsStreamAsync();
            var result = DecodeEndpoint(stream);
<<<<<<< HEAD
            // Store the time we fetched the fileed for reproducibility
=======
            // Store the time we fetched the file for reproducibility
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
            result["fetch_time"] = before;
            result["retries"] = retries;
            // Overwrite the file if it already exist
            using (var file = new FileStream(path, FileMode.Create)) {
                EncodeEndpoint(file, result);
            }
            return result;
        }

        private static Regex TokenExpression = new Regex("(?<=access_token=)[a-zA-Z0-9]*(?=&?)", RegexOptions.Compiled);

        private async Task<JObject> RequestOrCache(HttpClient client, int retries, String prefix, String url, bool ignoreCache) {
            string hash;
            using (var sha1 = new SHA1Managed()) {
                // the token is removed from the url before hashing
                // this is done so that production cache (captured with valid token) may be fed into testing framework (which makes request with invalid token)
                var toBeHashed = TokenExpression.Replace(url, string.Empty);
                var hash_byte = sha1.ComputeHash(Encoding.ASCII.GetBytes(toBeHashed));
                hash = BitConverter.ToString(hash_byte).Replace("-", string.Empty);
            }

            var path = CacheDirectory + "/" + prefix + "_" + hash + ".json";
            if (!File.Exists(path) || ignoreCache) {
                Console.WriteLine($"Could not find request: {path}");
                if (IgnoreAPI) {
                    throw new FacebookApiUnreachable();
                }
                if (!File.Exists(path)) {
                    return await RequestUrl(client, 0, path, url);
                } else {
                    return await RequestUrl(client, retries, path, url);
                }
            } else {
                using (FileStream stream = new FileStream(path, FileMode.Open)) {
                    return DecodeEndpoint(stream);
                }
            }
        }

        public async Task<JObject> CachedRequest(string prefix, string url) {
            JObject result = await RequestOrCache(client, 0, prefix, url, IgnoreCache);
            var retries = (int) (result["retries"] ?? 0);
            while (retries < 5) {
                var ttl = this.GetUtcTime().Subtract(result["fetch_time"].ToObject<DateTime>()).TotalHours;
                if (ttl > CacheTTL && !IgnoreTTL && result["error"] == null) {
                    // Ovewrite if the cache is too old;
                    result = await RequestOrCache(client, retries, prefix, url, true);
                    if (result["error"] == null) {
                        return result;
                    }
                } else {
                    if (result["error"] == null) {
                        return result;
                    } else {
                        var errorCode = (int) result["error"]["code"];
                        var errorMessage = (string) result["error"]["message"];
                        if (errorMessage != null) {
                            LoggerFactory.GetFacebookLogger().Warning(errorMessage);
                        }
                        var elapsed = this.GetUtcTime().Subtract(result["fetch_time"].ToObject<DateTime>()).TotalSeconds;
                        if (errorCode == ACCOUNT_LEVEL_THROTTLING) {
                            var remaining = Math.Max(WAIT_AFTER_USER_LIMIT_REACHED - Convert.ToInt32(elapsed), 0);
                            LoggerFactory.GetFacebookLogger().Warning("Rate limit reached. Retrying ({Retries} - {Remaining})", retries, remaining);
                            if (elapsed < WAIT_AFTER_USER_LIMIT_REACHED) {
                                System.Threading.Thread.Sleep(remaining * 1000);
                            }
                            retries++;
                            result = await RequestOrCache(client, retries, prefix, url, true);
                            if (result["error"] == null) {
                                return result;
                            }
                        } else if (errorCode == APPLICATION_LEVEL_THROTTLING) {
                            var waitTime = APPLICATION_QUOTA_WINDOW / 2;
                            var remaining = Math.Max(waitTime - Convert.ToInt32(elapsed), 0);
                            LoggerFactory.GetFacebookLogger().Warning("Rate limit reached. Retrying ({Retries} - {Remaining})", retries, remaining);
                            if (elapsed < waitTime) {
                                System.Threading.Thread.Sleep(remaining * 1000);
                            }
                            retries++;
                            result = await RequestOrCache(client, retries, prefix, url, true);
                            if (result["error"] == null) {
                                return result;
                            }
                        } else {
                            if (retries == 0) {
                                result = await RequestOrCache(client, retries + 1, prefix, url, true);
                            }
                            if (result["error"] == null) {
                                return result;
                            }
                            throw new FacebookApiException("Unexpected API error when calling url: " + url, (JObject) result["error"]);
                        }
                    }
                }
            }
            // Do additional retry after the limit
            result = await RequestOrCache(client, retries + 1, prefix, url, true);
            if (result["error"] == null) {
                return result;
            }
            throw new FacebookApiException("Retry limit reached", new JObject() { { "retries", retries } });
        }

        public string EndPoint(string node_id, string edge = null, Dictionary<string, string> url_params = null) {
            url_params = url_params ?? new Dictionary<string, string>();
            url_params.Add("access_token", Secret.Token);
            var url_params_string = new List<string>();
            foreach (var entry in url_params) {
                url_params_string.Add(entry.Key + '=' + entry.Value);
            }
            var param_string = String.Join('&', url_params_string);
            if (edge != null) {
                return string.Format("https://{0}/v{1}/{2}/{3}?{4}", BASE_URL, ApiVersion, node_id, edge, param_string);
            } else {
                if (url_params.Count > 0) {
                    return string.Format("https://{0}/v{1}/{2}?{3}", BASE_URL, ApiVersion, node_id, param_string);
                } else {
                    return string.Format("https://{0}/v{1}/{2}", BASE_URL, ApiVersion, node_id);
                }
            }
        }
    }
}
