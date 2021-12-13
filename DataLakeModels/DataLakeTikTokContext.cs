using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.Twitter.Ads;

/*namespace DataLakeModels {

    public partial class DataLakeTikTokContext : AbstractDataLakeContext {

        public static string TikTokCrawlerVersion = "1";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema($"tiktok_v{TikTokCrawlerVersion}");

            modelBuilder.Entity<Author>()
                .HasKey(table => new { table.Id, table.UniqueId });

            modelBuilder.Entity<AuthorStats>()
                .HasOne(table => table.Author)
                .WithMany(author => author.AuthorStats)
                .HasForeignKey(table => table.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Challenge>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<EffectSticker>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Music>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Post>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Stats>()
                .HasOne(table => table.Post)
                .WithMany(post => post.Stats)
                .HasForeignKey(table => table.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tag>()
                .HasKey(table => new { table.AweMeId });

            modelBuilder.Entity<Video>()
                .HasKey(table => new { table.Id });
        }

        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<AuthorStats> AuthorStats { get; set; }
        public virtual DbSet<Challenge> Challenges { get; set; }
        public virtual DbSet<Music> Music { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Stats> Stats { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Video> Videos { get; set; }
    }
}
*/
