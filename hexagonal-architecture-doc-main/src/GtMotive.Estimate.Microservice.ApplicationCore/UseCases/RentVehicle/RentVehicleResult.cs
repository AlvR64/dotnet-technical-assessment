namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>Rental result.</summary>
    /// <param name="Error">Outcome.</param>
    /// <param name="Rental">Created rental, if successful.</param>
    public sealed record RentVehicleResult(RentVehicleError Error, RentalDetails Rental = null);
}
