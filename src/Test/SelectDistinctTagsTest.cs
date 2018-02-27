using Xunit;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models.DataViewModels;

namespace Test {

    public class SelectDistinctTagsTests {

        [Fact]
        public void SelectDistinctTags() {

            var repeatedTags = new List<Tag> {
                new Tag { Type = "Type A", Value = "Val A" },
                new Tag { Type = "Type B", Value = "Val B" },
                new Tag { Type = "Type A", Value = "Val C" },
                new Tag { Type = "Type C", Value = "Val A" },
                new Tag { Type = "Type A", Value = "Val A" },
                new Tag { Type = "Type C", Value = "Val A" },
            };

            var distinctTags = repeatedTags.Distinct().ToList();

            var expectedTags = new List<Tag> {
                new Tag { Type = "Type A", Value = "Val A" },
                new Tag { Type = "Type B", Value = "Val B" },
                new Tag { Type = "Type A", Value = "Val C" },
                new Tag { Type = "Type C", Value = "Val A" },
            };

            Assert.Equal(expectedTags.Count(), distinctTags.Count());
            for (var i = 0; i < expectedTags.Count(); i++) {
                Assert.Equal(expectedTags[i].Value, distinctTags[i].Value);
                Assert.Equal(expectedTags[i].Type, distinctTags[i].Type);
            }
        }
    }
}
