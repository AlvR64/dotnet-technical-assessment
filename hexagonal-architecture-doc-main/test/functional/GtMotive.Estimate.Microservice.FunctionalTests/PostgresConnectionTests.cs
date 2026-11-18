using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests
{
    public sealed class PostgresConnectionTests
    {
        [Fact]
        [Trait("Category", "Postgres")]
        public async Task ConnectsToPostgres()
        {
            var connectionString = Environment.GetEnvironmentVariable("RENTING_TEST_CONNECTION")
                ?? throw new InvalidOperationException("Set RENTING_TEST_CONNECTION to run PostgreSQL integration tests.");
            var options = new DbContextOptionsBuilder<RentingDbContext>()
                .UseNpgsql(connectionString)
                .Options;
            await using var context = new RentingDbContext(options);

            Assert.True(await context.Database.CanConnectAsync());
        }
    }
}
