using System;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>A vehicle rental and its return.</summary>
    public sealed class Rental
    {
        /// <summary>Initializes a new instance of the <see cref="Rental"/> class.</summary>
        /// <param name="id">Rental identifier.</param>
        /// <param name="vehicleId">Vehicle identifier.</param>
        /// <param name="customerId">External customer identifier.</param>
        /// <param name="rentedAt">Start instant in UTC.</param>
        public Rental(Guid id, Guid vehicleId, Guid customerId, DateTime rentedAt)
        {
            if (id == Guid.Empty || vehicleId == Guid.Empty || customerId == Guid.Empty)
            {
                throw new ArgumentException("Rental, vehicle and customer identifiers are required.");
            }

            RequireUtc(rentedAt, nameof(rentedAt));
            Id = id;
            VehicleId = vehicleId;
            CustomerId = customerId;
            RentedAt = rentedAt;
        }

        private Rental()
        {
        }

        /// <summary>Gets the rental identifier.</summary>
        public Guid Id { get; private set; }

        /// <summary>Gets the vehicle identifier.</summary>
        public Guid VehicleId { get; private set; }

        /// <summary>Gets the external customer identifier.</summary>
        public Guid CustomerId { get; private set; }

        /// <summary>Gets the start instant in UTC.</summary>
        public DateTime RentedAt { get; private set; }

        /// <summary>Gets the return instant in UTC, if returned.</summary>
        public DateTime? ReturnedAt { get; private set; }

        /// <summary>Records the first return of the rental.</summary>
        /// <param name="returnedAt">Return instant in UTC.</param>
        public void Return(DateTime returnedAt)
        {
            if (ReturnedAt.HasValue)
            {
                return;
            }

            RequireUtc(returnedAt, nameof(returnedAt));
            if (returnedAt < RentedAt)
            {
                throw new ArgumentOutOfRangeException(nameof(returnedAt), "Return cannot precede rental.");
            }

            ReturnedAt = returnedAt;
        }

        private static void RequireUtc(DateTime value, string name)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("An instant in UTC is required.", name);
            }
        }
    }
}
