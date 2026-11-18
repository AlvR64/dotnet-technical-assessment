using System;
using System.Net;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests
{
    public sealed class MigrationStartupTests
    {
        [Fact]
        public async Task FailedOptInMigrationPreventsStartup()
        {
            await using var root = new WebApplicationFactory<Program>();
            await using var factory = root.WithWebHostBuilder(builder => builder
                .UseSetting("ConnectionStrings:Renting", "Host=127.0.0.1;Port=1;Database=missing;Username=missing;Password=secret;Timeout=1")
                .UseSetting("Database:ApplyMigrations", "true"));

            Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        }

        [Fact]
        [Trait("Category", "Postgres")]
        public async Task OptInMigratesAndRestartPreservesData()
        {
            var connection = Environment.GetEnvironmentVariable("RENTING_TEST_CONNECTION")
                ?? throw new InvalidOperationException("Set RENTING_TEST_CONNECTION.");
            var settings = new NpgsqlConnectionStringBuilder(connection)
            {
                Database = "renting_test_" + Guid.NewGuid().ToString("N"),
                Pooling = false,
            };
            var options = new DbContextOptionsBuilder<RentingDbContext>().UseNpgsql(settings.ConnectionString).Options;
            try
            {
                await using (var root = new WebApplicationFactory<Program>())
                await using (var factory = root.WithWebHostBuilder(builder => builder
                    .UseSetting("ConnectionStrings:Renting", settings.ConnectionString)
                    .UseSetting("Database:ApplyMigrations", "true")))
                {
                    using var client = factory.CreateClient();
                    using var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    await using var db = new RentingDbContext(options);
                    Assert.Single(await db.Database.GetAppliedMigrationsAsync());
                    var today = new DateOnly(2026, 11, 19);
                    db.Vehicles.Add(new Domain.Vehicles.Vehicle(Guid.NewGuid(), "PERSIST", "Brand", "Model", today, today));
                    await db.SaveChangesAsync();
                }

                await using (var root = new WebApplicationFactory<Program>())
                await using (var factory = root.WithWebHostBuilder(builder => builder
                    .UseSetting("ConnectionStrings:Renting", settings.ConnectionString)
                    .UseSetting("Database:ApplyMigrations", "true")))
                {
                    using var client = factory.CreateClient();
                    using var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    await using var db = new RentingDbContext(options);
                    Assert.Equal(1, await db.Vehicles.CountAsync());
                }
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }
    }
}
