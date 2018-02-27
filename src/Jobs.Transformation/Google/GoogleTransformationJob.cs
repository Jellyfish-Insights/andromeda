using System;
using ApplicationModels;
using ApplicationModels.Models.Metadata;
using DataLakeModels;

namespace Jobs.Transformation.Google {

    public abstract class GoogleTransformationJob<T>: TransformationJob where T : AbstractDataLakeContext, new() {

        public const string PLATFORM_YOUTUBE = "youtube";
        protected abstract Type TargetTable { get; }

        public override void Run() {
            using (var dlContext = new T()) {
                using (var apContext = new ApplicationDbContext())
                    using (var transaction = apContext.Database.BeginTransaction()) {
                        var trace = CreateTrace(TargetTable);
                        ExecuteJob(dlContext, apContext, trace);
                        trace.EndTime = DateTime.UtcNow;
                        apContext.Add(trace);
                        apContext.SaveChanges();
                        transaction.Commit();
                    }
            }
        }

        protected static DateTime ParseDateOrDefault(string s, DateTime def) {
            try {
                return DateTime.ParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            } catch (System.FormatException) {
                return def;
            }
        }

        public abstract void ExecuteJob(T dlContext, ApplicationDbContext apContext, JobTrace trace);
    }

    public abstract class BatchedGoogleTransformationJob<T, K>: TransformationJob where T : AbstractDataLakeContext, new() where K : class {

        public const string PLATFORM_YOUTUBE = "youtube";
        protected abstract Type TargetTable { get; }

        public int BatchSize = 1;

        public override void Run() {
            using (var dlContext = new T()) {
                using (var apContext = new ApplicationDbContext()) {
                    var hasNext = true;
                    K last = null;
                    var batchNum = 0;
                    while (hasNext) {
                        batchNum++;
                        using (var transaction = apContext.Database.BeginTransaction()) {
                            Logger.Debug("Starting batch {BatchNum}", batchNum);
                            var trace = CreateTrace(TargetTable);
                            last = ExecuteJob(dlContext, apContext, trace, last);
                            if (last == null) {
                                hasNext = false;
                            }
                            trace.EndTime = DateTime.UtcNow;
                            apContext.Add(trace);
                            apContext.SaveChanges();
                            transaction.Commit();
                        }
                    }
                }
            }
        }

        public abstract K ExecuteJob(T dlContext, ApplicationDbContext apContext, JobTrace trace, K offset);
    }
}
