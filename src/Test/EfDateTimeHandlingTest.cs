using System;
using System.Linq;
using ApplicationModels;
using ApplicationModels.Models;
using Test.Helpers;
using Xunit;
using Common;

namespace Test {

    public class EfDateTimeHandling {

        private AnalyticsPlatformSteps APS;

        public EfDateTimeHandling() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            APS = new AnalyticsPlatformSteps();
        }

        [Fact]
        public void DateTimeValuesAreConsistent() {

            var now = DateTime.UtcNow;

            var someVideos = new[] {
                new SourceVideo { Title = "A", PublishedAt = DateTime.MinValue },
                new SourceVideo { Title = "B", PublishedAt = now },
                new SourceVideo { Title = "C", PublishedAt = DateTime.MaxValue },
            };
            APS.TheseVideosExist(someVideos);

            var maxValue = DateTime.MaxValue;

            using (var context = new ApplicationDbContext()) {
                Assert.Equal(1, context.SourceVideos.Where(x => x.PublishedAt == now).Count());
                Assert.Equal(1, context.SourceVideos.Where(x => x.PublishedAt == maxValue).Count());
                Assert.Equal(1, context.SourceVideos.Where(x => x.PublishedAt == DateTime.MaxValue).Count());
            }
        }
    }
}
