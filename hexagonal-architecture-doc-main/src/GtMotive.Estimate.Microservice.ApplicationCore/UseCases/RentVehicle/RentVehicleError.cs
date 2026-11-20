namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>Rental outcome.</summary>
    public enum RentVehicleError
    {
        /// <summary>Rental created.</summary>
        None,

        /// <summary>Identifiers are invalid.</summary>
        Invalid,

        /// <summary>Vehicle does not exist.</summary>
        VehicleNotFound,

        /// <summary>Vehicle is not eligible.</summary>
        VehicleNotEligible,

        /// <summary>Vehicle is rented.</summary>
        VehicleAlreadyRented,

        /// <summary>Customer already has an active rental.</summary>
        CustomerHasActiveRental,
    }
}
