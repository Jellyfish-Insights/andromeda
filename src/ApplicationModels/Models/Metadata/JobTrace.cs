using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Helpers;
using Newtonsoft.Json;

namespace ApplicationModels.Models.Metadata {
    public class JobTrace {
        public string GitCommitHash { get; set; }
        public string JobName { get; set; }

        // this is the table into which the job wrote data
        public string Table { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [Column("Modifications", TypeName = "jsonb")]
        public string modifications {
            set {
                Modifications = JsonConvert.DeserializeObject<List<RowLog>>(value);
            }
            get {
                return JsonConvert.SerializeObject(Modifications);
            }
        }

        [NotMapped]
        public List<RowLog> Modifications { get; set; }

        public JobTrace() {
            GitCommitHash = VersioningHelper.GitCommitHash;
        }

        public JobTrace(string jobName, string table) {
            GitCommitHash = VersioningHelper.GitCommitHash;
            JobName = jobName;
            Table = table;
            StartTime = DateTime.UtcNow;
            Modifications = new List<RowLog>();
        }

        public void Add(RowLog log) {
            Modifications.Add(log);
        }
    }
}
