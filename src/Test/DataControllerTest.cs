using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models;
using ApplicationModels.Models.DataViewModels;
using Test.Helpers;
using WebApp.Controllers;
using Newtonsoft.Json;
using Xunit;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Test {

    public class DataControllerTest {

        private AnalyticsPlatformSteps APS;
        private ContentDataController contentController;
        private MarketingDataController marketingController;

        private TrendingController trendingApi;

        public DataControllerTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            APS = new AnalyticsPlatformSteps();
            contentController = new ContentDataController(new DBContentDataBackend());
            marketingController = new MarketingDataController(new DBMarketingDataBackend());
            trendingApi = new TrendingController(
                new ApplicationModels.ApplicationDbContext(),
                contentController,
                new DBContentDataBackend()
                );
        }

        [Fact]
        public void VideoListFilterByTitle() {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540 },
            };
            APS.TheseVideosExist(someVideos);

            APS.ApApTransformationsHaveRun();

            var filter = new Tag[] { new Tag { Type = "search", Value = "The Population" } };

            var ans = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Single(ans);
            Assert.Equal(someVideos[0].Title, ans[0].Title);
        }

        [Fact]
        public void VideoListFilterByPlatformAndTitle() {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67, Platform = Constants.FacebookSource },
                new SourceVideo { Title = "The Population Boom", VideoLength = 67, Platform = Constants.YouTubeSource },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540, Platform = Constants.FacebookSource },
            };
            APS.TheseVideosExist(someVideos);

            APS.ApApTransformationsHaveRun();

            var filter = new Tag[] { new Tag { Type = "search", Value = "The Population" },
                                     new Tag { Type = "platform", Value = Constants.FacebookSource } };

            var ans = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Single(ans);
            Assert.Equal(someVideos[0].Title, ans[0].Title);
            Assert.Equal(someVideos[0].Id, ans[0].Sources[0].SourceObjects[0].SourceId);
        }

        [Fact]
        public void VideoListFilterByLength() {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 1 },
                new SourceVideo { Title = "The Terror Jack", VideoLength = 181 },
                new SourceVideo { Title = "The Fall Nightmare Long", VideoLength = 361 },
            };
            APS.TheseVideosExist(someVideos);

            APS.ApApTransformationsHaveRun();

            var filter = new Tag[] { new Tag { Type = "Length", Value = "0-2min" } };
            var videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Single(videos);
            Assert.Equal(someVideos[0].Title, videos[0].Title);

            filter = new Tag[] { new Tag { Type = "Length", Value = "3-5min" } };
            videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Single(videos);
            Assert.Equal(someVideos[1].Title, videos[0].Title);

            filter = new Tag[] { new Tag { Type = "Length", Value = "6-15min" } };
            videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Single(videos);
            Assert.Equal(someVideos[2].Title, videos[0].Title);
        }

        [Fact]
        public void NoVideosAvailable() {

            APS.ApApTransformationsHaveRun();

            var ans = contentController.ChartData("Views", "Medium", new DateTime(2018, 2, 1), new DateTime(2018, 2, 5), new Tag[0]);

            Assert.Equal(0, ans.TotalOnPeriod);
            Assert.Empty(ans.TotalPerGroup);
            Assert.Empty(ans.TotalTimeSeries.Values);
            Assert.Empty(ans.Values);
            Assert.Equal(5, ans.Dates.Count());
        }

        [Fact]
        public void NoMetricValuesAvailable() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540 },
            };
            APS.TheseVideosExist(someVideos);

            APS.ApApTransformationsHaveRun();

            var ans = contentController.ChartData("Views", "Length", new DateTime(2018, 2, 1), new DateTime(2018, 2, 5), new Tag[0]);

            Assert.Equal(0, ans.TotalOnPeriod);
            Assert.Empty(ans.TotalPerGroup);
            Assert.Empty(ans.TotalTimeSeries.Values);
            Assert.Empty(ans.Values);
            Assert.Equal(5, ans.Dates.Count());
        }

        [Fact]
        public void FailedVideoEditing() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The Fall Nightmare", VideoLength = 67 },
            };
            APS.TheseVideosExist(someVideos);

            APS.ApApTransformationsHaveRun();

            var newTitle = "The Population Crisis";
            var filter = new Tag[] { new Tag { Type = "Length", Value = "0-2min" } };
            var videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Equal(2, videos.Count());
            var edits = new Dictionary<string, VideoEdit>() {
                { videos[0].Id, new VideoEdit() {
                      UpdateDate = DateTime.UtcNow,
                      Title = newTitle,
                      MetaTags = new Dictionary<string, string>() {
                          // Update Tag
                          { "Length", "6-15min" },
                          // Add new Tag
                          { "Topic", "Culture" },
                      },
                      AddedGenericTags = new List<string>() { "Fresh Tag" },
                      Flag = EditType.Update,
                  }
                },
                { videos[1].Id, new VideoEdit() {
                      UpdateDate = videos[1].UpdateDate,
                      Title = newTitle,
                      MetaTags = new Dictionary<string, string>() {
                          // Update Tag
                          { "Length", "6-15min" },
                      },
                      Flag = EditType.Update,
                  }
                },
            };
            var output = contentController.EditVideos(new VideoEdits { Edits = edits });
            Assert.Single(output.Edits);
            Assert.Equal(videos[0].Id, output.Edits.Keys.ToArray()[0]);
            videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Equal(1, videos.Count());
        }

        [Fact]
        public void VideoAssociationEditing() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67, Platform = Constants.FacebookSource },
                new SourceVideo { Title = "The Population Nightmare", VideoLength = 67, Platform = Constants.FacebookSource },
                new SourceVideo { Title = "The Population Nightmare", VideoLength = 67, Platform = Constants.YouTubeSource }
            };
            APS.TheseVideosExist(someVideos);

            var someCampaigns = new[] {
                (new SourceCampaign() { Title = "Views_Christmas_23.12.2019", Platform = Constants.FacebookSource }
                 , new[] {
                    (new SourceAdSet() { Title = "YEAR Liberals - Film bufs" }
                     , new[] {
                        new SourceAd() { Title = "Views 10" },
                        new SourceAd() { Title = "Traffic 10" },
                    })
                }),
                (new SourceCampaign() { Title = "Views_New Year_23.12.2019", Platform = Constants.FacebookSource }
                 , new[] {
                    (new SourceAdSet() { Title = "YEAR Conservatives" }
                     , new[] {
                        new SourceAd() { Title = "Views 11", VideoId = someVideos[0].Id },
                        new SourceAd() { Title = "Traffic 11" },
                    }),
                    (new SourceAdSet() { Title = "Fans of FEE" }
                     , new[] {
                        new SourceAd() { Title = "Views 12" },
                        new SourceAd() { Title = "Traffic 12" },
                    }),
                })
            };

            APS.TheseCampaignsExist(someCampaigns);
            APS.ApApTransformationsHaveRun();

            var controller = contentController;
            var videos = controller.GetVideoList().OrderBy(x => x.Id).ToList();
            Assert.Equal(2, videos[1].Sources.Count());
            var campaigns = controller.GetUnassociatedSources(SourceObjectType.Campaign).OrderBy(x => x.SourceId).ToList();

            // Create new tags and update Length tag

            var edits = new Dictionary<string, VideoEdit>() {
                { videos[1].Id, new VideoEdit() {
                      UpdateDate = videos[1].UpdateDate,
                      // Add new generic Tag
                      AddedCampaigns = new List<string>() { campaigns[0].SourceId },
                      RemovedVideos = new List<string>() { someVideos[2].Id },
                      Flag = EditType.Update,
                  }
                },
                { videos[0].Id, new VideoEdit() {
                      UpdateDate = videos[0].UpdateDate,
                      // Add new generic Tag
                      RemovedCampaigns = new List<string>() { someCampaigns[1].Item1.Id },
                      Flag = EditType.Update,
                  }
                },
                { "_new", new VideoEdit() {
                      Title = "Brand new Video",
                      MetaTags = new Dictionary<string, string>() {
                          // Add Tag
                          { "Topic", "Culture" }
                      },
                      AddedGenericTags = new List<string>() { "Fresh Tag" },
                      AddedVideos = new List<string>() { someVideos[2].Id },
                      Flag = EditType.New,
                  }
                },
            };

            var failedEdits = controller.EditVideos(new VideoEdits { Edits = edits });
            Assert.Empty(failedEdits.Edits);

            campaigns = controller.GetUnassociatedSources(SourceObjectType.Campaign);
            Assert.Single(campaigns);
            Assert.Equal(someCampaigns[1].Item1.Id, campaigns[0].SourceId);

            videos = controller.GetVideoList().OrderBy(x => x.Id).ToList();
            Assert.Single(videos[0].Sources.SelectMany(x => x.SourceObjects));
            Assert.Equal(2, videos[1].Sources.SelectMany(x => x.SourceObjects).Count());

            var filter = new Tag[] { new Tag { Type = "Topic", Value = "Culture" } };
            videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
            Assert.Single(videos);
        }

        [Fact]
        public void VideoEditing() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The Population Nightmare", VideoLength = 67 },
            };
            APS.TheseVideosExist(someVideos);

            APS.ApApTransformationsHaveRun();

            var videos = contentController.GetVideoList("[]");
            var filter = new Tag[] { new Tag { Type = "Generic", Value = "Fresh Tag" }, new Tag { Type = "Topic", Value = "Culture" } };

            // Create new tags and update Length tag
            {
                var newTitle = "The Population Crisis";
                var edits = new Dictionary<string, VideoEdit>() {
                    { videos[0].Id, new VideoEdit() {
                          UpdateDate = videos[0].UpdateDate,
                          Title = newTitle,
                          MetaTags = new Dictionary<string, string>() {
                              // Update Tag
                              { "Length", "6-15min" },
                              // Add new Tag
                              { "Topic", "Culture" },
                          },
                          // Add new generic Tag
                          AddedGenericTags = new List<string>() { "Fresh Tag" },
                          Flag = EditType.Update,
                      }
                    },
                    { videos[1].Id, new VideoEdit() {
                          UpdateDate = videos[1].UpdateDate,
                          Archive = true,
                          Flag = EditType.Update,
                      }
                    }
                };

                var failedEdits = contentController.EditVideos(new VideoEdits { Edits = edits });
                Assert.Empty(failedEdits.Edits);

                var allVideos = contentController.GetVideoList(archive: ArchiveMode.All);
                Assert.Equal(2, allVideos.Count());

                videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
                Assert.Single(videos);
                Assert.Equal(3, videos[0].Tags.Count());
                Assert.Equal(newTitle, videos[0].Title);
            }

            // Remove Tags leaving only the length tag
            {
                var edits = new Dictionary<string, VideoEdit>() {
                    { videos[0].Id, new VideoEdit() {
                          UpdateDate = videos[0].UpdateDate,
                          MetaTags = new Dictionary<string, string>() {
                              // Remove Tag
                              { "Topic", null }
                          },
                          RemovedGenericTags = new List<string>() { "Fresh Tag" },
                          Flag = EditType.Update,
                      }
                    },
                };

                var failedEdits = contentController.EditVideos(new VideoEdits { Edits = edits });
                Assert.Empty(failedEdits.Edits);

                videos = contentController.GetVideoList("[]");
                Assert.Single(videos[0].Tags);

                videos = contentController.GetVideoList(JsonConvert.SerializeObject(filter));
                Assert.Empty(videos);
            }
        }

        [Fact]
        public void DataRangeHandlingOnMetrics() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom" },
            };
            APS.TheseVideosExist(someVideos);

            var someMetrics = new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 34 },
                new SourceVideoMetric { ViewCount = 10 },
            };
            APS.ThisVideoHasSomeMetrics("The Population Boom", someMetrics, new DateTime(2018, 2, 3));

            someMetrics = new[] {
                new SourceVideoMetric { ViewCount = 23 },
                new SourceVideoMetric { ViewCount = 40 },
            };
            APS.ThisVideoHasSomeMetrics("The Population Boom", someMetrics, new DateTime(2018, 2, 7));

            APS.ApApTransformationsHaveRun();

            var cases = new[] {
                (new DateTime(2018, 2, 1), new DateTime(2018, 2, 10), new[] { 0, 0, 34, 10, 0, 0, 23, 40, 0, 0 }),
                (new DateTime(2018, 2, 4), new DateTime(2018, 2, 7), new[] { 10, 0, 0, 23 }),
            };

            foreach (var(from, to, expected) in cases) {
                var ans = contentController.ChartData("Views", "Length", from, to, new Tag[0]);

                for (var i = 0; i < expected.Count(); i++) {
                    Assert.Equal(expected[i], ans.TotalTimeSeries.Values[i]);
                }
            }
        }

        [Fact]
        public void ContentSumMetric() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540 },
            };
            APS.TheseVideosExist(someVideos);

            var someMetrics = new[] {
                new SourceVideoMetric { ViewCount = 34 },
                new SourceVideoMetric { ViewCount = 10 },
            };
            APS.ThisVideoHasSomeMetrics("The Population Boom", someMetrics, new DateTime(2018, 2, 20));

            someMetrics = new[] {
                new SourceVideoMetric { ViewCount = 12 },
                new SourceVideoMetric { ViewCount = 22 },
            };
            APS.ThisVideoHasSomeMetrics("The True Meaning of Patriotism", someMetrics, new DateTime(2018, 2, 21));

            APS.ApApTransformationsHaveRun();

            var ans = contentController.ChartData("Views", "Length", new DateTime(2018, 2, 19), new DateTime(2018, 2, 23), new Tag[0]);

            var daily2min = new[] { 0, 34, 10, 0, 0 };
            var daily10min = new[] { 0, 0, 12, 22, 0 };
            var dailyTotals = daily2min.Zip(daily10min, (x, y) => x + y).ToArray();

            var totals = new Dictionary<string, int[]> {
                { "0-2min", daily2min },
                { "6-15min", daily10min },
            };
            var grandTotal = dailyTotals.Sum();

            var dates = new[] { "20180219", "20180220", "20180221", "20180222", "20180223" };
            var days = dates.Count();

            Assert.Equal(totals.Count(), ans.TotalPerGroup.Count());
            foreach (var(key, value) in totals) {
                Assert.Equal(value.Sum(), ans.TotalPerGroup[key]);
            }
            Assert.Equal(grandTotal, ans.TotalOnPeriod);
            Assert.Equal(days, ans.Dates.Count());

            var ans2min = ans.Values.Where(x => x.GroupName == "0-2min").Single();
            var ans10min = ans.Values.Where(x => x.GroupName == "6-15min").Single();

            for (var i = 0; i < days; i++) {
                Assert.Equal(dates[i], ans.Dates[i]);
                Assert.Equal(dailyTotals[i], ans.TotalTimeSeries.Values[i]);
                Assert.Equal(daily2min[i], ans2min.Values[i]);
                Assert.Equal(daily10min[i], ans10min.Values[i]);
            }
        }

        [Fact]
        public void ContentDemographicsMetric() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540 },
            };
            APS.TheseVideosExist(someVideos);

            var someMetrics = new[] {
                (new DateTime(2018, 2, 20), new DateTime(2018, 2, 23), new[] {
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "M", TotalViewCount = 30 },
                    new SourceVideoDemographicMetric { AgeGroup = "18-24", Gender = "M", TotalViewCount = 300 },
                    new SourceVideoDemographicMetric { AgeGroup = "25-34", Gender = "M", TotalViewCount = 1000 },
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "F", TotalViewCount = 10 },
                    new SourceVideoDemographicMetric { AgeGroup = "18-24", Gender = "F", TotalViewCount = 300 },
                    new SourceVideoDemographicMetric { AgeGroup = "35-44", Gender = "F", TotalViewCount = 100 },
                }),
                (new DateTime(2018, 2, 23), DateTime.MaxValue, new[] {
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "M", TotalViewCount = 35 },
                    new SourceVideoDemographicMetric { AgeGroup = "18-24", Gender = "M", TotalViewCount = 250 },
                    new SourceVideoDemographicMetric { AgeGroup = "25-34", Gender = "M", TotalViewCount = 1500 },
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "F", TotalViewCount = 12 },
                    new SourceVideoDemographicMetric { AgeGroup = "18-24", Gender = "F", TotalViewCount = 400 },
                    new SourceVideoDemographicMetric { AgeGroup = "35-44", Gender = "F", TotalViewCount = 110 },
                }),
            };
            APS.ThisVideoHasSomeDemographicsMetrics("The Population Boom", someMetrics);

            someMetrics = new[] {
                (new DateTime(2018, 2, 18), new DateTime(2018, 2, 22), new[] {
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "M", TotalViewCount = 20 },
                    new SourceVideoDemographicMetric { AgeGroup = "25-34", Gender = "M", TotalViewCount = 300 },
                    new SourceVideoDemographicMetric { AgeGroup = "45-54", Gender = "M", TotalViewCount = 700 },
                    new SourceVideoDemographicMetric { AgeGroup = "18-24", Gender = "F", TotalViewCount = 70 },
                    new SourceVideoDemographicMetric { AgeGroup = "35-44", Gender = "F", TotalViewCount = 800 },
                    new SourceVideoDemographicMetric { AgeGroup = "45-54", Gender = "F", TotalViewCount = 300 },
                }),
                (new DateTime(2018, 2, 22), DateTime.MaxValue, new[] {
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "M", TotalViewCount = 20 },
                    new SourceVideoDemographicMetric { AgeGroup = "25-34", Gender = "M", TotalViewCount = 55 },
                    new SourceVideoDemographicMetric { AgeGroup = "45-54", Gender = "M", TotalViewCount = 120 },
                    new SourceVideoDemographicMetric { AgeGroup = "13-17", Gender = "F", TotalViewCount = 10 },
                    new SourceVideoDemographicMetric { AgeGroup = "25-34", Gender = "F", TotalViewCount = 400 },
                    new SourceVideoDemographicMetric { AgeGroup = "35-44", Gender = "F", TotalViewCount = 5 },
                }),
            };
            APS.ThisVideoHasSomeDemographicsMetrics("The True Meaning of Patriotism", someMetrics);

            APS.ApApTransformationsHaveRun();

            var ans = contentController.GetDemographics("Demographics View Count", "Length", new DateTime(2018, 2, 20), new DateTime(2018, 2, 24), new Tag[0]);

            var totals = new Dictionary<string, int> {
                { "0-2min", 35 + 250 + 1500 + 12 + 400 + 110 },
                { "6-15min", 20 + 55 + 120 + 10 + 400 + 5 },
                { Constants.GrandTotalName, 35 + 250 + 1500 + 12 + 400 + 110 + 20 + 55 + 120 + 10 + 400 + 5 },
            };

            Assert.Equal(totals.Count(), ans.Values.Count());
            foreach (var value in ans.Values) {
                Assert.Equal(totals[value.GroupName], value.Total);
            }
        }

        [Fact]
        public void ContentImpressionsMetric() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540 },
            };
            APS.TheseVideosExist(someVideos);

            var someMetrics = new[] {
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 23, EndDate = new DateTime(2018, 2, 7, 9, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 34, EndDate = new DateTime(2018, 2, 9, 12, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 100, EndDate = new DateTime(2018, 2, 9, 16, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 8, EndDate = new DateTime(2018, 2, 9, 20, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 23, EndDate = new DateTime(2018, 2, 10, 3, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 300, EndDate = new DateTime(2018, 2, 13, 12, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 21, EndDate = new DateTime(2018, 2, 13, 19, 0, 0) },
            };
            APS.ThisVideoHasSomeDeltaEncodedMetrics("The Population Boom", someMetrics, new DateTime(2018, 2, 6, 8, 17, 35));

            someMetrics = new[] {
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 48, EndDate = new DateTime(2018, 2, 6, 9, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 24, EndDate = new DateTime(2018, 2, 7, 12, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 89, EndDate = new DateTime(2018, 2, 8, 16, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 230, EndDate = new DateTime(2018, 2, 10, 4, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 245, EndDate = new DateTime(2018, 2, 10, 23, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 79, EndDate = new DateTime(2018, 2, 12, 12, 0, 0) },
                new SourceDeltaEncodedVideoMetric { ImpressionsCount = 110, EndDate = new DateTime(2018, 2, 12, 19, 0, 0) },
            };
            APS.ThisVideoHasSomeDeltaEncodedMetrics("The True Meaning of Patriotism", someMetrics, new DateTime(2018, 2, 6, 5, 0, 0));

            APS.ApApTransformationsHaveRun();

            var ans = contentController.ChartData("Impressions", "Length", new DateTime(2018, 2, 5), new DateTime(2018, 2, 14), new Tag[0]);

            var daily2min = new[] { 0, 23, 34, 0, 131, 300, 0, 0, 21, 0 };
            var daily10min = new[] { 0, 72, 89, 230, 0, 324, 0, 110, 0, 0 };
            var dailyTotals = daily2min.Zip(daily10min, (x, y) => x + y).ToArray();

            var totals = new Dictionary<string, int[]> {
                { "0-2min", daily2min },
                { "6-15min", daily10min },
            };
            var grandTotal = dailyTotals.Sum();

            var days = dailyTotals.Count();

            Assert.Equal(totals.Count(), ans.TotalPerGroup.Count());
            foreach (var(key, value) in totals) {
                Assert.Equal(value.Sum(), ans.TotalPerGroup[key]);
            }
            Assert.Equal(grandTotal, ans.TotalOnPeriod);
            Assert.Equal(days, ans.Dates.Count());

            var ans2min = ans.Values.Where(x => x.GroupName == "0-2min").Single();
            var ans10min = ans.Values.Where(x => x.GroupName == "6-15min").Single();

            for (var i = 0; i < days; i++) {
                Assert.Equal(dailyTotals[i], ans.TotalTimeSeries.Values[i]);
                Assert.Equal(daily2min[i], ans2min.Values[i]);
                Assert.Equal(daily10min[i], ans10min.Values[i]);
            }
        }

        [Fact]
        public void MarketingAverageMetric() {

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
                new SourceVideo { Title = "The True Meaning of Patriotism", VideoLength = 540 },
            };
            APS.TheseVideosExist(someVideos);

            var someCampaigns = new[] {
                (new SourceCampaign() { Title = "OoF_Pop Boom_20.02.2018" }, new[] {
                    (new SourceAdSet() { Title = "YEAR: Liberals" }, new[] {
                        new SourceAd() { Title = "Views 1" },
                        new SourceAd() { Title = "Traffic 1" },
                    }),
                    (new SourceAdSet() { Title = "YEAR: Conservatives v1" }, new[] {
                        new SourceAd() { Title = "Views 2" },
                        new SourceAd() { Title = "Traffic 2" },
                    }),
                    (new SourceAdSet() { Title = "Fans of FEE" }, new[] {
                        new SourceAd() { Title = "Views 3" },
                        new SourceAd() { Title = "Traffic 3" },
                    }),
                }),
                (new SourceCampaign() { Title = "CSS_Fake_19.02.2018" }, new[] {
                    (new SourceAdSet() { Title = "YEAR: Liberals - v1" }, new[] {
                        new SourceAd() { Title = "Views 4" },
                        new SourceAd() { Title = "Traffic 4" },
                    }),
                    (new SourceAdSet() { Title = "YEAR: Conservatives" }, new[] {
                        new SourceAd() { Title = "Views 5" },
                        new SourceAd() { Title = "Traffic 5" },
                    }),
                    (new SourceAdSet() { Title = "Fans of SFL" }, new[] {
                        new SourceAd() { Title = "Views 6" },
                        new SourceAd() { Title = "Traffic 6" },
                    }),
                }),
                (new SourceCampaign() { Title = "Lead Gen_Boom_17.01.2018" }, new[] {
                    (new SourceAdSet() { Title = "YEAR: Liberals" }, new[] {
                        new SourceAd() { Title = "Views 7" },
                        new SourceAd() { Title = "Traffic 7" },
                    }),
                    (new SourceAdSet() { Title = "YEAR: Conservatives v2" }, new[] {
                        new SourceAd() { Title = "Views 8" },
                        new SourceAd() { Title = "Traffic 8" },
                    }),
                    (new SourceAdSet() { Title = "Fans of FEE - worldwide" }, new[] {
                        new SourceAd() { Title = "Views 9" },
                        new SourceAd() { Title = "Traffic 9" },
                    }),
                }),
                (new SourceCampaign() { Title = "Views_Christmas_23.12.2019" }, new[] {
                    (new SourceAdSet() { Title = "YEAR Liberals - Film bufs" }, new[] {
                        new SourceAd() { Title = "Views 10" },
                        new SourceAd() { Title = "Traffic 10" },
                    }),
                    (new SourceAdSet() { Title = "YEAR Conservatives" }, new[] {
                        new SourceAd() { Title = "Views 11" },
                        new SourceAd() { Title = "Traffic 11" },
                    }),
                    (new SourceAdSet() { Title = "Fans of FEE" }, new[] {
                        new SourceAd() { Title = "Views 12" },
                        new SourceAd() { Title = "Traffic 12" },
                    }),
                }),
            };
            APS.TheseCampaignsExist(someCampaigns);

            APS.TheseAdsReferenceThisVideo(new[] { "Views 2", "Traffic 3", "Views 9" }, "The Population Boom");
            APS.TheseAdsReferenceThisVideo(new[] { "Views 4", "Views 5", "Traffic 7" }, "The True Meaning of Patriotism");

            var someAdMetrics = new[] {
                ("Views 1", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 20, CostPerView = 0.34 },
                    new SourceAdMetric() { Views = 100, CostPerView = 0.28 },
                }),
                ("Traffic 1", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 34, CostPerView = 0.45 },
                    new SourceAdMetric() { Views = 21, CostPerView = 0.23 },
                }),
                ("Views 2", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 45, CostPerView = 0.34 },
                    new SourceAdMetric() { Views = 567, CostPerView = 0.36 },
                }),
                ("Traffic 2", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 234, CostPerView = 0.12 },
                    new SourceAdMetric() { Views = 23, CostPerView = 0.23 },
                }),
                ("Views 3", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 46, CostPerView = 0.31 },
                    new SourceAdMetric() { Views = 67, CostPerView = 0.31 },
                }),
                ("Traffic 3", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 24, CostPerView = 0.34 },
                    new SourceAdMetric() { Views = 57, CostPerView = 0.56 },
                }),
                ("Views 4", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 68, CostPerView = 0.78 },
                    new SourceAdMetric() { Views = 325, CostPerView = 0.45 },
                }),
                ("Traffic 4", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 57, CostPerView = 0.23 },
                    new SourceAdMetric() { Views = 75, CostPerView = 0.12 },
                }),
                ("Views 5", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 56, CostPerView = 0.23 },
                    new SourceAdMetric() { Views = 235, CostPerView = 0.35 },
                }),
                ("Traffic 5", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 46, CostPerView = 0.12 },
                    new SourceAdMetric() { Views = 367, CostPerView = 0.45 },
                }),
                ("Views 6", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 235, CostPerView = 0.19 },
                    new SourceAdMetric() { Views = 23, CostPerView = 0.23 },
                }),
                ("Traffic 6", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 46, CostPerView = 0.45 },
                    new SourceAdMetric() { Views = 124, CostPerView = 0.44 },
                }),
                ("Views 7", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 24, CostPerView = 0.23 },
                    new SourceAdMetric() { Views = 125, CostPerView = 0.12 },
                }),
                ("Traffic 7", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 54, CostPerView = 0.24 },
                    new SourceAdMetric() { Views = 8, CostPerView = 0.46 },
                }),
                ("Views 8", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 5, CostPerView = 0.56 },
                    new SourceAdMetric() { Views = 345, CostPerView = 0.13 },
                }),
                ("Traffic 8", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 23, CostPerView = 0.34 },
                    new SourceAdMetric() { Views = 89, CostPerView = 0.33 },
                }),
                ("Views 9", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 23, CostPerView = 0.32 },
                    new SourceAdMetric() { Views = 56, CostPerView = 0.34 },
                }),
                ("Traffic 9", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 30, CostPerView = 0.34 },
                    new SourceAdMetric() { Views = 567, CostPerView = 0.12 },
                }),
                ("Views 10", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 23, CostPerView = 0.21 },
                    new SourceAdMetric() { Views = 573, CostPerView = 0.32 },
                }),
                ("Traffic 10", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 98, CostPerView = 0.23 },
                    new SourceAdMetric() { Views = 78, CostPerView = 0.34 },
                }),
                ("Views 11", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 678, CostPerView = 0.56 },
                    new SourceAdMetric() { Views = 788, CostPerView = 0.36 },
                }),
                ("Traffic 11", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 547, CostPerView = 0.35 },
                    new SourceAdMetric() { Views = 34, CostPerView = 0.14 },
                }),
                ("Views 12", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 544, CostPerView = 0.25 },
                    new SourceAdMetric() { Views = 23, CostPerView = 0.27 },
                }),
                ("Traffic 12", new DateTime(2018, 1, 12), new[] {
                    new SourceAdMetric() { Views = 12, CostPerView = 0.13 },
                    new SourceAdMetric() { Views = 235, CostPerView = 0.31 },
                }),
            };
            foreach (var(adTitle, date, metrics) in someAdMetrics) {
                APS.ThisAdHasSomeMetrics(adTitle, metrics, date);
            }

            APS.ApApTransformationsHaveRun();

            var ans = marketingController.ChartData("Cost per View", null, new DateTime(2018, 1, 11), new DateTime(2018, 1, 14), new Tag[0]);

            // Note that only campaigns linked to a selected application video are considered.
            var grandTotal = 0.343775474956822;
            var totals = new Dictionary<string, double> {
                { "Liberals", 0.394757142857143 },
                { "Conservatives", 0.329682136045772 },
                { "None", 0.322041800643087 },
            };

            Assert.Equal(grandTotal, ans.TotalOnPeriod, 5);
            Assert.Equal(totals.Count(), ans.TotalPerGroup.Count());
            foreach (var(key, value) in totals) {
                Assert.Equal(value, ans.TotalPerGroup[key], 5);
            }
        }

        [Fact]
        public void VideoWithMultipleMetaTag() {
            /**
               Test added to check bug that happened when video was associated to mulitple meta tags.
             */

            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67 },
            };
            APS.TheseVideosExist(someVideos);

            var someMetrics = new[] {
                new SourceVideoMetric { ViewCount = 34 },
                new SourceVideoMetric { ViewCount = 10 },
            };
            APS.ThisVideoHasSomeMetrics("The Population Boom", someMetrics, new DateTime(2018, 2, 20));

            APS.ApApTransformationsHaveRun();

            var metaTagUpdate = new Dictionary<string, string>() { { "Topic", "Culture" } };
            APS.VideoHasMetaTagsUpdated("The Population Boom", metaTagUpdate);

            var metricList = contentController.GetMetricList("20180101", "20190101");

            Assert.Single(metricList);
            Assert.Equal(44, metricList.First().TotalMetrics.Single(x => x.Type == "Views").Value);
        }

        [Theory]
        [InlineData("views", "The Population Boom")]
        [InlineData("reactions", "What's Wrong With Wakanda?")]
        public void TrendAPIReturnsTopOfWeek(string sortOnColumn, string expectedTitleOfTopVideo) {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom" },
                new SourceVideo { Title = "What's Wrong With Wakanda?" },
            };
            APS.TheseVideosExist(someVideos);

            APS.ThisVideoHasSomeMetrics(
                "The Population Boom",
                new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
            },
                new DateTime(2019, 1, 1)
                );

            APS.ThisVideoHasSomeMetrics(
                "What's Wrong With Wakanda?",
                new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
            },
                new DateTime(2019, 1, 1)
                );

            APS.ApApTransformationsHaveRun();

            var raw = trendingApi.TopK(
                1,
                new DateTime(2019, 1, 1),
                new DateTime(2019, 1, 7),
                sortOnColumn
                );
            var result = Assert.IsType<OkObjectResult>(raw.Result);
            var data = Assert.IsType<TrendingResult>(result.Value);

            Assert.Single(data.TopK);
            Assert.Equal(expectedTitleOfTopVideo, data.TopK.First().Title);
            Assert.Equal(70, data.TopK.First().Total[sortOnColumn]);
        }

        [Theory]
        // too big k
        [InlineData(1000, "2018-01-01", "2018-01-07", "views", "custom")]
        // too big date range
        [InlineData(1, "2018-01-01", "2019-01-07", "views", "custom")]
        // invalid metric
        [InlineData(1, "2018-01-01", "2018-01-07", "not a valid metric", "custom")]
        // no data in period
        [InlineData(1, "2018-01-01", "2018-01-07", "views", "custom")]
        public void TrendAPIReturnsError(
            int size,
            string dateStart,
            string dateStop,
            string sortMetric,
            string when
            ) {
            var raw = trendingApi.TopK(
                size,
                DateUtilities.ParseApiDateString(dateStart),
                DateUtilities.ParseApiDateString(dateStop),
                sortMetric,
                when
                );
            var result = Assert.IsType<BadRequestObjectResult>(raw.Result);
        }

        [Fact]
        public void TrendAPIReturnsDenseArray() {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom" },
            };
            APS.TheseVideosExist(someVideos);

            APS.ThisVideoHasSomeMetrics(
                "The Population Boom",
                new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
            },
                new DateTime(2019, 1, 1)
                );

            APS.ThisVideoHasSomeMetrics(
                "The Population Boom",
                new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 5, ReactionCount = 10 },
            },
                new DateTime(2019, 1, 5)
                );

            APS.ApApTransformationsHaveRun();

            var raw = trendingApi.TopK(
                1,
                new DateTime(2019, 1, 1),
                new DateTime(2019, 1, 7)
                );
            var result = Assert.IsType<OkObjectResult>(raw.Result);
            var data = Assert.IsType<TrendingResult>(result.Value);

            Assert.Equal(
                new string[] {
                "2019-01-01",
                "2019-01-02",
                "2019-01-03",
                "2019-01-04",
                "2019-01-05",
                "2019-01-06",
                "2019-01-07",
            }, data.TopK.Single().DailyBreakDown.Keys.ToArray());
        }

        [Fact]
        public void TrendAPIReturnsOkInCaseOfFewerVideos() {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom" },
            };
            APS.TheseVideosExist(someVideos);

            APS.ThisVideoHasSomeMetrics(
                "The Population Boom",
                new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 10, ReactionCount = 5 },
            },
                new DateTime(2019, 1, 1)
                );

            APS.ApApTransformationsHaveRun();

            var raw = trendingApi.TopK(
                10,
                new DateTime(2019, 1, 1),
                new DateTime(2019, 1, 7)
                );
            var result = Assert.IsType<OkObjectResult>(raw.Result);
        }

        [Fact]
        public void TotalViewsAreNotInflatedByMultipleGenericTags() {
            var someVideos = new[] {
                new SourceVideo { Title = "The Population Boom", VideoLength = 67, Platform = Constants.FacebookSource },
            };
            APS.TheseVideosExist(someVideos);

            var someMetrics = new SourceVideoMetric[] {
                new SourceVideoMetric { ViewCount = 10 },
                new SourceVideoMetric { ViewCount = 10 },
                new SourceVideoMetric { ViewCount = 10 },
                new SourceVideoMetric { ViewCount = 10 },
                new SourceVideoMetric { ViewCount = 10 },
                new SourceVideoMetric { ViewCount = 10 },
                new SourceVideoMetric { ViewCount = 10 },
            };
            APS.ThisVideoHasSomeMetrics("The Population Boom", someMetrics, new DateTime(2019, 1, 1));

            APS.ApApTransformationsHaveRun();

            var videos = contentController.GetVideoList().OrderBy(x => x.Id).ToList();
            var edits = new Dictionary<string, VideoEdit>() {
                { videos[0].Id, new VideoEdit() {
                      UpdateDate = videos[0].UpdateDate,
                      // Add new generic Tag
                      Flag = EditType.Update,
                      AddedGenericTags = new List<string>() {
                          "FOO",
                          "BAR",
                          "FOOBAR",
                      },
                  }
                },
            };
            contentController.EditVideos(new VideoEdits { Edits = edits });

            var chartDataByLength = contentController.GetChartData(
                "[\"Views\"]",
                "length",
                "20190101",
                "20190107"
                ).Single();

            var metricList = contentController.GetMetricList(
                "20190101",
                "20190107"
                );

            var chartDataByGeneric = contentController.GetChartData(
                "[\"Views\"]",
                "generic",
                "20190101",
                "20190107"
                ).Single();

            Assert.Equal(70, chartDataByLength.TotalPerGroup[Constants.GrandTotalName]);
            Assert.Equal(70, chartDataByGeneric.TotalPerGroup[Constants.GrandTotalName]);
            Assert.Equal(70, metricList.Sum(x => x.TotalMetrics.Single(y => y.Type == "Views").Value));
        }
    }
}
