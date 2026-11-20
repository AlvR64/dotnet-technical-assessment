using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle
{
    /// <summary>Registers an eligible vehicle.</summary>
    public sealed class RegisterVehicleHandler(IVehicleRepository store, TimeProvider timeProvider) : IRequestHandler<RegisterVehicleCommand, RegisterVehicleResult>
    {
        /// <inheritdoc />
        public async Task<RegisterVehicleResult> Handle(RegisterVehicleCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            Vehicle vehicle;
            try
            {
                var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
                vehicle = new Vehicle(Guid.NewGuid(), request.LicensePlate, request.Brand, request.Model, request.ManufacturedOn, today);
            }
            catch (ArgumentException exception)
            {
                return new RegisterVehicleResult(RegisterVehicleError.Invalid, Detail: exception.Message);
            }

            if (!await store.TryAdd(vehicle))
            {
                return new RegisterVehicleResult(RegisterVehicleError.DuplicatePlate);
            }

            return new RegisterVehicleResult(RegisterVehicleError.None, new RegisteredVehicle(vehicle.Id, vehicle.LicensePlate, vehicle.Brand, vehicle.Model, vehicle.ManufacturedOn));
        }
    }
}
