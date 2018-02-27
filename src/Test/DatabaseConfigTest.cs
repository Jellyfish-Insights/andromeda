using ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Test {

    public class DatabaseConfigTest {
        [Fact]
        public void QueryTimoutCommandTimeout() {
            using (var dbContext = new ApplicationDbContext())
            {
                dbContext.Database.SetCommandTimeout(1);
                Assert.Throws<Npgsql.NpgsqlException>(() =>
                                                      dbContext.Database.ExecuteSqlCommand("SELECT pg_sleep(2)"));
            }
        }
    }
}
