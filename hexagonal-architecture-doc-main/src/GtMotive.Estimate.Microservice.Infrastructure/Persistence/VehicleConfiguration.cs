using GtMotive.Estimate.Microservice.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GtMotive.Estimate.Microservice.Infrastructure.Persistence
{
    internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("vehicles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.LicensePlate).HasColumnName("license_plate").HasMaxLength(20).IsRequired();
            builder.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
            builder.Property(x => x.ManufacturedOn).HasColumnName("manufactured_on").IsRequired();
            builder.HasIndex(x => x.LicensePlate).IsUnique().HasDatabaseName("ux_vehicles_license_plate");
        }
    }
}
