using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jobs.Fetcher.YouTubeStudio
{
    public class CSV
    {
        private const string Separator = ",";
        static private readonly Regex csvFileRegex = new Regex(@"\.csv$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private List<string> _Header;
        public List<string> Header { get {return this._Header; } }
        private List<List<string>> _Rows;
        public List<List<string>> Rows { get {return this._Rows; } }

        public int NCols()
        {
            return Header.Count;
        }
        public int NRows()
        {
            return Rows.Count;
        }


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

    }
    class YouTubeStudioFetcher
    {

        public static void Main(string[] args)
        {
            Console.WriteLine($"Received {args.Length} CLI arguments:");
            foreach (string arg in args) {
                Console.WriteLine($"\t{arg}");
            }
            if (args.Length != 1) {
                Console.WriteLine("Program must be run with exactly one argument");
                return;
            }
            string filename = args[0];
            List<string> lines = CSV.StringListFromFile(filename);
            if (lines == null || lines.Count == 0) {
                Console.WriteLine("Zero lines read from file. Cannot continue.");
                return;
            }
        }
    }
}
