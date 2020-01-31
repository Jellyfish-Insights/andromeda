using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Andromeda.Common {
    public class FileSystemHelpers {

        public static void DeleteDirectoryIfExists(string dir) {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }

        public static JObject LoadJson(string filename) {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
                using (var msg = new StreamReader(stream))
                    using (var reader = new JsonTextReader(msg))
                        return (JObject) JObject.ReadFrom(reader);
        }

        public static void DumpJson(string path, JObject result) {
            using (var stream = new FileStream(path, FileMode.Create))
                using (var file = new StreamWriter(stream))
                    using (var writer = new JsonTextWriter(file)) {
                        var serializer = new JsonSerializer();
                        serializer.Serialize(writer, result);
                    }
        }

        // reference: adapted from https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        public static void DirectoryCopy(string sourceDirName, string destDirName) {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                          "Source directory does not exist or could not be found: "
                          + sourceDirName);
            }

            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            var targetDirectory = Path.Join(destDirName, dir.Name);
            if (!Directory.Exists(targetDirectory)) {
                Directory.CreateDirectory(targetDirectory);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(targetDirectory, file.Name);
                file.CopyTo(temppath, true);
            }
        }
    }
}
