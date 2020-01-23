using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
<<<<<<< HEAD
using ApplicationModels.Helpers;
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DataLakeModels.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp.Commands {

    public static class CheckSystemHealthAndReport {
        private static string reportPath = $"{Directory.GetCurrentDirectory()}/logs/health-check-report.json";

        private static Tuple<List<ErrorReportCommand.ReportRow>, bool> GetJobsHealthInfo(DateTime startDate) {
            var reports = ErrorReportCommand.GetReportData(startDate);
            var status = reports.Select(r => r.ErrorCount).Sum() == 0 &&
                         reports.Select(r => r.LastSuccess).Where(r => r.ToString() == "1/1/01 12:00:00 AM").Count() == 0;
            return Tuple.Create(reports, status);
        }

<<<<<<< HEAD
        private static Tuple<List<Report>, bool> GetApHealthInfo() {
            var reports = new List<Report>(){
                ApplicationConsistencyReport.CheckForOrphanApplicationVideos()
            };
            var status = reports.Where(x => x.Status == ReportStatus.FAILED).Count() == 0;
            return Tuple.Create(reports, status);
        }

=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
        private static Tuple<List<List<string>>, bool> GetDataLakeHealthInfo() {
            var allTables = DataLakeIntrospection.GetIValidityRangeEntities().ToDictionary(x => x.Relational().TableName, x => x);
            var tables = new List<(string schema, string table, List<string> keys)>();
            tables.AddRange(
                allTables.Select(x => Common.ContextIntrospection.GetDatabaseInfo(x.Value))
                );

            var reports = ContinuityReportCommand.GetDataLakeReportTable(tables);
            // Test if Contains a false value in report
            var status = !reports.Select(x => x.Contains("False")).Contains(true);
            return Tuple.Create(reports, status);
        }

        private static String GetHtmlSummary(DateTime startDate, bool systemStatus) {
            var rowPadding = "style='padding-left:25px;";
            var status = systemStatus ? $"color:green;'> OK" : $"color:orange;'> Warning";

            var summary = $@"
                        <h2 style='text-align:center;'> Health Check on YEAR-AP System </h2>
                        <table>
                            <tr style='font-size:18px;'>
                                <td><b> Status: </b></td>
                                <td {rowPadding}font-weight:bold; {status} </td>
                            </tr>
                            <tr>
                                <td><b>Date range:</b></td>
                                <td {rowPadding}'> {DateTime.UtcNow.ToString("yyyy-MM-dd")} ~ {startDate.ToString("yyyy-MM-dd")} </td>
                            </tr>
                            <tr>
                                <td><b>CommitHash:</b></td>
                                <td {rowPadding}'> {VersioningHelper.GitCommitHash} </td>
                            </tr>
                        </table>
                    ";
            return String.Join("\n", summary);
        }

        private static String GetHtmlJobsReport(DateTime startDate, List<ErrorReportCommand.ReportRow> reports) {
            var rowPadding = "style='padding-left:25px;";
            var totalErrors = reports.Select(r => r.ErrorCount).Sum();
            var totalNotFinishedJobs = reports.Where(r => r.LastSuccess.ToString() == "1/1/01 12:00:00 AM").Count();
            var totalFailedJobs = reports.Where(r => r.LastFailure > startDate).Count();

            var errorReport = new List<string>() {
                $@"
                        <h2 style='text-align:center;'> Jobs Error Report </h2>
                        <table>
                            <tr>
                                <td><b>Total Jobs failed:</b></td>
                                <td {rowPadding}'> {totalFailedJobs} </td>
                            </tr>
                            <tr>
                                <td><b>Total Jobs not executed:</b></td>
                                <td {rowPadding}'> {totalNotFinishedJobs} </td>
                            </tr>
                            <tr>
                                <td><b>Total Errors:</b></td>
                                <td {rowPadding}'> {totalErrors} </td>
                            </tr>
                    "
            };

            foreach (var reportRows in reports.GroupBy(x => x.FirstName)) {
                var firstName = reportRows.Key;
                errorReport.Add($@"
                            <tr>
                                <b> {firstName} </b>
                            </tr>
                            <tr>
                                <td></td>
                                <td {rowPadding}'><b>Job Name</b></td>
                                <td {rowPadding}'><b>Last Finished</b></td>
                                <td {rowPadding}'><b>Last Failed</b></td>
                                <td {rowPadding}'><b>Error Count</b></td>
                            </tr>
                        ");

                foreach (var row in reportRows) {
                    var lastFinished = row.LastSuccess.ToString() == "1/1/01 12:00:00 AM" ? "---" : row.LastSuccess.ToString();
                    var lastFailed = row.LastFailure.ToString() == "1/1/01 12:00:00 AM" ? "---" : row.LastFailure.ToString();
                    errorReport.Add($@"
                            <tr>
                                <td></td>
                                <td {rowPadding}'>{row.SurName}</td>
                                <td {rowPadding}'>{lastFinished}</td>
                                <td {rowPadding}'>{lastFailed}</td>
                                <td {rowPadding}'>{row.ErrorCount}</td>
                            </tr>
                        ");
                }
            }
            errorReport.Add("</table>");
            return String.Join("\n", errorReport);
        }

<<<<<<< HEAD
        private static String GetHtmlCheckApReport(List<Report> reports) {
            var rowPadding = "style='padding-left:75px;";

            var errorReport = new List<string>() {
                $@"
                        <h2 style='text-align:center;'> Check AP Report </h2>
                        <table>
                    "
            };

            foreach (var row in reports) {
                errorReport.Add($@"
                            <tr>
                                <td><b> {row.Title} </b></td>
                            </tr>
                            <tr>
                                <td><b> Status </b></td>
                                <td> {row.Status}</td>
                            </tr>
                             <tr>
                                <td><b> Description </b></td>
                                <td> {row.Description}</td>
                            </tr>
                            </table><table>
                            <tr>
                                <td style='padding-left:75px;'></td>
                                <td {rowPadding}'><b> {row.Data[0][0]} </b></td>
                                <td {rowPadding}'><b> {row.Data[0][1]} </b></td>
                                <td {rowPadding}'><b> {row.Data[0][2]} </b></td>
                            </tr>
                        ");
                for (int i = 1; i < row.Data.Count(); i++) {
                    var title = row.Data[i][1].Count() > 50 ? $"{row.Data[i][1].Substring(0, 50)}(...)" : row.Data[i][1];
                    errorReport.Add($@"
                            <tr>
                                <td style='padding-left:75px;'></td>
                                <td {rowPadding}'>{row.Data[i][0]} </td>
                                <td {rowPadding}'>{title} </td>
                                <td {rowPadding}'>{row.Data[i][2]} </td>
                            </tr>
                            ");
                }
            }
            errorReport.Add("</table>");
            return String.Join("\n", errorReport);
        }

=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
        private static String GetHtmlCheckDataLakeReport(List<List<string>> reports) {
            var rowPadding = "style='padding-right:50px;";

            var errorReport = new List<string>() {
                $@"
                        <h2 style='text-align:center;'> Check Data Lake Report </h2>
                        <table>
                            <tr>
                                <td {rowPadding}'><b> {reports[0][0]} </b></td>
                                <td {rowPadding}'><b> {reports[0][1]} </b></td>
                                <td {rowPadding}'><b> {reports[0][2]} </b></td>
                                <td {rowPadding}'><b> {reports[0][3]} </b></td>
                            </tr>"
            };

            for (int i = 1; i < reports.Count(); i++) {
                errorReport.Add($@"
                            <tr>
                                <td {rowPadding}'> {reports[i][0]} </td>
                                <td {rowPadding}'> {reports[i][1]} </td>
                                <td {rowPadding}'> {reports[i][2]} </td>
                                <td {rowPadding}'> {reports[i][3]} </td>
                            </tr>
                        ");
            }
            errorReport.Add("</table");
            return String.Join("\n", errorReport);
        }

        private static void SendReportEmail(string[] emails, string mailServiceKey, DateTime startDate,
                                            Tuple<List<ErrorReportCommand.ReportRow>, bool> jobsReportInfo,
<<<<<<< HEAD
                                            Tuple<List<Report>, bool> apReportInfo,
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                                            Tuple<List<List<string>>, bool> dataLakeReportInfo,
                                            bool systemStatus) {

            var mailgun = new MailgunEmailService(mailServiceKey);
            var plainText = String.Concat(GetHtmlSummary(startDate, systemStatus)
                                          , GetHtmlJobsReport(startDate, jobsReportInfo.Item1)
<<<<<<< HEAD
                                          , GetHtmlCheckApReport(apReportInfo.Item1)
=======
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                                          , GetHtmlCheckDataLakeReport(dataLakeReportInfo.Item1));

            foreach (var email in emails) {
                var emailSending = mailgun.SendHTMLMessage(email, "Health Check on YEAR-AP System", plainText);

                if (emailSending.IsSuccessful) {
                    Console.WriteLine($"Email sent to {email}");
                } else {
                    Console.WriteLine("Failed to send the e-mail");
                }
            }
        }

        private static DateTime GetLastExecuted() {
            try {
                var emailTimestamp = FileSystemHelpers.LoadJson(reportPath);
                return emailTimestamp["last_executed"].ToObject<DateTime>();
            } catch (Exception) {
                return DateTime.MinValue;
            }
        }

        private static void SetLastExecuted(DateTime now) {
            //TODO: use proper c# serialization
            try {
                var emailTimestamp = FileSystemHelpers.LoadJson(reportPath);
                emailTimestamp["last_executed"] = now;
                FileSystemHelpers.DumpJson(reportPath, emailTimestamp);
            } catch (Exception) {
                using (StreamWriter file = File.CreateText(reportPath)) {
                    JsonSerializer serialize = new JsonSerializer();
                    serialize.Serialize(file, JObject.Parse("{\"last_executed\" :\"" + now + "\"}"));
                }
            }
        }

        public static void Run(string[] emails, string mailServiceKey, DateTime startDate, bool forcedEmail) {
            // These functions will return {Item1 = reports, Item2 = status}
            var jobsReportInfo = GetJobsHealthInfo(startDate);
<<<<<<< HEAD
            var apReportInfo = GetApHealthInfo();
            var dataLakeReportInfo = GetDataLakeHealthInfo();

            var systemStatus = jobsReportInfo.Item2 && apReportInfo.Item2 && dataLakeReportInfo.Item2;

            if (forcedEmail) {
                SendReportEmail(emails, mailServiceKey, startDate, jobsReportInfo, apReportInfo, dataLakeReportInfo, systemStatus);
            } else {
                var lastExecuted = GetLastExecuted();
                if (lastExecuted < startDate && !systemStatus) {
                    SendReportEmail(emails, mailServiceKey, startDate, jobsReportInfo, apReportInfo, dataLakeReportInfo, systemStatus);
=======
            var dataLakeReportInfo = GetDataLakeHealthInfo();

            var systemStatus = jobsReportInfo.Item2  && dataLakeReportInfo.Item2;

            if (forcedEmail) {
                SendReportEmail(emails, mailServiceKey, startDate, jobsReportInfo, dataLakeReportInfo, systemStatus);
            } else {
                var lastExecuted = GetLastExecuted();
                if (lastExecuted < startDate && !systemStatus) {
                    SendReportEmail(emails, mailServiceKey, startDate, jobsReportInfo, dataLakeReportInfo, systemStatus);
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
                    SetLastExecuted(DateTime.Now);
                } else {
                    Console.WriteLine("Skipping report. To force sending e-mail use '-f'.");
                }
            }
        }
    }
}
