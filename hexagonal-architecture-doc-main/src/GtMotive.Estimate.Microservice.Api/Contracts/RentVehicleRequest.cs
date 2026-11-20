using System;
using System.ComponentModel.DataAnnotations;

namespace GtMotive.Estimate.Microservice.Api.Contracts
{
    public sealed class RentVehicleRequest
    {
        [Required]
        public Guid? VehicleId { get; set; }

        [Required]
        public Guid? CustomerId { get; set; }
    }
}
