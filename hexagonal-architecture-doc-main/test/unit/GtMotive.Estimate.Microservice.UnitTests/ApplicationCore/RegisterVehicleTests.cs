using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.ApplicationCore
{
    public sealed class RegisterVehicleTests
    {
        [Fact]
        public async Task RegistersAtExactAgeLimit()
        {
            var store = new Mock<IVehicleRepository>();
            var clock = new Mock<TimeProvider>();
            clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
            store.Setup(x => x.TryAdd(It.IsAny<Vehicle>())).ReturnsAsync(true);
            var useCase = new RegisterVehicleHandler(store.Object, clock.Object);

            var result = await useCase.Handle(new RegisterVehicleCommand(" ab ", "Brand", "Model", new DateOnly(2021, 11, 19)), CancellationToken.None);

            Assert.Equal("AB", result.Vehicle.LicensePlate);
        }

        [Fact]
        public async Task RejectsOldVehicleWithoutSaving()
        {
            var store = new Mock<IVehicleRepository>();
            var clock = new Mock<TimeProvider>();
            clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
            var useCase = new RegisterVehicleHandler(store.Object, clock.Object);

            var result = await useCase.Handle(new RegisterVehicleCommand("AB", "Brand", "Model", new DateOnly(2021, 11, 18)), CancellationToken.None);

            store.Verify(x => x.TryAdd(It.IsAny<Vehicle>()), Times.Never);
            Assert.Equal(RegisterVehicleError.Invalid, result.Error);
        }

        [Fact]
        public async Task HandlerReturnsTypedResultsAndDoesNotSaveInvalidVehicle()
        {
            var store = new Mock<IVehicleRepository>();
            var clock = new Mock<TimeProvider>();
            clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
            var handler = new RegisterVehicleHandler(store.Object, clock.Object);
            var invalid = await handler.Handle(new RegisterVehicleCommand("AB", "Brand", "Model", new DateOnly(2021, 11, 18)), CancellationToken.None);
            Assert.Equal(RegisterVehicleError.Invalid, invalid.Error);
            store.Verify(x => x.TryAdd(It.IsAny<Vehicle>()), Times.Never);
            store.Setup(x => x.TryAdd(It.IsAny<Vehicle>())).ReturnsAsync(true);
            var created = await handler.Handle(new RegisterVehicleCommand(" ab ", "Brand", "Model", new DateOnly(2021, 11, 19)), CancellationToken.None);
            Assert.Equal(RegisterVehicleError.None, created.Error);
            Assert.Equal("AB", created.Vehicle.LicensePlate);
            store.Setup(x => x.TryAdd(It.IsAny<Vehicle>())).ReturnsAsync(false);
            var duplicate = await handler.Handle(new RegisterVehicleCommand("AB", "Brand", "Model", new DateOnly(2021, 11, 19)), CancellationToken.None);
            Assert.Equal(RegisterVehicleError.DuplicatePlate, duplicate.Error);
        }
    }
}
