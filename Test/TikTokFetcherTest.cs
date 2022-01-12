/*using System;
   using System.Linq;
   using System.Collections.Generic;
   using DataLakeModels;
   using DataLakeModels.Models.TikTok;
   using Test.Helpers;
   using Xunit;
   using Andromeda.Common;

   namespace Test {

    public class TikTokFetcherTest {

        private TikTokDataSteps TTDS;

        public TikTokFetcherTest() {
            DatabaseReset.Drop(Databases.LakeTikTok);
            DatabaseReset.Migrate(Databases.LakeTikTok);

            TTDS = new TikTokDataSteps();
        }

        [Fact]
        public void TikTokPostsDbWriteTest() {
            using (var context = new DataLakeTikTokContext()) {

                var someLakeAuthors = new[] {
                    new Author() { Id = "123" }
                };
                TTDS.SomeAuthorsWereFound(someLakeAuthors, context);

                var someLakePosts = new[] {
                    new Post() { Id = "abc", Description = "First Post", AuthorId = "123" },
                    new Post() { Id = "def", Description = "Second Post", AuthorId = "123" },
                    new Post() { Id = "ghi", Description = "Third Post", AuthorId = "123" }
                };
                TTDS.SomePostsWereFound(someLakePosts, context);

                var firstDateFirstTime = new DateTime(2021, 1, 1, 0, 0, 0);
                var firstDateSecondTime = new DateTime(2021, 1, 1, 8, 0, 0);
                var firstDateThirdTime = new DateTime(2021, 1, 1, 16, 0, 0);
                var secondDateFirstTime = new DateTime(2021, 1, 2, 0, 0, 0);
                var secondDateSecondTime = new DateTime(2021, 1, 2, 8, 0, 0);
                var firstDate = new DateTime(2021, 1, 1);
                var secondDate = new DateTime(2021, 1, 2);
                var thirdDate = new DateTime(2021, 1, 3);
                var now = thirdDate;

                var someMetrics = new[] {
                    //First values
                    new PostStats() { DiggCount = 10, ShareCount = 5, CommentCount = 3, PlayCount = 20, PostId = "abc", EventDate = firstDate, ValidityStart = firstDateFirstTime, ValidityEnd = DateTime.MaxValue },
                    new PostStats() { DiggCount = 20, ShareCount = 10, CommentCount = 6, PlayCount = 40, PostId = "def", EventDate = firstDate, ValidityStart = firstDateFirstTime, ValidityEnd = DateTime.MaxValue },

                    //Same values, same date
                    new PostStats() { DiggCount = 10, ShareCount = 5, CommentCount = 3, PlayCount = 20, PostId = "abc", EventDate = firstDate, ValidityStart = firstDateSecondTime, ValidityEnd = DateTime.MaxValue },
                    new PostStats() { DiggCount = 20, ShareCount = 10, CommentCount = 6, PlayCount = 40, PostId = "def", EventDate = firstDate, ValidityStart = firstDateSecondTime, ValidityEnd = DateTime.MaxValue },

                    //New values, same date
                    new PostStats() { DiggCount = 12, ShareCount = 5, CommentCount = 4, PlayCount = 20, PostId = "abc", EventDate = firstDate, ValidityStart = firstDateThirdTime, ValidityEnd = DateTime.MaxValue },
                    new PostStats() { DiggCount = 20, ShareCount = 12, CommentCount = 6, PlayCount = 45, PostId = "def", EventDate = firstDate, ValidityStart = firstDateThirdTime, ValidityEnd = DateTime.MaxValue },

                    //Same values, different date
                    new PostStats() { DiggCount = 12, ShareCount = 5, CommentCount = 4, PlayCount = 20, PostId = "abc", EventDate = secondDate, ValidityStart = secondDateFirstTime, ValidityEnd = DateTime.MaxValue },
                    new PostStats() { DiggCount = 20, ShareCount = 12, CommentCount = 6, PlayCount = 45, PostId = "def", EventDate = secondDate, ValidityStart = secondDateFirstTime, ValidityEnd = DateTime.MaxValue },

                    //New values, same date
                    new PostStats() { DiggCount = 14, ShareCount = 6, CommentCount = 5, PlayCount = 21, PostId = "abc", EventDate = secondDate, ValidityStart = secondDateSecondTime, ValidityEnd = DateTime.MaxValue },
                    new PostStats() { DiggCount = 24, ShareCount = 16, CommentCount = 7, PlayCount = 50, PostId = "def", EventDate = secondDate, ValidityStart = secondDateSecondTime, ValidityEnd = DateTime.MaxValue },
                    new PostStats() { DiggCount = 30, ShareCount = 15, CommentCount = 9, PlayCount = 60, PostId = "ghi", EventDate = secondDate, ValidityStart = secondDateSecondTime, ValidityEnd = DateTime.MaxValue }
                };

                TTDS.SomePostsStatsWereFound(someMetrics, context);

                var currentStats = context.Stats.Where(x => x.ValidityStart <= now && now < x.ValidityEnd);
                Assert.Equal(5, currentStats.Count());
                Assert.Equal(2, currentStats.Where(x => x.EventDate == firstDate).Count());
                Assert.Equal(3, currentStats.Where(x => x.EventDate == secondDate).Count());

                long totalPlayCountFirstPost = 0;
                long totalPlayCountSecondPost = 0;
                long totalPlayCountThirdPost = 0;
                foreach (var eachCount in currentStats.Select(x => new { PlayCount = x.PlayCount, PostId = x.PostId })) {
                    switch (eachCount.PostId) {
                        case "abc":
                            totalPlayCountFirstPost += eachCount.PlayCount;
                            break;
                        case "def":
                            totalPlayCountSecondPost += eachCount.PlayCount;
                            break;
                        case "ghi":
                            totalPlayCountThirdPost += eachCount.PlayCount;
                            break;
                        default:
                            continue;
                    }
                }

                Assert.Equal(21, totalPlayCountFirstPost);
                Assert.Equal(50, totalPlayCountSecondPost);
                Assert.Equal(60, totalPlayCountThirdPost);
            }
        }
    }
   }
 */
