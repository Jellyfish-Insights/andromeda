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

/**
 Run tests with:
 dotnet test --filter "Category=YouTubeStudio"
 */
namespace Test.YouTubeStudio {

    public class FileSystemFixture : IDisposable
    {
        public List<string> CreatedFiles = new List<string>();
        public int TestFileCount = 0;
        public readonly Logger Logger = LoggerFactory.GetTestLogger
            ("YouTubeStudioFetcherTest_FileSystem");

        public void Dispose()
        {
            Logger.Debug("Cleaning...");
            foreach (var file in CreatedFiles) {
                File.Delete(file);
                Logger.Debug($"Removing test file '{file}'");
            }
        }
    }

    public class FileSystemTests : IClassFixture<FileSystemFixture>
    {
        private readonly FileSystemFixture Fixture;
        private readonly Logger Logger;
        public FileSystemTests(FileSystemFixture fixture)
        {
            Fixture = fixture;
            Logger = Fixture.Logger;
        }

        private string GetTestFile() {
            var newFile = $"testFile_{Fixture.TestFileCount++}.json";
            Fixture.CreatedFiles.Add(newFile);
            return newFile;
        }

        /* */

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void JsonDecodeSingle_Test() {
            Logger.Information("Trying to decode single video...");
            var jsonString = @"
            [
                {
                    ""videoId"": ""a38f0c8e8a66c85b5a13e9e5a6fe6a56"",
                    ""channelId"": ""3ac57fb4e69e34d0"",
                    ""eventDate"": 1407734809,
                    ""metric"": ""Impressions"",
                    ""value"": 6811
                }
            ]";
            var expected = new Video_DTO{
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1407734809,
                Metric = "Impressions",
                Value = 6811
            };
            var file = GetTestFile();
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
                    ""eventDate"": 1371063987,
                    ""metric"": ""Impressions"",
                    ""value"": 3201
                },
                {
                    ""videoId"": ""a38f0c8e8a66c85b5a13e9e5a6fe6a56"",
                    ""channelId"": ""3ac57fb4e69e34d0"",
                    ""eventDate"": 1548401269,
                    ""metric"": ""Shares"",
                    ""value"": 9764
                }
            ]";
            var expected_01 = new Video_DTO{
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1371063987,
                Metric = "Impressions",
                Value = 3201
            };
            var expected_02 = new Video_DTO{
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1548401269,
                Metric = "Shares",
                Value = 9764
            };
            var file = GetTestFile();
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
            var file = GetTestFile();
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
            var file = GetTestFile();
            File.WriteAllText(file, jsonString);
            var dtos = ImportFromFileSystem.VideoDTOsFromFile(file, Logger);
            Assert.Null(dtos);
        }

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void IntToDateTime_Test()
        {
            Logger.Information("Running conversion test (uint -> DateTime)");
            const uint ts_01 = 1643068800;
            var dt_01 = new DateTime(2022, 01, 25, 0, 0, 0, 0, DateTimeKind.Utc);

            const uint ts_02 = 1331510400;
            var dt_02 = new DateTime(2012, 03, 12, 0, 0, 0, 0, DateTimeKind.Utc);

            const uint ts_03 = 647481600;
            var dt_03 = new DateTime(1990, 07, 09, 0, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(dt_01, DbWriter.EpochToDateTime(ts_01));
            Assert.Equal(dt_02, DbWriter.EpochToDateTime(ts_02));
            Assert.Equal(dt_03, DbWriter.EpochToDateTime(ts_03));
        }
    }

    /*  We don't want to have the database dropped every time, so we define
    a fixture that is run once for all classes marked as belonging to
    "Database collection". As they are in the same collection, they will be
    run sequentially, not in parallel - Docs: https://archive.is/wip/WOZIq
    */
    public class DatabaseFixture
    {
        public DatabaseFixture()
        {
            DatabaseReset.Drop(Databases.LakeYouTubeStudio);
            DatabaseReset.Migrate(Databases.LakeYouTubeStudio);
        }
    }

    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {}

    [Collection("Database collection")]
    public abstract class AbstractDatabaseTest
    {
        private readonly DatabaseFixture Fixture;
        protected readonly Logger Logger;
        public AbstractDatabaseTest(DatabaseFixture fixture, string testName)
        {
            Fixture = fixture;
            using (var context = new DataLakeYouTubeStudioContext()) {
                DeleteAllVideos(context);
            }
            Logger = LoggerFactory.GetTestLogger($"YouTubeStudioFetcherTest_{testName}");
        }

        /* Defines an epsilon within we will consider two times to be the same
		(due to unforeseeable delays in I/O, database access, etc.) */
		private readonly TimeSpan Tolerance = TimeSpan.FromSeconds(2);

        protected void DeleteAllVideos(DataLakeYouTubeStudioContext ctx)
        {
            var videos = ctx.Videos;
            foreach (var v in videos) {
                ctx.Remove(v);
            }
            ctx.SaveChanges();
        }

        protected Func<Video, bool> FindDTO(Video_DTO videoDTO)
        {
            return v =>
                v.VideoId == videoDTO.VideoId
                && v.ChannelId == videoDTO.ChannelId
                && v.EventDate == DbWriter.EpochToDateTime(videoDTO.EventDate).ToUniversalTime().Date
                && v.Metric == videoDTO.Metric
                && v.Value == videoDTO.Value;
        }

        protected bool AreDatesTheSame(DateTime dt, DateTime other)
        {
            TimeSpan timeDelta = (dt - other).Duration();
            return timeDelta < Tolerance;
        }
    }

    public class DBTest_01 : AbstractDatabaseTest
    {
        public DBTest_01(DatabaseFixture fixture) : base(fixture, "DBTest_01") {}

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void SameVideo_DiffMetrics_Test() {
            Logger.Information("Inserting same video with different metrics...");
            var video_dto_01 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1371063987,
                Metric = "Impressions",
                Value = 3201
            };
            var video_dto_02 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1371063987,
                Metric = "Views",
                Value = 5
            };

            using (var context = new DataLakeYouTubeStudioContext()) {
                DeleteAllVideos(context);
                var now = DateTime.UtcNow;
                var video_DTOs = new List<Video_DTO>{video_dto_01, video_dto_02};
                DbWriter.Write(video_DTOs, Logger);

                var videos = context.Videos;
                var videosList = videos.ToList();
                Assert.Equal(2, videosList.Count());

                var video_01 = videosList.SingleOrDefault(FindDTO(video_dto_01));
                Assert.NotNull(video_01);
                Assert.True(AreDatesTheSame(DateTime.MaxValue, video_01.ValidityEnd));
                Assert.True(AreDatesTheSame(video_01.ValidityStart, now));

                var video_02 = videosList.SingleOrDefault(FindDTO(video_dto_02));
                Assert.NotNull(video_02);
                Assert.True(AreDatesTheSame(DateTime.MaxValue, video_02.ValidityEnd));
                Assert.True(AreDatesTheSame(video_02.ValidityStart, now));
            }
		}
	}

    public class DBTest_02 : AbstractDatabaseTest
    {
        public DBTest_02(DatabaseFixture fixture) : base(fixture, "DBTest_02") {}

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void SameVideo_SameMetrics_DiffDates_Test()
        {
            Logger.Information("Inserting same video with same metric, "
                + "different dates...");
            var video_dto_01 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1639969200, /* 2021-20-12 */
                Metric = "Impressions",
                Value = 1234
            };
            var video_dto_02 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1640401200, /* 2021-25-12 */
                Metric = "Impressions",
                Value = 5678
            };

            using (var context = new DataLakeYouTubeStudioContext()) {
                DeleteAllVideos(context);
                var now = DateTime.UtcNow;
                var video_DTOs = new List<Video_DTO>{video_dto_01, video_dto_02};
                DbWriter.Write(video_DTOs, Logger);

                var videos = context.Videos;
                var videosList = videos.ToList();
                Assert.Equal(2, videosList.Count());

                var video_01 = videosList.SingleOrDefault(FindDTO(video_dto_01));
                Assert.NotNull(video_01);
                Assert.True(AreDatesTheSame(DateTime.MaxValue, video_01.ValidityEnd));
                Assert.True(AreDatesTheSame(video_01.ValidityStart, now));

                var video_02 = videosList.SingleOrDefault(FindDTO(video_dto_02));
                Assert.NotNull(video_02);
                Assert.True(AreDatesTheSame(DateTime.MaxValue, video_02.ValidityEnd));
                Assert.True(AreDatesTheSame(video_02.ValidityStart, now));
            }

        }
    }


    public class DBTest_03 : AbstractDatabaseTest
    {
        public DBTest_03(DatabaseFixture fixture) : base(fixture, "DBTest_03") {}

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void SameVideo_SameMetric_DiffDates_01_Test()
        {
            Logger.Information("Inserting same video with same metric, "
                + "same date, different values...");
            var video_dto_01 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1639969250,
                Metric = "Impressions",
                Value = 1234
            };
            var video_dto_02 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1639969251,
                Metric = "Impressions",
                Value = 5678
            };

            using (var context = new DataLakeYouTubeStudioContext()) {
                DeleteAllVideos(context);
                var now = DateTime.UtcNow;
                var video_DTOs = new List<Video_DTO>{video_dto_01, video_dto_02};
                DbWriter.Write(video_DTOs, Logger);

                var videos = context.Videos;
                var videosList = videos.ToList();
                Assert.Equal(2, videosList.Count());

                var video_01 = videosList.SingleOrDefault(FindDTO(video_dto_01));
                Assert.NotNull(video_01);
                var video_02 = videosList.SingleOrDefault(FindDTO(video_dto_02));
                Assert.NotNull(video_02);

                Assert.True(AreDatesTheSame(
                        video_02.ValidityStart, video_01.ValidityEnd));
                Logger.Debug("Video 01 is: \n" + video_01.ToString());
                Logger.Debug("Video 02 is: \n" + video_02.ToString());

                var validVideo = videosList.SingleOrDefault(v =>
                        AreDatesTheSame(v.ValidityEnd, DateTime.MaxValue));
                Logger.Debug("Of the two, the valid video is: \n" + validVideo.ToString());
            }

        }
    }

    public class DBTest_04 : AbstractDatabaseTest
    {
        public DBTest_04(DatabaseFixture fixture) : base(fixture, "DBTest_04") {}

        [Fact]
        [Trait("Category","YouTubeStudio")]
        public void SameVideo_SameMetric_DiffDates_02_Test()
        {
            /* In case we insert first the one with the later EventDate,
            does it make any difference? It shouldn't, the matter here is
            order of insertion, not which one has the higher EventDate */
            Logger.Information("Inserting same video with same metric, "
                + "same date, different values...");
            var video_dto_01 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1639969251,
                Metric = "Impressions",
                Value = 1234
            };
            var video_dto_02 = new Video_DTO {
                VideoId = "a38f0c8e8a66c85b5a13e9e5a6fe6a56",
                ChannelId = "3ac57fb4e69e34d0",
                EventDate = 1639969250,
                Metric = "Impressions",
                Value = 5678
            };

            using (var context = new DataLakeYouTubeStudioContext()) {
                DeleteAllVideos(context);
                var now = DateTime.UtcNow;
                var video_DTOs = new List<Video_DTO>{video_dto_01, video_dto_02};
                DbWriter.Write(video_DTOs, Logger);

                var videos = context.Videos;
                var videosList = videos.ToList();
                Assert.Equal(2, videosList.Count());

                var video_01 = videosList.SingleOrDefault(FindDTO(video_dto_01));
                Assert.NotNull(video_01);
                var video_02 = videosList.SingleOrDefault(FindDTO(video_dto_02));
                Assert.NotNull(video_02);

                Assert.True(AreDatesTheSame(
                        video_02.ValidityStart, video_01.ValidityEnd));
                Logger.Debug("Video 01 is: \n" + video_01.ToString());
                Logger.Debug("Video 02 is: \n" + video_02.ToString());

                var validVideo = videosList.SingleOrDefault(v =>
                        AreDatesTheSame(v.ValidityEnd, DateTime.MaxValue));
                Logger.Debug("Of the two, the valid video is: \n" + validVideo.ToString());
            }

        }
    }
}
