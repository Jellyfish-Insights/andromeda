using System.Linq;
using System;
using ApplicationModels.Models;
using ApplicationModels;
using Common.Jobs;
using ConsoleApp.Commands;
using Jobs.Transformation.YouTube;
using Jobs.Transformation.AdWords;
using Xunit;
using System.Collections.Generic;
using ApplicationModels.Models.DataViewModels;
using WebApp.Controllers;

namespace Test.Helpers {

    public class AnalyticsPlatformSteps {

        private ApplicationDbContext Context = new ApplicationDbContext();
        private ContentDataController contentController;

        private int VideoCount = 0;
        private int CampaignCount = 0;
        private int AdSetCount = 0;
        private int AdCount = 0;

        public AnalyticsPlatformSteps() {
            contentController = new ContentDataController(new DBContentDataBackend());
        }

        public void TheseVideosExist(SourceVideo[] videos) {
            foreach (var video in videos) {
                VideoCount++;
                video.Id = VideoCount.ToString();
                video.PublishedStatus = true;
            }
            Context.AddRange(videos);
            Context.SaveChanges();
        }

        public void ThisVideoHasSomeMetrics(string videoTitle, SourceVideoMetric[] metrics, DateTime date) {
            var video = Context.SourceVideos.Where(x => x.Title == videoTitle).Single();

            foreach (var metric in metrics) {
                metric.VideoId = video.Id.ToString();
                metric.EventDate = date;
                date = date.AddDays(1);
            }
            Context.AddRange(metrics);
            Context.SaveChanges();
        }

        public void ThisVideoHasSomeDeltaEncodedMetrics(string videoTitle, SourceDeltaEncodedVideoMetric[] metrics, DateTime startDate) {
            var video = Context.SourceVideos.Where(x => x.Title == videoTitle).Single();

            foreach (var metric in metrics) {
                metric.VideoId = video.Id.ToString();
                metric.StartDate = startDate;
                startDate = metric.EndDate;
            }
            Context.AddRange(metrics);
            Context.SaveChanges();
        }

        public void ThisVideoHasSomeDemographicsMetrics(string videoTitle, (DateTime, DateTime, SourceVideoDemographicMetric[])[] metricsSet) {
            var video = Context.SourceVideos.Where(x => x.Title == videoTitle).Single();

            foreach (var(start, end, metrics) in metricsSet) {
                foreach (var metric in metrics) {
                    metric.StartDate = start;
                    metric.EndDate = end;
                    metric.VideoId = video.Id.ToString();
                }
                Context.AddRange(metrics);
            }
            Context.SaveChanges();
        }

        public void TheseCampaignsExist((SourceCampaign, (SourceAdSet, SourceAd[])[])[] campaigns) {
            foreach (var(campaign, adsets) in campaigns) {
                CampaignCount++;
                campaign.Id = CampaignCount.ToString();
                Context.Add(campaign);
                foreach (var(adset, ads) in adsets) {
                    AdSetCount++;
                    adset.Id = AdSetCount.ToString();
                    adset.Platform = campaign.Platform;
                    Context.Add(adset);
                    foreach (var ad in ads) {
                        AdCount++;
                        ad.Id = AdCount.ToString();
                        ad.Platform = campaign.Platform;
                        ad.CampaignId = campaign.Id;
                        ad.AdSetId = adset.Id;
                    }
                    Context.AddRange(ads);
                }
            }
            Context.SaveChanges();
        }

        public void TheseAdsReferenceThisVideo(string[] adTitles, string videoTitle) {
            var video = Context.SourceVideos.Where(x => x.Title == videoTitle).Single();
            var ads = Context.SourceAds.Where(x => adTitles.Contains(x.Title));

            foreach (var ad in ads) {
                ad.VideoId = video.Id;
            }
            Context.UpdateRange(ads);
            Context.SaveChanges();
        }

        public void ThisAdHasSomeMetrics(string adTitle, SourceAdMetric[] metrics, DateTime date) {
            var ad = Context.SourceAds.Where(x => x.Title == adTitle).Single();

            foreach (var metric in metrics) {
                metric.AdId = ad.Id.ToString();
                metric.EventDate = date;
                date = date.AddDays(1);
            }
            Context.AddRange(metrics);
            Context.SaveChanges();
        }

        public void ApApTransformationsHaveRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.Application, new string[] {}));
        }

        public void AdWordsTransformationsHaveRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.AdWords, new string[] {}));
        }

        public void AdWordsAudienceSyncJobHasRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.AdWords, new string[] { typeof(AudienceSync).Name }));
        }

        public void AdWordsAdSetSyncJobHasRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.AdWords, new string[] { typeof(AdSetSync).Name }));
        }

        public void FacebookTransformationsHaveRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.Facebook, new string[] {}));
        }

        public void YouTubeTransformationsHaveRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.YouTube, new string[] {}));
        }

        public void YouTubeVideoSyncJobHasRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.YouTube, new string[] { typeof(VideoSync).Name }));
        }

        public void YouTubeVideoDemographicMetricSyncHasRun() {
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Transformation, JobScope.YouTube, new string[] { typeof(VideoDemographicMetricSync).Name }));
        }

        public void FacebookFetcherHasRun() {
            var configuration = JobConfiguration.LoadConfiguration(JobConstants.JobConfigFile);
            configuration.IgnoreTTL = true;
            configuration.IgnoreAPI = true;
            Assert.Equal(0, RunJobsCommand.RunJobs(JobType.Fetcher, JobScope.Facebook, new string[] {}, configuration));
        }

        public void VideoHasMetaTagsUpdated(string videoTitle, Dictionary<string, string> metaTagEdit) {
            // Create new tags and update Length tag
            var videos = contentController.GetVideoList("[]");
            var targetVideo = videos.Single(x => x.Title == videoTitle);
            var edits = new Dictionary<string, VideoEdit>() {
                { targetVideo.Id,
                  new VideoEdit() {
                      UpdateDate = targetVideo.UpdateDate,
                      MetaTags = metaTagEdit,
                      Flag = EditType.Update,
                  }
                }
            };
            var failedEdits = contentController.EditVideos(new VideoEdits { Edits = edits });
            Assert.Empty(failedEdits.Edits);
        }
    }
}
