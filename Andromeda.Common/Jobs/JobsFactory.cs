using System.Linq;
using System.Collections.Generic;

namespace Andromeda.Common.Jobs {

    public abstract class JobsFactory {

        public List<AbstractJob> NoJobs = new List<AbstractJob>();

        public abstract JobScope Scope { get; }
        public abstract JobType Type { get; }

        public bool CheckTypeAndScope(JobType type, JobScope scope) {
            return (type != JobType.All && type != Type) || (scope != JobScope.All && scope != Scope);
        }

        public bool CheckNameIsScope(IEnumerable<string> names) {
            foreach (var name in names) {
                if (name.Contains(Scope.ToString()) || name == "All") {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<AbstractJob> FilterByName(IEnumerable<AbstractJob> jobs, IEnumerable<string> names) {
            if (!names.Any() || names.Contains("All")) {
                return jobs;
            } else {
                return jobs.Where(x => names.Where(n => x.Id().EndsWith(n)).Any());
            }
        }

        public abstract IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration config);

        public IEnumerable<AbstractJob> GetAllJobs(JobConfiguration config) {
            return GetJobs(JobType.All, JobScope.All, new string[] {}, config);
        }
    }

    public abstract class TransformationJobsFactory : JobsFactory {
        public override JobType Type { get; } = JobType.Transformation;
    }

    public abstract class FetcherJobsFactory : JobsFactory {
        public override JobType Type { get; } = JobType.Fetcher;
    }
}
