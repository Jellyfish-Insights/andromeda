using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.YouTube.Data;

namespace DataLakeModels {

    public partial class DataLakeYouTubeDataContext : AbstractDataLakeContext {

        public static string YtDataApiVersion = "v3";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(String.Format("youtube_data_{0}", YtDataApiVersion));

            modelBuilder.Entity<Video>()
                .HasKey(table => new { table.VideoId, table.ValidityStart });

            modelBuilder.Entity<Playlist>()
                .HasKey(table => new { table.PlaylistId, table.ValidityStart });

            modelBuilder.Entity<Statistics>()
                .HasKey(table => new { table.VideoId, table.CaptureDate, table.ValidityStart });
        }

        public virtual DbSet<Video> Videos { get; set; }
        public virtual DbSet<Playlist> Playlists { get; set; }
        public virtual DbSet<Statistics> Statistics { get; set; }
    }
}
