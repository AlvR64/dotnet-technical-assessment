using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Contracts;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Controllers
{
    [ApiController]
    [Route("vehicles")]
    public sealed class VehiclesController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool? available)
        {
            if (available == false)
            {
                var problem = new ProblemDetails { Status = 400, Title = "Unsupported availability filter" };
                problem.Extensions["code"] = "invalid_request";
                return BadRequest(problem);
            }

            var vehicles = await mediator.Send(new ListAvailableVehiclesQuery());
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterVehicleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = await mediator.Send(new RegisterVehicleCommand(request.LicensePlate, request.Brand, request.Model, request.ManufacturedOn.GetValueOrDefault()));
            if (result.Error == RegisterVehicleError.None)
            {
                return StatusCode(201, result.Vehicle);
            }

            var invalid = result.Error == RegisterVehicleError.Invalid;
            var problem = new ProblemDetails { Status = invalid ? 400 : 409, Title = invalid ? "Invalid vehicle" : "Conflict", Detail = invalid ? result.Detail : "The license plate already exists." };
            problem.Extensions["code"] = invalid ? "invalid_vehicle" : "duplicate_license_plate";
            return new ObjectResult(problem) { StatusCode = problem.Status };
        }
    }
}
