using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Contracts;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnRental;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Controllers
{
    [ApiController]
    [Route("rentals")]
    public sealed class RentalsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Rent([FromBody] RentVehicleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = await mediator.Send(new RentVehicleCommand(request.VehicleId.GetValueOrDefault(), request.CustomerId.GetValueOrDefault()));
            if (result.Error == RentVehicleError.None)
            {
                return StatusCode(201, result.Rental);
            }

            var (status, code) = result.Error switch
            {
                RentVehicleError.None => throw new InvalidOperationException("Rental already created."),
                RentVehicleError.Invalid => (400, "invalid_request"),
                RentVehicleError.VehicleNotFound => (404, "vehicle_not_found"),
                RentVehicleError.VehicleNotEligible => (409, "vehicle_not_eligible"),
                RentVehicleError.VehicleAlreadyRented => (409, "vehicle_already_rented"),
                RentVehicleError.CustomerHasActiveRental => (409, "customer_has_active_rental"),
                _ => throw new InvalidOperationException("Unknown rental result."),
            };
            return Rejected(status, code, "Rental rejected");
        }

        [HttpPut("{rentalId}/return")]
        public async Task<IActionResult> Return([FromRoute] Guid rentalId)
        {
            var result = await mediator.Send(new ReturnRentalCommand(rentalId));
            return result switch
            {
                ReturnRentalResult.Returned => NoContent(),
                ReturnRentalResult.Invalid => Rejected(400, "invalid_request", "Return rejected"),
                ReturnRentalResult.NotFound => Rejected(404, "rental_not_found", "Return rejected"),
                _ => throw new InvalidOperationException("Unknown return result."),
            };
        }

        private static ObjectResult Rejected(int status, string code, string title)
        {
            var problem = new ProblemDetails { Status = status, Title = title };
            problem.Extensions["code"] = code;
            return new ObjectResult(problem) { StatusCode = status };
        }
    }
}
