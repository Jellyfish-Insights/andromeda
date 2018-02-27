using System.IO;
using ApLogging = Common.Logging;
using Common.Logging.Models;
using ApplicationModels.Models;
using ApplicationModels.Models.Metadata;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.AspNetCore;

namespace ApplicationModels {

    public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser> {

        public static IConfiguration Configuration = new ConfigurationBuilder()
                                                         .SetBasePath(Directory.GetCurrentDirectory())
                                                         .AddJsonFile("appsettings.json")
                                                         .Build();

        private static readonly ILoggerFactory EfLoggerFactory = new SerilogLoggerFactory(ApLogging.LoggerFactory.GetEfLogger());

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseNpgsql(Configuration.GetConnectionString("BusinessDatabase"))
                    .UseLoggerFactory(EfLoggerFactory);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("pg_trgm");
            modelBuilder.HasDefaultSchema("application");

            // AP Schema
            modelBuilder.Entity<ApplicationVideoApplicationGenericTag>().HasKey(v => new { v.TagId, v.VideoId });
            modelBuilder.Entity<ApplicationVideoApplicationMetaTag>().HasKey(v => new { v.TypeId, v.VideoId });

            // AP - Platform Relations
            modelBuilder.Entity<ApplicationPlaylistApplicationVideo>().ToTable("ApplicationPlaylistApplicationVideos").HasKey(v => new { v.ApplicationPlaylistId, v.ApplicationVideoId });
            modelBuilder.Entity<ApplicationVideoSourceCampaign>().ToTable("ApplicationVideoSourceCampaigns").HasKey(v => new { v.CampaignId });
            modelBuilder.Entity<ApplicationVideoSourceVideo>().ToTable("ApplicationVideoSourceVideos").HasKey(v => new { v.SourceVideoId });
            modelBuilder.Entity<ApplicationPersonaVersionSourceAdSet>().ToTable("ApplicationPersonaVersionSourceAdSets").HasKey(v => new { v.AdSetId });

            modelBuilder.Entity<UserApplicationVideoSourceVideo>().HasKey(v => new { v.SourceVideoId });
            modelBuilder.Entity<UserApplicationVideoSourceCampaign>().HasKey(v => new { v.CampaignId });
            modelBuilder.Entity<UserApplicationPersonaVersionSourceAdSet>().HasKey(v => new { v.AdSetId });

            modelBuilder.Entity<GeneratedApplicationVideoSourceVideo>().HasKey(v => new { v.SourceVideoId });
            modelBuilder.Entity<GeneratedApplicationPlaylistSourcePlaylist>().HasKey(v => new { v.ApplicationPlaylistId, v.SourcePlaylistId });
            modelBuilder.Entity<GeneratedApplicationVideoSourceCampaign>().HasKey(v => new { v.CampaignId });
            modelBuilder.Entity<GeneratedApplicationPersonaVersionSourceAdSet>().HasKey(v => new { v.AdSetId });

            // Platform Schema
            modelBuilder.Entity<SourcePlaylistSourceVideo>().HasKey(sc => new { sc.VideoId, sc.PlaylistId });
            modelBuilder.Entity<JobTrace>().HasKey(sc => new { sc.Table, sc.JobName, sc.StartTime });
            modelBuilder.Entity<SourceVideoMetric>().HasKey(table => new { table.VideoId, table.EventDate });
            modelBuilder.Entity<SourceVideoDemographicMetric>().HasKey(table => new { table.VideoId, table.StartDate, table.Gender, table.AgeGroup });
            modelBuilder.Entity<SourceDeltaEncodedVideoMetric>().HasKey(table => new { table.VideoId, table.StartDate });
            modelBuilder.Entity<SourceAdMetric>().HasKey(table => new { table.AdId, table.EventDate });

            modelBuilder.Entity<SourcePlaylistSourceVideo>()
                .HasOne<SourceVideo>(sc => sc.Video)
                .WithMany(s => s.PlaylistVideos)
                .HasForeignKey(sc => sc.VideoId);

            modelBuilder.Entity<SourcePlaylistSourceVideo>()
                .HasOne<SourcePlaylist>(sc => sc.Playlist)
                .WithMany(s => s.PlaylistVideos)
                .HasForeignKey(sc => sc.PlaylistId);
        }

