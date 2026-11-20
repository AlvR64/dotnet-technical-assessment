using System;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnRental
{
    /// <summary>Requests a rental return.</summary>
    /// <param name="Id">Rental identifier.</param>
    public sealed record ReturnRentalCommand(Guid Id) : IRequest<ReturnRentalResult>;
}
