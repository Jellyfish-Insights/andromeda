using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels;
using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.Util.Reports;
using Google.Api.Ads.AdWords.v201809;
using DLM = DataLakeModels.Models.AdWords.Reports;
using Serilog.Core;

namespace Jobs.Fetcher.AdWords {

    public class StructuralVideoPerformanceReport : AdWordsFetcher {
        public StructuralVideoPerformanceReport(AdWordsUser user) {
            User = user;
        }

        protected override void Method(DataLakeAdWordsContext dbContext) {
            var now = DateTime.UtcNow;
            var definition = new ReportDefinition {
                reportName = "Structural VIDEO_PERFORMANCE_REPORT",
                dateRangeType = ReportDefinitionDateRangeType.ALL_TIME,
                reportType = ReportDefinitionReportType.VIDEO_PERFORMANCE_REPORT,
                downloadFormat = DownloadFormat.TSV,
                selector = new Selector {
                    fields = new string[] {
                        "CreativeId",
                        "VideoId"
                    }
                }
            };
            var utilities = new ReportUtilities(User, ApiVersion, definition);
            var records = ReportToRecords(utilities, r => new DLM.StructuralVideoPerformance {
                CreativeId = r.AdID,
                VideoId = r.VideoId,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue
            });
            var recordsToAdd = records.Where(r => !(from c in dbContext.StructuralVideoPerformanceReports
                                                    where c.VideoId == r.VideoId && c.CreativeId == r.CreativeId &&
                                                    c.ValidityStart <= now && now < c.ValidityEnd
                                                    select c).Any());
            var recordsToUpdate = dbContext.StructuralVideoPerformanceReports.Where(r => r.ValidityStart <= now && now < r.ValidityEnd);
            foreach (var r in recordsToUpdate) {
                if (!records.Where(n => n.CreativeId == r.CreativeId && n.VideoId == r.VideoId).Any()) {
                    r.ValidityEnd = now;
                }
            }

            dbContext.AddRange(recordsToAdd);
            Logger.Debug("Inserted {Count} new records", recordsToAdd.Count());
        }

        public override List<string> Dependencies() {
            return new List<string>() {};
        }
    }

    public class StructuralCriteriaPerformanceReport : AdWordsFetcher {
        public StructuralCriteriaPerformanceReport(AdWordsUser user) {
            User = user;
        }

        protected override void Method(DataLakeAdWordsContext dbContext) {
            var now = DateTime.UtcNow;
            var definition = new ReportDefinition {
                reportName = "Structural CRITERIA_PERFORMANCE_REPORT",
                dateRangeType = ReportDefinitionDateRangeType.LAST_30_DAYS,
                reportType = ReportDefinitionReportType.CRITERIA_PERFORMANCE_REPORT,
                downloadFormat = DownloadFormat.TSV,
                selector = new Selector {
                    fields = new string[] {
                        "Id",
                        "CampaignId",
                        "AdGroupId",
                        "AdGroupName",
                        "Criteria",
                        "CriteriaType",
                        "DisplayName",
                        "IsNegative"
                    }
                }
            };
            var utilities = new ReportUtilities(User, ApiVersion, definition);
            var records = ReportToRecords(utilities, r => new DLM.StructuralCriteriaPerformance {
                KeywordId = r.KeywordID,
                CampaignId = r.CampaignID,
                AdGroupId = r.AdgroupID,
                AdGroupName = r.Adgroup,
                Criteria = r.KeywordPlacement,
                CriteriaType = r.CriteriaType,
                DisplayName = r.CriteriaDisplayName,
                IsNegative = r.Isnegative,
                ValidityEnd = DateTime.MaxValue,
                ValidityStart = now
            });
            var groupedRecords = records.GroupBy(r => r.AdGroupId);

            Func<IGrouping<string, DLM.StructuralCriteriaPerformance>, bool> shouldAddRecord = record => {
                var lastValid = dbContext.StructuralCriteriaPerformanceReports
                                    .Where(r => r.AdGroupId == record.Key && r.ValidityStart <= now && r.ValidityEnd >= now);
                if (!lastValid.Any()) {
                    return true;
                }
                if (lastValid.Count() != record.Count()) {
                    lastValid.ToList().ForEach(r => r.ValidityEnd = now);
                    return true;
                }
                var lastValidOrdered = OrderCriteriaByAllFields(lastValid);
                var recordOrdered = OrderCriteriaByAllFields(record);
                var count = lastValid.Count();
                for (var i = 0; i < count; i++) {
                    var o = lastValidOrdered.ElementAt(i);
                    var n = recordOrdered.ElementAt(i);
                    if (!o.Equals(n)) {
                        lastValid.ToList().ForEach(r => r.ValidityEnd = now);
                        return true;
                    }
                }
                return false;
            };

            var recordsToAdd = groupedRecords.Where(shouldAddRecord).SelectMany(r => r);
            dbContext.AddRange(recordsToAdd);
            Logger.Debug("Inserted {Count} new records", recordsToAdd.Count());
        }

