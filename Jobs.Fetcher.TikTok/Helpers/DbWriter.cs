using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Helpers;
using DataLakeModels.Models.TikTok;
using Npgsql;

using Andromeda.Common;

using Microsoft.EntityFrameworkCore;

namespace Jobs.Fetcher.TikTok.Helpers {

    public static class DbWriter {
        //Functions to simplify writing information on DataLake for Tik Tok data. Have writers for every model or specific info when necessary

        public static void WriteAuthor(Author newEntry, DataLakeTikTokContext dbContext, Logger logger) {
            var oldEntry = dbContext.Authors.Find(newEntry.Id);
            Upsert<Author, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteAuthorStats(AuthorStats newEntry, DataLakeTikTokContext dbContext, Logger logger) {
            var now = DateTime.UtcNow;
            var oldEntry = dbContext.AuthorStats.SingleOrDefault(m => m.AuthorId == newEntry.AuthorId && m.ValidityStart <= now && m.ValidityEnd > now);
            Insert<AuthorStats, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteChallenges(List<Challenge> newEntries, DataLakeTikTokContext dbContext, Logger logger) {
            foreach (var newEntry in newEntries) {
                var oldEntry = dbContext.Challenges.Find(newEntry.Id);
                Upsert<Challenge, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteEffectStickers(List<EffectSticker> newEntries, DataLakeTikTokContext dbContext, Logger logger) {
            foreach (var newEntry in newEntries) {
                var oldEntry = dbContext.EffectStickers.Find(newEntry.Id);
                Upsert<EffectSticker, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteMusic(Music newEntry, DataLakeTikTokContext dbContext, Logger logger) {
            var oldEntry = dbContext.Music.Find(newEntry.Id);
            Upsert<Music, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WritePost(Post newEntry, DataLakeTikTokContext dbContext, Logger logger) {
            //var oldEntry = dbContext.Posts.Find(newEntry.Id);
            var now = DateTime.UtcNow;
            var oldEntry = dbContext.Posts.SingleOrDefault(m => m.Id == newEntry.Id && m.ValidityStart <= now && m.ValidityEnd > now);
            Insert<Post, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WritePostStats(PostStats newEntry, DataLakeTikTokContext dbContext, Logger logger) {
            var now = DateTime.UtcNow;
            var oldEntry = dbContext.Stats.SingleOrDefault(m => m.PostId == newEntry.PostId && m.ValidityStart <= now && m.ValidityEnd > now);
            Insert<PostStats, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void WriteTags(List<Tag> newEntries, DataLakeTikTokContext dbContext, Logger logger) {
            foreach (var newEntry in newEntries) {
                var oldEntry = dbContext.Tags.Find(newEntry.HashtagId);
                Upsert<Tag, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            }
            dbContext.SaveChanges();
        }

        public static void WriteVideo(Video newEntry, DataLakeTikTokContext dbContext, Logger logger) {
            var oldEntry = dbContext.Videos.Find(newEntry.Id);
            Upsert<Video, DataLakeTikTokContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        public static void InsertUsernameOnScraper(string username, Logger logger) {
            using (var connection = new NpgsqlConnection(DatabaseManager.ConnectionString()))
                using (var cmd = connection.CreateCommand()) {
                    connection.Open();
                    cmd.CommandText = String.Format(@"INSERT INTO account_name (
                                    account_name, 
                                    updated_time
                                ) VALUES (
                                    '{0}',
                                    '{1}'
                                );", username, DateTime.Now
                                                    );
                    cmd.ExecuteNonQuery();
                    logger.Debug("Inserting new username on scrapper: {username}", username);
                }
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
