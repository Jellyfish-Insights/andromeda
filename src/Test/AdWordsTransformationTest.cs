using System;
using System.Linq;
using System.Collections.Generic;
using ApplicationModels;
using DataLakeModels;
using DataLakeModels.Models.AdWords.Reports;
using VM = ApplicationModels.Models.DataViewModels;
using WebApp.Controllers;
using Test.Helpers;
using Xunit;
using Common;

namespace Test {

    public class AdWordsTransformationTest {

        private AnalyticsPlatformSteps APS;
        private AdWordsDataSteps AWDS;

        public AdWordsTransformationTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Drop(Databases.LakeYouTubeData);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.LakeAdWords);

            APS = new AnalyticsPlatformSteps();
            AWDS = new AdWordsDataSteps();
        }

        [Fact]
        public void JobsDryRun() {
            APS.AdWordsTransformationsHaveRun();
            APS.ApApTransformationsHaveRun();
        }

        [Fact]
        public void AdWordsInDataLakeAreTransformedIntoSources() {
            AWDS.ReportHasBeenFetched(
                new StructuralCriteriaPerformance() {
                KeywordId = "1",
                CampaignId = "zyx",
                AdGroupId = "abc",
                CriteriaType = "age",
                Criteria = "18-24",
                ValidityEnd = DateTime.MaxValue,
                ValidityStart = DateTime.UtcNow
            });

            AWDS.ReportHasBeenFetched(
                new StructuralCriteriaPerformance() {
                KeywordId = "2",
                CampaignId = "zyx",
                AdGroupId = "abc",
                CriteriaType = "age",
                Criteria = "18-24",
                ValidityEnd = DateTime.MaxValue,
                ValidityStart = DateTime.UtcNow
            });

            // Test if num of rows in StructuralCriteriaPerformanceReports
            // is same as num of data that we expect to insert on datalake
            using (var context = new DataLakeAdWordsContext()) {
                Assert.Equal(2, context.StructuralCriteriaPerformanceReports.Count());
            }

            APS.AdWordsAudienceSyncJobHasRun();
            APS.AdWordsAdSetSyncJobHasRun();

            // Test if we have just one row per AdGroupId in SourceAudiences and SourceAdSets
            using (var context = new ApplicationDbContext()) {
                Assert.Equal(1, context.SourceAudiences.Count());
                Assert.Equal(1, context.SourceAdSets.Count());
            }
        }
    }
}