        private IOrderedEnumerable<DLM.StructuralCriteriaPerformance> OrderCriteriaByAllFields(IEnumerable<DLM.StructuralCriteriaPerformance> records) {
            return records.ToList()
                       .OrderBy(r => r.DisplayName)
                       .ThenBy(r => r.Criteria)
                       .ThenBy(r => r.CriteriaType)
                       .ThenBy(r => r.CampaignId)
                       .ThenBy(r => r.IsNegative);
        }

        public override List<string> Dependencies() {
            return new List<string>() {};
        }
    }

    public class StructuralCampaignPerformanceReport : AdWordsFetcher {
        public StructuralCampaignPerformanceReport(AdWordsUser user) {
            User = user;
        }

        protected override void Method(DataLakeAdWordsContext dbContext) {
            var definition = new ReportDefinition {
                reportName = "Structural CAMPAIGN_PERFORMANCE_REPORT",
                dateRangeType = ReportDefinitionDateRangeType.LAST_30_DAYS,
                reportType = ReportDefinitionReportType.CAMPAIGN_PERFORMANCE_REPORT,
                downloadFormat = DownloadFormat.TSV,
                selector = new Selector {
                    fields = new string[] {
                        "CampaignId",
                        "CampaignName",
                        "StartDate",
                        "EndDate",
                        "CampaignStatus",
                        "ServingStatus",
                        "BiddingStrategyId",
                        "BiddingStrategyName",
                        "BiddingStrategyType"
                    }
                }
            };

            var utilities = new ReportUtilities(User, ApiVersion, definition);
            var records = ReportToRecords(utilities, r => new DLM.StructuralCampaignPerformance {
                CampaignId = r.CampaignID,
                StartDate = r.Startdate,
                EndDate = r.Enddate,
                CampaignName = r.Campaign,
                CampaignStatus = r.Campaignstate,
                ServingStatus = r.Campaignservingstatus,
                BiddingStrategyId = r.BidStrategyID,
                BiddingStrategyName = r.BidStrategyName,
                BiddingStrategyType = r.BidStrategyType,
            });
            var recordsToAdd = records.Where(record => SubstituteLast(
                                                 record,
                                                 dbContext,
                                                 (from i in dbContext.StructuralCampaignPerformanceReports where i.CampaignId == record.CampaignId select i)
                                                 ) != null
                                             );
            dbContext.AddRange(recordsToAdd);
            Logger.Debug("Inserted {Count} new records", recordsToAdd.Count());
        }

        public override List<string> Dependencies() {
            return new List<string>() {};
        }
    }

    public class AdPerformanceReport : AdWordsFetcher {

        protected readonly TimeSpan OneDay = new TimeSpan(1, 0, 0, 0);
        protected readonly TimeSpan TimeMargin = new TimeSpan(10, 0, 0, 0);
        protected readonly TimeSpan TimeStep = new TimeSpan(30, 0, 0, 0);
        protected readonly DateTime StartDate = new DateTime(2016, 1, 1, 0, 0, 0);

