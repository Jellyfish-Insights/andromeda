using System.Collections.Generic;
using System.IO;
using DataLakeModels;
using Newtonsoft.Json;

namespace Jobs.Fetcher.Facebook {

    public static class SchemaLoader {
        public static string credentialFileName;
        public static Schema LoadSchema(string name) {
            var version = FacebookDatabaseManager.ApiVersion;

            var stream = File.ReadAllText("schema/" + name + '_' + version + ".json");
            var schema = JsonConvert.DeserializeObject<Schema>(stream);
            schema.Version = version;
            schema.FinishSetup();
            return schema;
        }

        public static List<string> SchemaList() {
            var allSchemas = new List<string> { "page", "adaccount", "instagram" };
            return CheckCredentialStatus(allSchemas);
        }

        public static List<string> SchemaList(List<string> allSchemas) {
            return CheckCredentialStatus(allSchemas);
        }

        private static List<string> CheckCredentialStatus(List<string> allSchemas) {
            var validSchemas = new List<string>();
            foreach (var schemaName in allSchemas) {
                var credentialPath = GetCredentialPath(schemaName);
                if (Directory.Exists(credentialPath) && Directory.GetFiles(credentialPath).Length > 0) {
                    validSchemas.Add(schemaName);
                } else {
                    System.Console.WriteLine($"Missing or invalid {GetServiceName(schemaName)} credentials!");
                    System.Console.WriteLine($"Couldn't find any credential on folder '{credentialPath}'");
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

        public static string GetCredentialPath(string schemaName) {
            var prePath = schemaName == "instagram" ? "credentials" : "credentials/facebook";
            return $"{prePath}/{schemaName}";
        }

        public static T ParseCredentials<T>(string schemaName) {
            string contents = File.ReadAllText(credentialFileName);
            return JsonConvert.DeserializeObject<T>(contents);
        }

        public static Credentials GetCredentials(string schemaName) {
            return ParseCredentials<Credentials>(schemaName);
        }

        public static List<string> GetInstagramId() {
            var instagramIdList = new List<string>();
            foreach (var file in Directory.GetFiles(SchemaLoader.GetCredentialPath("instagram"))) {
                credentialFileName = file;
                instagramIdList.Add(GetCredentials("instagram").Id);
            }
            return instagramIdList;
        }
    }
}
