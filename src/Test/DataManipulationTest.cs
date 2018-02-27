using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models;
using ApplicationModels.Models.DataViewModels;
using Test.Helpers;
using WebApp.Controllers;
using Xunit;

namespace Test {

    public class DataManipulationTest {

        [Fact]
        public void ReduceTimeSeriesTest() {
            var dateSet = new[] {
                new DateTime(2018, 1, 1),
                new DateTime(2018, 1, 2),
                new DateTime(2018, 1, 3)
            };

            var data = new[] {
                new TimeSeriesDataGroup() {
                    GroupName = "A",
                    Values = new double[] { 1.0, 2.0, 4.0 }
                },
                new TimeSeriesDataGroup() {
                    GroupName = "B",
                    Values = new double[] { 8.0, 16.0, 32.0 }
                }
            };

            var reduced = AbstractDataBackend.ReduceTimeSeries(dateSet, data);

            var totals = new[] { 9.0, 18.0, 36.0 };

            Assert.Equal(totals.Sum(), reduced.TotalOnPeriod);
            Assert.Equal(7.0, reduced.TotalPerGroup["A"]);
            Assert.Equal(56.0, reduced.TotalPerGroup["B"]);
            for (var i = 0; i < totals.Count(); i++) {
                Assert.Equal(totals[i], reduced.TotalTimeSeries.Values[i]);
            }
        }
    }
}
