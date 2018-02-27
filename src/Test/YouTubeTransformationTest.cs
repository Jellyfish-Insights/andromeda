using System;
using System.Linq;
using ApplicationModels;
using DataLakeModels;
using DataLakeModels.Models.YouTube.Data;
using DataLakeModels.Models.YouTube.Analytics;
using VM = ApplicationModels.Models.DataViewModels;
using WebApp.Controllers;
using Test.Helpers;
using Xunit;
using Common;

namespace Test {

    public class YouTubeTransformationTest {

        private AnalyticsPlatformSteps APS;

        private YouTubeDataSteps YDS;

        public YouTubeTransformationTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Drop(Databases.LakeYouTubeData);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.LakeYouTubeData);
            DatabaseReset.Migrate(Databases.LakeYouTubeAnalytics);
            DatabaseReset.Migrate(Databases.LakeAdWords);

            APS = new AnalyticsPlatformSteps();
            YDS = new YouTubeDataSteps();
        }

        [Fact]
        public void JobsDryRun() {
            APS.YouTubeTransformationsHaveRun();
            APS.ApApTransformationsHaveRun();
        }

        [Fact]
        public void YouTubeVideosInDataLakeAreTransformedIntoSourceVideos() {
            var channelId = "fee";

            var titleAtypo = "The True Mening of Patriotism";
            var titleA = "The True Meaning of Patriotism";
            var titleB = "The Population Boom";

            var someLakeVideos = new[] {
                new Video() { VideoId = "xyz", Title = titleAtypo, Duration = "PT5M11S" },
                new Video() { VideoId = "zxy", Title = titleB, Duration = "PT2M6S" },
            };
            YDS.SomeVideosWereFound(someLakeVideos, channelId);

            APS.YouTubeVideoSyncJobHasRun();

            using (var context = new DataLakeYouTubeDataContext()) {
                var videos = context.Videos.OrderBy(x => x.Title).ToList();
                var titles = new[] { titleB, titleAtypo };

                Assert.Equal(titles.Count(), videos.Count());
                for (var i = 0; i < titles.Count(); i++) {
                    Assert.Equal(titles[i], videos[i].Title);
                }
            }

            using (var context = new ApplicationDbContext()) {
                var videos = context.SourceVideos.OrderBy(x => x.Title).ToList();
                var titles = new[] { titleB, titleAtypo };

                Assert.Equal(titles.Count(), videos.Count());
                for (var i = 0; i < titles.Count(); i++) {
                    Assert.Equal(titles[i], videos[i].Title);
                }
            }

            someLakeVideos = new[] {
                new Video() { VideoId = "xyz", Title = titleA, Duration = "PT5M11S" },
                new Video() { VideoId = "zxy", Title = titleB, Duration = "PT2M6S" },
            };
            YDS.SomeVideosWereFound(someLakeVideos, channelId);

            APS.YouTubeVideoSyncJobHasRun();

            using (var context = new DataLakeYouTubeDataContext()) {
                var videos = context.Videos.OrderBy(x => x.Title).ToList();
                var titles = new[] { titleB, titleA, titleAtypo };

                Assert.Equal(titles.Count(), videos.Count());
                for (var i = 0; i < titles.Count(); i++) {
                    Assert.Equal(titles[i], videos[i].Title);
                }
            }

            using (var context = new ApplicationDbContext()) {
                var videos = context.SourceVideos.OrderBy(x => x.Title).ToList();
                var titles = new[] { titleB, titleA };

                Assert.Equal(titles.Count(), videos.Count());
                for (var i = 0; i < titles.Count(); i++) {
                    Assert.Equal(titles[i], videos[i].Title);
                }
            }
        }

        [Fact]
        public void YouTubeMetricScaling() {
            var channelId = "fee";

            var titleA = "The True Meaning of Patriotism";
            var titleB = "The Population Boom";

            var someLakeVideos = new[] {
                new Video() { VideoId = "xyz", Title = titleA, Duration = "PT5M11S" },
                new Video() { VideoId = "zxy", Title = titleB, Duration = "PT2M6S" },
            };
            YDS.SomeVideosWereFound(someLakeVideos, channelId);

            var someContentMetrics = new[] {
                new VideoDailyMetric() { VideoId = "xyz", Views = 10, AverageViewDuration = 34, Date = new DateTime(2018, 2, 1) },
                new VideoDailyMetric() { VideoId = "zxy", Views = 12, AverageViewDuration = 30, Date = new DateTime(2018, 2, 1) },
                new VideoDailyMetric() { VideoId = "xyz", Views = 5, AverageViewDuration = 57, Date = new DateTime(2018, 2, 2) },
                new VideoDailyMetric() { VideoId = "zxy", Views = 23, AverageViewDuration = 81, Date = new DateTime(2018, 2, 2) },
            };
            YDS.SomeContentMetricsWereFound(someContentMetrics);

            APS.YouTubeTransformationsHaveRun();

            APS.ApApTransformationsHaveRun();

            var cases = new[] {
                (new DateTime(2018, 2, 1), new DateTime(2018, 2, 10), new[] { 0, 0, 34, 10, 0, 0, 23, 40, 0, 0 }),
                (new DateTime(2018, 2, 4), new DateTime(2018, 2, 7), new[] { 10, 0, 0, 23 }),
            };

            var from = new DateTime(2018, 2, 1);
            var to = new DateTime(2018, 2, 2);

            var controller = new ContentDataController(new DBContentDataBackend());
            var viewTime = controller.ChartData("View Time", "Medium", from, to, new VM.Tag[0]);
            var averageViewTime = controller.ChartData("Average View Time", "Medium", from, to, new VM.Tag[0]);

            var values = new[] {
                (10.0, 34),
                (12.0, 30),
                (5.0, 57),
                (23.0, 81),
            };

            var totalViewTime = values.Select(x => x.Item1 * x.Item2).Sum();
            var totalAverageViewTime = totalViewTime / values.Select(x => x.Item1).Sum();

            Assert.Equal(totalViewTime, viewTime.TotalOnPeriod);
            Assert.Equal(totalAverageViewTime, averageViewTime.TotalOnPeriod);
        }
    }
}
