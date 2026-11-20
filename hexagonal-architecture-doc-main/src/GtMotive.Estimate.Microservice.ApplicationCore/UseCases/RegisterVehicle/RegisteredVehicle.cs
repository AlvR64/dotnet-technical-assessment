using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle
{
    /// <summary>Registered vehicle details.</summary>
    /// <param name="Id">Identifier.</param>
    /// <param name="LicensePlate">Normalized plate.</param>
    /// <param name="Brand">Brand.</param>
    /// <param name="Model">Model.</param>
    /// <param name="ManufacturedOn">Manufacturing date.</param>
    public sealed record RegisteredVehicle(Guid Id, string LicensePlate, string Brand, string Model, DateOnly ManufacturedOn);
}
