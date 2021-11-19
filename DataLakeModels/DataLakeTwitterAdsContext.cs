using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.Twitter.Ads;

namespace DataLakeModels {

    public partial class DataLakeTwitterAdsContext : AbstractDataLakeContext {

        public static string TwitterAdsApiVersion = "10";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema($"twitter_ads_v{TwitterAdsApiVersion}");

            modelBuilder.Entity<AdsAccount>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Campaign>()
                .HasKey(table => new { table.Id, table.ValidityStart });

            modelBuilder.Entity<Campaign>()
                .HasOne(table => table.AdsAccount)
                .WithMany(account => account.Campaigns)
                .HasForeignKey(table => table.AdsAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LineItem>()
                .HasKey(table => new { table.Id, table.ValidityStart });

            modelBuilder.Entity<PromotedTweet>()
                .HasKey(table => new { table.Id, table.ValidityStart });

            modelBuilder.Entity<VideoLibrary>()
                .HasKey(table => new { table.Id, table.ValidityStart });

            modelBuilder.Entity<OrganicTweetDailyMetrics>()
                .HasKey(table => new { table.TweetId, table.Date, table.ValidityStart });

            modelBuilder.Entity<PromotedTweetDailyMetrics>()
                .HasKey(table => new { table.PromotedTweetId, table.Date, table.ValidityStart });

            modelBuilder.Entity<CustomAudience>()
                .HasKey(table => new { table.Id });
        }

        public virtual DbSet<AdsAccount> AdsAccounts { get; set; }
        public virtual DbSet<Campaign> Campaigns { get; set; }
        public virtual DbSet<LineItem> LineItems { get; set; }
        public virtual DbSet<PromotedTweet> PromotedTweets { get; set; }
        public virtual DbSet<VideoLibrary> VideoLibraries { get; set; }
        public virtual DbSet<OrganicTweetDailyMetrics> OrganicTweetDailyMetrics { get; set; }
        public virtual DbSet<PromotedTweetDailyMetrics> PromotedTweetDailyMetrics { get; set; }
        public virtual DbSet<CustomAudience> CustomAudiences { get; set; }
    }
}
