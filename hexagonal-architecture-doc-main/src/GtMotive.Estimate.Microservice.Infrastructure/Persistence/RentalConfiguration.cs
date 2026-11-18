using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    internal sealed class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.ToTable("rentals", table => table.HasCheckConstraint("ck_rentals_returned_at", "returned_at IS NULL OR returned_at >= rented_at"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.VehicleId).HasColumnName("vehicle_id").IsRequired();
            builder.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
            builder.Property(x => x.RentedAt).HasColumnName("rented_at").IsRequired();
            builder.Property(x => x.ReturnedAt).HasColumnName("returned_at");
            builder.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.VehicleId).IsUnique().HasFilter("returned_at IS NULL").HasDatabaseName("ux_rentals_active_vehicle");
            builder.HasIndex(x => x.CustomerId).IsUnique().HasFilter("returned_at IS NULL").HasDatabaseName("ux_rentals_active_customer");
        }
    }
}
