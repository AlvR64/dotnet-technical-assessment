using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    public sealed class DemoDataSeeder(RentingDbContext context, TimeProvider timeProvider)
    {
        public async Task Seed()
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            await context.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(7283471021)");
            if (await context.Vehicles.AnyAsync() || await context.Rentals.AnyAsync())
            {
                await transaction.CommitAsync();
                return;
            }

            var now = timeProvider.GetUtcNow().UtcDateTime;
            var today = DateOnly.FromDateTime(now);
            var vehicleId = Guid.Parse("11111111-1111-4111-8111-111111111111");
            var vehicle = new Vehicle(vehicleId, "DEMO-001", "Toyota", "Corolla", today.AddYears(-1), today);
            var rental = new Rental(Guid.Parse("22222222-2222-4222-8222-222222222222"), vehicleId, Guid.Parse("33333333-3333-4333-8333-333333333333"), now.AddDays(-2));
            rental.Return(now.AddDays(-1));
            context.Vehicles.Add(vehicle);
            context.Rentals.Add(rental);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
