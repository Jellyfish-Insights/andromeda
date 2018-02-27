using System.Collections.Generic;
using ApplicationModels.Helpers;
using Xunit;

namespace Test {
    public class TopologicalSortTest {
        [Fact]
        public void TestSort() {
            var result = TopologicalSort.Sort(
                new HashSet<int>(new[] { 7, 5, 3, 8, 11, 2, 9, 10 }),
                new HashSet<(int, int)>(new[] {
                (7, 11),
                (7, 8),
                (5, 11),
                (3, 8),
                (3, 10),
                (11, 2),
                (11, 9),
                (11, 10),
                (8, 9)
            }));
            Assert.Equal(new[] { 7, 5, 11, 2, 3, 8, 9, 10 }, result);
        }

        [Fact]
        public void TestNoEdges() {
            var result = TopologicalSort.Sort(new HashSet<int>(new[] { 1, 2 }),
                                              new HashSet<(int, int)>());
            Assert.Equal(new[] { 1, 2 }, result);
        }
    }
}
