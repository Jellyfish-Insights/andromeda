using System.Collections.Generic;
using System.IO;
using System;
using DataLakeModels;
using Newtonsoft.Json;

namespace Jobs.Fetcher.Facebook {

    public static class SchemaLoader {

        public static Schema LoadSchema(string name) {
            var version = FacebookDatabaseManager.ApiVersion;

            var stream = File.ReadAllText("schema/" + name + '_' + version + ".json");
            var schema = JsonConvert.DeserializeObject<Schema>(stream);
            schema.Version = version;
            schema.FinishSetup();
            return schema;
        }

        public static List<string> SchemaList() {
            // Instagram schema was removed because "Instagram.media" is failing and we need to collect new data to Unit Test.
            // Instagram.media problem is related to issue 182

            // return new List<string> { "page", "adaccount", "instagram" };
            return new List<string> { "page", "adaccount" };
        }

        public static T ParseCredentials<T>(string schema_name) {
            string contents = File.ReadAllText("credentials/facebook/" + schema_name + "_credentials.json");
            return JsonConvert.DeserializeObject<T>(contents);
        }

        public static Credentials GetCredentials(string schema_name) {
            return ParseCredentials<Credentials>(schema_name);
        }
    }
}
