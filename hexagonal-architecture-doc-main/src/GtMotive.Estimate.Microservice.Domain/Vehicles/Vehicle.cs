using System;

namespace GtMotive.Estimate.Microservice.Domain.Vehicles
{
    /// <summary>A vehicle in the fleet.</summary>
    public sealed class Vehicle
    {
        /// <summary>Initializes a new instance of the <see cref="Vehicle"/> class.</summary>
        /// <param name="id">Vehicle identifier.</param>
        /// <param name="licensePlate">Registration plate.</param>
        /// <param name="brand">Vehicle brand.</param>
        /// <param name="model">Vehicle model.</param>
        /// <param name="manufacturedOn">Manufacturing date.</param>
        /// <param name="today">Reference date.</param>
        public Vehicle(Guid id, string licensePlate, string brand, string model, DateOnly manufacturedOn, DateOnly today)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Vehicle identifier is required.", nameof(id));
            }

            Id = id;
            LicensePlate = Required(licensePlate, 20, nameof(licensePlate)).ToUpperInvariant();
            Brand = Required(brand, 100, nameof(brand));
            Model = Required(model, 100, nameof(model));
            ManufacturedOn = manufacturedOn;
            if (!IsEligibleOn(today))
            {
                throw new ArgumentOutOfRangeException(nameof(manufacturedOn), "Manufacturing date must not be in the future or more than five calendar years old.");
            }
        }

        private Vehicle()
        {
        }

        /// <summary>Gets the vehicle identifier.</summary>
        public Guid Id { get; private set; }

        /// <summary>Gets the normalized registration plate.</summary>
        public string LicensePlate { get; private set; }

        /// <summary>Gets the vehicle brand.</summary>
        public string Brand { get; private set; }

        /// <summary>Gets the vehicle model.</summary>
        public string Model { get; private set; }

        /// <summary>Gets the manufacturing date.</summary>
        public DateOnly ManufacturedOn { get; private set; }

        /// <summary>Checks fleet eligibility on the reference date.</summary>
        /// <param name="today">Reference date.</param>
        /// <returns>Whether the vehicle is eligible.</returns>
        public bool IsEligibleOn(DateOnly today) => ManufacturedOn <= today && ManufacturedOn >= today.AddYears(-5);

        private static string Required(string value, int maxLength, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A value is required.", name);
            }

            var result = value.Trim();
            if (result.Length > maxLength)
            {
                throw new ArgumentException("Value exceeds the maximum length.", name);
            }

            return result;
        }
    }
}
