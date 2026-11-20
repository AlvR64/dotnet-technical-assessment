using System;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles
{
    /// <summary>Available vehicle details.</summary>
    /// <param name="Id">Vehicle identifier.</param>
    /// <param name="LicensePlate">Registration plate.</param>
    /// <param name="Brand">Vehicle brand.</param>
    /// <param name="Model">Vehicle model.</param>
    /// <param name="ManufacturedOn">Manufacturing date.</param>
    public sealed record AvailableVehicle(Guid Id, string LicensePlate, string Brand, string Model, DateOnly ManufacturedOn);
}
