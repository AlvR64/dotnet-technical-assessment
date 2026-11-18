using System;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    public sealed class RentingDbContext(DbContextOptions<RentingDbContext> options) : DbContext(options)
    {
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();

        public DbSet<Rental> Rentals => Set<Rental>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RentingDbContext).Assembly);
        }
    }
}
