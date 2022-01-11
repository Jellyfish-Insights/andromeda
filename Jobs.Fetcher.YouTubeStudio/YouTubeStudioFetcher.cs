using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DL_YTS = DataLakeModels.Models.YouTube.Studio;

namespace Jobs.Fetcher.YouTubeStudio
{
    public class CSV
    {
        private const int AverageFieldSize = 15; // just an estimation
        private const string Separator = ",";
        static private readonly Regex csvFileRegex = new Regex(@"\.csv$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private List<string> _Header;
        public List<string> Header { get {return _Header; } }
        private List<List<string>> _Rows;
        public List<List<string>> Rows { get {return _Rows; } }

        public int NCols {get {return _Header.Count; }}
        public int NRows {get {return _Rows.Count; }}


        public static List<string> StringListFromFile(string filename)
        {
            var list = new List<string>();
            if (csvFileRegex.Matches(filename).Count == 0) {
                Console.WriteLine($"File '{filename}' does not have CSV extension.");
                return list;
            }
            if (!File.Exists(filename)) {
                Console.WriteLine($"Path '{filename}' does not correspond to a file.");
                return list;
            }
            try
            {
                list.AddRange(System.IO.File.ReadLines(filename));
            }
            catch (IOException exc)
            {
                Console.WriteLine($"An error occurred: {exc}");
            }
            return list;
        }

        public static void HeaderAndRowsFromStringList(
                List<string> strList,
                out List<string> header,
                out List<List<string>> rows
                )
        {
            if (strList == null || strList.Count == 0)
                throw new InvalidOperationException("String list is uninitialized or empty.");

            List<string> _header = new List<string>(strList[0].Split(separator: Separator));
            if (_header.Count == 0 || (_header.Count == 1 && _header[0] == ""))
                throw new InvalidOperationException("Header is empty.");

            List<List<string>> _rows = new List<List<string>>();
            for (var i = 1; i < strList.Count; i++) {
                List<string> row = new List<string>(strList[i].Split(separator: Separator));
                if (row.Count != _header.Count)
                    throw new InvalidOperationException("Row size does not match header.");
                _rows.Add(row);
            }

            header = _header;
            rows = _rows;
        }

        public CSV(List<string> header, List<List<string>> rows)
        {
            _Header = header;
            _Rows = rows;
        }

        public CSV(string filename)
        {
            List<string> strList = StringListFromFile(filename);
            List<string> header;
            List<List<string>> rows;
            HeaderAndRowsFromStringList(strList, out header, out rows);
            _Header = header;
            _Rows = rows;
        }

        public string ToJSON()
        {
            int estimatedCapacity = NRows * NCols * AverageFieldSize;
            StringBuilder sb = new StringBuilder("", estimatedCapacity);
            sb.Append('[');
            List<string> rowJSON = new List<string>();
            for (int i = 0; i < NRows; i++) {
                StringBuilder rowStr = new StringBuilder("{", NCols * AverageFieldSize);
                List<string> fieldJSON = new List<string>();
                for (int j = 0; j < NCols; j++) {
                    fieldJSON.Add($"\"{_Header[j]}\": \"{_Rows[i][j]}\"");
                }
                rowStr.Append(String.Join(",\n", fieldJSON));
                rowStr.Append('}');
                rowJSON.Add(rowStr.ToString());
            }
            sb.Append(String.Join(",\n", rowJSON));
            sb.Append(']');
            return sb.ToString();
        }

    }
    sealed class YouTubeStudioFetcher
    {
        static private readonly Regex csvFileRegex = new Regex(@"\.csv$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static List<string> GetCSVFiles(string path)
        {
            var allFiles = Directory.GetFiles(path);
            var csvFiles = allFiles
                        .Where(x => csvFileRegex.Matches(x).Count != 0)
                        .ToList();
            return csvFiles;
        }

        static
        (Dictionary<string,int> , Dictionary<string,int> posOptionalFields) FieldPositionsFromCSV(CSV csv)
        {
            var posMandatoryFields = new Dictionary<string,int>
            {
                {"Date Measure", -1},
                {"Video ID", -1},
                {"Channel ID", -1}
            };
            foreach (var key in posMandatoryFields.Keys.ToList()) {
                posMandatoryFields[key] = csv.Header.IndexOf(key);
            }
            foreach (var KVPair in posMandatoryFields) {
                if (KVPair.Value == -1) {
                    throw new InvalidOperationException("CSV provided does not " +
                        $"specify a '{KVPair.Key}' column.");
                }
            }

            var posOptionalFields = new Dictionary<string,int>();
            var mandatoryFieldsKeys = new List<string>(posMandatoryFields.Keys);
            for (int i = 0; i < csv.Header.Count; i++) {
                if (!mandatoryFieldsKeys.Contains(csv.Header[i])) {
                    posOptionalFields.Add(csv.Header[i], i);
                }
            }
            return (posMandatoryFields, posOptionalFields);
        }

        static DateTime EpochToDateTime(int epoch)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(epoch);
        }
        static List<DL_YTS.Video> VideosFromCSV(CSV csv)
        {
            var(posMandatoryFields, posOptionalFields) = FieldPositionsFromCSV(csv);
            int idxChannelId = posMandatoryFields["Channel ID"];
            int idxVideoId = posMandatoryFields["Video ID"];
            int idxDateMeasure = posMandatoryFields["Date Measure"];

            var validityStart = DateTime.Now;
            var validityEnd = DateTime.MaxValue;

            var videos = new List<DL_YTS.Video>();
            foreach (var row in csv.Rows) {
                var channelId = row[idxChannelId];
                var videoId = row[idxVideoId];
                var dateMeasure = EpochToDateTime(Int32.Parse(row[idxDateMeasure]));
                foreach (var metricKeyValue in posOptionalFields) {
                    var metricName = metricKeyValue.Key;
                    var metricValue = row[metricKeyValue.Value];
                    var video = new DL_YTS.Video(
                        validityStart: validityStart,
                        validityEnd: validityEnd,
                        dateMeasure: dateMeasure,
                        channelId: channelId,
                        videoId: videoId,
                        metric: metricName,
                        value: metricValue
                    );
                    videos.Add(video);
                }
            }

            return videos;
        }

        public static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("Program must be run with exactly one argument, " +
                    "the path to CSV directory.");
                return;
            }
            string path = args[0];
            var files = GetCSVFiles(path);
            foreach (var file in files){
                Console.WriteLine($"Now parsing file '{file}'");
                CSV csv = new CSV(file);
                var videos = VideosFromCSV(csv);
                Console.WriteLine($"File contained data for {videos.Count} Video DTOs");
                videos.ForEach(Console.WriteLine);
            }
        }
    }
}
