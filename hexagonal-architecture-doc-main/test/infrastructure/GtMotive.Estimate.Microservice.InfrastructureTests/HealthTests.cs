using System;
using System.Net;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GtMotive.Estimate.Microservice.InfrastructureTests
{
    public sealed class HealthTests
    {
        [Theory]
        [InlineData("Development")]
        [InlineData("Production")]
        public async Task LiveEndpointReturnsSuccess(string environment)
        {
            await using var rootFactory = new WebApplicationFactory<Program>();
            await using var factory = rootFactory.WithWebHostBuilder(builder => builder.UseEnvironment(environment));
            using var client = factory.CreateClient();
            using var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task MissingDatabaseConfigurationReportsSafeError()
        {
            await using var factory = new WebApplicationFactory<Program>();
            using var scope = factory.Services.CreateScope();

            var error = Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<RentingDbContext>());
            Assert.Contains("ConnectionStrings:Renting", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        [Trait("Category", "Postgres")]
        public async Task ConfiguredHostConnectsToPostgres()
        {
            var connection = Environment.GetEnvironmentVariable("RENTING_TEST_CONNECTION")
                ?? throw new InvalidOperationException("Set RENTING_TEST_CONNECTION to run PostgreSQL integration tests.");
            await using var rootFactory = new WebApplicationFactory<Program>();
            await using var factory = rootFactory.WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:Renting", connection));
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RentingDbContext>();

            Assert.True(await context.Database.CanConnectAsync());
        }
    }
}
