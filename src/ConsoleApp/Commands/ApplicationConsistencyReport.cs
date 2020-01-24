using System;
using System.Collections.Generic;
using Common.Report;

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
}
