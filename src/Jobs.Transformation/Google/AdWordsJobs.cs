using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using DataLakeModels;
using DataLakeModels.Models.AdWords.Reports;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using Jobs.Transformation.Google;
using Jobs.Transformation.YouTube;

namespace Jobs.Transformation.AdWords {

    public class AudienceSync : GoogleTransformationJob<DataLakeAdWordsContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Jobs.Fetcher.AdWords.StructuralCriteriaPerformanceReport>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceAudience);
        }

        public override void ExecuteJob(DataLakeAdWordsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListAudiences(dlContext, trace)) {
                var storedObject = apDbContext.SourceAudiences.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAudience>> ListAudiences(DataLakeAdWordsContext dbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var dlAudience in dbContext.StructuralCriteriaPerformanceReports.Where(x => x.ValidityStart <= now && now < x.ValidityEnd).GroupBy(x => x.AdGroupId)) {
                var adGroupName = dlAudience.First().AdGroupName;
                var updateDate = dlAudience.First().ValidityStart;
                var adGroupId = dlAudience.Key;
                var definition = JToken.FromObject(dlAudience.Select(x => new JObject() {
                    { "CriteriaType", x.CriteriaType },
                    { "Criteria", x.Criteria },
                    { "IsNegative", x.IsNegative },
                    { "DisplayName", x.DisplayName },
                })).ToString();

                var log = new RowLog();
                log.AddInput(typeof(StructuralCriteriaPerformance).Name, MutableEntityExtentions.AutoPK(adGroupId, updateDate));
                yield return new EntityUpdateParams<SourceAudience>() {
                           UpdateFunction = delegate(SourceAudience a) {
                               a.Id = adGroupId;
                               a.Platform = PLATFORM_YOUTUBE;
                               a.Title = adGroupName;
                               a.Definition = definition;
                               a.UpdateDate = updateDate;
                               return a;
                           },
                           MatchFunction = v => v.Platform == PLATFORM_YOUTUBE && v.Id == adGroupId,
                           ObjectValidity = new NpgsqlRange<DateTime>(updateDate, DateTime.MaxValue),
                           Trace = log
                };
            }
        }
    }

    public class AdSetSync : GoogleTransformationJob<DataLakeAdWordsContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Jobs.Fetcher.AdWords.StructuralCriteriaPerformanceReport>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceAdSet);
        }

        public override void ExecuteJob(DataLakeAdWordsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListAdSets(dlContext, trace)) {
                var storedObject = apDbContext.SourceAdSets.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAdSet>> ListAdSets(DataLakeAdWordsContext dbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var dlAudience in dbContext.StructuralCriteriaPerformanceReports.Where(a => a.ValidityStart <= now && now < a.ValidityEnd).GroupBy(a => a.AdGroupId)) {
                var adGroupName = dlAudience.First().AdGroupName;
                var updateDate = dlAudience.First().ValidityStart;
                var adGroupId = dlAudience.Key;
                var definition = JToken.FromObject(dlAudience.Select(x => new JObject() {
                    { "CriteriaType", x.CriteriaType },
                    { "Criteria", x.Criteria },
                    { "IsNegative", x.IsNegative },
                    { "DisplayName", x.DisplayName },
                })).ToString();

                var log = new RowLog();
                log.AddInput(typeof(StructuralCriteriaPerformance).Name, MutableEntityExtentions.AutoPK(adGroupId, updateDate));
                yield return new EntityUpdateParams<SourceAdSet>() {
                           UpdateFunction = delegate(SourceAdSet a) {
                               a.Id = adGroupId;
                               a.Platform = PLATFORM_YOUTUBE;
                               a.Title = adGroupName;
                               a.Definition = definition;
                               a.IncludeAudience = new string[] { adGroupId };
                               a.ExcludeAudience = null;
                               a.UpdateDate = updateDate;
                               return a;
                           },
                           MatchFunction = v => v.Platform == PLATFORM_YOUTUBE && v.Id == adGroupId,
                           ObjectValidity = new NpgsqlRange<DateTime>(updateDate, DateTime.MaxValue),
                           Trace = log
                };
            }
        }
    }

    public class CampaignSync : GoogleTransformationJob<DataLakeAdWordsContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<Jobs.Fetcher.AdWords.StructuralCampaignPerformanceReport>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceCampaign);
        }

        public override void ExecuteJob(DataLakeAdWordsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListCampaigns(dlContext, trace)) {
                var storedObject = apDbContext.SourceCampaigns.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceCampaign>> ListCampaigns(DataLakeAdWordsContext dbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var campaign in dbContext.StructuralCampaignPerformanceReports.Where(a => a.ValidityStart <= now && now < a.ValidityEnd)) {
                var log = new RowLog();
                log.AddInput(typeof(StructuralCampaignPerformance).Name, MutableEntityExtentions.AutoPK(campaign.CampaignId, campaign.ValidityStart));
                var startDate = ParseDateOrDefault(campaign.StartDate, DateTime.MinValue);
                var endDate = ParseDateOrDefault(campaign.EndDate, DateTime.MaxValue);
                yield return new EntityUpdateParams<SourceCampaign>() {
                           UpdateFunction = delegate(SourceCampaign c) {
                               c.Id = campaign.CampaignId;
                               c.Platform = PLATFORM_YOUTUBE;
                               c.Title = campaign.CampaignName;
                               c.Status = campaign.CampaignStatus;
                               c.Objective = campaign.BiddingStrategyType;
                               c.StartTime = startDate;
                               c.StopTime = endDate;
                               return c;
                           },
                           MatchFunction = v => v.Id == campaign.CampaignId,
                           ObjectValidity = new NpgsqlRange<DateTime>(campaign.ValidityStart, campaign.ValidityEnd),
                           Trace = log
                };
            }
        }
    }

    public class AdSync : GoogleTransformationJob<DataLakeAdWordsContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<CampaignSync>(), IdOf<VideoSync>(), IdOf<AdSetSync>(), IdOf<Jobs.Fetcher.AdWords.AdPerformanceReport>(), IdOf<Jobs.Fetcher.AdWords.StructuralVideoPerformanceReport>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceAd);
        }

        public override void ExecuteJob(DataLakeAdWordsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListAds(dlContext, apDbContext, trace)) {
                var storedObject = apDbContext.SourceAds.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAd>> ListAds(DataLakeAdWordsContext dbContext, ApplicationDbContext apDbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var ad in dbContext.AdPerformanceReports
                         .Where(a => a.ValidityStart <= now && now < a.ValidityEnd)
                         .GroupBy(a => a.AdId)
                         .Select(a => a.OrderBy(x => x.Date).Last())) {
                var campaign = apDbContext.SourceCampaigns.Where(c => c.Id == ad.CampaignId).LastOrDefault();
                var videoPerf = dbContext.StructuralVideoPerformanceReports.Where(v => v.CreativeId == ad.AdId).LastOrDefault();
                var video = (videoPerf != null) ? apDbContext.SourceVideos.Where(v => v.Id == videoPerf.VideoId).LastOrDefault() : null;
                var adSet = apDbContext.SourceAdSets.Where(a => ad.AdGroupId == a.Id).LastOrDefault();
                var endDate = ParseDateOrDefault(ad.Date, DateTime.MaxValue);

                var log = new RowLog();
                log.AddInput(typeof(AdPerformance).Name, MutableEntityExtentions.AutoPK(ad.AdId, ad.Date, ad.ValidityStart));
                yield return new EntityUpdateParams<SourceAd>() {
                           UpdateFunction = delegate(SourceAd a) {
                               a.Id = ad.AdId;
                               a.Platform = PLATFORM_YOUTUBE;
                               a.Title = ad.Headline;
                               if (video != null) a.Video = video;
                               if (adSet != null) a.AdSet = adSet;
                               if (campaign != null) a.Campaign = campaign;
                               return a;
                           },
                           MatchFunction = v => v.Id == ad.AdId,
                           ObjectValidity = new NpgsqlRange<DateTime>(endDate, DateTime.MaxValue),
                           Trace = log
                };
            }
        }
    }

    public class AdMetricSync : GoogleTransformationJob<DataLakeAdWordsContext> {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<AdSync>(), IdOf<Jobs.Fetcher.AdWords.AdPerformanceReport>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceAdMetric);
        }

        public override void ExecuteJob(DataLakeAdWordsContext dlContext, ApplicationDbContext apDbContext, JobTrace trace) {
            foreach (var updateParams in ListAdMetrics(dlContext, apDbContext, trace)) {
                var storedObject = apDbContext.SourceAdMetrics.Where(updateParams.MatchFunction);
                SaveMutableEntity(apDbContext, trace, storedObject, updateParams);
            }
        }

        public static IEnumerable<EntityUpdateParams<SourceAdMetric>> ListAdMetrics(DataLakeAdWordsContext dbContext, ApplicationDbContext apDbContext, JobTrace trace) {
            var now = DateTime.UtcNow;
            foreach (var ad in dbContext.AdPerformanceReports
                         .Where(a => a.ValidityStart <= now && now < a.ValidityEnd)
                         .GroupBy(a => new { a.AdId, a.Date })
                         .Select(a => a.OrderBy(x => x.Date).Last())) {
                var adAp = apDbContext.SourceAds.Where(a => a.Id == ad.AdId).LastOrDefault();
                if (adAp != null) {
                    var endDate = ParseDateOrDefault(ad.Date, DateTime.MaxValue);
                    var log = new RowLog();
                    log.AddInput(typeof(AdPerformance).Name, MutableEntityExtentions.AutoPK(ad.AdId, ad.Date, ad.ValidityStart));
                    yield return new EntityUpdateParams<SourceAdMetric>() {
                               UpdateFunction = delegate(SourceAdMetric a) {
                                   a.AdId = adAp.Id;
                                   a.EventDate = endDate;
                                   a.Clicks = int.Parse(ad.Clicks);
                                   a.Views = int.Parse(ad.VideoViews);
                                   a.Impressions = int.Parse(ad.Impressions);
                                   a.Cost = int.Parse(ad.Cost) / 1000000.0;
                                   a.EmailCapture = null;
                                   a.Engagements = int.Parse(ad.Engagements);
                                   a.Reach = null;
                                   a.CostPerView = int.Parse(ad.AverageCpv) / 1000000.0;
                                   a.CostPerClick = int.Parse(ad.AverageCpc) / 1000000.0;
                                   a.CostPerImpression = int.Parse(ad.AverageCpm) / 1000.0 / 1000000.0;
                                   a.CostPerEngagement = int.Parse(ad.AverageCpe) / 1000000.0;
                                   return a;
                               },
                               MatchFunction = v => v.AdId == ad.AdId && v.EventDate == endDate,
                               ObjectValidity = new NpgsqlRange<DateTime>(endDate, DateTime.MaxValue),
                               Trace = log
                    };
                }
            }
        }
    }
}
