using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DataLakeModels.Helpers;
using Andromeda.Common.Jobs;
using Andromeda.Common.Logging;
using Andromeda.Commands;
using Serilog;

namespace Andromeda.ConsoleApp {

    class Program {

        static void Main(string[] args) {
            var app = new CommandLineApplication();
            app.Name = "ConsoleApp";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() => {
                app.ShowHelp();
                return 0;
            });

            app.Command("fetcher", (command) => {
                command.Description = "Execute selected Fetcher jobs. All by default.";
                command.HelpOption("-?|-h|--help");

                var debugFlag = command.Option("-d|--debug", "Skip actual job execution.", CommandOptionType.NoValue);

                var printDependencyTreeFlag = command.Option("-p|--print-dependency-tree", "Print a Graphviz Dot file of the dependency tree and exit.", CommandOptionType.NoValue);
                var listAvailableJobsFlag = command.Option("-l|--list-available-jobs", "Print the list of available jobs and exit.", CommandOptionType.NoValue);

                var selectedJobNames = command.Option("-j|--job", "Filter jobs by name.", CommandOptionType.MultipleValue);
                var selectedJobScope = command.Option("-s|--scope", "Filter jobs by scope: 'YouTube', 'AdWords', 'Facebook' or 'Application'.", CommandOptionType.SingleValue);

                var fetchAll = command.Option("-a|--fetch-all", "Fetch daily metrics until completion", CommandOptionType.NoValue);
                var maxEntities = command.Option("-m|--max-entities", "Number of entities to fetch", CommandOptionType.SingleValue);
                var ignoreApi = command.Option("-i|--ignore-api", "Stops execution if request is not found on cache", CommandOptionType.NoValue);

                command.OnExecute(() => {

                    var configuration = new JobConfiguration {
                        IgnoreAPI = ignoreApi.HasValue(),
                        MaxEntities = maxEntities.HasValue() ? Convert.ToInt32(maxEntities.Value()) : 0,
                        Paginate = !fetchAll.HasValue(),
                    };

                    var jobType = JobType.Fetcher;
                    var jobScope = JobScope.All;
                    if (selectedJobScope.HasValue()) {
                        if (!Enum.TryParse<JobScope>(selectedJobScope.Value(), out jobScope)) {
                            Console.WriteLine("Invalid job scope. Possible values are:");
                            foreach (var x in Enum.GetNames(typeof(JobScope))) {
                                Console.WriteLine($"\t{x}");
                            }
                            return 1;
                        }
                    }

                    var jobNames = selectedJobNames.HasValue() ? selectedJobNames.Values : new List<string>{ "All"};

                    if (printDependencyTreeFlag.HasValue()) {
                        RunJobsCommand.PrintDependencyTree(jobType, jobScope, jobNames);
                    } else if (listAvailableJobsFlag.HasValue()) {
                        RunJobsCommand.ListAvailableJobs(jobType, jobScope, jobNames);
                    } else {
                        return RunJobsCommand.RunJobs(jobType, jobScope, jobNames, configuration, debugFlag.HasValue());
                    }
                    return 0;
                });
            });

            app.Command("migrate", (command) => {
                command.Description = "Apply current migrations. All by default.";
                command.HelpOption("-?|-h|--help");

                var lake = command.Option("--data-lake", "Migrate data-lake databases", CommandOptionType.NoValue);
                var facebook = command.Option("--facebook-lake", "Migrate facebook-lake database", CommandOptionType.NoValue);

                command.OnExecute(() => {
                    if (lake.HasValue()) {
                        MigrateCommand.MigrateDataLake();
                    } else if (facebook.HasValue()) {
                        MigrateCommand.MigrateFacebook();
                    } else {
                        MigrateCommand.MigrateDataLake();
                        MigrateCommand.MigrateFacebook();
                    }
                    return 0;
                });
            });

