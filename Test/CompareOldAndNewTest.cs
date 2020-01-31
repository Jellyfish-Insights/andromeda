using Xunit;
using DataLakeModels.Models;
using DataLakeModels.Models.YouTube.Analytics;
using Jobs.Fetcher.YouTube.Helpers;

public class CompareOldAndNewTest {

    [Fact]
    public void CompareOldAndNewViewerPercentages() {

        var sets = new[] {
            (
                Modified.Updated,
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
            },
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.1 },
            }
            ), (
                Modified.Updated,
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
            },
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.2 },
            }
                ), (
                Modified.New,
                null,
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
            }
                ), (
                Modified.Equal,
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
            },
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
            }
                ), (
                Modified.Equal,
                new[] {
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
            },
                new[] {
                new ViewerPercentage() { Gender = "F", AgeGroup = "21", Value = 0.1 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "21", Value = 0.2 },
                new ViewerPercentage() { Gender = "M", AgeGroup = "34", Value = 0.4 },
            }
                ),
        };

        foreach (var set in sets) {
            Assert.Equal(set.Item1, DbWriter.compareOldAndNew(set.Item2, set.Item3));
        }
    }
}
