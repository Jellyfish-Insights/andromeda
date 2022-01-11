using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.YouTube.Studio;

namespace DataLakeModels {

    public partial class DataLakeYouTubeStudioContext : AbstractDataLakeContext {

        public static string YtDataApiVersion = "v3";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(String.Format("youtube_data_{0}", YtDataApiVersion));

            modelBuilder.Entity<Video>()
                .HasKey(table => new { table.VideoId, table.ValidityStart });

        }

        public virtual DbSet<Video> Videos { get; set; }
    }
}
