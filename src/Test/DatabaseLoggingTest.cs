using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApplicationModels;
using Common.Logging;
using Test.Helpers;
using Xunit;
using DataLakeModels;
using Serilog.Core;
using Common;

namespace Test {

    public class DatabaseLoggingTest {

        public DatabaseLoggingTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Drop(Databases.LakeYouTubeData);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.LakeLogging);
        }

        private static void LoggerIsTypicallyUsed(Logger logger) {
            var fruits = new Dictionary<string, int> { { "Apple", 1 }, { "Pear", 5 } };
            logger.Information("In my bowl I have {Fruit}", fruits);

            var cars = new[] { "Ferrari", "Posche", "Lamborghini" };
            logger.Error("In my garage I have {Cars}", cars);

            var count = 456;
            logger.Debug("Retrieved {Count} records", count);

            var sensorInput = new { Latitude = 25, Longitude = 134 };
            logger.Warning("Processing {@SensorInput}", sensorInput);

            logger.Verbose("Intricate details");

            try {
                throw new Exception("Catch this!");
            } catch (Exception e) {
                logger.Error(e, "Great!");
            }
        }

        [Fact]
        public void CanLogToAnalyticsPlatform() {
            var logName = "foo";
            var logFileName = LoggerFactory.LogFileName(logName);
            if (File.Exists(logFileName)) File.Delete(logFileName);

            using (var logger = LoggerFactory.GetLogger<ApplicationDbContext>(logName)) {
                LoggerIsTypicallyUsed(logger);
            }

            using (var dbContext = new ApplicationDbContext()) {
                Assert.Equal(4, dbContext.RuntimeLog.Count());
            }

            Assert.True(File.Exists(logFileName));
            Assert.Equal(7, File.ReadLines(logFileName).Count());
        }

        [Fact]
        public void CanLogToDataLake() {
            var logName = "foo";
            var logFileName = LoggerFactory.LogFileName(logName);
            if (File.Exists(logFileName)) File.Delete(logFileName);

            using (var logger = LoggerFactory.GetLogger<DataLakeLoggingContext>(logName)) {
                LoggerIsTypicallyUsed(logger);
            }

            using (var dbContext = new DataLakeLoggingContext()) {
                Assert.Equal(4, dbContext.RuntimeLog.Count());
            }
            Assert.True(File.Exists(logFileName));
            Assert.Equal(7, File.ReadLines(logFileName).Count());
        }
    }
}
