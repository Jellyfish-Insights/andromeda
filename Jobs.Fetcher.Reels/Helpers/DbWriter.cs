using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Helpers;
using DataLakeModels.Models.Reels;
using Npgsql;

using Andromeda.Common;

using Microsoft.EntityFrameworkCore;

namespace Jobs.Fetcher.Reels.Helpers {

    public static class DbWriter {
        //Functions to simplify writing information on DataLake for Tik Tok data. Have writers for every model or specific info when necessary

        public static void WriteUser(User newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.Users.Find(newEntry.Pk);
            Upsert<User, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteReel(Reel newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.Reels.Find(newEntry.Id);
            Upsert<Reel, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteImageVersion(ImageVersion newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.ImageVersions.Find(newEntry.Id);
            Upsert<ImageVersion, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteImages(List<Image> newEntries, DataLakeReelsContext dbContext, Logger logger) {
            foreach (var newEntry in newEntries) {
                var oldEntry = dbContext.Images.Find(newEntry.Id);
                Upsert<Image, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteAnimatedThumbnail(AnimatedThumbnail newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.AnimatedThumbnails.Find(newEntry.Id);
            Upsert<AnimatedThumbnail, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteCaption(Caption newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.Captions.Find(newEntry.Pk);
            Upsert<Caption, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteClipsMeta(ClipsMeta newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.ClipsMetas.Find(newEntry.Id);
            Upsert<ClipsMeta, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteCommentInfo(List<CommentInfo> newEntries, DataLakeReelsContext dbContext, Logger logger) {
            foreach (var newEntry in newEntries) {
                var oldEntry = dbContext.Comments.Find(newEntry.Pk);
                Upsert<CommentInfo, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteConsumptionInfo(ConsumptionInfo newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.ConsumptionInfos.Find(newEntry.Id);
            Upsert<ConsumptionInfo, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteFriction(Friction newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.Frictions.Find(newEntry.Id);
            Upsert<Friction, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteMashupInfo(MashupInfo newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.MashupInfos.Find(newEntry.Id);
            Upsert<MashupInfo, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteOriginalSound(OriginalSound newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.OriginalSounds.Find(newEntry.Id);
            Upsert<OriginalSound, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteReelStats(ReelStats newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var now = DateTime.UtcNow;
            var oldEntry = dbContext.ReelStats.SingleOrDefault(m => m.ReelId == newEntry.ReelId && m.ValidityStart <= now && m.ValidityEnd > now);
            Insert<ReelStats, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteSquareCrop(SquareCrop newEntry, DataLakeReelsContext dbContext, Logger logger) {
            var oldEntry = dbContext.SquareCrops.Find(newEntry.Id);
            Upsert<SquareCrop, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteVideoVersion(List<VideoVersion> newEntries, DataLakeReelsContext dbContext, Logger logger) {
            foreach (var newEntry in newEntries) {
                var oldEntry = dbContext.VideoVersions.Find(newEntry.Id);
                Upsert<VideoVersion, DataLakeReelsContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        private static void Upsert<T, Context>(
            T oldEntry,
            T newEntry,
            DbContext dbContext,
            Logger logger) where T : IEquatable<T> where Context : DbContext {
            var modified = CompareEntries.CompareOldAndNewEntry<T>(oldEntry, newEntry);
            switch (modified) {
                case Modified.New:
                    logger.Debug("Inserting new {Type}: {Id}", typeof(T).Name, newEntry);
                    (dbContext as Context).Add(newEntry);
                    break;
                case Modified.Updated:
                    logger.Debug("Found update to {Type}: {Id}", typeof(T).Name, newEntry);
                    (dbContext as Context).Entry(oldEntry).CurrentValues.SetValues(newEntry);
                    break;
                default:
                    break;
            }
        }

        private static void Insert<T, Context>(
            T oldEntry,
            T newEntry,
            DbContext dbContext,
            Logger logger) where T : IValidityRange, IEquatable<T> where Context : DbContext {
            var modified = CompareEntries.CompareOldAndNewEntry<T>(oldEntry, newEntry);
            switch (modified) {
                case Modified.New:
                    logger.Debug("Inserting new {Type}: {Id}", typeof(T).Name, newEntry);
                    break;
                case Modified.Updated:
                    logger.Debug("Found update to {Type}: {Id}", typeof(T).Name, newEntry);
                    oldEntry.ValidityEnd = newEntry.ValidityStart;
                    (dbContext as Context).Update(oldEntry);
                    break;
                default:
                    return;
            }
            (dbContext as Context).Add(newEntry);
        }
    }
}
