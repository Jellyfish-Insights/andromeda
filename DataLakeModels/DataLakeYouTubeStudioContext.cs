using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.YouTube.Studio;

namespace DataLakeModels {

    public partial class DataLakeYouTubeStudioContext : AbstractDataLakeContext {

        public static string YtStudioVersion = "v1";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(String.Format("youtube_studio_{0}", YtStudioVersion));

            modelBuilder.Entity<Video>()
                .HasKey(table => new {
                table.VideoId,
                table.ValidityStart,
                table.Metric,
                table.EventDate
            });

            modelBuilder.Entity<Group>()
                .HasKey(table => new { table.GroupId });

            modelBuilder.Entity<Item>()
                .HasOne(table => table.Group)
                .WithMany(group => group.Items)
                .HasForeignKey(table => table.GroupId);

            modelBuilder.Entity<Item>()
                .HasKey(table => new { table.ItemId });
        }

        public virtual DbSet<Video> Videos { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Item> Items { get; set; }
    }
}
