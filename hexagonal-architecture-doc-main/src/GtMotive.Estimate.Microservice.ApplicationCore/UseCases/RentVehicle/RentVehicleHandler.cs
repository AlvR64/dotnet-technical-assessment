using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>Creates an immediate rental.</summary>
    public sealed class RentVehicleHandler(IVehicleRepository vehicles, IRentalRepository store, TimeProvider timeProvider) : IRequestHandler<RentVehicleCommand, RentVehicleResult>
    {
        /// <inheritdoc />
        public async Task<RentVehicleResult> Handle(RentVehicleCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.VehicleId == Guid.Empty || request.CustomerId == Guid.Empty)
            {
                return new(RentVehicleError.Invalid);
            }

            var now = timeProvider.GetUtcNow().UtcDateTime;
            var vehicle = await vehicles.FindVehicle(request.VehicleId);
            if (vehicle is null)
            {
                return new(RentVehicleError.VehicleNotFound);
            }

            if (!vehicle.IsEligibleOn(DateOnly.FromDateTime(now)))
            {
                return new(RentVehicleError.VehicleNotEligible);
            }

            if (await store.HasActiveVehicle(request.VehicleId))
            {
                return new(RentVehicleError.VehicleAlreadyRented);
            }

            if (await store.HasActiveCustomer(request.CustomerId))
            {
                return new(RentVehicleError.CustomerHasActiveRental);
            }

            var rental = new Rental(Guid.NewGuid(), request.VehicleId, request.CustomerId, now);
            return await store.TryAdd(rental) switch
            {
                RentalConflict.None => new(RentVehicleError.None, new RentalDetails(rental.Id, rental.VehicleId, rental.CustomerId, rental.RentedAt, rental.ReturnedAt)),
                RentalConflict.Vehicle => new(RentVehicleError.VehicleAlreadyRented),
                RentalConflict.Customer => new(RentVehicleError.CustomerHasActiveRental),
                _ => throw new InvalidOperationException("Unknown rental conflict."),
            };
        }
    }
}
