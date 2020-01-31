using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.YouTube.Analytics;

namespace DataLakeModels {

    public partial class DataLakeYouTubeAnalyticsContext : AbstractDataLakeContext {

        public static string YtAnalyticsApiVersion = "v2";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(String.Format("youtube_analytics_{0}", YtAnalyticsApiVersion));

            modelBuilder.Entity<VideoDailyMetric>()
                .HasKey(table => new { table.VideoId, table.Date, table.ValidityStart });

            modelBuilder.Entity<ViewerPercentage>()
                .HasKey(table => new { table.VideoId, table.StartDate, table.Gender, table.AgeGroup, table.ValidityStart });
        }

        public virtual DbSet<VideoDailyMetric> VideoDailyMetrics { get; set; }
        public virtual DbSet<ViewerPercentage> ViewerPercentageMetric { get; set; }
        public virtual DbSet<ViewerPercentageLastDate> ViewerPercentageLastDates { get; set; }
    }
}
