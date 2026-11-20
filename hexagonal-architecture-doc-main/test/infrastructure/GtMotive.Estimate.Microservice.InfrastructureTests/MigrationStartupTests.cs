using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Trait("Category", "Postgres")]
        public async Task DemoSeedingIsOptionalAndExposedByHost(bool enabled)
        {
            var connection = Environment.GetEnvironmentVariable("RENTING_TEST_CONNECTION") ?? throw new InvalidOperationException("Set RENTING_TEST_CONNECTION.");
            var settings = new NpgsqlConnectionStringBuilder(connection) { Database = "renting_test_" + Guid.NewGuid().ToString("N"), Pooling = false };
            var options = new DbContextOptionsBuilder<RentingDbContext>().UseNpgsql(settings.ConnectionString).Options;
            try
            {
                await using var root = new WebApplicationFactory<Program>();
                await using var factory = root.WithWebHostBuilder(builder => builder
                    .UseSetting("ConnectionStrings:Renting", settings.ConnectionString)
                    .UseSetting("Database:ApplyMigrations", "true")
                    .UseSetting("Database:SeedDemoData", enabled.ToString()));
                using var client = factory.CreateClient();
                using var response = await client.GetAsync(new Uri("/vehicles", UriKind.Relative));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(enabled ? 1 : 0, json.RootElement.GetArrayLength());
                if (enabled)
                {
                    Assert.Equal("DEMO-001", json.RootElement[0].GetProperty("licensePlate").GetString());
                }

                await using var db = new RentingDbContext(options);
                Assert.Equal(enabled ? 1 : 0, await db.Vehicles.CountAsync());
                Assert.Equal(enabled ? 1 : 0, await db.Rentals.CountAsync());
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }

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
                    using var initiallyAvailable = await client.GetAsync(new Uri("/vehicles", UriKind.Relative));
                    using var initiallyAvailableJson = JsonDocument.Parse(await initiallyAvailable.Content.ReadAsStringAsync());
                    Assert.Equal(0, initiallyAvailableJson.RootElement.GetArrayLength());
                    using var created = await client.PostAsJsonAsync(new Uri("/vehicles", UriKind.Relative), new { licensePlate = " test-api ", brand = "Brand", model = "Model", manufacturedOn = "2025-01-01" });
                    Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                    using var createdJson = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
                    var vehicleId = createdJson.RootElement.GetProperty("id").GetGuid();
                    var customerId = Guid.NewGuid();
                    using var rented = await client.PostAsJsonAsync(new Uri("/rentals", UriKind.Relative), new { vehicleId, customerId });
                    Assert.Equal(HttpStatusCode.Created, rented.StatusCode);
                    using var rentalJson = JsonDocument.Parse(await rented.Content.ReadAsStringAsync());
                    var rentalId = rentalJson.RootElement.GetProperty("id").GetGuid();
                    Assert.Equal(JsonValueKind.Null, rentalJson.RootElement.GetProperty("returnedAt").ValueKind);
                    Assert.Equal(DateTimeKind.Utc, rentalJson.RootElement.GetProperty("rentedAt").GetDateTime().Kind);
                    using var conflictRental = await client.PostAsJsonAsync(new Uri("/rentals", UriKind.Relative), new { vehicleId, customerId = Guid.NewGuid() });
                    Assert.Equal(HttpStatusCode.Conflict, conflictRental.StatusCode);
                    using var missingVehicle = await client.PostAsJsonAsync(new Uri("/rentals", UriKind.Relative), new { vehicleId = Guid.NewGuid(), customerId });
                    Assert.Equal(HttpStatusCode.NotFound, missingVehicle.StatusCode);
                    using var invalidRental = await client.PostAsJsonAsync(new Uri("/rentals", UriKind.Relative), new { vehicleId = Guid.Empty, customerId });
                    Assert.Equal(HttpStatusCode.BadRequest, invalidRental.StatusCode);
                    using var returned = await client.PutAsync(new Uri($"/rentals/{rentalId}/return", UriKind.Relative), null);
                    Assert.Equal(HttpStatusCode.NoContent, returned.StatusCode);
                    Assert.Equal(string.Empty, await returned.Content.ReadAsStringAsync());
                    using var repeated = await client.PutAsync(new Uri($"/rentals/{rentalId}/return", UriKind.Relative), null);
                    Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
                    using var missingReturn = await client.PutAsync(new Uri($"/rentals/{Guid.NewGuid()}/return", UriKind.Relative), null);
                    Assert.Equal(HttpStatusCode.NotFound, missingReturn.StatusCode);
                    using var invalidReturn = await client.PutAsync(new Uri("/rentals/not-a-uuid/return", UriKind.Relative), null);
                    Assert.Equal(HttpStatusCode.BadRequest, invalidReturn.StatusCode);
                    using var available = await client.GetAsync(new Uri("/vehicles?available=true", UriKind.Relative));
                    Assert.Equal(HttpStatusCode.OK, available.StatusCode);
                    Assert.Contains("TEST-API", await available.Content.ReadAsStringAsync(), StringComparison.Ordinal);
                    using var unsupported = await client.GetAsync(new Uri("/vehicles?available=false", UriKind.Relative));
                    Assert.Equal(HttpStatusCode.BadRequest, unsupported.StatusCode);
                    using var invalidFilter = await client.GetAsync(new Uri("/vehicles?available=invalid", UriKind.Relative));
                    Assert.Equal(HttpStatusCode.BadRequest, invalidFilter.StatusCode);
                    using var duplicate = await client.PostAsJsonAsync(new Uri("/vehicles", UriKind.Relative), new { licensePlate = "TEST-API", brand = "Brand", model = "Model", manufacturedOn = "2025-01-01" });
                    Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
                    using var invalid = await client.PostAsJsonAsync(new Uri("/vehicles", UriKind.Relative), new { licensePlate = "TEST-INVALID", brand = "Brand", model = "Model", manufacturedOn = "2000-01-01" });
                    Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
                    using var swagger = await client.GetAsync(new Uri("/swagger/v1/swagger.json", UriKind.Relative));
                    Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
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
                    Assert.Equal(2, await db.Vehicles.CountAsync());
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
