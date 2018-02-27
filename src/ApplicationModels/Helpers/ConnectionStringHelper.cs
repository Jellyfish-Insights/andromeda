using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Helpers {

    public static class ConnectionStringHelper {

        static string appsettings_path = "appsettings.json";

        public static string GetConnStr(string database) {
            using (StreamReader r = new StreamReader(appsettings_path)) {
                var json = JObject.Parse(r.ReadToEnd());
                return (string) json["ConnectionStrings"][database];
            }
        }
    }
}
