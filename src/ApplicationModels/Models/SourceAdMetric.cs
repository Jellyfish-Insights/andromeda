using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;

namespace ApplicationModels.Models {
    public class SourceAdMetric : IMutableEntity {
        public string AdId { get; set; }
        public SourceAd Ad { get; set; }
        [Column("EventDate", TypeName = "date")]
        public DateTime EventDate { get; set; }

        /**
         * Audiences Metrics

         ** Clicks
           - Facebook :: The daily number of clicks on your ads. See ~clicks~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: The number of clicks . See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#clicks][clicks]].
         */
        public int Clicks { get; set; }

        /**
        ** Views
           - Facebook :: The daily number of times your video was watched for an aggregate of at least 10 seconds, or for nearly its total length, whichever happened first.
                       See ~video_10_sec_watched_actions~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: the number of times your video ads were viewed. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#videoviews][videoViews]].
        */
        public int Views { get; set; }

        /**
        ** Impressions
           - Facebook :: The daily number of times your ads were on screen. See ~impressions~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: Count of how often your ad has appeared on a search results page or website on the Google Network. [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#impressions][impressions]].
        */
        public int Impressions { get; set; }

        /**
        ** Total Cost
           - Facebook :: The estimated daily total amount of money you've spent on your ad during its schedule. This metric is estimated. See ~spend~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: The sum of your cost-per-click (CPC) and cost-per-thousand impressions (CPM) costs during this period. Values can be one of: a) a money amount in micros. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#cost][cost]].
        */
        public double Cost { get; set; }

        /**
        ** Email Capture
           - Facebook :: The daily number of people who took an action  that was attributed to your ads.
                       Only action whose types contain string "lead" are considered. This metric is estimated.
                       See ~unique_actions~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
                       See ~action_type~ on [[https://developers.facebook.com/docs/marketing-api/reference/ads-action-stats/][ad action stats]].
           - YouTube :: N/A.
        */
        public int? EmailCapture { get; set; }

        /**
        ** Engagements
           - Facebook :: N/A.
           - YouTube :: Total number of ad engagements. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#engagements][engagements]].
        */
        public int? Engagements { get; set; }

        /**
        ** Reach
           - Facebook :: The daily number of people who saw your ads at least once.
                       Reach is different from impressions, which may include multiple views of your ads by the same people.
                       This metric is estimated.
                       See ~reach~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: N/A.
        */
        public int? Reach { get; set; }

        /**
        ** Cost per View
           - Facebook :: Daily average cost per view. Computed during transformation as:
                       $+BEGIN_SRC python
                         sum(number_of_actions_of_type(c) * cost_per_action_of_type(c) for c in types_of_action) / sum(number_of_actions_of_type(c) for c in types_of_action)
                       $+END_SRC
                       See ~video_10_sec_watched_actions~ and ~cost_per_action_type~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: The average amount you pay each time someone views your ad. The average CPV is defined by the total cost of all ad views divided by the number of views. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpv][averagecpv]].
        */
        public double CostPerView { get; set; }

        /**
        ** Cost per Click
           - Facebook :: Daily average cost for each click (all). See ~cpc~ at [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: the total cost of all clicks divided by the total number of clicks received. Values can be one of: a) a money amount in micros. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpc][averagecpc]].
        */
        public double CostPerClick { get; set; }

        /**
        ** Cost per Impression
           - Facebook :: Daily average cost for 1,000 impressions. See ~cpm~ at [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
           - YouTube :: Average Cost-per-thousand impressions (CPM). Values can be one of: a) a money amount in micros. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpm][averagecpm]].
        */
        public double CostPerImpression { get; set; }

        /**
        ** Cost per Email Capture
           - Facebook :: Daily average cost per email captures (event whose type contain string ~lead~). Computed during transformation as:
                       $+BEGIN_SRC python
                         sum(number_of_actions_of_type(c) * cost_per_action_of_type(c) for c in types_containg_string('lead')) / sum(number_of_actions_of_type(c) for c in types_containg_string('lead'))
                       $+END_SRC
                       See ~video_10_sec_watched_actions~ and ~cost_per_action_type~ on [[https://developers.facebook.com/docs/marketing-api/reference/adgroup/insights/][ad insights]].
                       See ~action_type~ on [[https://developers.facebook.com/docs/marketing-api/reference/ads-action-stats/][ad action stats]].
           - YouTube :: N/A.
        */
        public double CostPerEmailCapture { get; set; }

        /**
        ** Cost per Engagement
           - Facebook :: N/A.
           - YouTube :: The average amount that you've been charged for an ad engagement. This amount is the total cost of all ad engagements divided by the total number of ad engagements. See [[https://developers.google.com/adwords/api/docs/appendix/reports/ad-performance-report#averagecpe][averagecpe]].
        */
        public double CostPerEngagement { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(AdId, EventDate); }

        [NotMapped]

        DateTime IMutableEntity.UpdateDate { get => UpdateDate; }
    }
}
