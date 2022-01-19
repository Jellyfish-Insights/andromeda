using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DL_YTS = DataLakeModels.Models.YouTube.Studio;


namespace Jobs.Fetcher.YouTubeStudio.Helpers {
    public class ImportFromFileSystem
    {
        static private readonly Regex jsonFileRegex = new Regex(@"\.json$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<string> FindFiles(string path)
        {
            List<string> allFiles;
            try {
                allFiles = new List<string>(Directory.GetFiles(path));
            }
            catch (System.IO.DirectoryNotFoundException exc) {
                Console.WriteLine($"The directory {path} does not exist.");
                return null;
            }
            var jsonFiles = allFiles
                        .Where(x => jsonFileRegex.Matches(x).Count != 0)
                        .ToList();
            return jsonFiles;
        }
    }
}
