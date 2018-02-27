using System;
using System.Collections.Generic;
using ApplicationModels.Extensions;
using Xunit;

namespace Test {
    public class DictionaryExtensionTest {
        [Fact]
        public void TestNewElement() {
            var dict = new Dictionary<string, int>();
            dict.AddOrUpdate("foo", 10, current => current + 1);
            Assert.Equal(10, dict["foo"]);
        }

        [Fact]
        public void TestExistingElement() {
            var dict = new Dictionary<string, int>();
            dict.AddOrUpdate("foo", 10, current => current + 1);
            dict.AddOrUpdate("foo", 10, current => current + 1);
            Assert.Equal(11, dict["foo"]);
        }
    }
}
