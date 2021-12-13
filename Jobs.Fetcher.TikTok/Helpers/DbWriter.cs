using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Helpers;
using DataLakeModels.Models.Twitter.Data;

using Andromeda.Common;

using Microsoft.EntityFrameworkCore;

namespace Jobs.Fetcher.Twitter.Helpers {

    public static class DbWriter {
        //Functions to simplify writing information on DataLake for Tik Tok data. Have writers for every model or specific info when necessary

        public static void WriteAuthor(Author newEntry, DataLakeTikTokDataContext dbContext, Logger logger){
            var oldEntry = dbContext.Users.Find(newEntry.Id);
            Upsert<User, DataLakeTikTokDataContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }

        /*public static void WriteAuthor(Author newEntry, DataLakeTikTokDataContext dbContext, Logger logger){
            var oldEntry = dbContext.Users.Find(newEntry.Id);
            Upsert<User, DataLakeTikTokDataContext>(oldEntry, newEntry, dbContext, logger);
            dbContext.SaveChanges();
        }*/

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
