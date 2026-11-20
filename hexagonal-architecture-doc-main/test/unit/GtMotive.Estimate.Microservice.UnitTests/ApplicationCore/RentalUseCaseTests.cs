using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnRental;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.ApplicationCore
{
    public sealed class RentalUseCaseTests
    {
        [Fact]
        public async Task RentChecksEligibilityAndConflictsBeforeWriting()
        {
            var store = new Mock<IRentalRepository>();
            var vehicles = new Mock<IVehicleRepository>();
            var clock = new Mock<TimeProvider>();
            var now = new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero);
            clock.Setup(x => x.GetUtcNow()).Returns(now);
            var id = Guid.NewGuid();
            var customer = Guid.NewGuid();
            var input = new RentVehicleCommand(id, customer);
            var useCase = new RentVehicleHandler(vehicles.Object, store.Object, clock.Object);
            Assert.Equal(RentVehicleError.VehicleNotFound, (await useCase.Handle(input, CancellationToken.None)).Error);
            var old = new Vehicle(id, "OLD", "Brand", "Model", new DateOnly(2021, 11, 18), new DateOnly(2021, 11, 18));
            vehicles.Setup(x => x.FindVehicle(id)).ReturnsAsync(old);
            Assert.Equal(RentVehicleError.VehicleNotEligible, (await useCase.Handle(input, CancellationToken.None)).Error);
            var eligible = new Vehicle(id, "NEW", "Brand", "Model", new DateOnly(2021, 11, 19), new DateOnly(2026, 11, 19));
            vehicles.Setup(x => x.FindVehicle(id)).ReturnsAsync(eligible);
            store.Setup(x => x.HasActiveVehicle(id)).ReturnsAsync(true);
            Assert.Equal(RentVehicleError.VehicleAlreadyRented, (await useCase.Handle(input, CancellationToken.None)).Error);
            store.Setup(x => x.HasActiveVehicle(id)).ReturnsAsync(false);
            store.Setup(x => x.HasActiveCustomer(customer)).ReturnsAsync(true);
            Assert.Equal(RentVehicleError.CustomerHasActiveRental, (await useCase.Handle(input, CancellationToken.None)).Error);
            store.Verify(x => x.TryAdd(It.IsAny<Rental>()), Times.Never);
            store.Setup(x => x.HasActiveCustomer(customer)).ReturnsAsync(false);
            store.Setup(x => x.TryAdd(It.IsAny<Rental>())).ReturnsAsync(RentalConflict.None);
            var result = await useCase.Handle(input, CancellationToken.None);
            Assert.Equal(RentVehicleError.None, result.Error);
            Assert.Equal(id, result.Rental.VehicleId);
            Assert.Equal(customer, result.Rental.CustomerId);
            Assert.Equal(now.UtcDateTime, result.Rental.RentedAt);
            store.Setup(x => x.TryAdd(It.IsAny<Rental>())).ReturnsAsync(RentalConflict.Vehicle);
            Assert.Equal(RentVehicleError.VehicleAlreadyRented, (await useCase.Handle(input, CancellationToken.None)).Error);
            store.Setup(x => x.TryAdd(It.IsAny<Rental>())).ReturnsAsync(RentalConflict.Customer);
            Assert.Equal(RentVehicleError.CustomerHasActiveRental, (await useCase.Handle(input, CancellationToken.None)).Error);
        }

        [Fact]
        public async Task ReturnUsesUtcClockAndHandlesMissingOrRepeatedRental()
        {
            var store = new Mock<IRentalRepository>();
            var clock = new Mock<TimeProvider>();
            var now = new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero);
            clock.Setup(x => x.GetUtcNow()).Returns(now);
            var id = Guid.NewGuid();
            var useCase = new ReturnRentalHandler(store.Object, clock.Object);
            Assert.Equal(ReturnRentalResult.Invalid, await useCase.Handle(new ReturnRentalCommand(Guid.Empty), CancellationToken.None));
            Assert.Equal(ReturnRentalResult.NotFound, await useCase.Handle(new ReturnRentalCommand(id), CancellationToken.None));
            store.Setup(x => x.RecordReturn(id, now.UtcDateTime)).ReturnsAsync(true);
            Assert.Equal(ReturnRentalResult.Returned, await useCase.Handle(new ReturnRentalCommand(id), CancellationToken.None));
            Assert.Equal(ReturnRentalResult.Returned, await useCase.Handle(new ReturnRentalCommand(id), CancellationToken.None));
        }

        [Fact]
        public async Task ReturnDoesNotCheckEligibilityWhenVehicleHasAged()
        {
            var today = new DateOnly(2026, 11, 20);
            var vehicle = new Vehicle(Guid.NewGuid(), "AGED", "Brand", "Model", today.AddYears(-5).AddDays(-1), today.AddDays(-1));
            Assert.False(vehicle.IsEligibleOn(today));
            var rental = new Rental(Guid.NewGuid(), vehicle.Id, Guid.NewGuid(), new DateTime(2026, 11, 19, 0, 0, 0, DateTimeKind.Utc));
            var store = new Mock<IRentalRepository>(MockBehavior.Strict);
            var clock = new Mock<TimeProvider>();
            var now = new DateTimeOffset(2026, 11, 20, 0, 0, 0, TimeSpan.Zero);
            clock.Setup(x => x.GetUtcNow()).Returns(now);
            store.Setup(x => x.RecordReturn(rental.Id, now.UtcDateTime)).ReturnsAsync(true);
            var result = await new ReturnRentalHandler(store.Object, clock.Object).Handle(new ReturnRentalCommand(rental.Id), CancellationToken.None);
            Assert.Equal(ReturnRentalResult.Returned, result);
            store.VerifyAll();
        }
    }
}
