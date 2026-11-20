using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>Persistence operations for fleet vehicles.</summary>
    public interface IVehicleRepository
    {
        /// <summary>Finds a vehicle by identifier.</summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <returns>The vehicle or null.</returns>
        Task<Vehicle> FindVehicle(Guid id);

        /// <summary>Saves a vehicle, reporting duplicate registration plates.</summary>
        /// <param name="vehicle">Vehicle to save.</param>
        /// <returns>True if saved.</returns>
        Task<bool> TryAdd(Vehicle vehicle);

        /// <summary>Lists eligible vehicles with no active rental.</summary>
        /// <param name="today">Reference date.</param>
        /// <returns>The available vehicles.</returns>
        Task<IReadOnlyList<Vehicle>> ListAvailable(DateOnly today);
    }
}
