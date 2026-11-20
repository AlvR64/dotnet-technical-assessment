using System;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle
{
    /// <summary>Details of a vehicle to register.</summary>
    /// <param name="LicensePlate">Registration plate.</param>
    /// <param name="Brand">Vehicle brand.</param>
    /// <param name="Model">Vehicle model.</param>
    /// <param name="ManufacturedOn">Manufacturing date.</param>
    public sealed record RegisterVehicleCommand(string LicensePlate, string Brand, string Model, DateOnly ManufacturedOn) : IRequest<RegisterVehicleResult>;
}
