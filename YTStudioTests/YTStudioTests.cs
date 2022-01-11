using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Xunit;
using Jobs.Fetcher.YouTubeStudio;

namespace YTStudioTests
{
    public class YTStudioTests
    {
        private static bool WasClassInitialized = false;
        public YTStudioTests()
        {
            if (!WasClassInitialized) {
                // initial workingDirectory = MyProject/bin/debug/
                string workingDirectory = Directory.GetCurrentDirectory();
                Console.WriteLine($"We started running at {workingDirectory}");
                // projectDirectory = MyProject/
                string projectDirectory = Directory.GetParent(workingDirectory)
                                            .Parent.Parent.FullName;
                Directory.SetCurrentDirectory(projectDirectory);
                Console.WriteLine($"Now we are at {Directory.GetCurrentDirectory()}");
                WasClassInitialized = true;
            }
        }

        public string UseTestFile(string filename)
        {
            return Path.Join(
                Directory.GetCurrentDirectory(),
                "data",
                filename
            );
        }

        [Fact]
        public void Test01()
        {
            string nonExistentFile = UseTestFile("nonExistent.csv");
            List<string> file1 = CSV.StringListFromFile(nonExistentFile);
            bool result = file1.Count == 0;
            Assert.True(result, "File was not identified as non-existent.");
        }

        [Fact]
        public void Test02()
        {
            string nonExistentFile = UseTestFile("existent.json");
            List<string> file1 = CSV.StringListFromFile(nonExistentFile);
            bool result = file1.Count == 0;
            Assert.True(result, "File was not identified as non-CSV.");
        }

        [Fact]
        public void Test03()
        {
            string nonExistentFile = UseTestFile("empty_file.csv");
            List<string> file1 = CSV.StringListFromFile(nonExistentFile);
            bool result = file1.Count == 0;
            Assert.True(result, "File was not identified as empty.");
        }

        [Fact]
        public void Test04()
        {
            string nonExistentFile = UseTestFile("file_2_lines.CSV");
            List<string> file1 = CSV.StringListFromFile(nonExistentFile);
            const int expectedLines = 2;
            bool result = file1.Count == expectedLines;
            Assert.True(result, "Read wrong number of lines from file " +
                $"({file1.Count}, expected {expectedLines}).");
        }

        [Fact]
        public void Test05()
        {
            string nonExistentFile = UseTestFile("file_10_lines.csv");
            List<string> file1 = CSV.StringListFromFile(nonExistentFile);
            const int expectedLines = 10;
            bool result = file1.Count == expectedLines;
            Assert.True(result, "Read wrong number of lines from file " +
                $"({file1.Count}, expected {expectedLines}).");
        }

        [Fact]
        public void Test06()
        {
            const int headerExpectedSize = 3;
            const int rowsExpectedSize = 3;
            List<string> strList = new List<string>
                {
                    "name,id,date",
                    "joe,1,10-02-2003",
                    "jan,2,04-05-1975",
                    "max,3,29-11-1999"
                };
            List<string> header;
            List<List<string>> rows;
            CSV.HeaderAndRowsFromStringList(strList, out header, out rows);
            bool headerRightSize = header.Count == headerExpectedSize;
            Assert.True(headerRightSize, "Wrong header size " +
                $"({header.Count}, expected {headerExpectedSize}).");
            bool rowsRightSize = rows.Count == rowsExpectedSize;
            Assert.True(rowsRightSize, "Wrong rows size " +
                $"({rows.Count}, expected {rowsExpectedSize}).");
        }

        [Fact]
        public void Test07()
        {
            /* test for valid null values */
            const int headerExpectedSize = 3;
            const int rowsExpectedSize = 3;
            List<string> strList = new List<string>
                {
                    "name,id,date",
                    "joe,1,10-02-2003",
                    "jan,,04-05-1975",
                    "max,3,"
                };
            List<string> header;
            List<List<string>> rows;
            CSV.HeaderAndRowsFromStringList(strList, out header, out rows);
            bool headerRightSize = header.Count == headerExpectedSize;
            Assert.True(headerRightSize, "Wrong header size " +
                $"({header.Count}, expected {headerExpectedSize}).");
            bool rowsRightSize = rows.Count == rowsExpectedSize;
            Assert.True(rowsRightSize, "Wrong rows size " +
                $"({rows.Count}, expected {rowsExpectedSize}).");
        }

        [Fact]
        public void Test08()
        {
            const int headerExpectedSize = 3;
            const int rowsExpectedSize = 0;
            List<string> strList = new List<string>
                {
                    "name,id,date"
                };
            List<string> header;
            List<List<string>> rows;
            CSV.HeaderAndRowsFromStringList(strList, out header, out rows);
            bool headerRightSize = header.Count == headerExpectedSize;
            Assert.True(headerRightSize, "Wrong header size " +
                $"({header.Count}, expected {headerExpectedSize}).");
            bool rowsRightSize = rows.Count == rowsExpectedSize;
            Assert.True(rowsRightSize, "Wrong rows size " +
                $"({rows.Count}, expected {rowsExpectedSize}).");
        }

        [Fact]
        public void Test09()
        {
            List<string> strList = new List<string>();
            List<string> header;
            List<List<string>> rows;
            ;
            var exception = Assert.Throws<InvalidOperationException>(
                () => CSV.HeaderAndRowsFromStringList(strList, out header, out rows)
            );
            Assert.Equal("String list is uninitialized or empty.", exception.Message);
        }

        [Fact]
        public void Test10()
        {
            List<string> strList = new List<string>
                {
                    "",
                    "joe,1,10-02-2003",
                    "jan,2,04-05-1975",
                    "max,3,29-11-1999"
                };
            List<string> header;
            List<List<string>> rows;
            ;
            var exception = Assert.Throws<InvalidOperationException>(
                () => CSV.HeaderAndRowsFromStringList(strList, out header, out rows)
            );
            Assert.Equal("Header is empty.", exception.Message);
        }

        [Fact]
        public void Test11()
        {
            List<string> strList = new List<string>
                {
                    "name,id,date",
                    "joe,1,10-02-2003",
                    "jan,2,04-05-1975",
                    "max,3"
                };
            List<string> header;
            List<List<string>> rows;
            ;
            var exception = Assert.Throws<InvalidOperationException>(
                () => CSV.HeaderAndRowsFromStringList(strList, out header, out rows)
            );
            Assert.Equal("Row size does not match header.", exception.Message);
        }
    }
}
