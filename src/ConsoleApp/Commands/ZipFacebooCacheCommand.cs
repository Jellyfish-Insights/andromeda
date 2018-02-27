using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Common;
using Common.Jobs;

namespace ConsoleApp.Commands {
    public class ZipFacebooCacheCommand {

        public static void RemoveTokenFromCredentials(string filename) {
            var credential = FileSystemHelpers.LoadJson(filename);
            credential["token"] = "";
            File.Delete(filename);
            FileSystemHelpers.DumpJson(filename, credential);
        }

        public static void ZipCache(string dirPath, string zipFileName) {
            string zipPath = $"{zipFileName}.zip";

            var tmp = Path.Join(Path.GetTempPath(), zipFileName);

            FileSystemHelpers.DeleteDirectoryIfExists(tmp);
            Directory.CreateDirectory(tmp);

            FileSystemHelpers.DirectoryCopy("credentials", tmp);
            FileSystemHelpers.DirectoryCopy("cache", tmp);
            FileSystemHelpers.DirectoryCopy("schema", tmp);
            File.Copy(JobConstants.JobConfigFile, Path.Join(tmp, JobConstants.JobConfigFile));

            var credentialFiles = Directory.EnumerateFiles(Path.Join(tmp, "credentials"));
            foreach (var credentialFile in credentialFiles) {
                if (credentialFile.EndsWith("credentials.json")) {
                    RemoveTokenFromCredentials(credentialFile);
                } else {
                    File.Delete(credentialFile);
                }
            }

            ZipFile.CreateFromDirectory(tmp, zipPath);
        }
    }
}
