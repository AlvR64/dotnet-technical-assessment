using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    public sealed class RentingDbContextFactory : IDesignTimeDbContextFactory<RentingDbContext>
    {
        public RentingDbContext CreateDbContext(string[] args)
        {
            var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Renting")
                ?? "Host=localhost;Database=renting;Username=renting;Password=renting-local";
            var options = new DbContextOptionsBuilder<RentingDbContext>().UseNpgsql(connection).Options;
            return new RentingDbContext(options);
        }
    }
}
