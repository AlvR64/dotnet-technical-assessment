using System;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.Domain
{
    public sealed class ModelTests
    {
        [Fact]
        public void NormalizesPlateAndValidatesFields()
        {
            var today = new DateOnly(2026, 11, 19);
            var vehicle = new Vehicle(Guid.NewGuid(), " ab-123 ", " Toyota ", " Corolla ", today, today);
            Assert.Equal("AB-123", vehicle.LicensePlate);
            Assert.Throws<ArgumentException>(() => new Vehicle(Guid.Empty, "AB", "Brand", "Model", today, today));
            Assert.Throws<ArgumentException>(() => new Vehicle(Guid.NewGuid(), " ", "Brand", "Model", today, today));
            Assert.Throws<ArgumentException>(() => new Vehicle(Guid.NewGuid(), new string('A', 21), "Brand", "Model", today, today));
            Assert.Throws<ArgumentException>(() => new Vehicle(Guid.NewGuid(), "AB", new string('A', 101), "Model", today, today));
            Assert.Throws<ArgumentException>(() => new Vehicle(Guid.NewGuid(), "AB", "Brand", " ", today, today));
        }

        [Fact]
        public void AppliesCalendarCutoffIncludingLeapDays()
        {
            var today = new DateOnly(2025, 2, 28);
            var vehicle = new Vehicle(Guid.NewGuid(), "AB", "Brand", "Model", new DateOnly(2020, 2, 29), today);
            Assert.True(vehicle.IsEligibleOn(today));
            Assert.False(vehicle.IsEligibleOn(today.AddDays(1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vehicle(Guid.NewGuid(), "AB", "Brand", "Model", today.AddYears(-5).AddDays(-1), today));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Vehicle(Guid.NewGuid(), "AB", "Brand", "Model", today.AddDays(1), today));
        }

        [Fact]
        public void RentalValidatesUtcAndReturnIsIdempotent()
        {
            var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.Throws<ArgumentException>(() => new Rental(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), start));
            Assert.Throws<ArgumentException>(() => new Rental(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.SpecifyKind(start, DateTimeKind.Unspecified)));
            var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), start);
            Assert.Throws<ArgumentOutOfRangeException>(() => rental.Return(start.AddSeconds(-1)));
            Assert.Throws<ArgumentException>(() => rental.Return(DateTime.SpecifyKind(start, DateTimeKind.Unspecified)));
            rental.Return(start.AddHours(1));
            rental.Return(start.AddHours(2));
            Assert.Equal(start.AddHours(1), rental.ReturnedAt);
        }

        [Fact]
        public void CanReturnRentalAfterVehicleAgesOut()
        {
            var today = new DateOnly(2026, 11, 19);
            var vehicle = new Vehicle(Guid.NewGuid(), "AB", "Brand", "Model", today.AddYears(-5), today);
            var start = new DateTime(2026, 11, 19, 0, 0, 0, DateTimeKind.Utc);
            var rental = new Rental(Guid.NewGuid(), vehicle.Id, Guid.NewGuid(), start);
            Assert.False(vehicle.IsEligibleOn(today.AddDays(1)));
            rental.Return(start.AddDays(1));
            Assert.NotNull(rental.ReturnedAt);
        }
    }
}
