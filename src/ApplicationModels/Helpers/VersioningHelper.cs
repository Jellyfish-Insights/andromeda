using System.IO;

namespace ApplicationModels.Helpers {

    public static class VersioningHelper {

        private static string FileName = "version.txt";

        private static string gitCommitHash = null;

        public static string GitCommitHash {
            get {
                if (gitCommitHash == null) {
                    gitCommitHash = File.ReadAllText(FileName);
                }
                return gitCommitHash;
            }
        }
    }
}
