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
                if(File.Exists(GetSchemaPath(schema_name))) {
                    valid_schemas.Add(schema_name);
                } else { 
                    System.Console.WriteLine("Failed to get " + GetServiceName(schema_name) + " data!");
                    System.Console.WriteLine("Couldn't find file \'" + GetSchemaPath(schema_name) + "\'");
                }
            }
            return valid_schemas;
        }

        private static string GetServiceName(string schema_name) {
            switch (schema_name) {
                case "page":
                    return "Facebook";
                case "adaccount":
                    return "Facebook Marketing";
                case "instagram":
                    return "Instagram";
                default:
                    return "Undefined";
            }
        }

        private static string GetSchemaPath(string schema_name) {
            var pre_path = schema_name == "instagram" ? "credentials/instagram/" : "credentials/facebook/";
            return pre_path + schema_name + "_credentials.json";
        }


        public static T ParseCredentials<T>(string schema_name) {
            string contents = File.ReadAllText(GetSchemaPath(schema_name));
            return JsonConvert.DeserializeObject<T>(contents);
        }

        public static Credentials GetCredentials(string schema_name) {
            return ParseCredentials<Credentials>(schema_name);
        }
    }
}
