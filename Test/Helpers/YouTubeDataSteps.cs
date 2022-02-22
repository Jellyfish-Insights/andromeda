using System;
using System.Linq;
using System.Collections.Generic;
using DataLakeModels;
using DataLakeModels.Models.YouTube.Data;
using DataLakeModels.Models.YouTube.Analytics;
using Jobs.Fetcher.YouTube.Helpers;
using Andromeda.Common.Logging;

namespace Test.Helpers {

    /**
       This class is used to generate data for test scenarios. More specifically,
       this class generates data for YouTube Data Lake.

       Methods of this class should not be static because tests scenarios may be
       execute in parallel, and any shared state may lead to concurrency problems.
     */
    public class YouTubeDataSteps {

        public void SomeVideosWereFound(Video[] videos, string channelId) {
            DbWriter.Write(videos, channelId, LoggerFactory.GetTestLogger());
        }

        public void SomeContentMetricsWereFound(VideoDailyMetric[] metrics) {
            DbWriter.Write(metrics, LoggerFactory.GetTestLogger());
        }

        public void SomeVierPercentageMetricsWereFound((string videoId, DateTime date, IEnumerable<ViewerPercentage> viewerPercentages)[] metrics, DateTime now) {
            foreach (var metric in metrics) {
                DbWriter.Write(metric.videoId, metric.date, metric.viewerPercentages, now);
            }
        }

        public void ThereAreVideosWithManyVersions(List<(string VideoTitle, List<(DateTime ValidityStart, DateTime ValidityEnd)> dateRanges)> videos) {
            using (var context = new DataLakeYouTubeDataContext()) {
                foreach (var videoVersions in videos) {
                    var versions = videoVersions.dateRanges.Select(x => new Video() { VideoId = videoVersions.VideoTitle, ValidityStart = x.ValidityStart, ValidityEnd = x.ValidityEnd });
                    context.AddRange(versions.ToArray());
                }
                context.SaveChanges();
            }
        }
    }
}
