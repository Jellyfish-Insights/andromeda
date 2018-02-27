using System;
using System.Linq;
using System.Collections.Generic;
using Test.Helpers;
using Xunit;
using Common;
using DataLakeModels;
using DataLakeModels.Models.YouTube.Data;

public class TableOperationsTest {

    private YouTubeDataSteps YDS;

    public TableOperationsTest() {
        DatabaseReset.Drop(Databases.LakeYouTubeData);
        DatabaseReset.Migrate(Databases.LakeYouTubeData);
        YDS = new YouTubeDataSteps();
    }

    [Fact]
    public void TestDeleteFromTable() {
        var videos = new List<(string VideoTitle, List<(DateTime ValidityStart, DateTime ValidityEnd)>)>(){
            ("a-yt-video",
             new List<(DateTime ValidityStart, DateTime ValidityEnd)>(){
                (new DateTime(2018, 1, 1), new DateTime(2018, 1, 3)),
                (new DateTime(2018, 1, 2), DateTime.MaxValue)
            })
        };
        YDS.ThereAreVideosWithManyVersions(videos);

        using (var context = new DataLakeYouTubeDataContext()) {
            Assert.Equal(2, context.Videos.Count());
            var(schema, table, keys) = ContextIntrospection.GetDatabaseInfo(context, typeof(Video));

            TableOperations.DeleteFromTable(YearDatabase.DataLakeDatabase, schema, table);
            Assert.Empty(context.Videos);
        }
    }
}
