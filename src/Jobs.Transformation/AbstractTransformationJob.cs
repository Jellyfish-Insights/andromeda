using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ApplicationModels;
using ApplicationModels.Extensions;
using ApplicationModels.Models.Metadata;
using DataLakeModels.Models;
using NpgsqlTypes;
using Serilog.Core;
using Common.Logging;
using Common.Jobs;

namespace Jobs.Transformation {

    public delegate T UpdateEntity<T>(T value);

    public delegate bool MatchEntity<T>(T value);

    public abstract class TransformationJob : AbstractJob {

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<ApplicationDbContext>(Id());
        }

        public static T SaveMutableEntity<T>(ApplicationDbContext context, JobTrace trace, IEnumerable<T> storedObject, EntityUpdateParams<T> updateParams) where T : class, IMutableEntity, new() {
            return SaveMutableEntity(context, x => updateParams.UpdateFunction(x), trace, storedObject, updateParams.ObjectValidity, updateParams.Trace);
        }

        public static T SaveMutableEntity<T>(ApplicationDbContext context, UpdateEntity<T> updateFunction, JobTrace trace, IEnumerable<T> storedObject, NpgsqlRange<DateTime> objectValidity, RowLog log = null) where T : class, IMutableEntity, new() {
            var mod = ClassifyModification(storedObject, objectValidity);
            var entity = FirstOrCreate<T>(mod, storedObject);
            T newObject = null;
            switch (mod) {
                case Modified.New: {
                    newObject = updateFunction(entity);
                    log = log ?? new RowLog();
                    log.Id = entity.PrimaryKey;
                    log.NewVersion = entity.UpdateDate;
                    context.Add(newObject);
                    trace.Add(log);
                }
                break;
                case Modified.Updated: {
                    var oldVersion = entity.UpdateDate;
                    newObject = updateFunction(entity);
                    log = log ?? new RowLog();
                    log.Id = entity.PrimaryKey;
                    log.OldVersion = oldVersion;
                    log.NewVersion = entity.UpdateDate;
                    trace.Add(log);
                }
                break;
                default:
                    break;
            }
            return newObject;
        }

        public static Modified ClassifyModification(IEnumerable<IMutableEntity> entity, NpgsqlRange<DateTime> range) {
            if (!entity.Any()) {
                return Modified.New;
            } else if (!NpgsqlRangeExtensions.Includes(range, entity.First().UpdateDate)) {
                return Modified.Updated;
            } else {
                return Modified.Equal;
            }
        }

        public static T FirstOrCreate<T>(Modified comparisonResult, IEnumerable<T> storedObject) where T : class, new() {
            return comparisonResult == Modified.New ? new T() : storedObject.First();
        }

        public JobTrace CreateTrace(Type table) {
            return new JobTrace(this.GetType().Name, table.Name);
        }
    }

    public class EntityUpdateParams<T> where T : IMutableEntity {
        public Func<T, T> UpdateFunction { get; set; }
        public Expression<Func<T, bool>> MatchFunction { get; set; }
        public NpgsqlRange<DateTime> ObjectValidity { get; set; }
        public RowLog Trace { get; set; }
    }
}
