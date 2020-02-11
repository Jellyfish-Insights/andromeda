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
            var all_schemas = new List<string> { "page", "adaccount", "instagram" };
            var valid_schemas = new List<string>();
            foreach(var schema_name in all_schemas){
                if(File.Exists($"credentials/facebook/" + schema_name + "_credentials.json")) {
                    valid_schemas.Add(schema_name);
                }
            }
            return valid_schemas;
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
