using System;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>Requests an immediate rental.</summary>
    /// <param name="VehicleId">Vehicle identifier.</param>
    /// <param name="CustomerId">External customer identifier.</param>
    public sealed record RentVehicleCommand(Guid VehicleId, Guid CustomerId) : IRequest<RentVehicleResult>;
}
