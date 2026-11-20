using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnRental;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
            var counter = new CommandCounter();
            var options = new DbContextOptionsBuilder<RentingDbContext>().UseNpgsql(builder.ConnectionString).AddInterceptors(counter).Options;

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

                var registrations = await Task.WhenAll(
                    TryRegister(options, " concurrent-plate ", today),
                    TryRegister(options, "CONCURRENT-PLATE", today));
                Assert.Single(registrations, success => success);
                Assert.Equal(1, await context.Vehicles.CountAsync(x => x.LicensePlate == "CONCURRENT-PLATE"));

                var clock = new Mock<TimeProvider>();
                clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
                var vehicles = new VehicleRepository(context, unitOfWork);
                var rentals = new RentalRepository(context, unitOfWork);
                var register = new RegisterVehicleHandler(vehicles, clock.Object);
                var registered = await register.Handle(new RegisterVehicleCommand("functional", "Brand", "Model", today), CancellationToken.None);
                Assert.Equal("FUNCTIONAL", registered.Vehicle.LicensePlate);
                Assert.True(await context.Vehicles.AnyAsync(x => x.LicensePlate == "FUNCTIONAL"));

                var cutoffResult = await register.Handle(new RegisterVehicleCommand("CUTOFF", "Brand", "Model", today.AddYears(-5)), CancellationToken.None);
                Assert.Equal(RegisterVehicleError.None, cutoffResult.Error);
                var cutoffVehicle = await context.Vehicles.SingleAsync(x => x.Id == cutoffResult.Vehicle.Id);
                var futureVehicle = new Vehicle(Guid.NewGuid(), "FUTURE", "Brand", "Model", today.AddDays(1), today.AddDays(1));
                var historicalVehicle = new Vehicle(Guid.NewGuid(), "HISTORY", "Brand", "Model", today, today);
                context.Vehicles.AddRange(futureVehicle, historicalVehicle);
                var historicalRental = new Rental(Guid.NewGuid(), historicalVehicle.Id, Guid.NewGuid(), start);
                historicalRental.Return(start.AddHours(1));
                context.Rentals.Add(historicalRental);
                await context.SaveChangesAsync();
                var reader = vehicles;
                counter.Count = 0;
                var todayResult = await reader.ListAvailable(today);
                Assert.Equal(1, counter.Count);
                Assert.Contains(todayResult, x => x.Id == cutoffVehicle.Id);
                Assert.Contains(todayResult, x => x.Id == historicalVehicle.Id);
                Assert.DoesNotContain(todayResult, x => x.Id == futureVehicle.Id || x.Id == vehicle.Id || x.Id == raceVehicle.Id);
                Assert.DoesNotContain(await reader.ListAvailable(today.AddDays(1)), x => x.Id == cutoffVehicle.Id);
                Assert.True(await context.Vehicles.AnyAsync(x => x.Id == cutoffVehicle.Id));
                var listed = await new ListAvailableVehiclesHandler(reader, clock.Object).Handle(new ListAvailableVehiclesQuery(), CancellationToken.None);
                Assert.Contains(listed, v => v.Id == cutoffVehicle.Id);
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationConfiguration).Assembly));
                services.AddSingleton<IVehicleRepository>(reader);
                services.AddSingleton(clock.Object);
                await using (var provider = services.BuildServiceProvider())
                {
                    var mediated = await provider.GetRequiredService<IMediator>().Send(new ListAvailableVehiclesQuery());
                    Assert.Contains(mediated, v => v.Id == cutoffVehicle.Id);
                }

                var flowCustomer = Guid.NewGuid();
                var rentCase = new RentVehicleHandler(vehicles, rentals, clock.Object);
                var createdRental = await rentCase.Handle(new RentVehicleCommand(cutoffVehicle.Id, flowCustomer), CancellationToken.None);
                Assert.Equal(RentVehicleError.None, createdRental.Error);
                var rejected = await rentCase.Handle(new RentVehicleCommand(cutoffVehicle.Id, Guid.NewGuid()), CancellationToken.None);
                Assert.Equal(RentVehicleError.VehicleAlreadyRented, rejected.Error);
                var returnCase = new ReturnRentalHandler(rentals, clock.Object);
                Assert.Equal(ReturnRentalResult.Returned, await returnCase.Handle(new ReturnRentalCommand(createdRental.Rental.Id), CancellationToken.None));
                var secondRental = await rentCase.Handle(new RentVehicleCommand(cutoffVehicle.Id, flowCustomer), CancellationToken.None);
                Assert.Equal(RentVehicleError.None, secondRental.Error);
                Assert.Equal(2, await context.Rentals.CountAsync(x => x.VehicleId == cutoffVehicle.Id));

                var concurrentVehicle = new Vehicle(Guid.NewGuid(), "CONCURRENT", "Brand", "Model", today, today);
                context.Vehicles.Add(concurrentVehicle);
                await context.SaveChangesAsync();
                var concurrentRental = new Rental(Guid.NewGuid(), concurrentVehicle.Id, Guid.NewGuid(), start);
                context.Rentals.Add(concurrentRental);
                await context.SaveChangesAsync();
                var returns = await Task.WhenAll(TryReturn(options, concurrentRental.Id, start.AddHours(1)), TryReturn(options, concurrentRental.Id, start.AddHours(2)));
                Assert.All(returns, Assert.True);
                await context.Entry(concurrentRental).ReloadAsync();
                var firstReturn = concurrentRental.ReturnedAt;
                Assert.True(firstReturn == start.AddHours(1) || firstReturn == start.AddHours(2));
                Assert.True(await TryReturn(options, concurrentRental.Id, start.AddHours(3)));
                await context.Entry(concurrentRental).ReloadAsync();
                Assert.Equal(firstReturn, concurrentRental.ReturnedAt);
                var adapterVehicle = new Vehicle(Guid.NewGuid(), "ADAPTER", "Brand", "Model", today, today);
                var adapterVehicle2 = new Vehicle(Guid.NewGuid(), "ADAPTER2", "Brand", "Model", today, today);
                context.Vehicles.AddRange(adapterVehicle, adapterVehicle2);
                await context.SaveChangesAsync();
                var adapterRace = await Task.WhenAll(
                    TryRentWithStore(options, adapterVehicle.Id, Guid.NewGuid(), start),
                    TryRentWithStore(options, adapterVehicle.Id, Guid.NewGuid(), start));
                Assert.Single(adapterRace, x => x == RentalConflict.None);
                Assert.Single(adapterRace, x => x == RentalConflict.Vehicle);
                var sameCustomer = Guid.NewGuid();
                var anotherVehicle = new Vehicle(Guid.NewGuid(), "ADAPTER3", "Brand", "Model", today, today);
                context.Vehicles.Add(anotherVehicle);
                await context.SaveChangesAsync();
                var customerRace = await Task.WhenAll(
                    TryRentWithStore(options, adapterVehicle2.Id, sameCustomer, start),
                    TryRentWithStore(options, anotherVehicle.Id, sameCustomer, start));
                Assert.Single(customerRace, x => x == RentalConflict.None);
                Assert.Single(customerRace, x => x == RentalConflict.Customer);
                await using (var unknown = new RentingDbContext(options))
                {
                    var unknownStore = new RentalRepository(unknown, new UnitOfWork(unknown));
                    await Assert.ThrowsAsync<DbUpdateException>(() => unknownStore.TryAdd(new Rental(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start)));
                }

                var agedRental = await context.Rentals.AsNoTracking().SingleAsync(x => x.VehicleId == cutoffVehicle.Id && x.ReturnedAt == null);
                var nextDay = new Mock<TimeProvider>();
                nextDay.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 20, 0, 0, 0, TimeSpan.Zero));
                Assert.Equal(ReturnRentalResult.Returned, await new ReturnRentalHandler(rentals, nextDay.Object).Handle(new ReturnRentalCommand(agedRental.Id), CancellationToken.None));
                Assert.DoesNotContain(await reader.ListAvailable(today.AddDays(1)), x => x.Id == cutoffVehicle.Id);
            }
            finally
            {
                await using var cleanup = new RentingDbContext(options);
                await cleanup.Database.EnsureDeletedAsync();
            }
        }

        private static async Task<bool> TryRegister(DbContextOptions<RentingDbContext> options, string plate, DateOnly today)
        {
            await using var context = new RentingDbContext(options);
            var store = new VehicleRepository(context, new UnitOfWork(context));
            return await store.TryAdd(new Vehicle(Guid.NewGuid(), plate, "Brand", "Model", today, today));
        }

        private static async Task<bool> TryReturn(DbContextOptions<RentingDbContext> options, Guid id, DateTime time)
        {
            await using var context = new RentingDbContext(options);
            return await new RentalRepository(context, new UnitOfWork(context)).RecordReturn(id, time);
        }

        private static async Task<RentalConflict> TryRentWithStore(DbContextOptions<RentingDbContext> options, Guid vehicleId, Guid customerId, DateTime time)
        {
            await using var context = new RentingDbContext(options);
            return await new RentalRepository(context, new UnitOfWork(context)).TryAdd(new Rental(Guid.NewGuid(), vehicleId, customerId, time));
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
