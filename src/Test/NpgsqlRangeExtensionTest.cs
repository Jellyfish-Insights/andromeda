using System;
using System.Collections.Generic;
using ApplicationModels.Extensions;
using NpgsqlTypes;
using Xunit;

namespace Test {
    public class NpgsqlRangeExtensionTest {
        [Fact]
        public void TestContains() {
            var r = new NpgsqlRange<int>(0, 2);
            Assert.True(NpgsqlRangeExtensions.Includes(r, 1));
        }

        [Fact]
        public void TestContainsEqual() {
            var r = new NpgsqlRange<int>(0, 1);
            Assert.True(NpgsqlRangeExtensions.Includes(r, 1));
            Assert.True(NpgsqlRangeExtensions.Includes(r, 0));
        }

        [Fact]
        public void TestNotContains() {

            var r = new NpgsqlRange<int>(1, 2);
            Assert.False(NpgsqlRangeExtensions.Includes(r, 0));
            Assert.False(NpgsqlRangeExtensions.Includes(r, 3));
        }

        [Fact]
        public void TestInfity() {
            var ru = new NpgsqlRange<int>(0, false, false, 0, false, true);
            Assert.True(NpgsqlRangeExtensions.Includes(ru, 1));
            Assert.False(NpgsqlRangeExtensions.Includes(ru, -1));
            var rl = new NpgsqlRange<int>(0, false, true, 0, false, false);
            Assert.False(NpgsqlRangeExtensions.Includes(rl, 1));
            Assert.True(NpgsqlRangeExtensions.Includes(rl, -1));
            var ul = new NpgsqlRange<int>(0, false, true, 0, false, true);
            Assert.True(NpgsqlRangeExtensions.Includes(ul, 1));
            Assert.True(NpgsqlRangeExtensions.Includes(ul, -1));
        }
    }
}
