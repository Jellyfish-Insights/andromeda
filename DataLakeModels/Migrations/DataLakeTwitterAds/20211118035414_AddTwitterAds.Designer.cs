﻿// <auto-generated />
using System;
using DataLakeModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DataLakeModels.Migrations.DataLakeTwitterAds
{
    [DbContext(typeof(DataLakeTwitterAdsContext))]
    [Migration("20211118035414_AddTwitterAds")]
    partial class AddTwitterAds
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("twitter_ads_v10")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.AdsAccount", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApprovalStatus");

                    b.Property<string>("BusinessId");

                    b.Property<string>("BusinessName");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name");

                    b.Property<string>("TimeZone");

                    b.Property<string>("TimeZoneSwitchAt");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<string>("UserId");

                    b.Property<string>("Username");

                    b.HasKey("Id");

                    b.ToTable("AdsAccounts");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.Campaign", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<string>("AdsAccountId");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("Currency");

                    b.Property<long>("DailyBudgetAmountLocalMicro");

                    b.Property<bool>("Deleted");

                    b.Property<int?>("DurationInDays");

                    b.Property<string>("EffectiveStatus");

                    b.Property<DateTimeOffset?>("EndTime");

                    b.Property<string>("EntityStatus");

                    b.Property<int?>("FrequencyCap");

                    b.Property<string>("FundingInstrumentId");

                    b.Property<string>("Name");

                    b.Property<string>("PurchaseOrderNumber");

                    b.Property<bool>("Servable");

                    b.Property<bool>("StandardDelivery");

                    b.Property<DateTimeOffset>("StartTime");

                    b.Property<long?>("TotalBudgetAmountLocalMicro");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("Id", "ValidityStart");

                    b.HasIndex("AdsAccountId");

                    b.ToTable("Campaigns");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.CustomAudience", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AudienceSize");

                    b.Property<string>("AudienceType");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.Property<string>("OwnerAccountId");

                    b.Property<string>("PartnerSource");

                    b.Property<string>("PermissionLevel");

                    b.Property<string[]>("ReasonsNotTargetable");

                    b.Property<bool>("Targetable");

                    b.Property<string[]>("TargetableTypes");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("CustomAudiences");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.LineItem", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<string>("AdvertiserDomain");

                    b.Property<long?>("BidAmountLocalMicro");

                    b.Property<string>("BidStrategy");

                    b.Property<string>("CampaignId");

                    b.Property<string>("CampaignId1");

                    b.Property<DateTime?>("CampaignValidityStart");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("CreativeSource");

                    b.Property<string>("Currency");

                    b.Property<bool>("Deleted");

                    b.Property<int?>("DurationInDays");

                    b.Property<DateTimeOffset?>("EndTime");

                    b.Property<string>("EntityStatus");

                    b.Property<int?>("FrequencyCap");

                    b.Property<string>("Goal");

                    b.Property<string>("Name");

                    b.Property<string>("Objective");

                    b.Property<string>("PayBy");

                    b.Property<string>("PrimaryWebEventTag");

                    b.Property<string>("ProductType");

                    b.Property<DateTimeOffset?>("StartTime");

                    b.Property<long?>("TargetCpaLocalMicro");

                    b.Property<long?>("TotalBudgetAmountLocalMicro");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("Id", "ValidityStart");

                    b.HasIndex("CampaignId1", "CampaignValidityStart");

                    b.ToTable("LineItems");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.OrganicTweetDailyMetrics", b =>
                {
                    b.Property<string>("TweetId");

                    b.Property<DateTime>("Date");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("AppClicks");

                    b.Property<int>("CardEngagements");

                    b.Property<int>("Clicks");

                    b.Property<int>("Engagements");

                    b.Property<int>("Follows");

                    b.Property<int>("Impressions");

                    b.Property<int>("Likes");

                    b.Property<int>("QualifiedImpressions");

                    b.Property<int>("Replies");

                    b.Property<int>("Retweets");

                    b.Property<int>("UrlClicks");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<int>("Video15sViews");

                    b.Property<int>("Video3s100pctViews");

                    b.Property<int>("Video6sViews");

                    b.Property<int>("VideoContentStarts");

                    b.Property<int>("VideoCtaClicks");

                    b.Property<int>("VideoTotalViews");

                    b.Property<int>("VideoViews100");

                    b.Property<int>("VideoViews25");

                    b.Property<int>("VideoViews50");

                    b.Property<int>("VideoViews75");

                    b.HasKey("TweetId", "Date", "ValidityStart");

                    b.ToTable("OrganicTweetDailyMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.PromotedTweet", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<string>("ApprovalStatus");

                    b.Property<string>("CampaignId");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("EntityStatus");

                    b.Property<string>("LineItemId");

                    b.Property<string>("LineItemId1");

                    b.Property<DateTime?>("LineItemValidityStart");

                    b.Property<string>("TweetId");

                    b.Property<string>("TweetIdSt");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("Id", "ValidityStart");

                    b.HasIndex("LineItemId1", "LineItemValidityStart");

                    b.ToTable("PromotedTweets");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.PromotedTweetDailyMetrics", b =>
                {
                    b.Property<string>("PromotedTweetId");

                    b.Property<DateTime>("Date");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<int>("AppClicks");

                    b.Property<long>("BilledChargeLocalMicro");

                    b.Property<int>("BilledEngagements");

                    b.Property<int>("CardEngagements");

                    b.Property<int>("Clicks");

                    b.Property<int>("Engagements");

                    b.Property<int>("Follows");

                    b.Property<int>("Impressions");

                    b.Property<int>("Likes");

                    b.Property<int>("MediaEngagements");

                    b.Property<int>("MediaViews");

                    b.Property<int>("QualifiedImpressions");

                    b.Property<int>("Replies");

                    b.Property<int>("Retweets");

                    b.Property<int>("UrlClicks");

                    b.Property<DateTime>("ValidityEnd");

                    b.Property<int>("Video15sViews");

                    b.Property<int>("Video3s100pctViews");

                    b.Property<int>("Video6sViews");

                    b.Property<int>("VideoContentStarts");

                    b.Property<int>("VideoCtaClicks");

                    b.Property<int>("VideoTotalViews");

                    b.Property<int>("VideoViews100");

                    b.Property<int>("VideoViews25");

                    b.Property<int>("VideoViews50");

                    b.Property<int>("VideoViews75");

                    b.HasKey("PromotedTweetId", "Date", "ValidityStart");

                    b.ToTable("PromotedTweetDailyMetrics");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.VideoLibrary", b =>
                {
                    b.Property<string>("Id");

                    b.Property<DateTime>("ValidityStart");

                    b.Property<string>("AspectRatio");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Description");

                    b.Property<long>("Duration");

                    b.Property<string>("FileName");

                    b.Property<string>("MediaStatus");

                    b.Property<string>("MediaUrl");

                    b.Property<string>("Name");

                    b.Property<string>("PosterMediaKey");

                    b.Property<string>("PosterMediaUrl");

                    b.Property<string>("Title");

                    b.Property<bool>("Tweeted");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<string>("Username");

                    b.Property<DateTime>("ValidityEnd");

                    b.HasKey("Id", "ValidityStart");

                    b.ToTable("VideoLibraries");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.Campaign", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Ads.AdsAccount", "AdsAccount")
                        .WithMany("Campaigns")
                        .HasForeignKey("AdsAccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.LineItem", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Ads.Campaign", "Campaign")
                        .WithMany("LineItems")
                        .HasForeignKey("CampaignId1", "CampaignValidityStart");
                });

            modelBuilder.Entity("DataLakeModels.Models.Twitter.Ads.PromotedTweet", b =>
                {
                    b.HasOne("DataLakeModels.Models.Twitter.Ads.LineItem", "LineItem")
                        .WithMany("PromotedTweets")
                        .HasForeignKey("LineItemId1", "LineItemValidityStart");
                });
#pragma warning restore 612, 618
        }
    }
}
