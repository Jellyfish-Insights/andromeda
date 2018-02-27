using System.Globalization;
using System.Text.RegularExpressions;

namespace Jobs.Transformation.Application {

    public static class PersonaHelper {

        public static (string personaName, string personaVersion) ? ParsePersona(string s) {
            var prefix = "YEAR";
            if (s.StartsWith(prefix)) {
                var regex = new Regex(@"^YEAR:? (?<name>.+?)(?: (?<version>v\d+.*)|: (?<version>.+)| - (?<version>.+))?$");

                var match = regex.Match(s);
                if (match.Success) {
                    var version = match.Groups["version"].Success ? match.Groups["version"].Value : "v0";
                    var name = match.Groups["name"].Value;

                    return (CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name), version);
                }
            }
            return null;
        }
    }
}
