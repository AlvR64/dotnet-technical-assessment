using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>Rental details.</summary>
    /// <param name="Id">Rental identifier.</param>
    /// <param name="VehicleId">Vehicle identifier.</param>
    /// <param name="CustomerId">External customer identifier.</param>
    /// <param name="RentedAt">UTC start instant.</param>
    /// <param name="ReturnedAt">UTC return instant, if any.</param>
    public sealed record RentalDetails(Guid Id, Guid VehicleId, Guid CustomerId, DateTime RentedAt, DateTime? ReturnedAt);
}
