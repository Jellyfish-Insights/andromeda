using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using ConsoleApp.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using DataLakeModels.Helpers;
using Common.Jobs;
using Serilog;
using Common.Logging;
using Common;
using Newtonsoft.Json.Linq;

namespace ConsoleApp {

    class Program {

        static void Main(string[] args) {
            var app = new CommandLineApplication();
            app.Name = "ConsoleApp";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() => {
                app.ShowHelp();
                return 0;
            });

            app.Command("jobs", (command) => {
                command.Description = "Execute selected jobs. All by default.";
                command.HelpOption("-?|-h|--help");

                var debugFlag = command.Option("-d|--debug", "Skip actual job execution.", CommandOptionType.NoValue);

                var printDependencyTreeFlag = command.Option("-p|--print-dependency-tree", "Print a Graphviz Dot file of the dependency tree and exit.", CommandOptionType.NoValue);
                var listAvailableJobsFlag = command.Option("-l|--list-available-jobs", "Print the list of available jobs and exit.", CommandOptionType.NoValue);

                var selectedJobNames = command.Option("-j|--job", "Filter jobs by name.", CommandOptionType.MultipleValue);
                var selectedJobType = command.Option("-t|--type", "Filter jobs by type: 'Fetcher' or 'Transformation'.", CommandOptionType.SingleValue);
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

                    var jobType = JobType.All;
                    if (selectedJobType.HasValue()) {
                        if (!Enum.TryParse<JobType>(selectedJobType.Value(), out jobType)) {
                            Console.WriteLine("Invalid job type. Possible values are:");
                            foreach (var x in Enum.GetNames(typeof(JobType))) {
                                Console.WriteLine($"\t{x}");
                            }
                            return 1;
                        }
                    }

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

                    var jobNames = selectedJobNames.HasValue() ? selectedJobNames.Values : new List<string>();

                    if (printDependencyTreeFlag.HasValue()) {
                        RunJobsCommand.PrintDependencyTree(jobType, jobScope, jobNames);
                    } else if (listAvailableJobsFlag.HasValue()) {
                        RunJobsCommand.ListAvalableJobs(jobType, jobScope, jobNames);
                    } else {
                        return RunJobsCommand.RunJobs(jobType, jobScope, jobNames, configuration, debugFlag.HasValue());
                    }
                    return 0;
                });
            });

            app.Command("migrate", (command) => {
                command.Description = "Apply current migrations";
                command.HelpOption("-?|-h|--help");

                var lake = command.Option("--data-lake", "Migrate data-lake databases", CommandOptionType.NoValue);
                var analytics = command.Option("--application", "Migrate application database", CommandOptionType.NoValue);

                command.OnExecute(() => {
                    if (!lake.HasValue() && !analytics.HasValue()) {
                        Console.WriteLine("No database selected");
                        return 1;
                    }
                    if (lake.HasValue()) {
                        MigrateCommand.MigrateDataLake();
                    }
<<<<<<< HEAD
                    if (analytics.HasValue()) {
                        MigrateCommand.MigrateApplication();
                    }
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                    return 0;
                });
            });

            app.Command("init-facebook-lake", (command) => {
                command.Description = "Initialize the Facebook Data Lake database";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() => {
                    MigrateCommand.MigrateFacebook();
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

<<<<<<< HEAD
            app.Command("check-ap", (command) => {
                command.Description = "Report on properties of Application Database";
                command.HelpOption("-?|-h|--help");

                command.OnExecute(() => {
                    ApplicationConsistencyReport.Report();
                    return 0;
                });
            });

            app.Command("application-reset", (command) => {
                command.Description = "Reset application database table";
                command.HelpOption("-?|-h|--help");

                var table = command.Option("-t|--table", "Table to reset.", CommandOptionType.SingleValue);

                command.OnExecute(() => {
                    var allTables = ApplicationResetCommand.GetResetableTables().ToDictionary(x => x.TableName, x => x.Entity);
                    var tables = ApplyTableFilter(table, allTables);

                    if (tables.Any()) {
                        ApplicationResetCommand.ResetTables(tables);
                    } else {
                        Console.WriteLine("Available tables:");
                        foreach (var t in allTables.OrderBy(x => x.Key)) {
                            Console.WriteLine(t.Key);
                        }
                    }
                    return 0;
                });
            });

            app.Command("archive-old-videos", (command) => {
                command.Description = "Archieves all videos published before a specific date (defaults to 2015-06-01).";
                command.HelpOption("-?|-h|--help");

                var dateOption = command.Option("-d|--date", "Table to reset.", CommandOptionType.SingleValue);

                command.OnExecute(() => {
                    var date = dateOption.HasValue() ? DateTime.Parse(dateOption.Value()) : new DateTime(2015, 6, 1);

                    ArchiveVideosCommand.ArchiveVideosPublishedBefore(date);
                    return 0;
                });
            });

=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
            app.Command("check-health-and-send-email", (command) => {
                command.Description = "Check error in system and send a email if find some";
                command.HelpOption("-?|-h|--help");

                var forced = command.Option("-f|--force-send-email", "Force to send a report e-mail", CommandOptionType.NoValue);

                IConfiguration Configuration = new ConfigurationBuilder()
                                                   .SetBasePath(Directory.GetCurrentDirectory())
                                                   .AddJsonFile("appsettings.json")
                                                   .Build();

                var mailConfiguration = Configuration.GetSection("MailService");
                var mailServiceKey = mailConfiguration.GetValue<string>("MailgunKey");
                var emails = new List<string>();
                mailConfiguration.GetSection("EmailsOnCall").Bind(emails);

                command.OnExecute(() => {
                    CheckSystemHealthAndReport.Run(emails.ToArray(), mailServiceKey, DateTime.UtcNow.AddDays(-7), forced.HasValue());
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
