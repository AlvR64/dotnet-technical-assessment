using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.ApplicationCore
{
    public sealed class ListAvailableVehiclesTests
    {
        [Fact]
        public async Task UsesUtcDateAndPresentsResults()
        {
            var reader = new Mock<IVehicleRepository>();
            var clock = new Mock<TimeProvider>();
            var date = new DateOnly(2026, 11, 19);
#pragma warning disable SA1010 // StyleCop does not understand collection expressions.
            IReadOnlyList<Vehicle> vehicles = [new(Guid.NewGuid(), "AB", "Brand", "Model", date, date)];
#pragma warning restore SA1010
            clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
            reader.Setup(x => x.ListAvailable(date)).ReturnsAsync(vehicles);

            var result = await new ListAvailableVehiclesHandler(reader.Object, clock.Object).Handle(new ListAvailableVehiclesQuery(), CancellationToken.None);
            Assert.Equal(vehicles[0].Id, Assert.Single(result).Id);
            Assert.Equal("AB", result[0].LicensePlate);
        }

        [Fact]
        public async Task PresentsEmptyList()
        {
            var reader = new Mock<IVehicleRepository>();
            var clock = new Mock<TimeProvider>();
#pragma warning disable SA1010 // StyleCop does not understand collection expressions.
            IReadOnlyList<Vehicle> vehicles = [];
#pragma warning restore SA1010
            clock.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
            reader.Setup(x => x.ListAvailable(It.IsAny<DateOnly>())).ReturnsAsync(vehicles);

            var result = await new ListAvailableVehiclesHandler(reader.Object, clock.Object).Handle(new ListAvailableVehiclesQuery(), CancellationToken.None);
            Assert.Empty(result);
        }
    }
}
