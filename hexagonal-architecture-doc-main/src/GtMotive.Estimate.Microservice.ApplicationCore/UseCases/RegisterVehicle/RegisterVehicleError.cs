namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle
{
    /// <summary>Registration outcome.</summary>
    public enum RegisterVehicleError
    {
        /// <summary>Registration succeeded.</summary>
        None,

        /// <summary>Vehicle data was invalid.</summary>
        Invalid,

        /// <summary>The registration plate already exists.</summary>
        DuplicatePlate,
    }
}
