using Microsoft.EntityFrameworkCore;

using Common.Logging.Models;

namespace DataLakeModels {

    public partial class DataLakeLoggingContext : AbstractDataLakeContext {
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("logging");
        }

        public virtual DbSet<RuntimeLog> RuntimeLog { get; set; }
    }
}
