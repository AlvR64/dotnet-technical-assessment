using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RegisterVehicle;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests
{
    public sealed class VehicleValidationTests
    {
        [Fact]
        public async Task MissingRequiredFieldReturnsBadRequestWithoutCallingUseCase()
        {
            var useCase = new Mock<IRequestHandler<RegisterVehicleCommand, RegisterVehicleResult>>(MockBehavior.Strict);
            await using var root = new WebApplicationFactory<Program>();
            await using var factory = root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRequestHandler<RegisterVehicleCommand, RegisterVehicleResult>>();
                services.AddSingleton(useCase.Object);
            }));
            using var client = factory.CreateClient();
            using var response = await client.PostAsJsonAsync(new Uri("/vehicles", UriKind.Relative), new { brand = "Toyota", model = "Corolla", manufacturedOn = "2026-01-01" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            useCase.Verify(x => x.Handle(It.IsAny<RegisterVehicleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
