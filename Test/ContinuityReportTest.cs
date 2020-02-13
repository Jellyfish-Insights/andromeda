using System;
using Xunit;
using DataLakeModels.Models.YouTube.Data;
using DataLakeModels;
using Test.Helpers;
using Andromeda.Commands;
using Andromeda.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Test {

    using PropertyDict = Dictionary<ContinuityProperty, bool?>;

    public class ContinuityReportTest {

        private YouTubeDataSteps YDS;

        public ContinuityReportTest() {
            DatabaseReset.Drop(Databases.LakeYouTubeData);
            DatabaseReset.Migrate(Databases.LakeYouTubeData);
            YDS = new YouTubeDataSteps();
        }

        private static void CheckReport(PropertyDict report, PropertyDict expected) {
            Assert.Empty(report.Where(entry => expected[entry.Key] != entry.Value));
            Assert.Empty(expected.Where(entry => report[entry.Key] != entry.Value));
        }

        private static PropertyDict GetReportOnVideoTable() {
            using (var context = new DataLakeYouTubeDataContext()) {
                var(schema, table, keys) = ContextIntrospection.GetDatabaseInfo(context, typeof(Video));
                return ContinuityReportCommand.ReportOnTable(YearDatabase.DataLakeDatabase, schema, table, keys);
            }
        }

        [Fact]
        public void EmptyTable() {
            var expectedReport = new PropertyDict(){
                { ContinuityProperty.NotEmpty, false },
                { ContinuityProperty.NoOverlapingInterval, true },
                { ContinuityProperty.UniqueCurrentValue, true },
            };
            var videoReport = GetReportOnVideoTable();
            CheckReport(expectedReport, videoReport);
        }

        [Fact]
        public void OverlapingInterval() {
            var videos = new List<(string VideoTitle, List<(DateTime ValidityStart, DateTime ValidityEnd)>)>(){
                ("a-yt-video",
                 new List<(DateTime ValidityStart, DateTime ValidityEnd)>(){
                    (new DateTime(2018, 1, 1), new DateTime(2018, 1, 3)),
                    (new DateTime(2018, 1, 2), DateTime.MaxValue)
                })
            };
            YDS.ThereAreVideosWithManyVersions(videos);

            var expectedReport = new PropertyDict(){
                { ContinuityProperty.NotEmpty, true },
                { ContinuityProperty.NoOverlapingInterval, false },
                { ContinuityProperty.UniqueCurrentValue, true },
            };
            var videoReport = GetReportOnVideoTable();
            CheckReport(expectedReport, videoReport);
        }

        [Fact]
        public void NoCurrentValue() {
            var videos = new List<(string VideoTitle, List<(DateTime ValidityStart, DateTime ValidityEnd)>)>(){
                ("a-yt-video",
                 new List<(DateTime ValidityStart, DateTime ValidityEnd)>(){
                    (new DateTime(2018, 1, 1), new DateTime(2018, 1, 2)),
                    (new DateTime(2018, 1, 2), new DateTime(2018, 1, 3)),
                })
            };
            YDS.ThereAreVideosWithManyVersions(videos);

            var videoReport = GetReportOnVideoTable();
            var expectedReport = new PropertyDict(){
                { ContinuityProperty.NotEmpty, true },
                { ContinuityProperty.NoOverlapingInterval, true },
                { ContinuityProperty.UniqueCurrentValue, false },
            };
            CheckReport(expectedReport, videoReport);
        }

        [Fact]
        public void NormalBehavior() {
            var videos = new List<(string VideoTitle, List<(DateTime ValidityStart, DateTime ValidityEnd)>)>(){
                ("a-yt-video",
                 new List<(DateTime ValidityStart, DateTime ValidityEnd)>(){
                    (new DateTime(2018, 1, 1), new DateTime(2018, 1, 2)),
                    (new DateTime(2018, 1, 2), new DateTime(2018, 1, 3)),
                    (new DateTime(2018, 1, 3), DateTime.MaxValue),
                })
            };
            YDS.ThereAreVideosWithManyVersions(videos);

            var videoReport = GetReportOnVideoTable();
            var expectedReport = new PropertyDict(){
                { ContinuityProperty.NotEmpty, true },
                { ContinuityProperty.NoOverlapingInterval, true },
                { ContinuityProperty.UniqueCurrentValue, true },
            };
            CheckReport(expectedReport, videoReport);
        }

        [Fact]
        public void NormalBehaviorOnYoutubeVideoDbWrite() {
            var channelId = "fee";

            var titleAtypo = "The True Mening of Patriotism";
            var titleA = "The True Meaning of Patriotism";
            var titleB = "The Population Boom";

            var someLakeVideos = new[] {
                new Video() { VideoId = "xyz", Title = titleAtypo, Duration = "PT5M11S" },
                new Video() { VideoId = "zxy", Title = titleB, Duration = "PT2M6S" },
            };
            YDS.SomeVideosWereFound(someLakeVideos, channelId);

            Thread.Sleep(1000);

            someLakeVideos = new[] {
                new Video() { VideoId = "xyz", Title = titleA, Duration = "PT5M11S" },
                new Video() { VideoId = "zxy", Title = titleB, Duration = "PT2M6S" },
            };
            YDS.SomeVideosWereFound(someLakeVideos, channelId);

            var expectedReport = new PropertyDict(){
                { ContinuityProperty.NotEmpty, true },
                { ContinuityProperty.NoOverlapingInterval, true },
                { ContinuityProperty.UniqueCurrentValue, true },
            };
            var videoReport = GetReportOnVideoTable();
            CheckReport(expectedReport, videoReport);
        }
    }
}
