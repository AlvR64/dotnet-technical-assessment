using System.Collections.Generic;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles
{
    /// <summary>Requests currently available vehicles.</summary>
    public sealed record ListAvailableVehiclesQuery : IRequest<IReadOnlyList<AvailableVehicle>>;
}
