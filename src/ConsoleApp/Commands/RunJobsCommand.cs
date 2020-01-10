using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Jobs;
using Jobs.Transformation;
using Jobs.Fetcher.AdWords;
using Jobs.Fetcher.YouTube;
using Jobs.Fetcher.Facebook;

namespace ConsoleApp.Commands {

    public static class RunJobsCommand {

        public static JobsFactory[] Factories = new JobsFactory[] {
            new YouTubeFetchers(),
            new AdWordsFetchers(),
            new FacebookFetchers(),
            new YouTubeTransformations(),
            new AdWordsTransformations(),
            new FacebookTransformations(),
            new ApplicationTransformations(),
        };

        public static Dictionary<string, AbstractJob> BuildJobsDict(IEnumerable<AbstractJob> jobList) {
            var jobDict = new Dictionary<string, AbstractJob>();
            foreach (var job in jobList) {
                jobDict.Add(job.Id(), job);
            }
            return jobDict;
        }

        public static Dictionary<string, Task> ScheduleJobs(Dictionary<string, AbstractJob> jobs, bool debug = false) {

            var tasks = new Dictionary<string, Task>();

            // Declaring before initializing allows it to be called recursivelly.
            Action<string> addToTasks = null;
            addToTasks = (jobId) => {
                if (!tasks.ContainsKey(jobId)) {
                    var dependencies = jobs[jobId].Dependencies().Where(x => jobs.ContainsKey(x)).ToList();

                    if (dependencies.Any()) {
                        dependencies.ForEach(x => addToTasks(x));
                        tasks.Add(jobId, Task.WhenAll(dependencies.Select(x => tasks[x])).ContinueWith(x => jobs[jobId].Execute(debug), TaskContinuationOptions.None));
                    } else {
                        tasks.Add(jobId, Task.Run(() => jobs[jobId].Execute(debug)));
                    }
                }
            };

            foreach (var id in jobs.Keys) {
                addToTasks(id);
            }
            return tasks;
        }

        public static int WaitAll(Dictionary<string, Task> tasks) {
            try {
                Task.WaitAll(tasks.Select(x => x.Value).ToArray());
            } catch (AggregateException e) {
                Console.WriteLine(e.Message);
                foreach (var(jobId, task) in tasks) {
                    Console.WriteLine($"{jobId}: {task.Status}");
                }
                return 1;
            }
            return 0;
        }

        public static IEnumerable<AbstractJob> CreateJobList(
            JobType jobType,
            JobScope jobScope,
            IEnumerable<string> jobNames,
            JobConfiguration configuration
            ) {
            var jobsList = new List<AbstractJob>();
            foreach (var jobFactory in Factories) {
                jobsList.AddRange(jobFactory.GetJobs(jobType, jobScope, jobNames, configuration));
            }
            foreach (var job in jobsList) {
                job.Configuration = configuration;
            }
            return jobsList;
        }

        public static int RunJobs(
            JobType jobType,
            JobScope jobScope,
            IEnumerable<string> jobNames,
            JobConfiguration configuration = null,
            bool debug = false
            ) {
            configuration = configuration ?? JobConstants.DefaultJobConfiguration;
            JobConfiguration.DumpConfiguration(configuration, JobConstants.JobConfigFile);
            Console.WriteLine($"Running jobs with config: {configuration.ToString()}");
            var jobsList = CreateJobList(jobType, jobScope, jobNames, configuration);

            if (jobsList.Any()) {
                var jobs = RunJobsCommand.BuildJobsDict(jobsList);
                var tasks = RunJobsCommand.ScheduleJobs(jobs, debug);
                return RunJobsCommand.WaitAll(tasks);
            } else {
                Console.WriteLine("No job selected. Possible jobs are:");
                Console.WriteLine("TODO");
                return 1;
            }
        }

        public static void PrintDependencyTree(JobType jobType, JobScope jobScope, IEnumerable<string> jobNames) {
            var jobsList = CreateJobList(jobType, jobScope, jobNames, JobConstants.DefaultJobConfiguration);

            Console.WriteLine("digraph {");
            Console.WriteLine("    rankdir=LR;");
            foreach (var job in jobsList) {
                Console.WriteLine($"    \"{job.Id()}\";");
                foreach (var dep in job.Dependencies()) {
                    Console.WriteLine($"    \"{dep}\" -> \"{job.Id()}\";");
                }
            }
            Console.WriteLine("}");
        }

        public static void ListAvalableJobs(JobType jobType, JobScope jobScope, IEnumerable<string> jobNames) {
            var jobsList = CreateJobList(jobType, jobScope, jobNames, JobConstants.DefaultJobConfiguration);
            foreach (var job in jobsList) {
                Console.WriteLine(job.Id());
            }
        }
    }
}
