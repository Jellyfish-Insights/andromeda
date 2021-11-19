﻿// <auto-generated />
using System;
using DataLakeModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataLakeModels.Migrations.DataLakeTwitterData
{
    [DbContext(typeof(DataLakeTwitterDataContext))]
    [Migration("20211110043609_AddTwitterData")]
    partial class AddTwitterData
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("twitter_data_v2")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.Media", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DurationMs");

                    b.Property<int>("Height");

                    b.Property<string>("PreviewImageUrl");

                    b.Property<string>("Type");

                    b.Property<string>("Url");

                    b.Property<int>("Width");

                    b.HasKey("Id");

                    b.ToTable("Medias");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaNonPublicMetrics", b =>
                {
                    b.Property<string>("MediaId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("Playback0Count");

                    b.Property<int>("Playback100Count");

                    b.Property<int>("Playback25Count");

                    b.Property<int>("Playback50Count");

                    b.Property<int>("Playback75Count");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("MediaId", "ValidityStart");

                    b.ToTable("MediaNonPublicMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaOrganicMetrics", b =>
                {
                    b.Property<string>("MediaId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("Playback0Count");

                    b.Property<int>("Playback100Count");

                    b.Property<int>("Playback25Count");

                    b.Property<int>("Playback50Count");

                    b.Property<int>("Playback75Count");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<int>("ViewCount");

                    b.HasKey("MediaId", "ValidityStart");

                    b.ToTable("MediaOrganicMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaPromotedMetrics", b =>
                {
                    b.Property<string>("MediaId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("Playback0Count");

                    b.Property<int>("Playback100Count");

                    b.Property<int>("Playback25Count");

                    b.Property<int>("Playback50Count");

                    b.Property<int>("Playback75Count");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<int>("ViewCount");

                    b.HasKey("MediaId", "ValidityStart");

                    b.ToTable("MediaPromotedMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaPublicMetrics", b =>
                {
                    b.Property<string>("MediaId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<int>("ViewCount");

                    b.HasKey("MediaId", "ValidityStart");

                    b.ToTable("MediaPublicMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.Tweet", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConversationId");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("InReplyToUserId");

                    b.Property<string>("Lang");

                    b.Property<bool>("PossiblySensitive");

                    b.Property<string>("Source");

                    b.Property<string>("Text");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Tweets");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetMedia", b =>
                {
                    b.Property<string>("TweetId");

                    b.Property<string>("MediaId");

                    b.HasKey("TweetId", "MediaId");

                    b.HasIndex("MediaId");

                    b.ToTable("TweetMedia");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetNonPublicMetrics", b =>
                {
                    b.Property<string>("TweetId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("ImpressionCount");

                    b.Property<int>("UrlLinkClicks");

                    b.Property<int>("UserProfileClicks");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("TweetId", "ValidityStart");

                    b.ToTable("TweetNonPublicMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetOrganicMetrics", b =>
                {
                    b.Property<string>("TweetId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("ImpressionCount");

                    b.Property<int>("LikeCount");

                    b.Property<int>("ReplyCount");

                    b.Property<int>("RetweetCount");

                    b.Property<int>("UrlLinkClicks");

                    b.Property<int>("UserProfileClicks");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("TweetId", "ValidityStart");

                    b.ToTable("TweetOrganicMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetPromotedMetrics", b =>
                {
                    b.Property<string>("TweetId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("ImpressionCount");

                    b.Property<int>("LikeCount");

                    b.Property<int>("ReplyCount");

                    b.Property<int>("RetweetCount");

                    b.Property<int>("UrlLinkClicks");

                    b.Property<int>("UserProfileClicks");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("TweetId", "ValidityStart");

                    b.ToTable("TweetPromotedMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetPublicMetrics", b =>
                {
                    b.Property<string>("TweetId");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("LikeCount");

                    b.Property<int>("QuoteCount");

                    b.Property<int>("ReplyCount");

                    b.Property<int>("RetweetCount");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("TweetId", "ValidityStart");

                    b.ToTable("TweetPublicMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("IsProtected");

                    b.Property<string>("Location");

                    b.Property<string>("Name");

                    b.Property<string>("ProfileImageUrl");

                    b.Property<string>("Url");

                    b.Property<string>("Username");

                    b.Property<bool>("Verified");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaNonPublicMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Media", "Media")
                        .WithMany("NonPublicMetrics")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaOrganicMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Media", "Media")
                        .WithMany("OrganicMetrics")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaPromotedMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Media", "Media")
                        .WithMany("PromotedMetrics")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.MediaPublicMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Media", "Media")
                        .WithMany("PublicMetrics")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.Tweet", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.User", "User")
                        .WithMany("Tweets")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetMedia", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Media", "Media")
                        .WithMany("TweetMedia")
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DataLakeModels.Models.Twitter.Data.Tweet", "Tweet")
                        .WithMany("TweetMedia")
                        .HasForeignKey("TweetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetNonPublicMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Tweet", "Tweet")
                        .WithMany("NonPublicMetrics")
                        .HasForeignKey("TweetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetOrganicMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Tweet", "Tweet")
                        .WithMany("OrganicMetrics")
                        .HasForeignKey("TweetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetPromotedMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Tweet", "Tweet")
                        .WithMany("PromotedMetrics")
                        .HasForeignKey("TweetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Data.TweetPublicMetrics", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Data.Tweet", "Tweet")
                        .WithMany("PublicMetrics")
                        .HasForeignKey("TweetId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
