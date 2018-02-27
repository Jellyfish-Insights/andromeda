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

    public class FacebookTransformationTest {

        private AnalyticsPlatformSteps APS;

        public FacebookTransformationTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Drop(Databases.LakeFacebook);
            DatabaseReset.Drop(Databases.LakeLogging);
            DatabaseReset.Migrate(Databases.LakeLogging);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.LakeAdWords);
            DatabaseReset.Migrate(Databases.LakeFacebook);

            APS = new AnalyticsPlatformSteps();
        }

        [Fact]
        public void JobsDryRun() {
            APS.FacebookTransformationsHaveRun();
            APS.ApApTransformationsHaveRun();
        }
    }
}