        public AdPerformanceReport(AdWordsUser user) {
            User = user;
        }

        private List<DLM.AdPerformance> DefinitionToRecords(DataLakeAdWordsContext dbContext, ReportDefinition definition, DateTime now) {
            var utilities = new ReportUtilities(User, ApiVersion, definition);
            var records = ReportToRecords(utilities, r => new DLM.AdPerformance {
                CampaignId = r.CampaignID,
                AdGroupId = r.AdgroupID,
                AdId = r.AdID,
                Headline = r.Ad,
                Date = r.Day,
                Impressions = r.Impressions,
                VideoViews = r.Views,
                Clicks = r.Clicks,
                Engagements = r.Engagements,
                AverageCpm = r.AvgCPM,
                AverageCpv = r.AvgCPV,
                AverageCpc = r.AvgCPC,
                AverageCpe = r.AvgCPE,
                Cost = r.Cost,
                ValidityStart = now,
                ValidityEnd = DateTime.MaxValue,
            });
            var result = new List<DLM.AdPerformance>();
            foreach (var r in records) {
                var existing = dbContext.AdPerformanceReports
                                   .Where(a => a.AdId == r.AdId && a.Date == r.Date && a.ValidityStart <= now && now < a.ValidityEnd)
                                   .SingleOrDefault();
                if (existing == null) {
                    result = result.Append(r).ToList();
                } else if (!r.Equals(existing)) {
                    result = result.Append(r).ToList();
                    existing.ValidityEnd = now;
                }
            }
            return result;
        }

        protected string FormatDate(DateTime date) {
            return date.ToString("yyyyMMdd");
        }

        protected override void Method(DataLakeAdWordsContext dbContext) {
            var now = DateTime.UtcNow;
            var lastRecord = dbContext.AdPerformanceReports.OrderByDescending(r => r.Date).FirstOrDefault();
            var minDate = StartDate;
            if (lastRecord != null) {
                minDate = DateTime.ParseExact(lastRecord.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                if (now - minDate < TimeMargin) {
                    minDate = now.Date - TimeMargin;
                }
            }
            var maxDate = minDate + TimeStep;
            var records = new List<DLM.AdPerformance>();
            var definition = new ReportDefinition {
                reportName = "Last 30 days AD_PERFORMANCE_REPORT",
                dateRangeType = ReportDefinitionDateRangeType.CUSTOM_DATE,
                reportType = ReportDefinitionReportType.AD_PERFORMANCE_REPORT,
                downloadFormat = DownloadFormat.TSV,
                selector = new Selector {
                    fields = new string[] {
                        "CampaignId",
                        "AdGroupId",
                        "Id",
                        "Headline",
                        "Date",
                        "Impressions",
                        "VideoViews",
                        "Clicks",
                        "Engagements",
                        "AverageCpm",
                        "AverageCpv",
                        "AverageCpc",
                        "AverageCpe",
                        "Cost"
                    },
                    dateRange = new DateRange {
                        min = FormatDate(minDate),
                        max = FormatDate(maxDate)
                    }
                }
            };

            while (maxDate < now) {
                definition.selector.dateRange.min = FormatDate(minDate);
                definition.selector.dateRange.max = FormatDate(maxDate);

                records = records.Concat(DefinitionToRecords(dbContext, definition, now)).ToList();
                minDate = maxDate + OneDay;
                maxDate += TimeStep;
            }
            definition.selector.dateRange.min = FormatDate(minDate);
            definition.selector.dateRange.max = FormatDate(now);
            records = records.Concat(DefinitionToRecords(dbContext, definition, now)).ToList();
            dbContext.AddRange(records);
            Logger.Debug("Inserted {Count} new records", records.Count());
        }

        public override List<string> Dependencies() {
            return new List<string>() {};
        }
    }
}
