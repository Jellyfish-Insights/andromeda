using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
<<<<<<< HEAD
using ApplicationModels;
using Common.Logging.Models;
using Common.Report;
using Common.Jobs;
using DataLakeModels;
using Microsoft.EntityFrameworkCore;
using Common;
using Npgsql;
using Jobs.Transformation.Facebook;
=======
using Common.Logging.Models;
using Common.Report;
using Common.Jobs;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using DataLakeModels.Helpers;
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
using Microsoft.Extensions.Configuration;

namespace ConsoleApp.Commands {

    public static class ErrorReportCommand {

        public class ReportRow {
            public string FullName;
            public string FirstName;
            public string SurName;
            public int ErrorCount;
            public DateTime LastSuccess;
            public DateTime LastFailure;
            public string LastException;
        }

<<<<<<< HEAD
        private static Tuple<string, string> GetDatabaseConnectionStrings() {
=======
        private static string GetDatabaseConnectionStrings() {
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
            IConfiguration Configuration = new ConfigurationBuilder()
                                               .SetBasePath(Directory.GetCurrentDirectory())
                                               .AddJsonFile("appsettings.json")
                                               .Build();

            var ConnectionStringsConfiguration = Configuration.GetSection("ConnectionStrings");
<<<<<<< HEAD
            var APConnectionString = ConnectionStringsConfiguration.GetValue<string>("BusinessDatabase");
            var DLConnectionString = ConnectionStringsConfiguration.GetValue<string>("DataLakeDatabase");

            return Tuple.Create(APConnectionString, DLConnectionString);
=======
            var DLConnectionString = ConnectionStringsConfiguration.GetValue<string>("DataLakeDatabase");

            return DLConnectionString;
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
        }

        private static List<ReportRow> GetLogs(String schema, DateTime startDate, String connectionString, List<String> jobsList) {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $@"
                SELECT  Jobs.Name,
		                T.ErrorCount,
		                T.LastFailure,
		                T.LastException,
		                TT.LastSuccess
                FROM (VALUES ('{String.Join("'), ('", jobsList)}')) AS Jobs (Name)
                LEFT JOIN(
	                SELECT  ""Name"",
		                    Count(*) as ErrorCount,
		                    MAX(""When"") as LastFailure,
		                    MAX(""Exception"") as LastException
	                FROM {schema}.""RuntimeLog""
	                WHERE ""When"" >= @startDate and ""Message"" like 'Failed.%'
	                GROUP BY ""Name"") as T on Jobs.Name = T.""Name""
                LEFT JOIN(
	                SELECT  ""Name"",
		                    MAX(""When"") as LastSuccess
	                FROM {schema}.""RuntimeLog""
	                WHERE ""When"" >= @startDate and ""Message"" like 'Finished.%'
	                GROUP BY ""Name"") as TT on Jobs.Name = TT.""Name""
                ";

                cmd.Parameters.AddWithValue("startDate", startDate);
                using (var reader = cmd.ExecuteReader()) {
                    var report = new List<ReportRow>();
                    while (reader.Read()) {
                        report.Add(new ReportRow(){
                            FullName = reader.Prim<string>("Name"),
                            ErrorCount = reader.OptClass<int>("errorcount"),
                            LastSuccess = reader.OptClass<DateTime>("lastsuccess"),
                            LastFailure = reader.OptClass<DateTime>("lastfailure"),
                            LastException = reader.OptClass<string>("lastexception") ?? "---"
                        });
                    }
                    return report;
                }
            }
        }

        public static List<ReportRow> GetReportData(DateTime startDate) {
<<<<<<< HEAD
            var ConnectionStrings = GetDatabaseConnectionStrings();
            var APConnectionString = ConnectionStrings.Item1;
            var DLConnectionString = ConnectionStrings.Item2;

            var jobsList = RunJobsCommand.CreateJobList(JobType.All, JobScope.All, new List<string>(), JobConstants.DefaultJobConfiguration);
            var APJobs = jobsList.Select(x => x.Id()).Where(x => x.Contains("Jobs.Transformation")).ToList();
            var DLJobs = jobsList.Select(x => x.Id()).Where(x => x.Contains("Jobs.Fetcher")).ToList();

            var apLogEntries = GetLogs("application", startDate, APConnectionString, APJobs);
            var dlLogEntries = GetLogs("logging", startDate, DLConnectionString, DLJobs);

            var all = dlLogEntries.Union(apLogEntries).OrderBy(x => x.FullName).ToList();
            all.ForEach(x => {
                (x.FirstName, x.SurName) = RuntimeLog.ParseLogName(x.FullName, '.', 3);
            });
            return all;
=======
            var DLConnectionString = GetDatabaseConnectionStrings();

            var jobsList = RunJobsCommand.CreateJobList(JobType.All, JobScope.All, new List<string>(), JobConstants.DefaultJobConfiguration);
            var DLJobs = jobsList.Select(x => x.Id()).Where(x => x.Contains("Jobs.Fetcher")).ToList();

            var dlLogEntries = GetLogs("logging", startDate, DLConnectionString, DLJobs);
            dlLogEntries.ForEach(x => {
                (x.FirstName, x.SurName) = RuntimeLog.ParseLogName(x.FullName, '.', 3);
            });
            return dlLogEntries;
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
        }

        public static void Report(DateTime startDate) {
            Console.WriteLine($"Since: {startDate}");
            foreach (var reportRows in GetReportData(startDate).GroupBy(x => x.FirstName)) {
                var firstName = reportRows.Key;
                Console.WriteLine($"\n{firstName}\n");
                var table = new List<List<string>>(){
                    new List<string>() {
                        "Job Name",
                        "Error Count",
                        "Last Finished",
                        "Last Failed",
                        "Last Error"
                    }
                };

                foreach (var row in reportRows) {
                    table.Add(new List<string>(){
                        row.SurName,
                        row.ErrorCount.ToString(),
                        (row.LastSuccess.ToString() == "1/1/01 12:00:00 AM" ? "---" : row.LastSuccess.ToString()),
                        (row.LastFailure.ToString() == "1/1/01 12:00:00 AM" ? "---" : row.LastFailure.ToString()),
                        row.LastException.Split("\n").First() ?? ""
                    });
                }
                ConsoleReport.WriteTable(table);
            }
        }
    }
}
