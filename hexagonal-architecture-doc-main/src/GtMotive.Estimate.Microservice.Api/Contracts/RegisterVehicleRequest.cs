using System;
using System.ComponentModel.DataAnnotations;

namespace GtMotive.Estimate.Microservice.Api.Contracts
{
    public sealed class RegisterVehicleRequest
    {
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [Required]
        [StringLength(100)]
        public string Brand { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        [Required]
        public DateOnly? ManufacturedOn { get; set; }
    }
}
