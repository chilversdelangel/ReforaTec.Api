using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReforaTec.Api.Entities;

namespace ReforaTec.Api.Database.Configurations;

internal sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.Property(measurement => measurement.HeightCentimeters)
            .HasPrecision(7, 2);

        builder.Property(measurement => measurement.PhotoUrl)
            .HasMaxLength(300);

        builder.HasIndex(measurement => measurement.TenantId);
        builder.HasIndex(measurement => measurement.TreeId);
        builder.HasIndex(measurement => measurement.UserId);

        builder.HasOne(measurement => measurement.Tenant)
            .WithMany()
            .HasForeignKey(measurement => measurement.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(measurement => measurement.Tree)
            .WithMany()
            .HasForeignKey(measurement => measurement.TreeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(measurement => measurement.User)
            .WithMany()
            .HasForeignKey(measurement => measurement.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
