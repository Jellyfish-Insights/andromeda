using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models.DataViewModels;
using Test.Helpers;
using WebApp.Controllers;
using Xunit;
using Common;

namespace Test {

    public class MetaTagsTest {

        private AnalyticsPlatformSteps APS;

        public MetaTagsTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            APS = new AnalyticsPlatformSteps();
        }

        [Fact]
        public void SanitizeTags() {

            {
                var newName = " super cOOl   tag  ";
                var normalizedName = "super cool tag";
                var result = AbstractDBDataBackend.SanitizeTags(newName, true);
                Assert.Equal(normalizedName, result);
            }
            {
                var newName = " Pop/Nice  Tag ";
                var normalizedName = "Pop/Nice Tag";
                var result = AbstractDBDataBackend.SanitizeTags(newName, false);
                Assert.Equal(normalizedName, result);
            }
        }

        [Fact]
        public void MetaTagsEditing() {

            var controller = new TagController();
            var tags = controller.GetMetaTags().Where(x => x.Value.Count() > 0).OrderBy(x => x.Key).ToList();
            var firstType = tags[0];
            var firstTags = tags[0].Value.ToList();
            var updateName = "cool   tag ";
            var newName = " super cOOl  tag";
            var deleteName = firstTags[1].Value.Value;
            var edits = new TagEdits(){
                Type = firstType.Key,
                Edits = new Dictionary<string, TagEdit>(){
                    { firstTags[0].Value.Value, new TagEdit(){
                          Name = updateName,
                          Flag = EditType.Update,
                          UpdateDate = firstTags[0].Value.UpdateDate
                      }
                    },
                    { firstTags[1].Value.Value, new TagEdit(){
                          Flag = EditType.Delete,
                          UpdateDate = firstTags[1].Value.UpdateDate
                      }
                    },
                    { "_new", new TagEdit(){
                          Flag = EditType.New,
                          Name = newName
                      }
                    }
                }
            };
            var failedEdits = controller.EditMetaTags(edits);
            Assert.Empty(failedEdits.Edits);
            tags = controller.GetMetaTags().Where(x => x.Value.Count() > 0).OrderBy(x => x.Key).ToList();
            firstType = tags[0];
            Assert.DoesNotContain(firstType.Value, y => y.Value.Value == deleteName);
            Assert.Contains(firstType.Value, y => y.Value.Value == AbstractDBDataBackend.SanitizeTags(updateName, y.Key == Constants.GenericTag));
            Assert.Contains(firstType.Value, y => y.Value.Value == AbstractDBDataBackend.SanitizeTags(newName, y.Key == Constants.GenericTag));
        }
    }
}
