using System;
using System.Collections.Generic;
using System.Threading;
using Common.Jobs;
using Common.Logging;
using Serilog.Core;

namespace Test {

    public class JobsHelper {

        public abstract class AbstractTestJob : AbstractJob {

            public override void Run() {
                Thread.Sleep(1000);
                if (Fail) {
                    throw new Exception($"{this.GetType().Name} exploded.");
                }
            }

            protected override Logger GetLogger() {
                return LoggerFactory.GetTestLogger(Id());
            }

            protected bool Fail = false;

            public AbstractTestJob(bool fail = false) {
                Fail = fail;
            }
        }

        /**
            The jobs bellow form the following tree:
            A ---> B ---> C
         |
            D -----+----> E
         |
         +----> F
         */

        public class JobA : AbstractTestJob {
            public override List<string> Dependencies() {
                return new List<string>() {};
            }

            public JobA(bool fail = false): base(fail) {}
        }

        public class JobB : AbstractTestJob {
            public override List<string> Dependencies() {
                return new List<string>() { IdOf<JobA>() };
            }

            public JobB(bool fail = false): base(fail) {}
        }

        public class JobC : AbstractTestJob {
            public override List<string> Dependencies() {
                return new List<string>() { IdOf<JobB>() };
            }

            public JobC(bool fail = false): base(fail) {}
        }

        public class JobD : AbstractTestJob {
            public override List<string> Dependencies() {
                return new List<string>() {};
            }

            public JobD(bool fail = false): base(fail) {}
        }

        public class JobE : AbstractTestJob {
            public override List<string> Dependencies() {
                return new List<string>() { IdOf<JobB>(), IdOf<JobD>() };
            }

            public JobE(bool fail = false): base(fail) {}
        }

        public class JobF : AbstractTestJob {
            public override List<string> Dependencies() {
                return new List<string>() { IdOf<JobD>() };
            }

            public JobF(bool fail = false): base(fail) {}
        }
    }
}
