﻿// <auto-generated />
using System;
using DataLakeModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataLakeModels.Migrations.DataLakeYouTubeStudio
{
    [DbContext(typeof(DataLakeYouTubeStudioContext))]
    partial class DataLakeYouTubeStudioContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("youtube_studio_v1")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DataLakeModels.Models.YouTube.Studio.Group", b =>
                {
                    b.Property<string>("GroupId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChannelId");

                    b.Property<DateTime>("RegistrationDate");

                    b.Property<string>("Title");

                    b.Property<DateTime>("UpdateDate");

                    b.HasKey("GroupId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("DataLakeModels.Models.YouTube.Studio.Item", b =>
                {
                    b.Property<string>("ItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ChannelId");

                    b.Property<string>("GroupId");

                    b.Property<DateTime>("RegistrationDate");

                    b.Property<DateTime>("UpdateDate");

                    b.HasKey("ItemId");

                    b.HasIndex("GroupId");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("DataLakeModels.Models.YouTube.Studio.Video", b =>
                {
                    b.Property<string>("ChannelId");

                    b.Property<string>("VideoId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<string>("Metric");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("date");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<double>("Value");

                    b.HasKey("ChannelId", "VideoId", "ValidityStart", "Metric", "EventDate");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("DataLakeModels.Models.YouTube.Studio.Item", b =>
                {
                    b.HasOne("DataLakeModels.Models.YouTube.Studio.Group", "Group")
                        .WithMany("Items")
                        .HasForeignKey("GroupId");
                });
#pragma warning restore 612, 618
        }
    }
}
