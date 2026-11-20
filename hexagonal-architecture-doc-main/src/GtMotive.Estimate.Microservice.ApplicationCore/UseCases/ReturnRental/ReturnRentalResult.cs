namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnRental
{
    /// <summary>Return outcome.</summary>
    public enum ReturnRentalResult
    {
        /// <summary>Returned or already returned.</summary>
        Returned,

        /// <summary>Invalid identifier.</summary>
        Invalid,

        /// <summary>Rental does not exist.</summary>
        NotFound,
    }
}
