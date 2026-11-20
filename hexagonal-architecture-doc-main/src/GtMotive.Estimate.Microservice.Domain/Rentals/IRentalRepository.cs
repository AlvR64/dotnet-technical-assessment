using System;
using System.Threading.Tasks;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>Persistence operations for rentals.</summary>
    public interface IRentalRepository
    {
        /// <summary>Checks for an active vehicle rental.</summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <returns>Whether one exists.</returns>
        Task<bool> HasActiveVehicle(Guid id);

        /// <summary>Checks for an active customer rental.</summary>
        /// <param name="id">Customer identifier.</param>
        /// <returns>Whether one exists.</returns>
        Task<bool> HasActiveCustomer(Guid id);

        /// <summary>Saves a rental.</summary>
        /// <param name="rental">Rental to save.</param>
        /// <returns>The persistence conflict, if any.</returns>
        Task<RentalConflict> TryAdd(Rental rental);

        /// <summary>Records the first return or recognizes a previous one.</summary>
        /// <param name="id">Rental identifier.</param>
        /// <param name="returnedAt">UTC return time.</param>
        /// <returns>Whether the rental exists.</returns>
        Task<bool> RecordReturn(Guid id, DateTime returnedAt);
    }
}
