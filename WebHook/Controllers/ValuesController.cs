using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebHook.Controllers {

    [Route("[controller]")]
    [ApiController]
    public class WebhooksController : Controller {
        private string verify_token_expected;
        private string database;
        private string schema;
        private string table_name;
        private string host;
        private string username;
        private string password;
        private string port;
        private string app_secret;
        public WebhooksController(IConfiguration configuration) {
            verify_token_expected = configuration["VERIFY_TOKEN"];
            app_secret = configuration["APP_SECRET"];
            database = configuration["DATABASE"];
            host = configuration["HOST"];
            username = configuration["DBUSERNAME"];
            password = configuration["PASSWORD"];
            port = configuration["POSTGRESPORT"];
            schema = configuration["SCHEMA"] + configuration["API_VERSION"];
            table_name = schema + "." + configuration["TABLE_NAME"];
        }

        private string GetDateString() {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private void Info(string s) {
            Console.WriteLine($"{GetDateString()} [INF] {s}");
        }

        private void Err(string s) {
            Console.WriteLine($"{GetDateString()} [ERR] {s}");
        }

        public bool CheckSignature(string signatureWithPrefix, string payload_test) {
            if (signatureWithPrefix.StartsWith("sha1=", StringComparison.OrdinalIgnoreCase)) {
                var signature = signatureWithPrefix.Substring("sha1=".Length);

                var payload = EncodeUnicodeCharacters(payload_test);

                var encoded_secret = EncodeUnicodeCharacters(app_secret);

                byte[] secret = Encoding.ASCII.GetBytes(app_secret).ToArray();

                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

                using (var hmSha1 = new HMACSHA1(secret))
                {
                    byte[] hash = hmSha1.ComputeHash(payloadBytes);
                    string hashString = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
                    Info($"We are expecting signature {hashString}");
                    return hashString.Equals(signature);
                }
            }
            Err("Signature is not prefixed with 'sha1=', rejecting");
            return false;
        }

        private static string EncodeUnicodeCharacters(string payload) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char character in payload)
            {
                if (character > 127) {
                    stringBuilder.Append("\\u" + ((int) character).ToString("X4"));
                } else {
                    stringBuilder.Append(character);
                }
            }
            return stringBuilder.ToString();
        }

        [HttpGet]
        public ActionResult<string> Details([FromQuery(Name = "hub.mode")] string mode,
                                            [FromQuery(Name = "hub.challenge")] string challenge,
                                            [FromQuery(Name = "hub.verify_token")] string verify_token) {
            if (mode == "subscribe" && verify_token == verify_token_expected) {
                return challenge;
            } else
                return "Not Ok";
        }

        // POST api/values
        [HttpPost]
        public ActionResult<string> Post([FromBody] JObject jsonPayload) {
            if (!Request.Headers.TryGetValue("X-Hub-Signature", out StringValues signature)) {
                Err("X-Hub-Signature header missing");
                Response.StatusCode = 400;
                return "X-Hub-Signature header missing";
            }

            Info($"Signature = {signature}");

            var bodyStream = new StreamReader(Request.Body);
            bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            var bodyText = bodyStream.ReadToEnd();

            Info($"Payload: {bodyText}");

            const bool check_signature = true;
            if (check_signature) {
                Info("Checking signature");
                if (!CheckSignature(signature, bodyText)) {
                    Err("Signature was not valid");
                    Response.StatusCode = 400;
                    return "Signature invalid";
                } else {
                    Info("Signature validated successfully");
                }
            } else {
                Info("Signature won't be checked");
            }

            try {
                var decoded = jsonPayload["object"].ToString();
                if (decoded == "instagram") {
                    var instagram_data = jsonPayload["entry"];
                    Info($"Found {instagram_data.Count()} Instagram updates");

                    foreach (var each_update in instagram_data) {
                        var time = each_update["time"];
                        var id = each_update["id"];
                        foreach (var change in each_update["changes"]) {
                            InsertInsights((JObject) change, time.ToString(), id.ToString());
                        }
                    }
                    return "Ok";
                } else {
                    Err($"Invalid object field: {decoded}");
                    Response.StatusCode = 400;
                    return "Invalid payload";
                }
            }
            catch (Exception exc) {
                Err($"Unknown error processing payload: {exc.ToString()}");
                Response.StatusCode = 500;
                return "Unknown error processing payload";
            }
        }

        public void InsertInsights(JObject insight_data, string time, string id) {
            System.DateTime fetch_time = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            fetch_time = fetch_time.AddSeconds(Double.Parse(time));

            var systime = String.Format("[\"{0}\",)", fetch_time);
            var exits = insight_data["value"]["exits"];
            var media_id = insight_data["value"]["media_id"];
            var impressions = insight_data["value"]["impressions"];
            var reach = insight_data["value"]["reach"];
            var replies = insight_data["value"]["replies"];
            var taps_forward = insight_data["value"]["taps_forward"];
            var taps_back = insight_data["value"]["taps_back"];

            Info($"InsertInsights: systime={systime}, exits={exits}, media_id={media_id}, "
                 + $"impressions={impressions}, reach={reach}, replies={replies}, taps_forward={taps_forward}, "
                 + $"taps_back={taps_back}");

            var connection_string = String.Format("Host={0};Username={1};Password={2};Database={3};Port={4}",
                                                  host, username, password, database, port);

            var connection = new NpgsqlConnection(connection_string);
            using (var cmd = connection.CreateCommand()) {

                connection.Open();
                var insertion_command = String.Format(@"INSERT INTO {0}
                    (fetch_time, media_id, systime, exits, impressions, reach, replies, taps_forward, taps_back)
                    VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')",
                                                      table_name, fetch_time, media_id, systime, exits, impressions, reach, replies, taps_forward, taps_back);

                cmd.CommandText = insertion_command;

                try {
                    cmd.ExecuteNonQuery();
                } catch (Npgsql.PostgresException e) {
                    Err($"Could not insert story insight: {e.ToString()}");
                }
            }
        }
    }
}
