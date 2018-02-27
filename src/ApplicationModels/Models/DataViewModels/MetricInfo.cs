using System;
namespace ApplicationModels.Models.DataViewModels {
    public enum ChartType
    {
        LINE = 0,
        BAR = 1,
    }

    public class AverageMetric : MetricInfo {
        public MetricType Numerator;

        public MetricType Denominator;
    }

    public enum MetricType
    {
        // The order of definitions is important
        // Do not change it unless asked by the client
        // Content Metrics
        Views,
        ViewTime,
        Reactions,
        Likes,
        Dislikes,
        Comments,
        Shares,
        Impressions,
        AverageViewTime,
        DemographicsViewCount,
        DemographicsViewTime,
        // Marketing Metrics
        Clicks,
        ClickCost,
        CostPerView,
        CostPerClick,
        CostPerEmailCapture,
        CostPerEngagement,
        CostPerImpression,
        EmailCaptures,
        EmailCaptureCost,
        Engagements,
        EngagementCost,
        ImpressionCost,
        Reach,
        TotalCost,
        ViewCost,
    }

    public class MetricInfo {
        // Cost Per View, Cost per click...
        public MetricType TypeId { get; set; }
        public string Type { get; set; }
        public string Abbreviation { get; set; }
        // symbol for unit: $, %
        public string Unit { get; set; }
        // Left or Right, depending if unit appears on left or right of the value
        public string UnitSide { get; set; }

        public ChartType ChartType { get; set; }

        // either content or marketing
        private string _PageType;
        public string PageType
        {
            get { return _PageType; }
            set
            {
                if (!(value.Equals("content") || value.Equals("marketing")))
                    throw new ArgumentException("Not valid PageType");
                _PageType = value;
            }
        }

        public string MarkdownSource { get; set; }
    }
}
