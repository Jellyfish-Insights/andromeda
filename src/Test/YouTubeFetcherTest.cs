using System;
using System.Linq;
using System.Collections.Generic;
using DataLakeModels;
using DataLakeModels.Models.YouTube.Analytics;
using DataLakeModels.Models.YouTube.Data;
using Test.Helpers;
using Xunit;
using Common;

namespace Test {

    public class YouTubeFetcherTest {

        private YouTubeDataSteps YDS;

        public YouTubeFetcherTest() {
            DatabaseReset.Drop(Databases.LakeYouTubeAnalytics);
            DatabaseReset.Migrate(Databases.LakeYouTubeAnalytics);
            DatabaseReset.Migrate(Databases.LakeYouTubeData);

            YDS = new YouTubeDataSteps();
        }

        [Fact]
        public void YouTubeViewerPercentageDbWriteTest() {

            var someLakeVideos = new[] {
                new Video() { VideoId = "xyz", Title = "The Population Boom", Duration = "PT5M11S" },
            };
            YDS.SomeVideosWereFound(someLakeVideos, "fee");

            var someMetrics = new (string videoId, DateTime date, IEnumerable<ViewerPercentage> viewerPercentages)[] {
                (
                    "xyz",
                    new DateTime(2018, 2, 1),
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
                }
                ), (
                    "xyz",
                    new DateTime(2018, 2, 2),  // Same value as previous
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 3),  // Some values have changed
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.3 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.2 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 4),  // Same values as previous
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.3 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.2 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 5),  // Different groups
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.3 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "18", Value = 0.3 },
                }
                    )
            };

            YDS.SomeVierPercentageMetricsWereFound(someMetrics, new DateTime(2018, 2, 5));

            using (var context = new DataLakeYouTubeAnalyticsContext()) {
                var now = new DateTime(2018, 2, 6);
                var currentMetrics = context.ViewerPercentageMetric.Where(x => x.ValidityStart <= now && now < x.ValidityEnd);
                Assert.Equal(9, currentMetrics.Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 1)).Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 3)).Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 5)).Count());
            }

            var someNewMetrics = new (string videoId, DateTime date, IEnumerable<ViewerPercentage> viewerPercentages)[] {
                (
                    "xyz",
                    new DateTime(2018, 2, 2),  // Same value as before
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
                }
                ), (
                    "xyz",
                    new DateTime(2018, 2, 3),  // Same value as before
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.3 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.2 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 4),  // Different values
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.3 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 5),  // Same as previous, different from before
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.3 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 6),  // Same as previous, new date
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.3 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 7),  // Some values have changed, new date
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.3 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.3 },
                }
                    )
            };

            YDS.SomeVierPercentageMetricsWereFound(someNewMetrics, new DateTime(2018, 2, 7));

            using (var context = new DataLakeYouTubeAnalyticsContext()) {
                var now = new DateTime(2018, 2, 8);
                var currentMetrics = context.ViewerPercentageMetric.Where(x => x.ValidityStart <= now && now < x.ValidityEnd);
                Assert.Equal(12, currentMetrics.Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 1)).Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 3)).Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 4)).Count());
                Assert.Equal(3, currentMetrics.Where(x => x.StartDate == new DateTime(2018, 2, 7)).Count());
            }

            var someNewerMetrics = new (string videoId, DateTime date, IEnumerable<ViewerPercentage> viewerPercentages)[] {
                (
                    "xyz",
                    new DateTime(2018, 2, 7),  // Same date, different values
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.6 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.4 },
                }
                ), (
                    "xyz",
                    new DateTime(2018, 2, 8),  // Same value
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.6 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.4 },
                }
                    ), (
                    "xyz",
                    new DateTime(2018, 2, 9),  // Different value
                    new[] {
                    new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.3 },
                    new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.2 },
                    new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.5 },
                }
                    )
            };

            YDS.SomeVierPercentageMetricsWereFound(someNewerMetrics, new DateTime(2018, 2, 8));
        }
    }
}
