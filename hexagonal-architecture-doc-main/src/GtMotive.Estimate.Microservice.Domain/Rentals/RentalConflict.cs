namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>Possible persistence conflicts when creating a rental.</summary>
    public enum RentalConflict
    {
        /// <summary>No conflict.</summary>
        None,

        /// <summary>The vehicle has an active rental.</summary>
        Vehicle,

        /// <summary>The customer has an active rental.</summary>
        Customer,
    }
}
