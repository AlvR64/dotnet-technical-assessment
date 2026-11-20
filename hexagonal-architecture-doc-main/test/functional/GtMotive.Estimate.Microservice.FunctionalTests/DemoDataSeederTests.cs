using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Npgsql;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests
{
    public sealed class DemoDataSeederTests
    {
        [Fact]
        [Trait("Category", "Postgres")]
        public async Task SeedsOnceUnderConcurrencyAndPreservesExistingData()
        {
            var options = CreateOptions();
            try
            {
                await using var context = new RentingDbContext(options);
                await context.Database.MigrateAsync();
                var now = new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);
                var clock = new Mock<TimeProvider>();
                clock.Setup(x => x.GetUtcNow()).Returns(now);
                await Task.WhenAll(Seed(options, clock.Object), Seed(options, clock.Object));
                var vehicle = await context.Vehicles.SingleAsync();
                var rental = await context.Rentals.SingleAsync();
                Assert.Equal("DEMO-001", vehicle.LicensePlate);
                Assert.Equal(new DateOnly(2025, 11, 19), vehicle.ManufacturedOn);
                Assert.Equal(now.UtcDateTime.AddDays(-1), rental.ReturnedAt);
                Assert.Single(await new VehicleRepository(context, new UnitOfWork(context)).ListAvailable(new DateOnly(2026, 11, 19)));
                await Seed(options, TimeProvider.System);
                await context.Entry(vehicle).ReloadAsync();
                await context.Entry(rental).ReloadAsync();
                Assert.Equal(new DateOnly(2025, 11, 19), vehicle.ManufacturedOn);
                Assert.Equal(now.UtcDateTime.AddDays(-1), rental.ReturnedAt);
                Assert.Equal(1, await context.Vehicles.CountAsync());
                Assert.Equal(1, await context.Rentals.CountAsync());
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        [Trait("Category", "Postgres")]
        public async Task ExistingVehiclePreventsSeeding()
        {
            var options = CreateOptions();
            try
            {
                await using var context = new RentingDbContext(options);
                await context.Database.MigrateAsync();
                var today = new DateOnly(2026, 11, 19);
                context.Vehicles.Add(new Vehicle(Guid.NewGuid(), "EXISTING", "Brand", "Model", today, today));
                await context.SaveChangesAsync();
                await Seed(options, TimeProvider.System);
                Assert.Equal("EXISTING", (await context.Vehicles.SingleAsync()).LicensePlate);
                Assert.Empty(await context.Rentals.ToListAsync());
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        [Fact]
        [Trait("Category", "Postgres")]
        public async Task FailedRentalInsertRollsBackVehicle()
        {
            var options = CreateOptions();
            try
            {
                await using var context = new RentingDbContext(options);
                await context.Database.MigrateAsync();
                await context.Database.ExecuteSqlRawAsync("CREATE FUNCTION reject_demo_rental() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'test failure'; END $$");
                await context.Database.ExecuteSqlRawAsync("CREATE TRIGGER reject_demo BEFORE INSERT ON rentals FOR EACH ROW EXECUTE FUNCTION reject_demo_rental()");
                await Assert.ThrowsAsync<DbUpdateException>(() => Seed(options, TimeProvider.System));
                Assert.Empty(await context.Vehicles.ToListAsync());
                Assert.Empty(await context.Rentals.ToListAsync());
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        private static DbContextOptions<RentingDbContext> CreateOptions()
        {
            var connection = Environment.GetEnvironmentVariable("RENTING_TEST_CONNECTION") ?? throw new InvalidOperationException("Set RENTING_TEST_CONNECTION.");
            var builder = new NpgsqlConnectionStringBuilder(connection) { Database = "renting_test_" + Guid.NewGuid().ToString("N"), Pooling = false };
            return new DbContextOptionsBuilder<RentingDbContext>().UseNpgsql(builder.ConnectionString).Options;
        }

        private static async Task Seed(DbContextOptions<RentingDbContext> options, TimeProvider clock)
        {
            await using var context = new RentingDbContext(options);
            await new DemoDataSeeder(context, clock).Seed();
        }
    }
}
