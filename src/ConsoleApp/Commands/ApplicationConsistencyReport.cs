using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.Linq;
using ApplicationModels;
using Common;
using Common.Report;
using Microsoft.EntityFrameworkCore;
=======
using Common.Report;
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021

namespace ConsoleApp.Commands {

    public enum ReportStatus {
        OK,
        FAILED
    }

    public class Report {

        public List<List<string>> Data;
        public ReportStatus Status;
        public string Description;
        public string Title;
    }
<<<<<<< HEAD

    public class ApplicationConsistencyReport {
        public static Report CheckForOrphanApplicationVideos() {

            using (var context = new ApplicationDbContext()) {
                var orphanVideos = context.ApplicationVideos.FromSql(@"
                SELECT
                    a.*
                FROM
                    application.""ApplicationVideos"" a
                JOIN
                    (
                        SELECT
                            a.""Id""
                        FROM
                            application.""ApplicationVideos"" a
                        EXCEPT
                        SELECT
                            avsv.""ApplicationVideoId""
                        FROM
                            application.""ApplicationVideoSourceVideos"" avsv
                    ) AS orphans ON a.""Id"" = orphans.""Id""").ToArray();

                return new Report(){
                           Title = "Orphan Application Video",
                           Status = orphanVideos.Any() ? ReportStatus.FAILED : ReportStatus.OK,
                           Description = "This report lists application videos that where left without any source video.",
                           Data = orphanVideos.Select(
                               x => new List<string>(){ x.Id.ToString(), x.Title == null ? "empty" : x.Title, x.UpdateDate.ToString() }).Prepend(
                               new List<string>(){ "ApplicationVideoId", "Title", "UpdateDate" }).ToList()
                };
            }
        }

        public static void Report() {
            var reports = new List<Report>(){
                CheckForOrphanApplicationVideos()
            };

            foreach (var r in reports) {
                Console.WriteLine(r.Title);
                Console.WriteLine($"Status: {r.Status}");
                if (r.Status == ReportStatus.FAILED) {
                    Console.Write($"Description: {r.Description}");
                    Console.WriteLine();
                    ConsoleReport.WriteTable(r.Data);
                }
            }
        }
    }
}
=======
}
>>>>>>> 4dc2fdf6b22fa256af8c3fca1fbf198adf722021
