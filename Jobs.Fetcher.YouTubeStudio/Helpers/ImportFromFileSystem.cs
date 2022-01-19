using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using DataLakeModels.Models.YouTube.Studio;


namespace Jobs.Fetcher.YouTubeStudio.Helpers {
    public class Video_DTO
    {
            public long ValidityStart {get; set;}
            public long DateMeasure {get; set;}
            public string ChannelId {get; set;}
            public string VideoId {get; set;}
            public string Metric {get; set;}
            public double Value {get; set;}

            public override string ToString()
            {
                return $@"
                    ValidityStart: {ValidityStart},
                    DateMeasure: {DateMeasure},
                    ChannelId: {ChannelId},
                    VideoId: {VideoId},
                    Metric: {Metric},
                    Value: {Value}
                ";
            }
    }



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
            catch (System.IO.DirectoryNotFoundException) {
                Console.WriteLine($"The directory {path} does not exist.");
                return null;
            }
            var jsonFiles = allFiles
                        .Where(x => jsonFileRegex.Matches(x).Count != 0)
                        .ToList();
            return jsonFiles;
        }

        public static List<Video_DTO> VideoDTOsFromFile (string pathToFile)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            string fileContents;

            try {
                fileContents = File.ReadAllText(pathToFile);
            } catch (System.IO.FileNotFoundException) {
                Console.WriteLine($"File not found: '{pathToFile}'");
                return null;
            } catch (System.UnauthorizedAccessException) {
                Console.WriteLine($"You are not authorized to read this file: '{pathToFile}'");
                return null;
            }

            List<Video_DTO> videos;
            try {
                videos = JsonConvert.DeserializeObject<List<Video_DTO>>(fileContents, settings);
            } catch (Newtonsoft.Json.JsonReaderException exc) {
                Console.WriteLine($"File '{pathToFile}' contains invalid JSON");
                Console.WriteLine(exc);
                return null;
            }
            catch (Newtonsoft.Json.JsonSerializationException exc) {
                Console.WriteLine($"File '{pathToFile}' contains field unknown to destination object");
                Console.WriteLine(exc);
                return null;
            }
            return videos;
        }

        static DateTime EpochToDateTime(long epochMilliseconds)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(epochMilliseconds / 1000);
        }

        public static Video DTOToVideo (Video_DTO dto)
        {
            var validityStart = EpochToDateTime(dto.ValidityStart);
            var dateMeasure = EpochToDateTime(dto.DateMeasure);
            var validityEnd = DateTime.MaxValue;
            return new Video {
                ValidityStart = validityStart,
                ValidityEnd = validityEnd,
                DateMeasure = dateMeasure,
                ChannelId = dto.ChannelId,
                VideoId = dto.VideoId,
                Metric = dto.Metric,
                Value = dto.Value
            };
        }

        public static List<Video> FileToVideos (string pathToFile)
        {
            var videoDTOs = VideoDTOsFromFile(pathToFile);
            var videos = new List<Video>();
            foreach (var videoDTO in videoDTOs) {
                if (videoDTO == null) {
                    Console.WriteLine("Cannot parse DTO: is null.");
                    continue;
                }
                videos.Add(DTOToVideo(videoDTO));
            }

            return videos;
        }

        public static List<Video> GetVideosFromPath (string path)
        {
            var files = ImportFromFileSystem.FindFiles(path);
            if (files == null || files.Count() == 0) {
                Console.WriteLine("No files to work with. Terminating.");
                return null;
            }

            Console.WriteLine($"We found {files.Count()} files:");
            files.ForEach(Console.WriteLine);

            var videoList = new List<Video>();
            foreach (var file in files) {
                var videos = ImportFromFileSystem.FileToVideos(file);
                if (videos == null || videos.Count() == 0) {
                    Console.WriteLine($"Failed to decode file '{file}'. Skipping...");
                    continue;
                }
                videoList.AddRange(videos);
            }

            return videoList;
        }
    }
}
