using System.Collections.Generic;
using System.IO;
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
            var allSchemas = new List<string> { "page", "adaccount" };
            return CheckCredentialStatus(allSchemas);
        }

        public static List<string> SchemaList(List<string> allSchemas) {
            return CheckCredentialStatus(allSchemas);
        }

        private static List<string> CheckCredentialStatus(List<string> allSchemas) {
            var validSchemas = new List<string>();
            foreach (var schemaName in allSchemas) {
                if (File.Exists(GetCredentialPath(schemaName))) {
                    validSchemas.Add(schemaName);
                } else {
                    System.Console.WriteLine($"Failed to get {GetServiceName(schemaName)} data!");
                    System.Console.WriteLine($"Couldn't find file '{GetCredentialPath(schemaName)}'.");
                }
            }
            return validSchemas;
        }

        private static string GetServiceName(string schemaName) {
            switch (schemaName) {
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

        private static string GetCredentialPath(string schemaName) {
            var prePath = schemaName == "instagram" ? "credentials/instagram/" : "credentials/facebook/";
            return prePath + schemaName + "_credentials.json";
        }

        public static T ParseCredentials<T>(string schemaName) {
            string contents = File.ReadAllText(GetCredentialPath(schemaName));
            return JsonConvert.DeserializeObject<T>(contents);
        }

        public static Credentials GetCredentials(string schemaName) {
            return ParseCredentials<Credentials>(schemaName);
        }
    }
}
