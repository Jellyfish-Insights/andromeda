using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.Twitter.Data;

namespace DataLakeModels {

    public partial class DataLakeTwitterDataContext : AbstractDataLakeContext {

        public static string TwitterApiVersion = "v2";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema($"twitter_data_{TwitterApiVersion}");

            modelBuilder.Entity<User>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Tweet>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Tweet>()
                .HasOne(table => table.User)
                .WithMany(user => user.Tweets)
                .HasForeignKey(table => table.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TweetPublicMetrics>()
                .HasKey(table => new { table.TweetId, table.ValidityStart });

            modelBuilder.Entity<TweetPublicMetrics>()
                .HasOne(table => table.Tweet)
                .WithMany(tweet => tweet.PublicMetrics)
                .HasForeignKey(table => table.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TweetNonPublicMetrics>()
                .HasKey(table => new { table.TweetId, table.ValidityStart });

            modelBuilder.Entity<TweetNonPublicMetrics>()
                .HasOne(table => table.Tweet)
                .WithMany(tweet => tweet.NonPublicMetrics)
                .HasForeignKey(table => table.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TweetOrganicMetrics>()
                .HasKey(table => new { table.TweetId, table.ValidityStart });

            modelBuilder.Entity<TweetOrganicMetrics>()
                .HasOne(table => table.Tweet)
                .WithMany(tweet => tweet.OrganicMetrics)
                .HasForeignKey(table => table.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TweetPromotedMetrics>()
                .HasKey(table => new { table.TweetId, table.ValidityStart });

            modelBuilder.Entity<TweetPromotedMetrics>()
                .HasOne(table => table.Tweet)
                .WithMany(tweet => tweet.PromotedMetrics)
                .HasForeignKey(table => table.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Media>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<MediaPublicMetrics>()
                .HasKey(table => new { table.MediaId, table.ValidityStart });

            modelBuilder.Entity<MediaPublicMetrics>()
                .HasOne(table => table.Media)
                .WithMany(media => media.PublicMetrics)
                .HasForeignKey(table => table.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaNonPublicMetrics>()
                .HasKey(table => new { table.MediaId, table.ValidityStart });

            modelBuilder.Entity<MediaNonPublicMetrics>()
                .HasOne(table => table.Media)
                .WithMany(media => media.NonPublicMetrics)
                .HasForeignKey(table => table.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaOrganicMetrics>()
                .HasKey(table => new { table.MediaId, table.ValidityStart });

            modelBuilder.Entity<MediaOrganicMetrics>()
                .HasOne(table => table.Media)
                .WithMany(media => media.OrganicMetrics)
                .HasForeignKey(table => table.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MediaPromotedMetrics>()
                .HasKey(table => new { table.MediaId, table.ValidityStart });

            modelBuilder.Entity<MediaPromotedMetrics>()
                .HasOne(table => table.Media)
                .WithMany(media => media.PromotedMetrics)
                .HasForeignKey(table => table.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TweetMedia>()
                .HasKey(table => new { table.TweetId, table.MediaId });

            modelBuilder.Entity<TweetMedia>()
                .HasOne(table => table.Tweet)
                .WithMany(tweet => tweet.TweetMedia)
                .HasForeignKey(table => table.TweetId);

            modelBuilder.Entity<TweetMedia>()
                .HasOne(table => table.Media)
                .WithMany(media => media.TweetMedia)
                .HasForeignKey(table => table.MediaId);
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Tweet> Tweets { get; set; }
        public virtual DbSet<TweetPublicMetrics> TweetPublicMetrics { get; set; }
        public virtual DbSet<TweetNonPublicMetrics> TweetNonPublicMetrics { get; set; }
        public virtual DbSet<TweetOrganicMetrics> TweetOrganicMetrics { get; set; }
        public virtual DbSet<TweetPromotedMetrics> TweetPromotedMetrics { get; set; }
        public virtual DbSet<Media> Medias { get; set; }
        public virtual DbSet<MediaPublicMetrics> MediaPublicMetrics { get; set; }
        public virtual DbSet<MediaNonPublicMetrics> MediaNonPublicMetrics { get; set; }
        public virtual DbSet<MediaOrganicMetrics> MediaOrganicMetrics { get; set; }
        public virtual DbSet<MediaPromotedMetrics> MediaPromotedMetrics { get; set; }
        public virtual DbSet<TweetMedia> TweetMedia { get; set; }
    }
}
