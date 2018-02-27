using System.Collections.Generic;
using System.Linq;
using System;
using ApplicationModels;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using Npgsql;
using FF = Jobs.Fetcher.Facebook.FacebookFetcher;

namespace Jobs.Transformation.Facebook {

    public class CampaignSync : TracedFacebookJob {

        public override List<string> Dependencies() {
            return new List<string>() { FF.IdOf("adaccount", "campaigns") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceCampaign));
            foreach (var val in ListCampaigns(cmd, trace)) {
                var result = context.SourceCampaigns.Where(val.MatchFunction);
                SaveMutableEntity(context, trace, result, val);
            }
            return trace;
        }
    }

    public class AdSync : TracedFacebookJob {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<CampaignSync>(), IdOf<VideoSync>(), IdOf<AdSetSync>(), FF.IdOf("adaccount", "ads") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceAd));
            foreach (var val in ListAds(cmd, trace, context)) {
                var result = context.SourceAds.Where(val.MatchFunction);
                SaveMutableEntity(context, trace, result, val);
            }
            return trace;
        }
    }

    public class AdMetricSync : BatchedFacebookTransformationJob<SourceAd> {

        public override List<string> Dependencies() {
            return new List<string>() { IdOf<AdSync>() };
        }

        protected override Type TargetTable {
            get => typeof(SourceAdMetric);
        }
        public override SourceAd ExecuteJob(ApplicationDbContext context, NpgsqlConnection cmd, JobTrace trace, SourceAd previous) {

            IEnumerable<SourceAd> ads;

            if (previous != null)
                ads = context.SourceAds.Where(x => x.Platform == PLATFORM_FACEBOOK && x.Id.CompareTo(previous.Id) > 0).OrderBy(x => x.Id).Take(BatchSize);
            else
                ads = context.SourceAds.Where(x => x.Platform == PLATFORM_FACEBOOK).OrderBy(x => x.Id).Take(BatchSize);
            foreach (var a in ads) {
                var latest = context.SourceAdMetrics.Where(x => x.AdId == a.Id)
                                 .Select(x => x.UpdateDate)
                                 .DefaultIfEmpty(DateTime.MinValue)
                                 .Max();

                Logger.Debug("Processing metrics for ad {AdId}, latest date is: {LatestDate}", a.Id, latest);
                foreach (var val in ListAdsDailyMetrics(cmd, trace, a.Id, latest)) {
                    var existing = context.SourceAdMetrics.Where(val.MatchFunction);
                    SaveMutableEntity(context, trace, existing, val);
                }
            }
            return ads.LastOrDefault();
        }
    }

    public class AudienceSync : TracedFacebookJob {
        public override List<string> Dependencies() {
            return new List<string>() { FF.IdOf("adaccount", "customaudiences") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceAudience));
            foreach (var val in ListAudiences(cmd, trace)) {
                var existing = context.SourceAudiences.Where(val.MatchFunction);
                SaveMutableEntity(context, trace, existing, val);
            }
            return trace;
        }
    }

    public class FlexibleAudienceSync : TracedFacebookJob {
        public override List<string> Dependencies() {
            return new List<string>() { FF.IdOf("adaccount", "adsets") };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceAudience));
            foreach (var val in ListAdSets(cmd, trace)) {
                if (val.Item5 == null) {
                    var existing = context.SourceAudiences.Where(x => x.Id == val.Item4.ToString());
                    SaveMutableEntity(context, val.Item2, trace, existing, val.Item6, val.Item3);
                }
            }
            return trace;
        }
    }

    public class AdSetSync : TracedFacebookJob {
        public override List<string> Dependencies() {
            return new List<string>() { IdOf<AudienceSync>(), IdOf<FlexibleAudienceSync>() };
        }

        public override JobTrace Job(ApplicationDbContext context, NpgsqlConnection cmd) {
            var trace = CreateTrace(typeof(SourceAdSet));
            foreach (var val in ListAdSets(cmd, trace)) {
                var existing = context.SourceAdSets.Where(x => x.Id == val.Item4.ToString());
                SaveMutableEntity(context, val.Item1, trace, existing, val.Item6, val.Item3);
            }
            return trace;
        }
    }
}