            app.Command("zip-facebook-cache", (command) => {
                command.Description = "Creates zip file with current facebook cache. Useful for recording test data.";
                command.HelpOption("-?|-h|--help");
                var zipDir = command.Argument("cache-dir", "Path to the cache directory");
                var zipName = command.Argument("zip-name", "Name of zip file to be created");

                command.OnExecute(() => {
                    if (zipDir.Value == null || zipName.Value == null) {
                        Console.WriteLine(command.GetHelpText());
                        return 1;
                    }
                    ZipFacebooCacheCommand.ZipCache(zipDir.Value, zipName.Value);
                    Console.WriteLine("Successfully zipped Facebook cache.");
                    return 0;
                });
            });

            app.Command("error-report", (command) => {
                command.Description = "Report on errors occurring during job execution";
                command.HelpOption("-?|-h|--help");

                var since = command.Option("-s|--since", "Start date from which to aggregate logs", CommandOptionType.SingleValue);

                command.OnExecute(() => {
                    var startDate = since.HasValue() ? DateTime.Parse(since.Value()) : DateTime.UtcNow.AddDays(-30);
                    ErrorReportCommand.Report(startDate);
                    return 0;
                });
            });

            app.Command("check-data-lake", (command) => {
                command.Description = "Report on properties of Data Lake table";
                command.HelpOption("-?|-h|--help");

                var table = command.Option("-t|--table", "Table to check.", CommandOptionType.SingleValue);

                command.OnExecute(() => {
                    var allTables = DataLakeIntrospection.GetIValidityRangeEntities().ToDictionary(x => x.Relational().TableName, x => x);
                    var tables = ApplyTableFilter(table, allTables);

                    if (tables.Any()) {
                        ContinuityReportCommand.Report(tables);
                    } else {
                        Console.WriteLine("Available tables:");
                        foreach (var t in allTables.OrderBy(x => x.Key)) {
                            Console.WriteLine(t.Key);
                        }
                    }
                    return 0;
                });
            });

            app.Command("export", (command) =>
            {
                command.Description = "Export Data Lake tables into a file.";
                command.HelpOption("-?|-h|--help");

                var fileType = command.Option("-t|--type", "Select file type: 'csv' or 'json'.", CommandOptionType.SingleValue);
                var source = command.Option("-s|--source", "Select platform to export: 'facebook', 'youtube' or 'adwords'. All by default", CommandOptionType.SingleValue);
                var limit = command.Option("-l|--limit", "Select a limit of rows. 10 by default.", CommandOptionType.SingleValue);

                var platforms = new List<string> {"facebook", "youtube", "adwords", ""};

                command.OnExecute(() => {
                    var selectedFileType = fileType.HasValue() ? fileType.Value() : "json";
                    var selectedPlatform = source.HasValue() ? source.Value() : "";
                    var queryLimit = limit.HasValue() ? Convert.ToInt32(limit.Value()) : 100;

                    if(!platforms.Contains(selectedPlatform)){
                        Console.WriteLine("Invalid source!\n\nList sources:");
                        foreach(var s in platforms) {
                            Console.WriteLine($"\t{s}");
                        }
                        Environment.Exit(1);
                    }

                    ExportData.QueryMetrics(selectedFileType, selectedPlatform, queryLimit);
                    return 0;
                });
            });

            Log.Logger = LoggerFactory.GetFacebookLogger();
            app.Execute(args);
            Log.CloseAndFlush();
        }

        private static List<(string schema, string table, List<string> keys)> ApplyTableFilter(CommandOption table, Dictionary<string, IEntityType> allTables) {
            var tables = new List<(string schema, string table, List<string> keys)>();
            if (table.HasValue()) {
                try {
                    tables.Add(Common.ContextIntrospection.GetDatabaseInfo(allTables[table.Value()]));
                } catch (KeyNotFoundException e) {
                    Console.WriteLine(e.Message);
                }
            } else {
                tables.AddRange(
                    allTables.Select(x => Common.ContextIntrospection.GetDatabaseInfo(x.Value))
                    );
            }
            return tables;
        }
    }
}