        // AP Schema
        public virtual DbSet<ApplicationPlaylist> ApplicationPlaylists { get; set; }
        public virtual DbSet<ApplicationVideo> ApplicationVideos { get; set; }
        public virtual DbSet<ApplicationMetaTagType> ApplicationMetaTagsTypes { get; set; }
        public virtual DbSet<ApplicationMetaTag> ApplicationMetaTags { get; set; }
        public virtual DbSet<ApplicationVideoApplicationMetaTag> ApplicationVideoApplicationMetaTags { get; set; }
        public virtual DbSet<ApplicationGenericTag> ApplicationGenericTags { get; set; }
        public virtual DbSet<ApplicationVideoApplicationGenericTag> ApplicationVideoApplicationGenericTags { get; set; }
        public virtual DbSet<ApplicationPersona> ApplicationPersonas { get; set; }
        public virtual DbSet<ApplicationPersonaVersion> ApplicationPersonaVersions { get; set; }

        // AP-AP Relations Views
        public DbSet<ApplicationPlaylistApplicationVideo> ApplicationPlaylistApplicationVideos { get; set; }
        public DbSet<ApplicationVideoSourceCampaign> ApplicationVideoSourceCampaigns { get; set; }
        public DbSet<ApplicationVideoSourceVideo> ApplicationVideoSourceVideos { get; set; }
        public DbSet<ApplicationPersonaVersionSourceAdSet> ApplicationPersonaVersionSourceAdSets { get; set; }

        // AP-Platform Relations Persistent
        public virtual DbSet<UserApplicationVideoSourceCampaign> UserApplicationVideoSourceCampaigns { get; set; }
        public virtual DbSet<UserApplicationVideoSourceVideo> UserApplicationVideoSourceVideos { get; set; }
        public virtual DbSet<UserApplicationPersonaVersionSourceAdSet> UserApplicationPersonaVersionSourceAdSets { get; set; }

        // AP-Platform Relations Generated
        public virtual DbSet<GeneratedApplicationVideoSourceCampaign> GeneratedApplicationVideoSourceCampaigns { get; set; }
        public virtual DbSet<GeneratedApplicationVideoSourceVideo> GeneratedApplicationVideoSourceVideos { get; set; }
        public virtual DbSet<GeneratedApplicationPlaylistSourcePlaylist> GeneratedApplicationPlaylistSourcePlaylists { get; set; }
        public virtual DbSet<GeneratedApplicationPersonaVersionSourceAdSet> GeneratedApplicationPersonaVersionSourceAdSets { get; set; }

        // Platform Schema
        public virtual DbSet<SourcePlaylist> SourcePlaylists { get; set; }
        public virtual DbSet<SourcePlaylistSourceVideo> SourcePlaylistSourceVideos { get; set; }
        public virtual DbSet<SourceVideo> SourceVideos { get; set; }
        public virtual DbSet<SourceDeltaEncodedVideoMetric> SourceDeltaEncodedVideoMetrics { get; set; }
        public virtual DbSet<SourceVideoDemographicMetric> SourceVideoDemographicMetrics { get; set; }
        public virtual DbSet<SourceAd> SourceAds { get; set; }
        public virtual DbSet<SourceAdMetric> SourceAdMetrics { get; set; }
        public virtual DbSet<SourceCampaign> SourceCampaigns { get; set; }
        public virtual DbSet<SourceAudience> SourceAudiences { get; set; }
        public virtual DbSet<SourceAdSet> SourceAdSets { get; set; }
        public virtual DbSet<JobTrace> JobTraces { get; set; }
        public virtual DbSet<SourceVideoMetric> SourceVideoMetrics { get; set; }

        // Logging
        public virtual DbSet<RuntimeLog> RuntimeLog { get; set; }
    }
}
