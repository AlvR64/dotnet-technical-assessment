using System;
using System.Linq;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    public sealed class RentalRepository(RentingDbContext context, IUnitOfWork unitOfWork) : IRentalRepository
    {
        public Task<bool> HasActiveVehicle(Guid id) => context.Rentals.AnyAsync(rental => rental.VehicleId == id && rental.ReturnedAt == null);

        public Task<bool> HasActiveCustomer(Guid id) => context.Rentals.AnyAsync(rental => rental.CustomerId == id && rental.ReturnedAt == null);

        public async Task<RentalConflict> TryAdd(Rental rental)
        {
            context.Rentals.Add(rental);
            try
            {
                await unitOfWork.Save();
                return RentalConflict.None;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_rentals_active_vehicle" })
            {
                return RentalConflict.Vehicle;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_rentals_active_customer" })
            {
                return RentalConflict.Customer;
            }
        }

        public async Task<bool> RecordReturn(Guid id, DateTime returnedAt)
        {
            var updated = await context.Rentals.Where(rental => rental.Id == id && rental.ReturnedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(rental => rental.ReturnedAt, returnedAt));
            return updated != 0 || await context.Rentals.AnyAsync(rental => rental.Id == id);
        }
    }
}
