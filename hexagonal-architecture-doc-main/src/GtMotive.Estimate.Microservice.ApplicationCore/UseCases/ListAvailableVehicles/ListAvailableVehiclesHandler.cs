using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles
{
    /// <summary>Reads currently available vehicles.</summary>
    public sealed class ListAvailableVehiclesHandler(IVehicleRepository repository, TimeProvider timeProvider) : IRequestHandler<ListAvailableVehiclesQuery, IReadOnlyList<AvailableVehicle>>
    {
        /// <inheritdoc />
        public async Task<IReadOnlyList<AvailableVehicle>> Handle(ListAvailableVehiclesQuery request, CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
            var vehicles = await repository.ListAvailable(today);
#pragma warning disable IDE0305 // StyleCop's parser does not support collection expressions.
            return vehicles.Select(vehicle => new AvailableVehicle(vehicle.Id, vehicle.LicensePlate, vehicle.Brand, vehicle.Model, vehicle.ManufacturedOn)).ToList();
#pragma warning restore IDE0305
        }
    }
}
