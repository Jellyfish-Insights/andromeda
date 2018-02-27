using System;
using ApplicationModels;
using ApplicationModels.Models.Metadata;

namespace Jobs.Transformation.Application {

    public abstract class ApplicationTransformationJob : TransformationJob {

        public abstract Type Target { get; }
        public abstract void Run(ApplicationDbContext context, JobTrace trace);

        public JobTrace CreateTrace() {
            return new JobTrace(this.GetType().Name, Target.Name);
        }

        public override void Run() {
            using (var context = new ApplicationDbContext()) {
                using (var transaction = context.Database.BeginTransaction()) {
                    var trace = CreateTrace();
                    Run(context, trace);
                    trace.EndTime = DateTime.UtcNow;
                    context.Add(trace);
                    context.SaveChanges();
                    transaction.Commit();
                }
            }
        }
    }
}
