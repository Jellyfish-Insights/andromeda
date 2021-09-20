using System;
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
            var validSchemas = CheckCredentialStatus(allSchemas);

            // First check for the old folder structure
            if (validSchemas.Count > 0) {
                Console.WriteLine($"Detected old folder structure. Loading only the old structure schemas. Please, consider changing to the new folder structure");
                return validSchemas;
            }

            var setOfSchemas = new HashSet<string>();
            var userDirs = new List<string>(Directory.GetDirectories("./credentials"));
            foreach(var userDir in userDirs) {
                validSchemas = CheckCredentialStatus(allSchemas, userDir);
                foreach(var schema in validSchemas) {
                    setOfSchemas.Add(schema);
                }
            }
            return new List<string>(setOfSchemas);
        }

        public static List<string> SchemaList(string usrFolderName) {
            var allSchemas = new List<string> { "page", "adaccount", "instagram" };
            return CheckCredentialStatus(allSchemas, usrFolderName);
        }

        public static List<string> SchemaList(List<string> allSchemas) {
            return CheckCredentialStatus(allSchemas);
        }

        public static List<string> SchemaList(List<string> allSchemas, string usrFolderName) {
            return CheckCredentialStatus(allSchemas, usrFolderName);
        }

        private static List<string> CheckCredentialStatus(List<string> allSchemas) {
            var validSchemas = new List<string>();
            foreach (var schemaName in allSchemas) {
                var credentialPath = GetCredentialPath(schemaName);
                if (Directory.Exists(credentialPath) && Directory.GetFiles(credentialPath).Length > 0) {
                    validSchemas.Add(schemaName);
                } else {
                    Console.WriteLine($"Missing or invalid {GetServiceName(schemaName)} credentials!");
                    Console.WriteLine($"Couldn't find any credential on folder '{credentialPath}'");
                }
            }
            return validSchemas;
        }

        private static List<string> CheckCredentialStatus(List<string> allSchemas, string usrFolderName) {
            var validSchemas = new List<string>();
            foreach (var schemaName in allSchemas) {
                var credentialPath = GetCredentialPath(schemaName, usrFolderName);
                if (Directory.Exists(credentialPath) && Directory.GetFiles(credentialPath).Length > 0) {
                    validSchemas.Add(schemaName);
                } else {
                    Console.WriteLine($"Missing or invalid {GetServiceName(schemaName)} credentials!");
                    Console.WriteLine($"Couldn't find any credential on folder '{credentialPath}'");
                    Console.WriteLine($"File: {credentialFileName}\n");
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
            switch (schemaName) {
                case "instagram":
                    return "instagram";
                case "page":
                    return $"facebook/{schemaName}";
                case "adaccount":
                    return $"facebook/{schemaName}";
                default:
                    return "";
            }
        }

        public static string GetCredentialPath(string schemaName, string usrFolder) {
            switch (schemaName) {
                case "instagram":
                    return $"{usrFolder}/instagram";
                case "page":
                    return $"{usrFolder}/facebook/{schemaName}";
                case "adaccount":
                    return $"{usrFolder}/facebook/{schemaName}";
                default:
                    return usrFolder;
            }
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
