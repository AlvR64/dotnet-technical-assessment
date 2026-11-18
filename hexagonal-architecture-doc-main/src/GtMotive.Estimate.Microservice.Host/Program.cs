using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GtMotive.Estimate.Microservice.Api;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Infrastructure;
using GtMotive.Estimate.Microservice.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, configuration) => configuration.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));
builder.Services.AddControllers(ApiConfiguration.ConfigureControllers).WithApiControllers();
builder.Services.AddBaseInfrastructure(true);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<RentingDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Renting");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:Renting must be configured to use the database.");
    }

    options.UseNpgsql(connectionString);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<RentingDbContext>();
    await context.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapHealthChecks("/health/live");
app.MapControllers();

await app.RunAsync();

[SuppressMessage("Performance", "CA1515:Consider making public types internal", Justification = "Required as the entry point for WebApplicationFactory integration tests.")]
public partial class Program
{
    protected Program()
    {
    }
}
