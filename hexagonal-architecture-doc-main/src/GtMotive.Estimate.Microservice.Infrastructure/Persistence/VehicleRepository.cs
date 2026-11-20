using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    public sealed class VehicleRepository(RentingDbContext context, IUnitOfWork unitOfWork) : IVehicleRepository
    {
        public Task<Vehicle> FindVehicle(Guid id) => context.Vehicles.AsNoTracking().SingleOrDefaultAsync(vehicle => vehicle.Id == id);

        public async Task<IReadOnlyList<Vehicle>> ListAvailable(DateOnly today)
        {
            var cutoff = today.AddYears(-5);
            return await context.Vehicles.AsNoTracking()
                .Where(vehicle => vehicle.ManufacturedOn >= cutoff && vehicle.ManufacturedOn <= today
                    && !context.Rentals.Any(rental => rental.VehicleId == vehicle.Id && rental.ReturnedAt == null))
                .OrderBy(vehicle => vehicle.LicensePlate)
                .ThenBy(vehicle => vehicle.Id)
                .ToListAsync();
        }

        public async Task<bool> TryAdd(Vehicle vehicle)
        {
            context.Vehicles.Add(vehicle);
            try
            {
                await unitOfWork.Save();
                return true;
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_vehicles_license_plate" })
            {
                return false;
            }
        }
    }
}
