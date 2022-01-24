using Serilog.Core;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;

using DataLakeModels;
using DataLakeModels.Models.YouTube.Studio;
using Jobs.Fetcher.YouTubeStudio.Helpers;
using Test.Helpers;
using Andromeda.Common;
using Andromeda.Common.Logging;

namespace Test {

    public class YouTubeStudioFetcherTest : IDisposable {
        private List<string> CreatedFiles = new List<string>();
        private int TestFileCount = 0;
        private Logger Logger = LoggerFactory.GetTestLogger
            ("YouTubeStudioFetcherTest");

        public YouTubeStudioFetcherTest() {
            DatabaseReset.Drop(Databases.LakeYouTubeStudio);
            DatabaseReset.Migrate(Databases.LakeYouTubeStudio);
            DatabaseReset.Drop(Databases.LakeLogging);
            DatabaseReset.Migrate(Databases.LakeLogging);

        }

        public void Dispose() {
            Logger.Debug("Cleaning...");
            foreach (var file in CreatedFiles) {
                File.Delete(file);
                Logger.Debug($"Removing test file '{file}'");
            }
        }

        private string getTestFile() {
            var newFile = $"testFile_{TestFileCount++}.json";
            CreatedFiles.Add(newFile);
            return newFile;
        }

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void JsonDecodeSingle_Test() {
            Logger.Information("Trying to decode single video...");
            var jsonString = @"
            [
                {
                    ""videoId"": ""a38f0c8e8a66c85b5a13e9e5a6fe6a56"",
                    ""channelId"": ""3ac57fb4e69e34d0"",
                    ""dateMeasure"": 1407734809,
                    ""metric"": ""Impressions"",
                    ""value"": 6811
                }
            ]";
            var expected = new Video_DTO{
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                DateMeasure = 1407734809,
                Metric = "Impressions",
                Value = 6811
            };
            var file = getTestFile();
            File.WriteAllText(file, jsonString);
            var dtos = ImportFromFileSystem.VideoDTOsFromFile(file, Logger);
            Assert.Single(dtos);
            Assert.True(dtos[0].Equals(expected));
        }

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void JsonDecodeMany_Test() {
            Logger.Information("Trying to decode many videos...");
            var jsonString = @"
            [
                {
                    ""videoId"": ""a38f0c8e8a66c85b5a13e9e5a6fe6a56"",
                    ""channelId"": ""3ac57fb4e69e34d0"",
                    ""dateMeasure"": 1371063987,
                    ""metric"": ""Impressions"",
                    ""value"": 3201
                },
                {
                    ""videoId"": ""a38f0c8e8a66c85b5a13e9e5a6fe6a56"",
                    ""channelId"": ""3ac57fb4e69e34d0"",
                    ""dateMeasure"": 1548401269,
                    ""metric"": ""Shares"",
                    ""value"": 9764
                }
            ]";
            var expected_01 = new Video_DTO{
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                DateMeasure = 1371063987,
                Metric = "Impressions",
                Value = 3201
            };
            var expected_02 = new Video_DTO{
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                DateMeasure = 1548401269,
                Metric = "Shares",
                Value = 9764
            };
            var file = getTestFile();
            File.WriteAllText(file, jsonString);
            var dtos = ImportFromFileSystem.VideoDTOsFromFile(file, Logger);
            Assert.Equal(2, dtos.Count());
            Assert.True(dtos[0].Equals(expected_01));
            Assert.True(dtos[1].Equals(expected_02));
        }

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void JsonDecodeBadData01_Test() {
            Logger.Information("Trying to decode bad data (01/02)...");
            var jsonString = @"
            [
                this { is not actually , a json __
                ;
            ]";
            var file = getTestFile();
            File.WriteAllText(file, jsonString);
            var dtos = ImportFromFileSystem.VideoDTOsFromFile(file, Logger);
            Assert.Null(dtos);
        }

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void JsonDecodeBadData02_Test() {
            Logger.Information("Trying to decode bad data (02/02)...");
            var jsonString = @"
            [
                ""unknown"": ""field"",
                ""something"": ""else""
            ]";
            var file = getTestFile();
            File.WriteAllText(file, jsonString);
            var dtos = ImportFromFileSystem.VideoDTOsFromFile(file, Logger);
            Assert.Null(dtos);
        }
    }
}
