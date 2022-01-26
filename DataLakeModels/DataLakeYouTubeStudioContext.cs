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
                    table.ChannelId,
                    table.VideoId,
                    table.ValidityStart,
                    table.Metric,
                    table.EventDate
                });

        }

        public virtual DbSet<Video> Videos { get; set; }
    }
}
