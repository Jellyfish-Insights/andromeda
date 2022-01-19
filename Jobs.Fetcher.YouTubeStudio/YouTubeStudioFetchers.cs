using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DL_YTS = DataLakeModels.Models.YouTube.Studio;
using Jobs.Fetcher.YouTubeStudio.Helpers;

namespace Jobs.Fetcher.YouTubeStudio
{
    sealed class YouTubeStudioFetchers
    {
        // static
        // (Dictionary<string,int> , Dictionary<string,int> posOptionalFields) FieldPositionsFromCSV(CSV csv)
        // {
        //     var posMandatoryFields = new Dictionary<string,int>
        //     {
        //         {"Date Measure", -1},
        //         {"Video ID", -1},
        //         {"Channel ID", -1}
        //     };
        //     foreach (var key in posMandatoryFields.Keys.ToList()) {
        //         posMandatoryFields[key] = csv.Header.IndexOf(key);
        //     }
        //     foreach (var KVPair in posMandatoryFields) {
        //         if (KVPair.Value == -1) {
        //             throw new InvalidOperationException("CSV provided does not " +
        //                 $"specify a '{KVPair.Key}' column.");
        //         }
        //     }

        //     var posOptionalFields = new Dictionary<string,int>();
        //     var mandatoryFieldsKeys = new List<string>(posMandatoryFields.Keys);
        //     for (int i = 0; i < csv.Header.Count; i++) {
        //         if (!mandatoryFieldsKeys.Contains(csv.Header[i])) {
        //             posOptionalFields.Add(csv.Header[i], i);
        //         }
        //     }
        //     return (posMandatoryFields, posOptionalFields);
        // }

        // static DateTime EpochToDateTime(int epoch)
        // {
        //     DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //     return origin.AddSeconds(epoch);
        // }
        // static List<DL_YTS.Video> VideosFromCSV(CSV csv)
        // {
        //     var(posMandatoryFields, posOptionalFields) = FieldPositionsFromCSV(csv);
        //     int idxChannelId = posMandatoryFields["Channel ID"];
        //     int idxVideoId = posMandatoryFields["Video ID"];
        //     int idxDateMeasure = posMandatoryFields["Date Measure"];

        //     var validityStart = DateTime.Now;
        //     var validityEnd = DateTime.MaxValue;

        //     var videos = new List<DL_YTS.Video>();
        //     foreach (var row in csv.Rows) {
        //         var channelId = row[idxChannelId];
        //         var videoId = row[idxVideoId];
        //         var dateMeasure = EpochToDateTime(Int32.Parse(row[idxDateMeasure]));
        //         foreach (var metricKeyValue in posOptionalFields) {
        //             var metricName = metricKeyValue.Key;
        //             var metricValue = row[metricKeyValue.Value];
        //             var video = new DL_YTS.Video(
        //                 validityStart: validityStart,
        //                 validityEnd: validityEnd,
        //                 dateMeasure: dateMeasure,
        //                 channelId: channelId,
        //                 videoId: videoId,
        //                 metric: metricName,
        //                 value: double.Parse(metricValue)
        //             );
        //             videos.Add(video);
        //         }
        //     }

        //     return videos;
        // }

        private static string PathToData = @"../../../Data/";
        public static void Main(string[] args)
        {
            var files = ImportFromFileSystem.FindFiles(PathToData);
            if (files == null || files.Count() == 0) {
                Console.WriteLine("No files to work with. Terminating.");
                return;
            }
            Console.WriteLine(files);
            // foreach (var file in files){
            //     Console.WriteLine($"Now parsing file '{file}'");
            //     CSV csv = new CSV(file);
            //     var videos = VideosFromCSV(csv);
            //     Console.WriteLine($"File contained data for {videos.Count} Video DTOs");
            //     videos.ForEach(Console.WriteLine);
            // }
        }
    }
}
