namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle
{
    /// <summary>Result of registration.</summary>
    /// <param name="Error">Outcome.</param>
    /// <param name="Vehicle">Registered vehicle, if successful.</param>
    /// <param name="Detail">Safe validation detail.</param>
    public sealed record RegisterVehicleResult(RegisterVehicleError Error, RegisteredVehicle Vehicle = null, string Detail = null);
}
