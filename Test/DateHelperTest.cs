using System;
using System.Linq;
using Xunit;
using Andromeda.Common;

namespace Test {

    public class DateHelperTest {

        [Fact]
        public void DaysInRange() {

            var a = new DateTime(2017, 1, 1);
            var b = new DateTime(2017, 1, 4);

            var days = DateHelper.DaysInRange(a, b).ToList();

            var expected = new[] {
                new DateTime(2017, 1, 1),
                new DateTime(2017, 1, 2),
                new DateTime(2017, 1, 3),
                new DateTime(2017, 1, 4),
            };

            Assert.Equal(expected.Count(), days.Count());
            for (var i = 0; i < expected.Count(); i++) {
                Assert.Equal(expected[i], days[i]);
            }
        }

        [Fact]
        public void DaysInRangeReversed() {

            var a = new DateTime(2017, 1, 1);
            var b = new DateTime(2017, 1, 4);

            var days = DateHelper.DaysInRange(a, b, true).ToList();

            var expected = new[] {
                new DateTime(2017, 1, 4),
                new DateTime(2017, 1, 3),
                new DateTime(2017, 1, 2),
                new DateTime(2017, 1, 1),
            };

            Assert.Equal(expected.Count(), days.Count());
            for (var i = 0; i < expected.Count(); i++) {
                Assert.Equal(expected[i], days[i]);
            }
        }

        [Fact]
        public void DaysInRangeSingle() {

            var a = new DateTime(2017, 1, 1);

            var days = DateHelper.DaysInRange(a, a).ToList();

            var expected = new DateTime(2017, 1, 1);

            Assert.Single(days);
            Assert.Equal(expected, days[0]);
        }

        [Fact]
        public void DaysInRangeNone() {

            var a = new DateTime(2017, 1, 4);
            var b = new DateTime(2017, 1, 1);

            var days = DateHelper.DaysInRange(a, b).ToList();

            Assert.Empty(days);
        }
    }
}
