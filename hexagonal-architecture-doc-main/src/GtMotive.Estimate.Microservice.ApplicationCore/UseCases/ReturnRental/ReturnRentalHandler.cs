using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using MediatR;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnRental
{
    /// <summary>Records a rental return idempotently.</summary>
    public sealed class ReturnRentalHandler(IRentalRepository store, TimeProvider timeProvider) : IRequestHandler<ReturnRentalCommand, ReturnRentalResult>
    {
        /// <inheritdoc />
        public async Task<ReturnRentalResult> Handle(ReturnRentalCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.Id == Guid.Empty)
            {
                return ReturnRentalResult.Invalid;
            }

            return await store.RecordReturn(request.Id, timeProvider.GetUtcNow().UtcDateTime)
                ? ReturnRentalResult.Returned : ReturnRentalResult.NotFound;
        }
    }
}
