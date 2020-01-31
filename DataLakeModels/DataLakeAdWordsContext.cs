using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.AdWords.Reports;

namespace DataLakeModels {
    public partial class DataLakeAdWordsContext : AbstractDataLakeContext {
        public static string AdWordsApiVersion = "v201809";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(String.Format("adwords_{0}", AdWordsApiVersion));

            modelBuilder.Entity<AdPerformance>()
                .HasKey(table => new { table.AdId, table.Date, table.ValidityStart });

            modelBuilder.Entity<StructuralVideoPerformance>()
                .HasKey(table => new { table.CreativeId, table.ValidityStart });

            modelBuilder.Entity<StructuralCampaignPerformance>()
                .HasKey(table => new { table.CampaignId, table.ValidityStart });

            modelBuilder.Entity<StructuralCriteriaPerformance>()
                .HasKey(table => new { table.KeywordId, table.AdGroupId, table.CriteriaType, table.Criteria, table.ValidityStart });
        }

        public virtual DbSet<StructuralVideoPerformance> StructuralVideoPerformanceReports { get; set; }
        public virtual DbSet<StructuralCriteriaPerformance> StructuralCriteriaPerformanceReports { get; set; }
        public virtual DbSet<StructuralCampaignPerformance> StructuralCampaignPerformanceReports { get; set; }
        public virtual DbSet<AdPerformance> AdPerformanceReports { get; set; }
    }
}
