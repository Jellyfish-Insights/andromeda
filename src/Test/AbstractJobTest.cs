using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Common.Jobs;
using ConsoleApp.Commands;

namespace Test {

    public class AbstractJobTest {

        [Fact]
        public void SuccessfulJobExecution() {
            var jobList = new AbstractJob[] {
                new JobsHelper.JobA(),
                new JobsHelper.JobB(),
                new JobsHelper.JobC(),
                new JobsHelper.JobD(),
                new JobsHelper.JobE(),
            };

            var jobs = RunJobsCommand.BuildJobsDict(jobList);
            var tasks = RunJobsCommand.ScheduleJobs(jobs);
            RunJobsCommand.WaitAll(tasks);

            foreach (var task in tasks.Values) {
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            }
        }

        [Fact]
        public void FaultyMiddleJobExecution() {
            var jobList = new AbstractJob[] {
                new JobsHelper.JobA(),
                new JobsHelper.JobB(true),
                new JobsHelper.JobC(),
                new JobsHelper.JobD(),
                new JobsHelper.JobE(),
                new JobsHelper.JobF(),
            };

            var jobs = RunJobsCommand.BuildJobsDict(jobList);
            var tasks = RunJobsCommand.ScheduleJobs(jobs);
            RunJobsCommand.WaitAll(tasks);

            var expected = new[] {
                TaskStatus.RanToCompletion,
                TaskStatus.Faulted,
                TaskStatus.RanToCompletion,
                TaskStatus.RanToCompletion,
                TaskStatus.RanToCompletion,
                TaskStatus.RanToCompletion,
            };

            for (var i = 0; i < expected.Count(); i++) {
                Assert.Equal(expected[i], tasks[jobList[i].Id()].Status);
            }
        }
    }
}
