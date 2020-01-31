using System;
using System.Collections.Generic;
using Serilog.Core;
using Andromeda.Common.Logging.Models;

namespace Andromeda.Common.Jobs {

    public abstract class AbstractJob {

        public abstract List<string> Dependencies();
        public abstract void Run();
        protected abstract Logger GetLogger();
        protected Logger Logger;
        public JobConfiguration Configuration { get; set; }
        static public bool IsJobRuntimeLog(RuntimeLog row) {
            return row.Name.StartsWith("Jobs.");
        }

        public const string JobStartedMessage = "Started.";
        public const string JobFinishedMessage = "Finished.";
        public const string JobFailedMessage = "Failed.";

        public void Execute(bool debug = false) {
            using (Logger = GetLogger()) {
                Logger.Information(JobStartedMessage);
                var start = DateTime.UtcNow;
                try {
                    if (debug) {
                        Logger.Debug($"Skipping call of 'Run' method.");
                    } else {
                        Run();
                    }
                } catch (Exception e) {
                    Logger.Error(e, JobFailedMessage + " Took {Duration}.", DateTime.UtcNow - start);
                    throw new Exception($"Job failed: {this.GetType().Name}.", e);
                }
                Logger.Information(JobFinishedMessage + " Took {Duration}.", DateTime.UtcNow - start);
            }
        }

        public static string IdOf<T>() where T : AbstractJob {
            return typeof(T).FullName;
        }

        public virtual string Id() {
            return this.GetType().FullName;
        }
    }
}
