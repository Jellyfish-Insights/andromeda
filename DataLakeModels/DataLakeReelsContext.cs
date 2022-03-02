using System;
using Microsoft.EntityFrameworkCore;
using DataLakeModels.Models.Reels;

namespace DataLakeModels {

    public partial class DataLakeReelsContext : AbstractDataLakeContext {

        public static string ReelsScraperVersion = "1";

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema($"reels_v{ReelsScraperVersion}");

            modelBuilder.Entity<AnimatedThumbnail>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Caption>()
                .HasKey(table => new { table.Pk });

            modelBuilder.Entity<ClipsMeta>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<CommentInfo>()
                .HasKey(table => new { table.Pk });

            modelBuilder.Entity<CommentInfo>()
                .HasOne(table => table.Reel)
                .WithMany(reel => reel.Comments)
                .HasForeignKey(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConsumptionInfo>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Friction>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<ImageVersion>()
                .HasKey(table => new { table.Id });
            
            modelBuilder.Entity<Image>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Image>()
                .HasOne(table => table.ImageVersion)
                .WithMany(imageVersion => imageVersion.Candidates)
                .HasForeignKey(table => table.ImageVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnimatedThumbnail>()
                .HasOne(table => table.ImageVersion)
                .WithOne(imageVersion => imageVersion.AnimatedThumbnailSpritesheetInfo)
                .HasForeignKey<AnimatedThumbnail>(table => table.ImageVersionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MashupInfo>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<MashupInfo>()
                .HasOne(table => table.Clip)
                .WithOne(clip => clip.MashupInfo)
                .HasForeignKey<MashupInfo>(table => table.ClipsId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OriginalSound>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<OriginalSound>()
                .HasOne(table => table.User)
                .WithMany(reel => reel.Sounds)
                .HasForeignKey(table => table.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reel>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<Reel>()
                .HasOne(table => table.User)
                .WithMany(reel => reel.Reels)
                .HasForeignKey(table => table.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Reel>()
                .HasOne(table => table.Caption)
                .WithOne(caption => caption.Reel)
                .HasForeignKey<Caption>(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<Reel>()
                .HasOne(table => table.ClipsMetaData)
                .WithOne(clipsMeta => clipsMeta.Reel)
                .HasForeignKey<ClipsMeta>(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Reel>()
                .HasOne(table => table.ImageVersions)
                .WithOne(imageVersion => imageVersion.Reel)
                .HasForeignKey<ImageVersion>(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Reel>()
                .HasOne(table => table.MediaCroppingInfo)
                .WithOne(squareCrop => squareCrop.Reel)
                .HasForeignKey<SquareCrop>(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Reel>()
                .HasOne(table => table.SharingFrictionInfo)
                .WithOne(friction => friction.Reel)
                .HasForeignKey<Friction>(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);

            
            modelBuilder.Entity<ReelStats>()
                .HasKey(table => new { table.ReelId, table.ValidityStart });

            modelBuilder.Entity<ReelStats>()
                .HasOne(table => table.Reel)
                .WithMany(reel => reel.Stats)
                .HasForeignKey(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasKey(table => new { table.Pk });

            modelBuilder.Entity<SquareCrop>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<VideoVersion>()
                .HasKey(table => new { table.Id });

            modelBuilder.Entity<VideoVersion>()
                .HasOne(table => table.Reel)
                .WithMany(reel => reel.VideoVersions)
                .HasForeignKey(table => table.ReelId)
                .OnDelete(DeleteBehavior.Cascade);


        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Reel> Reels { get; set; }
        public virtual DbSet<ImageVersion> ImageVersions { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<AnimatedThumbnail> AnimatedThumbnails { get; set; }
        public virtual DbSet<Caption> Captions { get; set; }
        public virtual DbSet<ClipsMeta> ClipsMetas { get; set; }
        public virtual DbSet<CommentInfo> Comments { get; set; }
        public virtual DbSet<ConsumptionInfo> ConsumptionInfos { get; set; }
        public virtual DbSet<Friction> Frictions { get; set; }
        public virtual DbSet<MashupInfo> MashupInfos { get; set; }
        public virtual DbSet<OriginalSound> OriginalSounds { get; set; }
        public virtual DbSet<ReelStats> ReelStats { get; set; }
        public virtual DbSet<SquareCrop> SquareCrops { get; set; }
        public virtual DbSet<VideoVersion> VideoVersions { get; set; }
    }
}
