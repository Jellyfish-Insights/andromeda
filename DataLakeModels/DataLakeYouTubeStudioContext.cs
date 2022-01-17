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
            // ChannelId == other.ChannelId &&
            //         VideoId == other.VideoId &&
            //         Metric  == other.Metric &&
            //         DateMeasure == other.DateMeasure;
                .HasKey(table => new {
                    table.VideoId,
                    table.ValidityStart
                });

        }

        public virtual DbSet<Video> Videos { get; set; }
    }
}
