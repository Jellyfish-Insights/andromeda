using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Serilog.Core;
using Newtonsoft.Json;
using DataLakeModels.Models.YouTube.Studio;


namespace Jobs.Fetcher.YouTubeStudio.Helpers {
    public class Video_DTO : IEquatable<Video_DTO>
    {
            /* In seconds */
            public uint DateMeasure {get; set;}
            public string ChannelId {get; set;}
            public string VideoId {get; set;}
            public string Metric {get; set;}
            public double Value {get; set;}

            public override string ToString()
            {
                return $@"
                    DateMeasure: {DateMeasure},
                    ChannelId: {ChannelId},
                    VideoId: {VideoId},
                    Metric: {Metric},
                    Value: {Value}
                ";
            }

            public bool Equals(Video_DTO other) {
                return DateMeasure == other.DateMeasure
                    && ChannelId == other.ChannelId
                    && VideoId == other.VideoId
                    && Metric == other.Metric
                    && Value == other.Value;
            }
    }


    public static class ImportFromFileSystem
    {
        private static readonly Regex jsonFileRegex = new Regex(@"\.json$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<string> FindFiles(string path, Logger logger)
        {
            List<string> allFiles;
            try {
                allFiles = new List<string>(Directory.GetFiles(path));
            }
            catch (System.IO.DirectoryNotFoundException) {
                logger.Error($"The directory {path} does not exist.");
                return null;
            }
            var jsonFiles = allFiles
                        .Where(x => jsonFileRegex.Matches(x).Count != 0)
                        .ToList();
            return jsonFiles;
        }

        public static List<Video_DTO> VideoDTOsFromFile (string pathToFile, Logger logger)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            string fileContents;

            try {
                fileContents = File.ReadAllText(pathToFile);
            } catch (System.IO.FileNotFoundException) {
                logger.Error($"File not found: '{pathToFile}'");
                return null;
            } catch (System.UnauthorizedAccessException) {
                logger.Error($"You are not authorized to read this file: '{pathToFile}'");
                return null;
            }

            List<Video_DTO> videos;
            try {
                videos = JsonConvert.DeserializeObject<List<Video_DTO>>(fileContents, settings);
            } catch (Newtonsoft.Json.JsonReaderException exc) {
                logger.Error($"File '{pathToFile}' contains invalid JSON");
                logger.Error(exc.ToString());
                return null;
            }
            catch (Newtonsoft.Json.JsonSerializationException exc) {
                logger.Error($"File '{pathToFile}' contains field unknown to destination object");
                logger.Error(exc.ToString());
                return null;
            }
            return videos;
        }

        public static List<Video_DTO> GetDTOsFromPath (string path, Logger logger)
        {
            var files = ImportFromFileSystem.FindFiles(path, logger);
            if (files == null || files.Count() == 0) {
                logger.Debug("No files to work with. Terminating.");
                return null;
            }

            var printFiles = string.Join("\n\t", files);
            logger.Information($"We found {files.Count()} files:"
                + $"\n\t{printFiles}");

            var dtoList = new List<Video_DTO>();
            foreach (var file in files) {
                var dto = ImportFromFileSystem.VideoDTOsFromFile(file, logger);
                if (dto == null || dto.Count() == 0) {
                    logger.Warning($"Failed to decode file '{file}'. Skipping...");
                    continue;
                }
                dtoList.AddRange(dto);
            }

            return dtoList;
        }
    }
}
