using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests
{
    public sealed class RentingPersistenceTests
    {
        [Fact]
        [Trait("Category", "Postgres")]
        public async Task MigrationAndConstraintsWorkInIsolatedDatabase()
        {
            var connection = Environment.GetEnvironmentVariable("RENTING_TEST_CONNECTION")
                ?? throw new InvalidOperationException("Set RENTING_TEST_CONNECTION to run PostgreSQL integration tests.");
            var builder = new NpgsqlConnectionStringBuilder(connection);
            var database = "renting_test_" + Guid.NewGuid().ToString("N");
            builder.Database = database;
            builder.Pooling = false;
            var options = new DbContextOptionsBuilder<RentingDbContext>().UseNpgsql(builder.ConnectionString).Options;

            try
            {
                await using var context = new RentingDbContext(options);
                await context.Database.MigrateAsync();
                await context.Database.MigrateAsync();
                var today = new DateOnly(2026, 11, 19);
                var vehicle = new Vehicle(Guid.NewGuid(), "test-123", "Brand", "Model", today, today);
                var secondVehicle = new Vehicle(Guid.NewGuid(), "test-456", "Brand", "Model", today, today);
                context.Vehicles.AddRange(vehicle, secondVehicle);
                var unitOfWork = new UnitOfWork(context);
                Assert.Equal(2, await unitOfWork.Save());
                Assert.Equal("TEST-123", (await context.Vehicles.SingleAsync(x => x.Id == vehicle.Id)).LicensePlate);
                var customer = Guid.NewGuid();
                var start = new DateTime(2026, 11, 19, 0, 0, 0, DateTimeKind.Utc);
                var first = new Rental(Guid.NewGuid(), vehicle.Id, customer, start);
                context.Rentals.Add(first);
                await context.SaveChangesAsync();

                await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE rentals SET returned_at = {start.AddSeconds(-1)} WHERE id = {first.Id}"));

                await using (var conflict = new RentingDbContext(options))
                {
                    conflict.Rentals.Add(new Rental(Guid.NewGuid(), secondVehicle.Id, customer, start));
                    await Assert.ThrowsAsync<DbUpdateException>(() => conflict.SaveChangesAsync());
                }

                await using (var conflict = new RentingDbContext(options))
                {
                    conflict.Rentals.Add(new Rental(Guid.NewGuid(), vehicle.Id, Guid.NewGuid(), start));
                    await Assert.ThrowsAsync<DbUpdateException>(() => conflict.SaveChangesAsync());
                }

                await using (var conflict = new RentingDbContext(options))
                {
                    conflict.Vehicles.Add(new Vehicle(Guid.NewGuid(), " TEST-123 ", "Brand", "Model", today, today));
                    await Assert.ThrowsAsync<DbUpdateException>(() => conflict.SaveChangesAsync());
                }

                first.Return(start.AddHours(1));
                await context.SaveChangesAsync();
                context.Rentals.Add(new Rental(Guid.NewGuid(), vehicle.Id, customer, start.AddHours(2)));
                await context.SaveChangesAsync();
                Assert.Equal(2, await context.Rentals.CountAsync());

                await using (var invalid = new RentingDbContext(options))
                {
                    invalid.Rentals.Add(new Rental(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start));
                    await Assert.ThrowsAsync<DbUpdateException>(() => invalid.SaveChangesAsync());
                }

                await using (var deletion = new RentingDbContext(options))
                {
                    deletion.Vehicles.Remove(await deletion.Vehicles.SingleAsync(x => x.Id == vehicle.Id));
                    await Assert.ThrowsAsync<DbUpdateException>(() => deletion.SaveChangesAsync());
                }

                var raceCustomer = Guid.NewGuid();
                var competitor = new Vehicle(Guid.NewGuid(), "test-789", "Brand", "Model", today, today);
                context.Vehicles.Add(competitor);
                await context.SaveChangesAsync();
                var results = await Task.WhenAll(
                    TryRent(options, secondVehicle.Id, raceCustomer, start),
                    TryRent(options, competitor.Id, raceCustomer, start));
                Assert.Single(results, success => success);
                Assert.Equal(1, await context.Rentals.CountAsync(x => x.CustomerId == raceCustomer && x.ReturnedAt == null));
                var raceVehicle = new Vehicle(Guid.NewGuid(), "test-999", "Brand", "Model", today, today);
                context.Vehicles.Add(raceVehicle);
                await context.SaveChangesAsync();
                var vehicleResults = await Task.WhenAll(
                    TryRent(options, raceVehicle.Id, Guid.NewGuid(), start),
                    TryRent(options, raceVehicle.Id, Guid.NewGuid(), start));
                Assert.Single(vehicleResults, success => success);
                Assert.Equal(1, await context.Rentals.CountAsync(x => x.VehicleId == raceVehicle.Id && x.ReturnedAt == null));
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        private static async Task<bool> TryRent(DbContextOptions<RentingDbContext> options, Guid vehicleId, Guid customerId, DateTime start)
        {
            await using var context = new RentingDbContext(options);
            context.Rentals.Add(new Rental(Guid.NewGuid(), vehicleId, customerId, start));
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return false;
            }
        }
    }
}
